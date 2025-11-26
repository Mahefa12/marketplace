using System.Threading.Tasks;

namespace Marketplace.Services
{
    public interface INotificationService
    {
        Task NotifyUserAsync(string userId, string message, int? bookId = null);
        Task MarkAsReadAsync(int notificationId);
    }
}
