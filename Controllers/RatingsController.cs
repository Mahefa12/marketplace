using System.Threading.Tasks;
using Marketplace.Services;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    public class RatingsController : Controller
    {
        private readonly IPurchaseService _purchaseService;
        public RatingsController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
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
                await _purchaseService.CreateSellerRatingAsync(purchaseRequestId, stars, comment);
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

