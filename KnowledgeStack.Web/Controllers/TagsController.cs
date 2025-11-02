using KnowledgeStack.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeStack.Web.Controllers
{
    [Authorize]
    public class TagsController : Controller
    {
        private readonly AppDbContext _context;
        public TagsController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Tags.OrderBy(t => t.Name).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return RedirectToAction(nameof(Index));
            if (!await _context.Tags.AnyAsync(t => t.Name == name))
            {
                await _context.Tags.AddAsync(new Tag { Name = name });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _context.Tags.Include(t => t.PostTags).FirstOrDefaultAsync(t => t.Id == id);
            if (tag != null && !tag.PostTags.Any())
            {
                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}



