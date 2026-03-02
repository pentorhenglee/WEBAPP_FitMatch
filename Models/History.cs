using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace WEBAPP_FitMatch.Models
{
    [Table("History")]
    public class History
    {
        public int HistoryId {get;set;}
        public int UserId {get;set;}
        public int PostId {get;set;}
        public String? ActionType {get;set;}
    }
}