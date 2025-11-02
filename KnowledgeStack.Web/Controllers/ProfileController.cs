using KnowledgeStack.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KnowledgeStack.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Posts.OrderByDescending(p => p.CreatedAt).Take(10))
                .ThenInclude(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(u => u.Id == uid);

            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.TotalPosts = await _context.Posts.CountAsync(p => p.UserId == uid);
            ViewBag.TotalComments = await _context.Comments.CountAsync(c => c.UserId == uid);
            ViewBag.TotalLikes = await _context.PostLikes.CountAsync(l => l.UserId == uid);

            return View(user);
        }
    }
}

