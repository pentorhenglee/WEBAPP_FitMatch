namespace WEBAPP_FitMatch.Models;

public class UpdateProfile
{
    public required string username { get; set; }
    public string? info { get; set; }
    public required string profileUrl { get; set; }
}