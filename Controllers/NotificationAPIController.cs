using Microsoft.AspNetCore.Mvc;
using WEBAPP_FitMatch.Data;
using WEBAPP_FitMatch.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WEBAPP_FitMatch.model;


namespace WEBAPP_FitMatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationAPIController : ControllerBase
{
    private readonly AppDbContext _db ;
    public NotificationAPIController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Joined([FromBody] NotificationRequest req)
    {
        //แจ้งคนกด join
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null) 
            return Unauthorized("กรุณาเข้าสู่ระบบ");

        var ownerPost = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == req.PostId);
        if (ownerPost == null)
            return NotFound("ไม่พบโพสต์ที่ระบุ");

        var ownerId = ownerPost.UserId;

        var noti_joined = new Notification
        {
            UserId = req.UserId,         
            TriggerId = user_id.Value,  
            PostId = req.PostId,         
            Type = "Join",              
            Message = "You have been joined Post", 
            IsRead = false,              
            EventDateTime = DateTime.Now 
        };

        var noti_owner = new Notification
        {
            UserId = ownerId,         
            TriggerId = user_id.Value,  
            PostId = req.PostId,         
            Type = "Join",              
            Message = "มีคนสนใจเข้าร่วมกิจกรรมของคุณ", 
            IsRead = false,              
            EventDateTime = DateTime.Now 
        };
        try 
        {
            // 5. บันทึกทั้งสองรายการลงฐานข้อมูล
            _db.Notifications.Add(noti_joined);
            _db.Notifications.Add(noti_owner);
            await _db.SaveChangesAsync();
            
            return Ok(new { success = true, message = "บันทึกแจ้งเตือนเรียบร้อย" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "เกิดข้อผิดพลาด: " + ex.Message);
        }
    }

    public async Task<IActionResult> Kicked([FromBody] NotificationRequest req)
    {
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null)
            return Unauthorized("กรุณาเข้าสู่ระบบ");

        var ownerPost = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == req.PostId);
        if (ownerPost == null)
            return NotFound("ไม่พบโพสต์ที่ระบุ");

        var noti_kicked = new Notification
        {
            UserId = req.UserId,         
            TriggerId = user_id.Value,  
            PostId = req.PostId,         
            Type = "Kick",              
            Message = "You have been Kicked from the Post", 
            IsRead = false,              
            EventDateTime = DateTime.Now 
        };
        try 
        {
            // 5. บันทึกทั้งสองรายการลงฐานข้อมูล
            _db.Notifications.Add(noti_kicked);
            await _db.SaveChangesAsync();
            
            return Ok(new { success = true, message = "บันทึกแจ้งเตือนเรียบร้อย" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "เกิดข้อผิดพลาด: " + ex.Message);
        }
    }   

    public async Task<IActionResult> Edited([FromBody] NotificationRequest req)
    {
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null)
            return Unauthorized("กรุณาเข้าสู่ระบบ");

        var ownerPost = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == req.PostId);
        if (ownerPost == null)
            return NotFound("ไม่พบโพสต์ที่ระบุ");
        
        var members_in_post = await _db.Members
            .Where(p => p.PostId == req.PostId && p.UserId != user_id.Value)
            .Select(p => p.UserId).ToArrayAsync();
        
        var notifications = new List<Notification>();

        foreach (var targetUserId in members_in_post)
        {
            var noti_edited = new Notification
            {
                UserId = targetUserId,         // ผู้รับคือ member แต่ละคน
                TriggerId = user_id.Value,     // ผู้กระทำคือคนที่กดแก้โพสต์
                PostId = req.PostId,
                Type = "Edit",
                Message = $"กิจกรรม '{ownerPost.Title}' มีการแก้ไขข้อมูล ตรวจสอบเลย!", 
                IsRead = false,
                EventDateTime = DateTime.Now
            };
            
            notifications.Add(noti_edited);
        }

        try 
        {
            if (notifications.Any())
            {
                // ใช้ AddRange เพื่อแอดทั้ง List ลงไปทีเดียว (ประสิทธิภาพดีกว่าแอดทีละตัวใน loop)
                _db.Notifications.AddRange(notifications);
                await _db.SaveChangesAsync();
            }
            
            return Ok(new { success = true, count = notifications.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "เกิดข้อผิดพลาด: " + ex.Message);
        }
    }

    public async Task<IActionResult> Closed([FromBody] NotificationRequest req)
    {
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null)
            return Unauthorized("กรุณาเข้าสู่ระบบ");

        var ownerPost = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == req.PostId);
        if (ownerPost == null)
            return NotFound("ไม่พบโพสต์ที่ระบุ");

        var members_in_post = await _db.Members
            .Where(p => p.PostId == req.PostId)
            .Select(p => p.UserId).ToArrayAsync();
        
        var notifications = new List<Notification>();

        foreach (var targetUserId in members_in_post)
        {
            var noti_edited = new Notification
            {
                UserId = targetUserId,         // ผู้รับคือ member แต่ละคน
                TriggerId = user_id.Value,     // ผู้กระทำคือคนที่กดแก้โพสต์
                PostId = req.PostId,
                Type = "Close",
                Message = $"กิจกรรม '{ownerPost.Title}' ปิดโพสต์แล้ว", 
                IsRead = false,
                EventDateTime = DateTime.Now
            };
            
            notifications.Add(noti_edited);
        }
        
        try 
        {
            if (notifications.Any())
            {
                // ใช้ AddRange เพื่อแอดทั้ง List ลงไปทีเดียว (ประสิทธิภาพดีกว่าแอดทีละตัวใน loop)
                _db.Notifications.AddRange(notifications);
                await _db.SaveChangesAsync();
            }
            
            return Ok(new { success = true, count = notifications.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "เกิดข้อผิดพลาด: " + ex.Message);
        }
    }
    public async Task<IActionResult> ReadNotification([FromBody] ReadNotificationRequest req)
    {
        
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null)
            return Unauthorized("กรุณาเข้าสู่ระบบ");

        var notification = await _db.Notifications.FirstOrDefaultAsync(p => p.NotificationId == req.NotificationId);
        if (notification == null)
            return NotFound("ไม่พบแจ้งที่ระบุ");

        notification.IsRead = !notification.IsRead;

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> RenderNotification()
    {
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null)
            return Unauthorized("กรุณาเข้าสู่ระบบ");
        
        var notifications = await _db.Notifications
            .Where(p => p.UserId == user_id.Value)
            .OrderByDescending(p => p.EventDateTime) // เรียงตามเวลา ล่าสุดขึ้นก่อน
            .Select(p => new {
                p.NotificationId,
                p.TriggerId,
                p.PostId,
                p.Type,
                p.Message,
                p.IsRead,
                // ฟอร์แมตวันที่ให้ JS อ่านง่าย
                Date = p.EventDateTime.ToString("dd/MM/yyyy HH:mm"),
                // ถ้าอยากได้ชื่อคนที่มาทำ Action (Trigger) สามารถ Join เพิ่มได้ที่นี่
                TriggerName = _db.Users
                    .Where(u => u.Id == p.TriggerId)
                    .Select(u => u.Username)
                    .FirstOrDefault()
            })
            .ToListAsync(); // หรือ ToArrayAsync()

        return Ok(notifications);
    }
}