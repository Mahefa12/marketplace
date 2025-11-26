using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public enum MessageRole
    {
        Buyer = 0,
        Seller = 1,
        Admin = 2
    }

    public class Message
    {
        public int Id { get; set; }
        [Required]
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public string? BuyerId { get; set; }
        public string? SellerId { get; set; }
        [Required]
        public MessageRole FromRole { get; set; }
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

