using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Buyer
    {
        public int Id { get; set; }

        [Required]
        public ContactInfo Contact { get; set; } = new ContactInfo();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

