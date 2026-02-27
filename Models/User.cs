namespace MyWeb.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("User")]
public class User
{
    [Column("UserId")]
    public int Id { get; set;}

    [Column("Username")]   
    public string Username { get; set;}
    [Column("Password_hash")]
    public string PasswordHash { get; set;}
    [Column("Email")]
    public string Email { get; set;}
}