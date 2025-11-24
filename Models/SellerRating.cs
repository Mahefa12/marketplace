using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class SellerRating
    {
        public int Id { get; set; }
        [Required]
        public int SellerId { get; set; }
        public Seller? Seller { get; set; }
        [Range(1,5)]
        public int Stars { get; set; }
        [StringLength(2000)]
        public string? Comment { get; set; }
        public int? PurchaseRequestId { get; set; }
        public PurchaseRequest? PurchaseRequest { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

