using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.ViewComponents
{
    public class NotificationsViewComponent : ViewComponent
    {
        private readonly MarketplaceDbContext _db;

        public NotificationsViewComponent(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // In a real app, we'd get the current user's ID. 
            // For this demo, we'll check if the user is authenticated as Admin (using "admin" username)
            // or if we have a way to identify the student.
            // Since we don't have full Identity, we'll use a cookie or session if available, 
            // or just show notifications for "admin" if logged in as admin.

            // However, the requirement says "fetch unread notifications for the logged-in user".
            // Our current auth is simple cookie "AdminAuth".
            // We also have "Buyer" and "Seller" concepts but no persistent login for them yet, 
            // except maybe we can simulate it or just show admin notifications.

            // Let's assume we are tracking the current user via User.Identity.Name if authenticated.

            var userId = User.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return Content(string.Empty);
            }

            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }
    }
}
