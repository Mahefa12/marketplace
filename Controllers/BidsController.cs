using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Marketplace.Controllers
{
    public class BidsController : Controller
    {
        private readonly MarketplaceDbContext _db;
        private readonly Services.INotificationService _notifications;
        private const decimal MinimumBidIncrement = 10m;

        public BidsController(MarketplaceDbContext db, Services.INotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(int bookId, decimal amount)
        {
            var book = await _db.Books
                .Include(b => b.Bids)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null) return NotFound();

            // Get current highest bid or starting price
            var currentHighest = book.Bids.Any()
                ? book.Bids.Max(b => b.Amount)
                : 0m;

            // Validate bid amount
            if (amount >= book.Price)
            {
                TempData["Error"] = "Bid cannot equal or exceed the Buy Now Price. Please use the 'Buy Now' option instead.";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            if (amount < currentHighest + MinimumBidIncrement)
            {
                TempData["Error"] = $"Bid must be at least R{currentHighest + MinimumBidIncrement:F2} (R10 above current highest)";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Get previous highest bidder (if any)
            var previousHighestBid = book.Bids
                .Where(b => b.Amount == currentHighest)
                .FirstOrDefault();

            // Create bid
            var bid = new Bid
            {
                BookId = bookId,
                BidderId = userId,
                Amount = amount
            };

            _db.Bids.Add(bid);
            await _db.SaveChangesAsync();

            // Notify seller about new bid
            await _notifications.NotifyUserAsync(
                book.SellerId,
                $"New bid of R{amount:F2} placed on your book '{book.Title}'!",
                bookId);

            // Notify previous highest bidder that they've been outbid
            if (previousHighestBid != null && previousHighestBid.BidderId != userId)
            {
                await _notifications.NotifyUserAsync(
                    previousHighestBid.BidderId,
                    $"You've been outbid on '{book.Title}'! New highest bid: R{amount:F2}",
                    bookId);
            }

            TempData["Success"] = $"Your bid of R{amount:F2} has been placed successfully!";
            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptBid(int id)
        {
            var bid = await _db.Bids
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bid == null) return NotFound();

            // Ensure only the seller or admin can accept bids
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (bid.Book!.SellerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Mark bid as accepted
            bid.IsAccepted = true;

            // Create purchase request to trigger payment flow
            var purchaseRequest = new PurchaseRequest
            {
                BookId = bid.BookId,
                BuyerId = bid.BidderId,
                OfferPrice = bid.Amount,
                Status = PurchaseRequestStatus.New
            };

            _db.PurchaseRequests.Add(purchaseRequest);

            // Reserve the book
            bid.Book!.Status = BookStatus.Reserved;

            await _db.SaveChangesAsync();

            // Notify buyer that their bid was accepted
            await _notifications.NotifyUserAsync(
                bid.BidderId,
                $"Congratulations! Your bid of R{bid.Amount:F2} for '{bid.Book.Title}' was accepted!",
                bid.BookId);

            TempData["Success"] = "Bid accepted! Buyer will be contacted for payment arrangement.";
            return RedirectToAction("Details", "Books", new { id = bid.BookId });
        }
    }
}
