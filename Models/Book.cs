using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Author { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Version { get; set; }

        [Range(0, 1000000)]
        public decimal Price { get; set; }

        [Required]
        public BookCondition Condition { get; set; } = BookCondition.Good;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(120)]
        public string? Location { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public BookStatus Status { get; set; } = BookStatus.Active;

        [Required]
        public int SellerId { get; set; }

        public Seller? Seller { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookImage> Images { get; set; } = new List<BookImage>();
        public ICollection<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}
