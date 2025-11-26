using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Storing Email or Username

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? RelatedBookId { get; set; }
    }
}
