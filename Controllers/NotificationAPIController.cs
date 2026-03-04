using Microsoft.AspNetCore.Mvc;
using WEBAPP_FitMatch.Data;
using WEBAPP_FitMatch.Models;
using WEBAPP_FitMatch.model;
using Microsoft.EntityFrameworkCore;

namespace WEBAPP_FitMatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationAPIController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotificationAPIController(AppDbContext db)
    {
        _db = db;
    }

    // ทำเครื่องหมายว่าอ่านแล้ว (toggle)
    [HttpPost("read")]
    public async Task<IActionResult> ReadNotification([FromBody] ReadNotificationRequest req)
    {
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null) return Unauthorized("กรุณาเข้าสู่ระบบ");

        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == req.NotificationId && n.UserId == user_id.Value);
        if (notification == null) return NotFound("ไม่พบการแจ้งเตือน");

        notification.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok();
    }

    // ดึง notifications ของ user ที่ล็อกอิน
    [HttpGet("render")]
    public async Task<IActionResult> RenderNotification()
    {
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null) return Unauthorized("กรุณาเข้าสู่ระบบ");

        var notifications = await _db.Notifications
            .Where(n => n.UserId == user_id.Value)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new
            {
                n.NotificationId,
                n.TriggerId,
                n.PostId,
                n.Type,
                n.Message,
                n.IsRead,
                Date = n.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                TriggerName = _db.Users
                    .Where(u => u.Id == n.TriggerId)
                    .Select(u => u.Username)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(notifications);
    }
}