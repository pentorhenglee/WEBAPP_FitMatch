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
public class MissionAPIController : ControllerBase
{
    private readonly AppDbContext _db;
    public MissionAPIController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("addmission")]
    public async Task<IActionResult> Addmission([FromBody] Mission req)
    {
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null) 
            return Unauthorized("กรุณาเข้าสู่ระบบ");
        var mission = new Mission
        {
            UserId = user_id.Value,
            Description = req.Description,
            IsCompleted = false
        };
        _db.Missions.Add(mission);
        await _db.SaveChangesAsync();

        return Ok(new { message = "สร้าง Mission สำเร็จ", id = mission.MissionId });
    }

    [HttpGet("mission")]
    public async Task<IActionResult> GetMissions()
    {
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null) 
            return Unauthorized("กรุณาเข้าสู่ระบบ");
        
        var missions = await _db.Missions.Where(p => p.UserId == user_id.Value).ToListAsync();
        return Ok(missions);
    }

    [HttpPost("tick")]
    public async Task<IActionResult> Tick([FromBody] Tickmission req)
    { 
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null)
        {
            return Unauthorized("กรุณาเข้าสู่ระบบ");
        }

        var mission = await _db.Missions.FirstOrDefaultAsync(p => p.MissionId == req.MissionId);
        if (mission == null)
        {
            return NotFound("ไม่พบ Mission หรือคุณไม่มีสิทธิ์แก้ไข");
        }
        mission.IsCompleted = !mission.IsCompleted;
        await _db.SaveChangesAsync();
        return Ok(new { message = "อัปเดตสถานะสำเร็จ", isCompleted = mission.IsCompleted });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMission(int id)
    {
        var user_id = HttpContext.Session.GetInt32("user_id");
        if (user_id == null)
            return Unauthorized("กรุณาเข้าสู่ระบบ");

        var mission = await _db.Missions.FirstOrDefaultAsync(p => p.MissionId == id && p.UserId == user_id.Value);
        if (mission == null)
            return NotFound("ไม่พบ Mission หรือคุณไม่มีสิทธิ์ลบ");

        _db.Missions.Remove(mission);
        await _db.SaveChangesAsync();
        return Ok(new { message = "ลบ Mission สำเร็จ" });
    }

}