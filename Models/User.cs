namespace WEBAPP_FitMatch.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("User")]
public class User
{
    [Column("UserId")]
    public int Id { get; set;}

    [Column("Username")]   
    public required string Username { get; set;}
    [Column("Password_hash")]
    public required string PasswordHash { get; set;}
    [Column("Email")]
    public required string Email { get; set;}
}