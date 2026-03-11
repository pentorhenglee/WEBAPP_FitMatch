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

    // มีคนกด join โพสต์ แจ้งคนที่ join + owner
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
                Message   = $"You have joined the activity '{postTitle}'",
                IsRead    = false,
                CreatedAt = DateTime.UtcNow
            },
            new Notification
            {
                UserId    = ownerId,
                TriggerId = joinerId,
                PostId    = postId,
                Type      = "Join",
                Message   = $"Someone has joined your activity '{postTitle}'",
                IsRead    = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _db.Notifications.AddRange(notifications);
        await _db.SaveChangesAsync();
    }

    // owner เตะสมาชิก แจ้งคนที่โดนเตะ
    public async Task NotifyKicked(int postId, int kickedUserId, int ownerId, string postTitle)
    {
        _db.Notifications.Add(new Notification
        {
            UserId    = kickedUserId,
            TriggerId = ownerId,
            PostId    = postId,
            Type      = "Kick",
            Message   = $"You have been removed from the activity '{postTitle}'",
            IsRead    = false,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    // owner แก้ไขโพสต์ แจ้งทุกคนในโพสต์ (รวม owner)
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
            Message   = $"The activity '{postTitle}' has been updated. Check it out!",
            IsRead    = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        if (notifications.Any())
        {
            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();
        }
    }

    // owner ปิดโพสต์ แจ้งทุกคนในโพสต์ (รวม owner)
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
            Message   = $"Congratulations! The activity '{postTitle}' has been closed. See you there!",
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
