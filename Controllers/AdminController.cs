using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly MarketplaceDbContext _db;
        private readonly Services.INotificationService _notifications;

        public AdminController(MarketplaceDbContext db, Services.INotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public async Task<IActionResult> Dashboard()
        {
            var requests = await _db.PurchaseRequests
                .Include(r => r.Book)
                .OrderByDescending(r => r.CreatedAt).ToListAsync();

            // Add pending books for approval
            var pendingBooks = await _db.Books
                .Where(b => b.Status == Models.BookStatus.Pending)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // Add awaiting payment purchases
            var awaitingPayment = await _db.PurchaseRequests
                .Include(pr => pr.Book)
                .Where(pr => pr.Status == Models.PurchaseRequestStatus.AwaitingPayment)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();

            ViewData["PendingBooks"] = pendingBooks;
            ViewData["AwaitingPayment"] = awaitingPayment;
            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRequestStatus(int id, Models.PurchaseRequestStatus status)
        {
            var pr = await _db.PurchaseRequests.FindAsync(id);
            if (pr == null) return NotFound();
            pr.Status = status;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBook(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) return NotFound();
            book.Status = Models.BookStatus.Active;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Book #{id} approved and is now live on the marketplace.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBook(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) return NotFound();
            _db.Books.Remove(book);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Book #{id} has been rejected and removed.";
            return RedirectToAction(nameof(Dashboard));
        }

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
            purchaseRequest.Status = Models.PurchaseRequestStatus.Paid;

            // Mark book as sold
            purchaseRequest.Book!.Status = Models.BookStatus.Sold;

            await _db.SaveChangesAsync();

            // Notify Buyer
            await _notifications.NotifyUserAsync(purchaseRequest.BuyerId,
                $"Payment Received! Please collect your book '{purchaseRequest.Book!.Title}' at the Admin Desk. Ref: #BK-{purchaseRequest.BookId}",
                purchaseRequest.BookId);

            // Notify Seller
            await _notifications.NotifyUserAsync(purchaseRequest.Book.SellerId,
                $"Item #BK-{purchaseRequest.BookId} ({purchaseRequest.Book.Title}) Sold! Please drop it off at the Admin Desk.",
                purchaseRequest.BookId);

            TempData["Success"] = $"Payment confirmed for Purchase Request #{id}! Notifications sent.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var purchaseRequest = await _db.PurchaseRequests
                .Include(pr => pr.Book)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (purchaseRequest == null) return NotFound();

            // Revert book status
            purchaseRequest.Book!.Status = Models.BookStatus.Active;

            // Notify Seller
            await _notifications.NotifyUserAsync(purchaseRequest.Book.SellerId,
                $"Reservation for #BK-{purchaseRequest.BookId} ({purchaseRequest.Book.Title}) was cancelled. It is back on the market.",
                purchaseRequest.BookId);

            // Remove purchase request
            _db.PurchaseRequests.Remove(purchaseRequest);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Reservation cancelled for Book #{purchaseRequest.BookId}. It is now Active again.";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
