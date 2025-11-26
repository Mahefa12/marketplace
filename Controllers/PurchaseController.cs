using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly MarketplaceDbContext _db;

        public PurchaseController(MarketplaceDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyNow(int bookId, string buyerName, string buyerEmail, string buyerPhone)
        {
            var book = await _db.Books.FindAsync(bookId);
            if (book == null) return NotFound();

            if (book.Status != BookStatus.Active)
            {
                TempData["Error"] = "This book is no longer available for purchase.";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            // Create buyer
            var buyer = new Buyer
            {
                Contact = new ContactInfo
                {
                    Name = buyerName,
                    Email = buyerEmail,
                    Phone = buyerPhone
                }
            };
            _db.Buyers.Add(buyer);
            await _db.SaveChangesAsync();

            // Create purchase request
            var purchaseRequest = new PurchaseRequest
            {
                BookId = bookId,
                BuyerId = buyer.Id,
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

        public async Task<IActionResult> PaymentInstructions(int id)
        {
            var purchaseRequest = await _db.PurchaseRequests
                .Include(pr => pr.Book)
                .Include(pr => pr.Buyer).ThenInclude(b => b!.Contact)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (purchaseRequest == null) return NotFound();

            return View(purchaseRequest);
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var purchaseRequest = await _db.PurchaseRequests
                .Include(pr => pr.Book)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (purchaseRequest == null) return NotFound();

            // Mark as paid
            purchaseRequest.IsPaid = true;
            purchaseRequest.PaymentConfirmedAt = System.DateTime.UtcNow;
            purchaseRequest.Status = PurchaseRequestStatus.Paid;

            // Mark book as sold
            purchaseRequest.Book!.Status = BookStatus.Sold;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Payment confirmed for Purchase Request #{id}! Book marked as sold.";
            return RedirectToAction("Dashboard", "Admin");
        }
    }
}
