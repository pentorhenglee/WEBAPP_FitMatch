using Microsoft.AspNetCore.Mvc;
using WEBAPP_FitMatch.Data;
using WEBAPP_FitMatch.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.AccessControl;

namespace WEBAPP_FitMatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PostAPIController(AppDbContext db)
        {
            _db = db;
        }

        private NpgsqlConnection GetConnection()
        {
            var connectionString = _db.Database.GetConnectionString();
            return new NpgsqlConnection(connectionString);
        }

        [HttpGet]
        public async Task<ActionResult> GetPost()
        {
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null)
                return Unauthorized("User not logged in");

            var posts = await _db.Posts
                .Where(p => p.UserId == user_id.Value)
                .ToListAsync();

            return Ok(posts);
        }

        [HttpPost]
        public async Task<ActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null)
                return Unauthorized("User not logged in ");

            var post = new Post
            {
                Title = dto.Title,
                Location = dto.Location,
                DateTime = DateTime.SpecifyKind(dto.DateTime, DateTimeKind.Utc), // ✅ กำหนด UTC
                Description = dto.Description,
                SportType = dto.SportType,
                MaxPeople = dto.MaxPeople,
                UserId = user_id.Value
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
            return Ok(post);

        }



    }
}
