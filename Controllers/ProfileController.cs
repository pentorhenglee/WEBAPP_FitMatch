using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WEBAPP_FitMatch.Models;
using WEBAPP_FitMatch.Filters;

namespace WEBAPP_FitMatch.Controllers;

public class ProfileController : Controller
{
    [SessionCheck]
    public IActionResult Index()
    {
        ViewData["UserName"] = "Earthy"; // ส่งชื่อไป
        ViewData["UserEmail"] = "suphasin@kmitl.ac.th"; // ส่ง Email ไป
        ViewData["UserProfilePic"] = "~/images/profile.jpg"; // ส่ง URL รูปโปรไฟล์ไป
        ViewData["UserBio"] = "I'm a passionate fitness enthusiast who loves to stay active and healthy."; // ส่ง Bio ไป
        return View();
    }

    // [HttpPost("update-profile")]
    // public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto model)
    // {

    //     return Ok();
    // }


    public IActionResult Missions()
    {
        return View();
    }
    public IActionResult About()
    {
        return View();
    }   

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
