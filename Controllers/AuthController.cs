using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPP_FitMatch;
using WEBAPP_FitMatch.Data;
using WEBAPP_FitMatch.Models;

namespace WEBAPP_FitMatch.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == req.Email);
        
         if (user == null)
            return Unauthorized(new { message = "User not found" });

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Wrong password" });

        HttpContext.Session.SetString("user", user.Username);
        HttpContext.Session.SetString("user_email", user.Email);
        HttpContext.Session.SetString("user_profile", user.ProfileUrl ?? "");
        HttpContext.Session.SetInt32("user_id", user.Id);
        return Ok(new { message = "Login success" });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new
        {
           success = true,
           message = "Logged out"
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        bool isUserExist = await _db.Users.AnyAsync(u =>
            u.Username == req.Username || u.Email == req.Email);
        
        if (isUserExist)
            return BadRequest("Username or Email already used");
        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Register success" });

       
    }
   
    
}
