namespace WEBAPP_FitMatch.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[PrimaryKey(nameof(PostId), nameof(UserId))]
[Table("Member")]
public class PostUser{
    public int PostId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
