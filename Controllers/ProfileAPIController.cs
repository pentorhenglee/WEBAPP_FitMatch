using System.Net;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
namespace WEBAPP_FitMatch.Controllers;

using System.Data.SqlTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using WEBAPP_FitMatch.Models;

[ApiController]
[Route("api/[controller]")]
public class ProfileapiController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProfileapiController(AppDbContext db)
    {
        _db = db;
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

}