namespace WEBAPP_FitMatch.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


[Table("Mission")]
public class Mission
{
    [Key]
    [Column("MissionId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MissionId { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }

    [Column("Description")]
    public string Description { get; set; } = "";

    [Column("IsCompleted")]
    public bool IsCompleted { get; set; } = false;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}