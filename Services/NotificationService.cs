using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.SignalR;

namespace Marketplace.Services
{
    public class NotificationService : INotificationService
    {
        private readonly MarketplaceDbContext _db;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<Marketplace.Hubs.NotificationHub> _hubContext;

        public NotificationService(MarketplaceDbContext db, Microsoft.AspNetCore.SignalR.IHubContext<Marketplace.Hubs.NotificationHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
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

            // Send real-time notification
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message, bookId);
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
