using System.Security.Claims;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    public class AdminController : Controller
    {
        private readonly MarketplaceDbContext _db;
        private const string DemoUser = "admin";
        private const string DemoPass = "admin123";

        public AdminController(MarketplaceDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["DemoUser"] = DemoUser;
            ViewData["DemoPass"] = DemoPass;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (username == DemoUser && password == DemoPass)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, DemoUser),
                    new Claim(ClaimTypes.Role, "Admin"),
                };
                var identity = new ClaimsIdentity(claims, "AdminAuth");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("AdminAuth", principal, new AuthenticationProperties
                {
                    IsPersistent = true
                });
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.AddModelError(string.Empty, "Invalid credentials");
            ViewData["DemoUser"] = DemoUser;
            ViewData["DemoPass"] = DemoPass;
            return View();
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        public async Task<IActionResult> Dashboard()
        {
            var requests = await _db.PurchaseRequests.Include(r => r.Book).OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View(requests);
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminAuth");
            return RedirectToAction(nameof(Login));
        }
    }
}

