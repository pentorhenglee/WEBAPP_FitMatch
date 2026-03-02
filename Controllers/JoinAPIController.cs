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
public class JoinAPIController : ControllerBase
{
    private readonly AppDbContext _db;
    public JoinAPIController(AppDbContext db)
    {
        _db = db ;
    }
    
    [HttpPost("join")]
    public async Task<IActionResult> Joinasfuck([FromBody] JoinRequest req)
    {
        return BadRequest("Not implemented yet");
    }
}