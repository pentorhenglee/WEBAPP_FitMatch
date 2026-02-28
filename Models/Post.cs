namespace WEBAPP_FitMatch.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
[Table("Post")]
public class Post{
    [Key]
    [Column("PostId")]
    public int PostId { get; set;}
    [Column("UserId")]
    public int UserId { get; set;}
    public required string Title { get; set;}
    public string? Location { get; set;}
    public DateTime DateTime { get; set;}
    
    public required string Description { get; set;}
    public required string SportType { get; set;}
    public DateTime CreateDate { get; set;}
    public int MaxPeople { get; set;}

    public ICollection<PostUser> Members { get; set;} = new List<PostUser>();
}
