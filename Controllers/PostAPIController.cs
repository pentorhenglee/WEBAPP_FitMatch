using Microsoft.AspNetCore.Mvc;
using WEBAPP_FitMatch.Data;
using WEBAPP_FitMatch.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.AccessControl;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Formats.Asn1;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using Microsoft.Extensions.FileProviders;

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
                .Select(p => new
                {
                    p.PostId,
                    p.UserId,

                    Owner = _db.Users.Where(u => u.Id == p.UserId).Select(u => u.Username).FirstOrDefault(),
                    p.Title,
                    p.Location,
                    p.EventDateTime,
                    p.Description,
                    p.SportType,
                    p.CreateDate,
                    p.MaxPeople,
                    p.ImageUrl,
                    Status = (p.Status == "open" && p.EventDateTime <= DateTime.UtcNow) ? "close" : p.Status,


                    Members = p.Members.Join(_db.Users,
                        m => m.UserId,
                        u => u.Id,
                        (m, u) => new
                        {
                            m.UserId,
                            name = u.Username,
                            Join = m.JoinedAt,
                            status = m.Status
                        }).ToList(),


                    Comments = p.Comments.Join(_db.Users,
                        c => c.UserId,
                        u => u.Id,
                        (c, u) => new
                        {
                            c.CommentId,
                            c.UserId,
                            username = u.Username,
                            profileUrl = u.ProfileUrl,
                            Comment_date = c.CreatedAt,
                            c.Text
                        }).ToList()
                })
                .ToArrayAsync();

            return Ok(posts);
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null) return Unauthorized("User not logged in ");

            var post = new Post
            {
                Title = dto.Title,
                CreateDate = DateTime.UtcNow,
                EventDateTime = DateTime.SpecifyKind(dto.EventDateTime, DateTimeKind.Utc),
                Description = dto.Description ?? "",
                SportType = dto.SportType ?? "",
                MaxPeople = dto.MaxPeople,
                UserId = user_id.Value,
                Location = dto.Location,
                ImageUrl = dto.ImageUrl,
                Status = "open"
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            var member = new Member
            {
                PostId = post.PostId,
                UserId = user_id.Value,
                Status = "owner",
                JoinedAt = DateTime.UtcNow
            };

            _db.Members.Add(member);
            await _db.SaveChangesAsync();

            var histories = new History
            {
                UserId = user_id.Value,
                PostId = post.PostId,
                ActionType = $"Create Post {post.Title}"
            };
            _db.Histories.Add(histories);
            await _db.SaveChangesAsync();
            
            return Ok(post);
        }

        [HttpPut]
        [Route("/api/postapi/close/{postid}")]
        public async Task<ActionResult> ClosePost(int postid)
        {
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null)
                return Unauthorized("User not logged in");

            var post = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == postid);

            if (post == null)
                return NotFound("Post not found");

            if (post.UserId != user_id.Value)
                return Forbid("You are not the owner of this post");


            post.Status = "close";

            try
            {
                await _db.SaveChangesAsync();
                var histories = new History
                {
                    UserId = user_id.Value,
                    PostId = post.PostId,
                    ActionType = $"Close Post {post.Title}"
                };
                _db.Histories.Add(histories);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Close Post successfully",
                    postId = post.PostId,
                    status = post.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Close Post failed: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("/api/postapi/open/{postid}")]
        public async Task<ActionResult> OpenPost(int postid)
        {
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null)
                return Unauthorized("User not logged in");

            var post = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == postid);

            if (post == null)
                return NotFound("Post not found");

            if (post.UserId != user_id.Value)
                return Forbid("You are not the owner of this post");


            post.Status = "open";

            try
            {
                await _db.SaveChangesAsync();
                var histories = new History
                {
                    UserId = user_id.Value,
                    PostId = post.PostId,
                    ActionType = $"Open Post {post.Title}"
                };
                _db.Histories.Add(histories);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Open Post successfully",
                    postId = post.PostId,
                    status = post.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Open Post failed: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("/api/all_post")]
        public async Task<ActionResult> GetAllPost(string? query, string? category)
        {                 
            try
            {               
                var post_filter = _db.Posts
                    .Where(p => p.Status == "open")
                    .AsQueryable();

                if (!string.IsNullOrEmpty(category))
                {
                    post_filter = post_filter.Where(p => p.SportType == category);
                }
                if (!string.IsNullOrEmpty(query))
                {
                    post_filter = post_filter.Where(p => 
                        (p.Title != null && p.Title.Contains(query)) || 
                        (p.Location != null && p.Location.Contains(query)) || 
                        (p.Description != null && p.Description.Contains(query)));
                }

                var posts = await post_filter
                    .OrderByDescending(p => p.EventDateTime)
                    .Select(p => new
                    {
                        p.PostId,
                        p.UserId,

                        Owner = _db.Users.Where(u => u.Id == p.UserId).Select(u => u.Username).FirstOrDefault(),
                        p.Title,
                        p.Location,
                        p.EventDateTime,
                        p.Description,
                        p.SportType,
                        p.CreateDate,
                        p.MaxPeople,
                        p.ImageUrl,
                        p.Status,

                        

                        Members = p.Members.Join(_db.Users,
                            m => m.UserId,
                            u => u.Id,
                            (m, u) => new
                            {
                                m.UserId,
                                name = u.Username,
                                Status = m.Status
                            }).ToList(),


                        Comments = p.Comments.Join(_db.Users,
                            c => c.UserId,
                            u => u.Id,
                            (c, u) => new
                            {
                                c.CommentId,
                                c.UserId,
                                username = u.Username,
                                profileUrl = u.ProfileUrl,
                                Comment_date = c.CreatedAt,
                                c.Text
                            }).ToList()
                    })
                    .ToArrayAsync();

                return Ok(posts);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult> GetPostDetail(int id)
        {
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null) return Unauthorized("User not logged in");

            var post = await _db.Posts
                .Where(p => p.PostId == id)
                .Select(p => new 
                {
                    p.PostId,
                    ownerId = p.UserId, // ส่ง ownerId กลับไปด้วย เผื่อ JS เอาไปเช็คสิทธิ์
                    currentUserId = user_id.Value, // ส่ง id คนที่ล็อกอินอยู่ไปด้วย
                    Owner = _db.Users.Where(u => u.Id == p.UserId).Select(u => u.Username).FirstOrDefault(),
                    p.Title,
                    p.Location,
                    p.EventDateTime,
                    p.Description,
                    p.SportType,
                    p.CreateDate,
                    p.MaxPeople,
                    p.ImageUrl,
                    p.Status,
                    
                    // ดึง Members มาด้วย จะได้เอาไป .length นับจำนวนคน และโชว์รายชื่อได้
                    Members = p.Members.Join(_db.Users, 
                        m => m.UserId, 
                        u => u.Id, 
                        (m, u) => new {
                            userId = m.UserId,
                            name = u.Username,
                            join = m.JoinedAt,
                            status = m.Status,
                            profileUrl = u.ProfileUrl
                        }).ToList(),

                    // ดึง Comments มาเผื่อทำระบบ Chat
                    Comments = p.Comments.Join(_db.Users,
                        c => c.UserId,
                        u => u.Id,
                        (c, u) => new {
                            c.CommentId,
                            c.UserId,
                            username = u.Username,
                            profileUrl = u.ProfileUrl,
                            comment_date = c.CreatedAt,
                            c.Text
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (post == null)
                return NotFound("Post not found");

            return Ok(post);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null)
                return Unauthorized("User not logged in");

            var post = await _db.Posts
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
                return NotFound();

            if (post.UserId != user_id.Value)
                return Forbid();

            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();
            
            var histories = new History
            {
                UserId = user_id.Value,
                PostId = post.PostId,
                ActionType = $"Delete Post {post.Title}"
            };
            _db.Histories.Add(histories);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Delete successful" });
        }

    }
}