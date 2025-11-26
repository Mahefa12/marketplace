using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    public class BidsController : Controller
    {
        private readonly MarketplaceDbContext _db;
        private const decimal MinimumBidIncrement = 10m;

        public BidsController(MarketplaceDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(int bookId, decimal amount, string buyerName, string buyerEmail, string buyerPhone)
        {
            var book = await _db.Books
                .Include(b => b.Bids)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null) return NotFound();

            // Get current highest bid or starting price
            var currentHighest = book.Bids.Any() 
                ? book.Bids.Max(b => b.Amount) 
                : book.Price;

            // Validate bid amount
            if (amount < currentHighest + MinimumBidIncrement)
            {
                TempData["Error"] = $"Bid must be at least R{currentHighest + MinimumBidIncrement:F2} (R10 above current highest)";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            // Create or find buyer
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

            // Create bid
            var bid = new Bid
            {
                BookId = bookId,
                BidderUserId = buyer.Id,
                Amount = amount
            };

            _db.Bids.Add(bid);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Your bid of R{amount:F2} has been placed successfully!";
            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptBid(int id)
        {
            var bid = await _db.Bids
                .Include(b => b.Book)
                .Include(b => b.Bidder).ThenInclude(b => b!.Contact)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bid == null) return NotFound();

            // Mark bid as accepted
            bid.IsAccepted = true;

            // Create purchase request to trigger payment flow
            var purchaseRequest = new PurchaseRequest
            {
                BookId = bid.BookId,
                BuyerId = bid.BidderUserId,
                OfferPrice = bid.Amount,
                Status = PurchaseRequestStatus.New
            };

            _db.PurchaseRequests.Add(purchaseRequest);

            // Reserve the book
            bid.Book!.Status = BookStatus.Reserved;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Bid accepted! Buyer will be contacted for payment arrangement.";
            return RedirectToAction("Details", "Books", new { id = bid.BookId });
        }
    }
}
