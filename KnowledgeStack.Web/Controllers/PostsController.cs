using KnowledgeStack.Web.Models;
using Markdig;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KnowledgeStack.Web.Controllers
{
    public class PostsController : Controller
    {
        private readonly AppDbContext _context;

        public PostsController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<int?> GetCurrentUserIdOrSignOut()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var uid)) return null;
            var exists = await _context.Users.AnyAsync(u => u.Id == uid);
            if (!exists)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return null;
            }
            return uid;
        }

        // GET: /Posts?page=1&keyword=abc&tag=2
        public async Task<IActionResult> Index(int page = 1, string? keyword = null, int? tag = null)
        {
            const int pageSize = 5;
            var query = _context.Posts.Include(p => p.User).Include(p => p.PostTags).ThenInclude(pt => pt.Tag).OrderByDescending(p => p.CreatedAt).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.Title.Contains(keyword) || p.Content.Contains(keyword));
            }
            if (tag.HasValue)
            {
                query = query.Where(p => p.PostTags.Any(t => t.TagId == tag.Value));
            }
            var allPosts = await query.ToListAsync();
            var total = allPosts.Count;
            var posts = allPosts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.Tags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
            return View(posts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts.Include(p => p.User)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound();
            ViewBag.Html = Markdown.ToHtml(post.Content);
            ViewBag.Likes = await _context.PostLikes.CountAsync(l => l.PostId == id);
            return View(post);
        }

        [Authorize]
        public IActionResult Create()
        {
            ViewBag.AllTags = _context.Tags.OrderBy(t => t.Name).ToList();
            return View();
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> Create(string title, string content, int[]? tags)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError(string.Empty, "标题与内容必填");
                ViewBag.AllTags = _context.Tags.OrderBy(t => t.Name).ToList();
                return View();
            }
            var uid = await GetCurrentUserIdOrSignOut();
            if (uid == null) return RedirectToAction("Login", "Account");
            var post = new Post { Title = title, Content = content, UserId = uid.Value };
            if (tags != null)
            {
                foreach (var t in tags.Distinct()) post.PostTags.Add(new PostTag { TagId = t });
            }
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = post.Id });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var uid = await GetCurrentUserIdOrSignOut();
            if (uid == null) return RedirectToAction("Login", "Account");
            var post = await _context.Posts.Include(p => p.PostTags).FirstOrDefaultAsync(p => p.Id == id && p.UserId == uid.Value);
            if (post == null) return NotFound();
            ViewBag.AllTags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
            return View(post);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> Edit(int id, string title, string content, int[]? tags)
        {
            var uid = await GetCurrentUserIdOrSignOut();
            if (uid == null) return RedirectToAction("Login", "Account");
            var post = await _context.Posts.Include(p => p.PostTags).FirstOrDefaultAsync(p => p.Id == id && p.UserId == uid.Value);
            if (post == null) return NotFound();
            post.Title = title; post.Content = content; post.UpdatedAt = DateTime.UtcNow;
            post.PostTags.Clear();
            if (tags != null) foreach (var t in tags.Distinct()) post.PostTags.Add(new PostTag { PostId = id, TagId = t });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var uid = await GetCurrentUserIdOrSignOut();
            if (uid == null) return RedirectToAction("Login", "Account");
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == uid.Value);
            if (post == null) return NotFound();
            return View(post);
        }

        [Authorize, HttpPost, ActionName("Delete")] 
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uid = await GetCurrentUserIdOrSignOut();
            if (uid == null) return RedirectToAction("Login", "Account");
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == uid.Value);
            if (post == null) return NotFound();
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            var uid = await GetCurrentUserIdOrSignOut();
            if (uid == null) return RedirectToAction("Login", "Account");
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();
            await _context.Comments.AddAsync(new Comment { PostId = postId, Content = content, UserId = uid.Value });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = postId });
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var uid = await GetCurrentUserIdOrSignOut();
            if (uid == null) return Unauthorized();
            var exists = await _context.PostLikes.FindAsync(id, uid.Value);
            if (exists == null)
            {
                await _context.PostLikes.AddAsync(new PostLike { PostId = id, UserId = uid.Value });
                await _context.SaveChangesAsync();
            }
            var count = await _context.PostLikes.CountAsync(l => l.PostId == id);
            return Json(new { ok = true, likes = count });
        }
    }
}



