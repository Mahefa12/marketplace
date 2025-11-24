using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Marketplace.Models;

namespace Marketplace.ViewModels
{
    public class BookCreateViewModel
    {
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
        public ContactInfo SellerContact { get; set; } = new ContactInfo();

        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}

