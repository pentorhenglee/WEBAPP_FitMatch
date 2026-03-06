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
            var userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null) return Unauthorized(new { message = "กรุณาล็อกอิน" });

            // pull all the post we're in (owner too)
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
                    p.SportType
                })
                .ToListAsync();

            // DB เก็บเวลาไทย (ค่าจริงเป็น +7 แต่ Kind=Unspecified)  ต้องเทียบกับเวลาไทยเช่นกัน
            var now = DateTime.UtcNow.AddHours(7);

            var upcoming = myPosts
                .Where(p => p.EventDateTime > now && p.Status.ToLower() != "closed" && p.Status.ToLower() != "close")
                .ToList();

            var history = myPosts
                .Where(p => p.EventDateTime <= now || p.Status.ToLower() == "closed" || p.Status.ToLower() == "close")
                .OrderByDescending(p => p.EventDateTime) 
                .ToList();

            return Ok(new { upcoming, history });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null) return Unauthorized(new { message = "Login Please" });

            var now = DateTime.UtcNow;

            // ดึง PostId ทั้งหมดที่เราเคยเข้าร่วม
            var myJoinedPostIds = await _db.Members
                .Where(m => m.UserId == userId.Value)
                .Select(m => m.PostId)
                .ToListAsync();

            //Participated Rate (เดือนนี้)
            var participatedCount = await _db.Posts
                .Where(p => myJoinedPostIds.Contains(p.PostId) && 
                            p.EventDateTime.Month == now.Month && 
                            p.EventDateTime.Year == now.Year)
                .CountAsync();

            //Activity Stats
            var activityStats = await _db.Posts
                .Where(p => myJoinedPostIds.Contains(p.PostId))
                .GroupBy(p => p.SportType)
                .Select(g => new { 
                    sport = string.IsNullOrEmpty(g.Key) ? "Other" : g.Key, 
                    count = g.Count() 
                })
                .ToListAsync();

            // Mission Succeeded (ดึงจากตาราง Mission)
            var missions = await _db.Missions
                .Where(m => m.UserId == userId.Value)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new {
                    m.MissionId,
                    m.Description,
                    m.IsCompleted
                })
                .ToListAsync();

            var nowThai = DateTime.UtcNow.AddHours(7);
            
            var eligiblePostIds = await _db.Posts
                .Where(p => myJoinedPostIds.Contains(p.PostId) &&
                            (p.EventDateTime <= nowThai ||
                            p.Status.ToLower() == "closed" ||
                            p.Status.ToLower() == "close"))
                .Select(p => p.PostId)
                .ToListAsync();

            // Top Partners (3 people)
            var topPartners = await _db.Members
                .Where(m => eligiblePostIds.Contains(m.PostId) && m.UserId != userId.Value)
                .GroupBy(m => m.UserId)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => new {
                    partnerId = g.Key,
                    meetCount = g.Count()
                })
                .ToListAsync();

            var partnerDetails = new List<object>();
            foreach(var tp in topPartners)
            {
                var user = await _db.Users.FindAsync(tp.partnerId);
                if(user != null)
                {
                    partnerDetails.Add(new {
                        userId = user.Id,
                        username = user.Username,
                        profileUrl = user.ProfileUrl ?? "https://upload.wikimedia.org/wikipedia/commons/7/7c/Profile_avatar_placeholder_large.png?_=20150327203541",
                        meetCount = tp.meetCount
                    });
                }
            }

            var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            return Ok(new {
                participated = participatedCount,
                targetParticipated = daysInMonth,
                activityStats = activityStats,
                missions = missions,
                favoritePartners = partnerDetails
            });
        }

        [HttpGet("stats/{targetUserId:int}")]
        public async Task<IActionResult> GetUserStats(int targetUserId)
        {
            var sessionUserId = HttpContext.Session.GetInt32("user_id");
            if (sessionUserId == null) return Unauthorized(new { message = "Login Please" });

            var userExists = await _db.Users.AnyAsync(u => u.Id == targetUserId);
            if (!userExists) return NotFound(new { message = "User not found" });

            var now = DateTime.UtcNow;
            var nowThai = now.AddHours(7);

            var joinedPostIds = await _db.Members
                .Where(m => m.UserId == targetUserId)
                .Select(m => m.PostId)
                .ToListAsync();

            var participatedCount = await _db.Posts
                .Where(p => joinedPostIds.Contains(p.PostId) &&
                            p.EventDateTime.Month == now.Month &&
                            p.EventDateTime.Year == now.Year)
                .CountAsync();

            var activityStats = await _db.Posts
                .Where(p => joinedPostIds.Contains(p.PostId))
                .GroupBy(p => p.SportType)
                .Select(g => new {
                    sport = string.IsNullOrEmpty(g.Key) ? "Other" : g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            var eligiblePostIds = await _db.Posts
                .Where(p => joinedPostIds.Contains(p.PostId) &&
                            (p.EventDateTime <= nowThai ||
                             p.Status.ToLower() == "closed" ||
                             p.Status.ToLower() == "close"))
                .Select(p => p.PostId)
                .ToListAsync();

            var topPartners = await _db.Members
                .Where(m => eligiblePostIds.Contains(m.PostId) && m.UserId != targetUserId)
                .GroupBy(m => m.UserId)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => new { partnerId = g.Key, meetCount = g.Count() })
                .ToListAsync();

            var partnerDetails = new List<object>();
            foreach (var tp in topPartners)
            {
                var user = await _db.Users.FindAsync(tp.partnerId);
                if (user != null)
                {
                    partnerDetails.Add(new {
                        userId = user.Id,
                        username = user.Username,
                        profileUrl = user.ProfileUrl ?? "https://upload.wikimedia.org/wikipedia/commons/7/7c/Profile_avatar_placeholder_large.png?_=20150327203541",
                        meetCount = tp.meetCount
                    });
                }
            }

            var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            return Ok(new {
                participated = participatedCount,
                targetParticipated = daysInMonth,
                activityStats,
                favoritePartners = partnerDetails
            });
        }
    }
}