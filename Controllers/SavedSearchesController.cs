using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    public class SavedSearchesController : Controller
    {
        private readonly MarketplaceDbContext _db;
        public SavedSearchesController(MarketplaceDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string? q, string? category, ContactInfo contact)
        {
            var buyer = new Buyer { Contact = contact };
            _db.Buyers.Add(buyer);
            await _db.SaveChangesAsync();

            var ss = new SavedSearch { BuyerId = buyer.Id, Query = q, Category = category };
            _db.SavedSearches.Add(ss);
            await _db.SaveChangesAsync();

            _db.Messages.Add(new Message
            {
                BookId = 0, // not tied to a specific book
                BuyerId = buyer.Id,
                FromRole = MessageRole.Admin,
                Content = $"Alert created for search: '{q}' category '{category}'"
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Search saved. Alerts mocked via admin notifications.";
            return RedirectToAction("Index", "Books", new { q, category });
        }
    }
}

