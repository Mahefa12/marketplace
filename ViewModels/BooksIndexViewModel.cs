using System.Collections.Generic;
using Marketplace.Models;

namespace Marketplace.ViewModels
{
    public class BooksIndexViewModel
    {
        public IEnumerable<Book> Items { get; set; } = new List<Book>();
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public string? Query { get; set; }
        public string? Category { get; set; }
        public string Sort { get; set; } = "newest";
    }
}

