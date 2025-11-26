using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        [Required]
        public string BuyerId { get; set; } = string.Empty;
        [Required]
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

