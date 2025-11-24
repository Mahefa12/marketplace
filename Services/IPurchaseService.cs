using System.Threading.Tasks;
using Marketplace.Models;
using Marketplace.ViewModels;

namespace Marketplace.Services
{
    public interface IPurchaseService
    {
        Task<Buyer> EnsureBuyerAsync(ContactInfo contact);
        Task<PurchaseRequest> CreatePurchaseRequestAsync(PurchaseRequestViewModel dto);
        Task UpdateRequestStatusAsync(int id, PurchaseRequestStatus status);
        Task<SellerRating> CreateSellerRatingAsync(int purchaseRequestId, int stars, string? comment);
    }
}

