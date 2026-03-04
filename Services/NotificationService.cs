using Microsoft.EntityFrameworkCore;
using WEBAPP_FitMatch.Data;
using WEBAPP_FitMatch.Models;

namespace WEBAPP_FitMatch.Services;

public class NotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    // มีคนกด join โพสต์ → แจ้งคนที่ join + owner
    public async Task NotifyJoined(int postId, int joinerId, string postTitle, int ownerId)
    {
        var notifications = new List<Notification>
        {
            new Notification
            {
                UserId    = joinerId,
                TriggerId = joinerId,
                PostId    = postId,
                Type      = "Join",
                Message   = $"คุณได้เข้าร่วมกิจกรรม '{postTitle}'",
                IsRead    = false,
                CreatedAt = DateTime.UtcNow
            },
            new Notification
            {
                UserId    = ownerId,
                TriggerId = joinerId,
                PostId    = postId,
                Type      = "Join",
                Message   = $"มีคนสนใจเข้าร่วมกิจกรรม '{postTitle}'",
                IsRead    = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _db.Notifications.AddRange(notifications);
        await _db.SaveChangesAsync();
    }

    // owner เตะสมาชิก → แจ้งคนที่โดนเตะ
    public async Task NotifyKicked(int postId, int kickedUserId, int ownerId, string postTitle)
    {
        _db.Notifications.Add(new Notification
        {
            UserId    = kickedUserId,
            TriggerId = ownerId,
            PostId    = postId,
            Type      = "Kick",
            Message   = $"คุณถูกนำออกจากกิจกรรม '{postTitle}'",
            IsRead    = false,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    // owner แก้ไขโพสต์ → แจ้งทุกคนในโพสต์ (รวม owner)
    public async Task NotifyEdited(int postId, int editorId, string postTitle)
    {
        var memberIds = await _db.Members
            .Where(m => m.PostId == postId)
            .Select(m => m.UserId)
            .ToListAsync();

        var notifications = memberIds.Select(uid => new Notification
        {
            UserId    = uid,
            TriggerId = editorId,
            PostId    = postId,
            Type      = "Edit",
            Message   = $"กิจกรรม '{postTitle}' มีการแก้ไขข้อมูล ตรวจสอบเลย!",
            IsRead    = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        if (notifications.Any())
        {
            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();
        }
    }

    // owner ปิดโพสต์ → แจ้งทุกคนในโพสต์ (รวม owner)
    public async Task NotifyClosed(int postId, int closerId, string postTitle)
    {
        var memberIds = await _db.Members
            .Where(m => m.PostId == postId)
            .Select(m => m.UserId)
            .ToListAsync();

        var notifications = memberIds.Select(uid => new Notification
        {
            UserId    = uid,
            TriggerId = closerId,
            PostId    = postId,
            Type      = "Close",
            Message   = $"กิจกรรม '{postTitle}' ปิดโพสต์แล้ว",
            IsRead    = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        if (notifications.Any())
        {
            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();
        }
    }
}
