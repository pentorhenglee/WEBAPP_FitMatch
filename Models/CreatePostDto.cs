using Microsoft.VisualBasic;

namespace WEBAPP_FitMatch.Models
{
    public class CreatePostDto
    {
        public Int32 UserId {get;set;}
        public string? Title {get;set;}
        public string? Location {get;set;}
        public DateTime EventDateTime {get;set;}
        
        public string? Description {get;set;}
        public string? SportType {get;set;}
        public Int32 MaxPeople {get;set;}
        public string? ImageUrl{get;set;}
        public double? Lat { get; set; }
        public double? Lon { get; set; }

    }
}