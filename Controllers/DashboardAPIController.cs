using Microsoft.AspNetCore.Mvc;
using WEBAPP_FitMatch.Data;
using Microsoft.EntityFrameworkCore;

namespace WEBAPP_FitMatch.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardAPIController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DashboardAPIController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("my-activities")]
        public async Task<IActionResult> GetMyActivities()
        {
            // 1. เช็คว่าใครล็อกอินอยู่
            var userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null) return Unauthorized(new { message = "กรุณาล็อกอิน" });

            // 2. ดึงโพสต์ทั้งหมดที่เราเป็นสมาชิกอยู่ (รวมถึงโพสต์ที่เราเป็นคนสร้าง)
            var myPosts = await _db.Posts
                .Include(p => p.Members)
                .Where(p => p.UserId == userId || p.Members.Any(m => m.UserId == userId))
                .OrderBy(p => p.EventDateTime) 
                .Select(p => new 
                {
                    p.PostId,
                    p.Title,
                    p.Location,
                    p.EventDateTime,
                    p.Status,
                    p.SportType // ดึงประเภทกีฬามาด้วยเพื่อไปเปลี่ยนไอคอนที่หน้าเว็บ
                })
                .ToListAsync();

            var now = DateTime.UtcNow;

            // 3. คัดแยก Upcoming (เวลายังไม่ถึง และ สถานะยังไม่ปิด)
            var upcoming = myPosts
                .Where(p => p.EventDateTime > now && p.Status.ToLower() != "closed" && p.Status.ToLower() != "close")
                .ToList();

            // 4. คัดแยก History (เวลาผ่านมาแล้ว หรือ สถานะถูกปิดไปแล้ว)
            var history = myPosts
                .Where(p => p.EventDateTime <= now || p.Status.ToLower() == "closed" || p.Status.ToLower() == "close")
                .OrderByDescending(p => p.EventDateTime) 
                .ToList();

            // 5. ส่งข้อมูล 2 ก้อนกลับไปให้ JavaScript ที่หน้าเว็บ
            return Ok(new { upcoming, history });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null) return Unauthorized(new { message = "Login Please" });

            var now = DateTime.UtcNow;

            // --- เตรียมข้อมูล: ดึง PostId ทั้งหมดที่เราเคยเข้าร่วม ---
            var myJoinedPostIds = await _db.Members
                .Where(m => m.UserId == userId.Value)
                .Select(m => m.PostId)
                .ToListAsync();

            // 📊 1. Participated Rate (เดือนนี้)
            var participatedCount = await _db.Posts
                .Where(p => myJoinedPostIds.Contains(p.PostId) && 
                            p.EventDateTime.Month == now.Month && 
                            p.EventDateTime.Year == now.Year)
                .CountAsync();

            // 🥧 2. Activity Stats (แยกประเภทกีฬา)
            var activityStats = await _db.Posts
                .Where(p => myJoinedPostIds.Contains(p.PostId))
                .GroupBy(p => p.SportType)
                .Select(g => new { 
                    sport = string.IsNullOrEmpty(g.Key) ? "Other" : g.Key, 
                    count = g.Count() 
                })
                .ToListAsync();

            // 📝 3. Mission Succeeded (ดึงจากตาราง Mission)
            var missions = await _db.Missions
                .Where(m => m.UserId == userId.Value)
                .OrderByDescending(m => m.CreatedAt)
                .Take(5) // ดึงมาโชว์ 5 อันล่าสุด
                .Select(m => new {
                    m.MissionId,
                    m.Description,
                    m.IsCompleted
                })
                .ToListAsync();

            // 🤝 4. Favorite Partners (เพื่อนที่เล่นด้วยบ่อยสุด 3 อันดับ)
            var topPartners = await _db.Members
                .Where(m => myJoinedPostIds.Contains(m.PostId) && m.UserId != userId.Value) // ไม่เอาตัวเอง
                .GroupBy(m => m.UserId)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => new {
                    partnerId = g.Key,
                    meetCount = g.Count()
                })
                .ToListAsync();

            // ไปดึงชื่อและรูปโปรไฟล์ของเพื่อนมาประกอบร่าง
            var partnerDetails = new List<object>();
            foreach(var tp in topPartners)
            {
                var user = await _db.Users.FindAsync(tp.partnerId);
                if(user != null)
                {
                    partnerDetails.Add(new {
                        username = user.Username,
                        profileUrl = user.ProfileUrl ?? "https://upload.wikimedia.org/wikipedia/commons/7/7c/Profile_avatar_placeholder_large.png?_=20150327203541",
                        meetCount = tp.meetCount
                    });
                }
            }

            // 🚀 ส่งข้อมูลทั้ง 4 กล่องกลับไปให้หน้าเว็บรวดเดียวจบ!
            return Ok(new {
                participated = participatedCount,
                targetParticipated = 30, // เป้าหมายรายเดือน
                activityStats = activityStats,
                missions = missions,
                favoritePartners = partnerDetails
            });
        }
    }
}