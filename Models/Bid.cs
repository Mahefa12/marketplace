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
        public int BidderUserId { get; set; }

        // Using Buyer entity as the bidder
        public Buyer? Bidder { get; set; }

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsAccepted { get; set; }
    }
}
