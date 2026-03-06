using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPP_FitMatch.Data;
using WEBAPP_FitMatch.Models;
using Npgsql;
using System.Runtime.Versioning;

namespace WEBAPP_FitMatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HistoryAPIController(AppDbContext db)
        {
            _db = db;
        }
        // ไม่ได้ใช้
        [HttpGet]
        public async Task<ActionResult> GetHistory()
        {
            var user_id = HttpContext.Session.GetInt32("user_id");
            if (user_id == null) return Unauthorized("User not logged in");

            var histories = await _db.Histories
                .Where(h => h.UserId == user_id.Value)
                .OrderByDescending(h=>h.HistoryId)
                .Select(h => new
                {
                    h.HistoryId,
                    h.UserId,
                    h.PostId,
                    h.ActionType
                })
                .ToListAsync();
            
            return Ok(histories);
        }
    }
}