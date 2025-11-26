using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    public class SellersController : Controller
    {
        private readonly MarketplaceDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public SellersController(MarketplaceDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Profile(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Check if user is actually a seller? 
            // For now, anyone can have a profile, or we check role.
            // if (!await _userManager.IsInRoleAsync(user, "Seller")) return NotFound();

            var avg = await _db.SellerRatings.Where(r => r.SellerId == id).Select(r => r.Stars).DefaultIfEmpty(0).AverageAsync();
            var books = await _db.Books.Where(b => b.SellerId == id).OrderByDescending(b => b.CreatedAt).ToListAsync();

            ViewData["Avg"] = avg;
            ViewData["Books"] = books;

            return View(user);
        }
    }
}
