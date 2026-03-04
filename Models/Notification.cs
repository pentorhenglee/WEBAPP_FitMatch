using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEBAPP_FitMatch.Models;

[Table("Notification")]
public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int NotificationId {get;set;} //
    public int UserId {get;set;} //แจ้งเตือนใคร
    public int TriggerId {get;set;} //ใครเป็นคนทำ
    public int PostId {get;set;} //เกี่ยวข้องกับ post ไหน
    public required string Type {get;set;}
    public string Message {get;set;}
    public bool IsRead {get;set;}
    public DateTime CreatedAt { get; set;}
}