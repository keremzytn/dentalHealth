using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DentalHealthTracker.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using DentalHealthTracker.Data;

namespace DentalHealthTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Home/Index çağrıldı. Kullanıcı: {User}", User.Identity?.Name);
        return View();
    }

    public IActionResult Privacy()
    {
        _logger.LogInformation("Home/Privacy çağrıldı. Kullanıcı: {User}", User.Identity?.Name);
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        _logger.LogError("Home/Error çağrıldı. Kullanıcı: {User}", User.Identity?.Name);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult GetWeeklySummary()
    {
        _logger.LogInformation("Home/GetWeeklySummary çağrıldı. Kullanıcı: {User}", User.Identity?.Name);
        return PartialView("_WeeklySummary");
    }

    public IActionResult GetDailyRecommendation()
    {
        var random = new Random();
        var recommendation = _context.Recommendations
            .OrderBy(r => Guid.NewGuid())
            .FirstOrDefault();

        return PartialView("_DailyRecommendation", recommendation);
    }
}
