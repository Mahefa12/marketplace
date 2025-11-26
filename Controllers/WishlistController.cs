using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Marketplace.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly MarketplaceDbContext _db;

        public WishlistController(MarketplaceDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var exists = await _db.WishlistItems.AnyAsync(w => w.BuyerId == userId && w.BookId == bookId);
            if (!exists)
            {
                _db.WishlistItems.Add(new WishlistItem { BuyerId = userId, BookId = bookId });
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Added to wishlist";
            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var item = await _db.WishlistItems.FirstOrDefaultAsync(w => w.BuyerId == userId && w.BookId == bookId);
            if (item != null)
            {
                _db.WishlistItems.Remove(item);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Removed from wishlist";
            return RedirectToAction("Details", "Books", new { id = bookId });
        }
    }
}
