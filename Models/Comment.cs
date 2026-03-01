using System.ComponentModel.DataAnnotations.Schema;

namespace WEBAPP_FitMatch.Models
{
    [Table("Comment")]
    public class Comment
    {
        public int CommentId {get;set;}
        public int UserId {get;set;}
        public int PostId {get;set;}
        public DateTime CreatedAt {get;set;}
        public string? Text {get;set;}
    }
}