using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class BookImage
    {
        public int Id { get; set; }
        [Required]
        public int BookId { get; set; }
        public Book? Book { get; set; }

        [Required]
        [StringLength(500)]
        public string Url { get; set; } = string.Empty;
    }
}

