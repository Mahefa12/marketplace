using System.ComponentModel.DataAnnotations;
using Marketplace.Models;

namespace Marketplace.ViewModels
{
    public class PurchaseRequestViewModel
    {
        [Required]
        public int BookId { get; set; }



        public decimal? OfferPrice { get; set; }
    }
}

