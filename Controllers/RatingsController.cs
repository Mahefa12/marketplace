using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly MarketplaceDbContext _db;

        public RatingsController(MarketplaceDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Create(int purchaseRequestId)
        {
            ViewData["PurchaseRequestId"] = purchaseRequestId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int purchaseRequestId, int stars, string? comment)
        {
            try
            {
                var pr = await _db.PurchaseRequests.Include(x => x.Book).FirstOrDefaultAsync(x => x.Id == purchaseRequestId);
                if (pr == null || pr.Book == null) throw new System.InvalidOperationException("Invalid purchase request");
                if (pr.Status != PurchaseRequestStatus.Completed) throw new System.InvalidOperationException("Request not completed");

                var rating = new SellerRating
                {
                    SellerId = pr.Book.SellerId,
                    Stars = stars,
                    Comment = comment,
                    PurchaseRequestId = pr.Id
                };
                _db.SellerRatings.Add(rating);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Thank you for your rating.";
                return RedirectToAction("Dashboard", "Admin");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewData["PurchaseRequestId"] = purchaseRequestId;
                return View();
            }
        }
    }
}
