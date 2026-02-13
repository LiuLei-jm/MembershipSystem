using Microsoft.AspNetCore.Mvc;

namespace MembershipSystemWeb.Controllers
{
    [ApiController]
    [Route("/[controller]/[action]/{*path}")]
    public class SpaFallbackController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            // 返回index.html以支持SPA路由
            var indexPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");

            if (System.IO.File.Exists(indexPath))
            {
                var content = System.IO.File.ReadAllText(indexPath);
                return Content(content, "text/html; charset=utf-8");
            }

            return NotFound();
        }
    }
}