using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Marketplace.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    public class WishlistController : Controller
    {
        private readonly MarketplaceDbContext _db;
        private readonly IPurchaseService _purchase;
        public WishlistController(MarketplaceDbContext db, IPurchaseService purchase)
        {
            _db = db;
            _purchase = purchase;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int bookId, ContactInfo contact)
        {
            var buyer = await _purchase.EnsureBuyerAsync(contact);
            var exists = await _db.WishlistItems.AnyAsync(w => w.BuyerId == buyer.Id && w.BookId == bookId);
            if (!exists)
            {
                _db.WishlistItems.Add(new WishlistItem { BuyerId = buyer.Id, BookId = bookId });
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Added to wishlist";
            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int bookId, ContactInfo contact)
        {
            var buyer = await _purchase.EnsureBuyerAsync(contact);
            var item = await _db.WishlistItems.FirstOrDefaultAsync(w => w.BuyerId == buyer.Id && w.BookId == bookId);
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

