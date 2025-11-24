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
        [StringLength(120)]
        public string BuyerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string BuyerEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(30)]
        public string BuyerPhone { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

