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

        [Required]
        [StringLength(120)]
        public string SellerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string SellerEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(30)]
        public string SellerPhone { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();
    }
}

