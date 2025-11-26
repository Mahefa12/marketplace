using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Bid
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        public Book? Book { get; set; }

        [Required]
        public string BidderId { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsAccepted { get; set; }
    }
}
