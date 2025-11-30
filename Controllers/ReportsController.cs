using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly Services.INotificationService _notifications;

        public ReportsController(Services.INotificationService notifications)
        {
            _notifications = notifications;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookId, string? reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var message = $"Abuse report for Book #{bookId}: {(string.IsNullOrWhiteSpace(reason) ? "No reason provided" : reason)}";

            // Send to admin user; in a real app, iterate all admins.
            await _notifications.NotifyUserAsync("admin", message, bookId);

            TempData["Success"] = "Report submitted. Our team will review it.";
            return RedirectToAction("Details", "Books", new { id = bookId });
        }
    }
}
