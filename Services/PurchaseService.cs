using System;
using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Marketplace.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly MarketplaceDbContext _db;
        public PurchaseService(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<Buyer> EnsureBuyerAsync(ContactInfo contact)
        {
            var existing = await _db.Buyers.FirstOrDefaultAsync(b => b.Contact.Email == contact.Email);
            if (existing != null) return existing;
            var buyer = new Buyer { Contact = contact };
            _db.Buyers.Add(buyer);
            await _db.SaveChangesAsync();
            return buyer;
        }

        public async Task<PurchaseRequest> CreatePurchaseRequestAsync(PurchaseRequestViewModel dto)
        {
            var buyer = await EnsureBuyerAsync(dto.BuyerContact);
            var pr = new PurchaseRequest
            {
                BookId = dto.BookId,
                BuyerId = buyer.Id,
                OfferPrice = dto.OfferPrice,
                Status = PurchaseRequestStatus.New
            };
            _db.PurchaseRequests.Add(pr);
            await _db.SaveChangesAsync();
            return pr;
        }

        public async Task UpdateRequestStatusAsync(int id, PurchaseRequestStatus status)
        {
            var pr = await _db.PurchaseRequests.FindAsync(id);
            if (pr == null) throw new InvalidOperationException("Request not found");
            pr.Status = status;
            await _db.SaveChangesAsync();
        }

        public async Task<SellerRating> CreateSellerRatingAsync(int purchaseRequestId, int stars, string? comment)
        {
            var pr = await _db.PurchaseRequests.Include(x => x.Book).FirstOrDefaultAsync(x => x.Id == purchaseRequestId);
            if (pr == null || pr.Book == null) throw new InvalidOperationException("Invalid purchase request");
            if (pr.Status != PurchaseRequestStatus.Completed) throw new InvalidOperationException("Request not completed");
            var rating = new SellerRating
            {
                SellerId = pr.Book.SellerId,
                Stars = stars,
                Comment = comment,
                PurchaseRequestId = pr.Id
            };
            _db.SellerRatings.Add(rating);
            await _db.SaveChangesAsync();
            return rating;
        }
    }
}

