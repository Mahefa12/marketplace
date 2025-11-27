using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly MarketplaceDbContext _db;

        public ProfileController(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // My Active Listings (Books I am selling)
            var myListings = await _db.Books
                .Where(b => b.SellerId == userId)
                .Include(b => b.Images)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // My Bids (Items I am trying to win)
            var myBids = await _db.Bids
                .Where(b => b.BidderId == userId)
                .Include(b => b.Book)
                    .ThenInclude(book => book!.Images)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // My Purchases (Items I need to pay for or have purchased)
            var myPurchases = await _db.PurchaseRequests
                .Where(pr => pr.BuyerId == userId)
                .Include(pr => pr.Book)
                    .ThenInclude(book => book!.Images)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();

            ViewData["MyListings"] = myListings;
            ViewData["MyBids"] = myBids;
            ViewData["MyPurchases"] = myPurchases;

            return View();
        }
    }
}
