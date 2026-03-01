using Microsoft.AspNetCore.Mvc;
using WEBAPP_FitMatch.Data;
using WEBAPP_FitMatch.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;


namespace WEBAPP_FitMatch.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MemberAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MemberAPIController(AppDbContext db)
        {
            _db = db;
        }
        
        [HttpPost]
        [Route("/api/memberapi/join/{postid}")]
        public async Task<ActionResult> JoinPost(int postid)
        {
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null)
                return Unauthorized("User no logged in");

            var post = await _db.Posts.FirstOrDefaultAsync(p=>p.PostId == postid);
            if (post == null)
                return NotFound("Post not found");
            
            var existingMember = await _db.Members
                .FirstOrDefaultAsync(m => m.PostId == postid && m.UserId == user_id.Value);
            
            if (existingMember != null)
                return BadRequest("You have already joined this post");
            
            var member = new Member
            {
                PostId = postid,
                UserId = user_id.Value,
                Status = "panding",
                JoinedAt = DateTime.UtcNow,
            };
            _db.Members.Add(member);
            await _db.SaveChangesAsync();
            return Ok(member);
        }
    }
}