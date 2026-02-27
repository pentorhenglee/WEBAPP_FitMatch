namespace MyWeb.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
[Table("Post")]
public class Post{
    [Key]
    [Column("PostId")]
    public int PostId { get; set;}
    [Column("UserId")]
    public int UserId { get; set;}
    public string Title { get; set;}
    public string? Location { get; set;}
    public DateOnly Date { get; set;}
    public TimeOnly Time { get; set;}
    public string Description { get; set;}
    public string SportType { get; set;}
    public DateTime CreateDate { get; set;}
    public int MaxPeople { get; set;}

    public ICollection<PostUser> Members { get; set;}
}
