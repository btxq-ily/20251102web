using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KnowledgeStack.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeStack.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var context = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        ViewBag.TotalPosts = await context.Posts.CountAsync();
        ViewBag.TotalUsers = await context.Users.CountAsync();
        ViewBag.RecentPosts = await context.Posts
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
