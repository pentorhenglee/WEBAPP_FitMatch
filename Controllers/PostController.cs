using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WEBAPP_FitMatch.Models;
using WEBAPP_FitMatch.Filters;

namespace WEBAPP_FitMatch.Controllers;

public class PostController : Controller
{   
    [SessionCheck]
    [HttpGet("Post/Detail")]
    public IActionResult Detail(int id)
    {
        ViewBag.PostId = id;
        return View();
    }

    [SessionCheck]
    public IActionResult Create()
    {
        return View();
    }
}