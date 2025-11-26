using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class PurchaseRequest
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        public Book? Book { get; set; }

        [Required]
        public string BuyerId { get; set; } = string.Empty;

        public decimal? OfferPrice { get; set; }

        [Required]
        public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.New;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsPaid { get; set; }

        public DateTime? PaymentConfirmedAt { get; set; }
    }
}
