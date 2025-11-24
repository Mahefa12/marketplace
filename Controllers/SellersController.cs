using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    public class SellersController : Controller
    {
        private readonly MarketplaceDbContext _db;
        public SellersController(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Profile(int id)
        {
            var seller = await _db.Sellers.Include(s => s.Contact).FirstOrDefaultAsync(s => s.Id == id);
            if (seller == null) return NotFound();
            var avg = await _db.SellerRatings.Where(r => r.SellerId == id).Select(r => r.Stars).DefaultIfEmpty(0).AverageAsync();
            var books = await _db.Books.Where(b => b.SellerId == id).OrderByDescending(b => b.CreatedAt).ToListAsync();
            ViewData["Avg"] = avg;
            ViewData["Books"] = books;
            return View(seller);
        }
    }
}

