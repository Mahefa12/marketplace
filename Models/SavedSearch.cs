using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class SavedSearch
    {
        public int Id { get; set; }
        [Required]
        public int BuyerId { get; set; }
        public Buyer? Buyer { get; set; }
        [StringLength(200)]
        public string? Query { get; set; }
        [StringLength(100)]
        public string? Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

