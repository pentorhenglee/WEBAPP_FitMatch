namespace WEBAPP_FitMatch.model;

public class NotificationRequest
{
    public int UserId {get;set;} //แจ้งเตือนใคร
    public int TriggerId {get;set;} //ใครเป็นคนทำ
    public int PostId {get;set;} //เกี่ยวข้องกับ post ไหน
    public required string Type {get;set;}

}