using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeStack.Web.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IWebHostEnvironment env, ILogger<UploadController> logger)
        {
            _env = env;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Image(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "请选择文件" });
            }

            // 验证文件类型
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { error = "仅支持 JPG、PNG、GIF、WEBP 格式图片" });
            }

            // 验证文件大小（最大 5MB）
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { error = "图片大小不能超过 5MB" });
            }

            try
            {
                // 生成唯一文件名
                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                
                // 确保目录存在
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // 保存文件
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 返回可访问的 URL
                var url = $"/uploads/{fileName}";
                return Json(new { ok = true, url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "图片上传失败");
                return StatusCode(500, new { error = "上传失败，请稍后重试" });
            }
        }
    }
}
