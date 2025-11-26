using System.Security.Claims;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    public class AdminController : Controller
    {
        private readonly MarketplaceDbContext _db;
        private readonly Services.INotificationService _notifications;
        private const string DemoUser = "admin";
        private const string DemoPass = "admin123";

        public AdminController(MarketplaceDbContext db, Services.INotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["DemoUser"] = DemoUser;
            ViewData["DemoPass"] = DemoPass;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (username == DemoUser && password == DemoPass)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, DemoUser),
                    new Claim(ClaimTypes.Role, "Admin"),
                };
                var identity = new ClaimsIdentity(claims, "AdminAuth");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("AdminAuth", principal, new AuthenticationProperties
                {
                    IsPersistent = true
                });
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.AddModelError(string.Empty, "Invalid credentials");
            ViewData["DemoUser"] = DemoUser;
            ViewData["DemoPass"] = DemoPass;
            return View();
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        public async Task<IActionResult> Dashboard()
        {
            var requests = await _db.PurchaseRequests.Include(r => r.Book).ThenInclude(b => b.Seller).ThenInclude(s => s.Contact)
                .Include(r => r.Buyer).ThenInclude(b => b.Contact)
                .OrderByDescending(r => r.CreatedAt).ToListAsync();

            // Add pending books for approval
            var pendingBooks = await _db.Books
                .Include(b => b.Seller).ThenInclude(s => s!.Contact)
                .Where(b => b.Status == Models.BookStatus.Pending)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // Add awaiting payment purchases
            var awaitingPayment = await _db.PurchaseRequests
                .Include(pr => pr.Book).ThenInclude(b => b!.Seller).ThenInclude(s => s!.Contact)
                .Include(pr => pr.Buyer).ThenInclude(b => b!.Contact)
                .Where(pr => pr.Status == Models.PurchaseRequestStatus.AwaitingPayment)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();

            ViewData["PendingBooks"] = pendingBooks;
            ViewData["AwaitingPayment"] = awaitingPayment;
            return View(requests);
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
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

        [Authorize(AuthenticationSchemes = "AdminAuth")]
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

        [Authorize(AuthenticationSchemes = "AdminAuth")]
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

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var purchaseRequest = await _db.PurchaseRequests
                .Include(pr => pr.Book).ThenInclude(b => b!.Seller).ThenInclude(s => s!.Contact)
                .Include(pr => pr.Buyer).ThenInclude(b => b!.Contact)
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
            if (purchaseRequest.Buyer != null)
            {
                await _notifications.NotifyUserAsync(purchaseRequest.Buyer.Contact.Name,
                    $"Payment Received! Please collect your book '{purchaseRequest.Book!.Title}' at the Admin Desk. Ref: #BK-{purchaseRequest.BookId}",
                    purchaseRequest.BookId);
            }

            // Notify Seller
            if (purchaseRequest.Book!.Seller != null)
            {
                await _notifications.NotifyUserAsync(purchaseRequest.Book.Seller.Contact.Name,
                    $"Item #BK-{purchaseRequest.BookId} ({purchaseRequest.Book.Title}) Sold! Please drop it off at the Admin Desk.",
                    purchaseRequest.BookId);
            }

            TempData["Success"] = $"Payment confirmed for Purchase Request #{id}! Notifications sent.";
            return RedirectToAction(nameof(Dashboard));
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var purchaseRequest = await _db.PurchaseRequests
                .Include(pr => pr.Book).ThenInclude(b => b!.Seller).ThenInclude(s => s!.Contact)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (purchaseRequest == null) return NotFound();

            // Revert book status
            purchaseRequest.Book!.Status = Models.BookStatus.Active;

            // Notify Seller
            if (purchaseRequest.Book.Seller != null)
            {
                await _notifications.NotifyUserAsync(purchaseRequest.Book.Seller.Contact.Name,
                    $"Reservation for #BK-{purchaseRequest.BookId} ({purchaseRequest.Book.Title}) was cancelled. It is back on the market.",
                    purchaseRequest.BookId);
            }

            // Remove purchase request
            _db.PurchaseRequests.Remove(purchaseRequest);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Reservation cancelled for Book #{purchaseRequest.BookId}. It is now Active again.";
            return RedirectToAction(nameof(Dashboard));
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminAuth");
            return RedirectToAction(nameof(Login));
        }
    }
}
