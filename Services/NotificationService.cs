using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;

namespace Marketplace.Services
{
    public class NotificationService : INotificationService
    {
        private readonly MarketplaceDbContext _db;

        public NotificationService(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task NotifyUserAsync(string userId, string message, int? bookId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                RelatedBookId = bookId,
                IsRead = false
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _db.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _db.SaveChangesAsync();
            }
        }
    }
}
