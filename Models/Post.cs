using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEBAPP_FitMatch.Models;

[Table("Post")]
public class Post
{
    [Key]
    [Column("PostId")]
    public int PostId { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }             
    
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [Required]
    public string? Title { get; set; }

    public string? Location { get; set; }

    public DateTime? EventDateTime { get; set; }

    [Required]
    public string? Description { get; set; }

    [Required]
    public string? SportType { get; set; }

    public DateTime CreateDate { get; set; }

    public int MaxPeople { get; set; }

    public string? ImageUrl { get; set; }   
    public string? Status {get;set;}    

    public List<Member> Members {get;set;} = new List<Member>();
    public List<Comment>? Comments { get; set; } = new List<Comment>();
}