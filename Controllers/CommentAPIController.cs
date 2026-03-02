using Microsoft.AspNetCore.Mvc;
using WEBAPP_FitMatch.Data;
using WEBAPP_FitMatch.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace WEBAPP_FitMatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CommentAPIController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("{post_id}")]
        public async Task<ActionResult> CreateComment([FromBody] Comment comment,int post_id)
        {
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null) return Unauthorized("User not logged in");

            var createcomment = new Comment
            {
                UserId = user_id.Value,
                PostId = post_id,
                CreatedAt = DateTime.UtcNow,
                Text = comment.Text
            };

            _db.Comments.Add(createcomment);
            await _db.SaveChangesAsync();
            return Ok(createcomment);
        }

        [HttpDelete("delete/{comment_id}")]
        public async Task<IActionResult> Delete(int comment_id)
        {
            
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null) return Unauthorized("You are not logged in");

            
            var comment = await _db.Comments
                .Include(c => c.Post) 
                .FirstOrDefaultAsync(c => c.CommentId == comment_id);

            if (comment == null) return NotFound("Comment not found");

           
            bool isCommentOwner = comment.UserId == user_id.Value;
            bool isPostOwner = comment.Post.UserId == user_id.Value;

            if (!isCommentOwner && !isPostOwner)
            {
                return StatusCode(403, "You do not have permission to delete this comment");
            }

            
            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Comment deleted successfully" });
        }
    }
}