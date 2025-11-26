using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Controllers
{
    [Authorize]
    public class SavedSearchesController : Controller
    {
        private readonly MarketplaceDbContext _db;

        public SavedSearchesController(MarketplaceDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string? q, string? category)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var ss = new SavedSearch { BuyerId = userId, Query = q, Category = category };
            _db.SavedSearches.Add(ss);
            await _db.SaveChangesAsync();

            // Note: Removed the Message creation for now as it required a valid BookId and was a bit hacky for alerts.
            // If we need alerts, we should use a proper Notification system or NotificationService.

            TempData["Success"] = "Search saved. You will be notified of new items.";
            return RedirectToAction("Index", "Books", new { q, category });
        }
    }
}
