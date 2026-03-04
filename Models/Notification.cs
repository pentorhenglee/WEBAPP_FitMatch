using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEBAPP_FitMatch.Models;

public class Notification
{
    public int NotificationId {get;set;}
    public int TriggerId {get;set;}
    public int PostId {get;set;}
    public required string Type {get;set;}
    public string Message {get;set;}
    public bool IsRead {get;set;}
    public DateTime EventDateTime { get; set;}
}