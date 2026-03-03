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
        public async Task<ActionResult> GetMyActivities()
        {
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null) return Unauthorized("User not logged in");

            // 1. ดึง ID โพสต์ทั้งหมดที่ User คนนี้เข้าร่วม
            var myJoinedPostIds = await _db.Members
                .Where(m => m.UserId == user_id.Value)
                .Select(m => m.PostId)
                .ToListAsync();

            // 🌟 ใช้ DateTime.UtcNow เป็นเกณฑ์มาตรฐานโลก
            var currentTime = DateTime.UtcNow;

            // 🌟 2. ให้ Database กรอง Upcoming: เวลายังไม่ถึง (>) และ ต้องยังเปิดรับอยู่ (open)
            var upcoming = await _db.Posts
                .Where(p => myJoinedPostIds.Contains(p.PostId) && p.Status.ToLower() == "open" && p.EventDateTime > currentTime)
                .OrderBy(p => p.EventDateTime) // เรียงจากงานที่ใกล้จะถึงที่สุดขึ้นก่อน
                .Select(p => new {
                    p.PostId,
                    p.Title,
                    p.Location,
                    p.EventDateTime,
                    p.SportType
                }).ToListAsync();

            // 🌟 3. ให้ Database กรอง History: เวลาผ่านไปแล้ว (<=) หรือ โพสต์ถูกปิดไปแล้ว (close)
            var history = await _db.Posts
                .Where(p => myJoinedPostIds.Contains(p.PostId) && (p.Status.ToLower() == "close" || p.EventDateTime <= currentTime))
                .OrderByDescending(p => p.EventDateTime) // เรียงจากงานที่เพิ่งจบไปหางานเก่าๆ
                .Select(p => new {
                    p.PostId,
                    p.Title,
                    p.Location,
                    p.EventDateTime,
                    p.SportType
                }).ToListAsync();

            return Ok(new 
            {
                upcoming = upcoming,
                history = history
            });
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