using System.Net;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
namespace WEBAPP_FitMatch.Controllers;

using System.Data.SqlTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using WEBAPP_FitMatch.Models;
using WEBAPP_FitMatch.Data;

[ApiController]
[Route("api/profileapi")]
public class ProfileAPIController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProfileAPIController(AppDbContext db)
    {
        _db = db;
    }
    [HttpGet("user/{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound(new { message = "User not found" });
        return Ok(new {
            id = user.Id,
            username = user.Username,
            profileUrl = user.ProfileUrl,
            info = user.Info,
            email = user.Email
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUser()
    {
        var user_id = HttpContext.Session.GetInt32("user_id");
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == user_id);
        if (user == null)
        {
            return NotFound(new { message = "User not found or not logged in" });
        }
        return Ok(user);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfile req)
    {   
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null) return Unauthorized("กรุณาเข้าสู่ระบบ");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == user_id);
        if (user == null) return NotFound("ไม่พบข้อมูลผู้ใช้");

        if (string.IsNullOrWhiteSpace(req.username)) 
            return BadRequest("ชื่อผู้ใช้ห้ามว่าง");

        user.Username = req.username ?? user.Username;
        user.ProfileUrl = req.profileUrl;
        user.Info = req.info ?? "";
        try 
        {
            await _db.SaveChangesAsync();
            HttpContext.Session.SetString("user_profile", user.ProfileUrl ?? "");
            return Ok(new { message = "อัปเดตสำเร็จ" });
            
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, "เกิดข้อผิดพลาดในการบันทึกข้อมูล");
        }
    }

}