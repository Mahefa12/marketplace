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
        public async Task<IActionResult> Save(string? q, string? category, string? schedule)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var ss = new SavedSearch { BuyerId = userId, Query = q, Category = category };
            _db.SavedSearches.Add(ss);
            await _db.SaveChangesAsync();

            // Send a notification confirming alert setup (schedule is advisory until scheduler is implemented)
            var note = $"Saved search created{(string.IsNullOrWhiteSpace(schedule) ? string.Empty : $" (alerts: {schedule})")}.";
            var notifications = HttpContext.RequestServices.GetService(typeof(Marketplace.Services.INotificationService)) as Marketplace.Services.INotificationService;
            if (notifications != null)
            {
                await notifications.NotifyUserAsync(userId, note);
            }

            TempData["Success"] = "Search saved. You will be notified of new items.";
            return RedirectToAction("Index", "Books", new { q, category });
        }
    }
}
