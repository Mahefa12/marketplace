using System.ComponentModel.DataAnnotations;
using Marketplace.Models;

namespace Marketplace.ViewModels
{
    public class PurchaseRequestViewModel
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        public ContactInfo BuyerContact { get; set; } = new ContactInfo();

        public decimal? OfferPrice { get; set; }
    }
}

