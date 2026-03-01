using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WEBAPP_FitMatch.Models;
using WEBAPP_FitMatch.Filters;

namespace WEBAPP_FitMatch.Controllers;

public class PostController : Controller
{   
    [SessionCheck]
    public IActionResult Detail(int id)
    {
        return View();
    }

    public IActionResult Create()
    {
        return View();
    }
}