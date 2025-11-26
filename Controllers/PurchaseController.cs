using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Marketplace.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly MarketplaceDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public PurchaseController(MarketplaceDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyNow(int bookId)
        {
            var book = await _db.Books.FindAsync(bookId);
            if (book == null) return NotFound();

            if (book.Status != BookStatus.Active)
            {
                TempData["Error"] = "This book is no longer available for purchase.";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Create purchase request
            var purchaseRequest = new PurchaseRequest
            {
                BookId = bookId,
                BuyerId = userId,
                OfferPrice = book.Price,
                Status = PurchaseRequestStatus.AwaitingPayment
            };
            _db.PurchaseRequests.Add(purchaseRequest);

            // Reserve the book
            book.Status = BookStatus.Reserved;
            await _db.SaveChangesAsync();

            // Redirect to payment instructions
            return RedirectToAction("PaymentInstructions", new { id = purchaseRequest.Id });
        }

        [Authorize]
        public async Task<IActionResult> PaymentInstructions(int id)
        {
            var purchaseRequest = await _db.PurchaseRequests
                .Include(pr => pr.Book)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (purchaseRequest == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Ensure only the buyer or admin can view instructions
            if (purchaseRequest.BuyerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(purchaseRequest);
        }
    }
}
