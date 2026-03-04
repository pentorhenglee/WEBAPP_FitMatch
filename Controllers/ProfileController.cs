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
        return View();
    }

    [SessionCheck]
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
