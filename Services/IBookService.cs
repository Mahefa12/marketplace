using System.Collections.Generic;
using System.Threading.Tasks;
using Marketplace.Models;
using Marketplace.ViewModels;

namespace Marketplace.Services
{
    public interface IBookService
    {
        Task<Book> CreateBookAsync(BookCreateViewModel dto);
        Task<Book?> GetDetailsAsync(int id);
        Task<(IEnumerable<Book> items, int total)> GetBooksAsync(string? q, string? sort, string? category, int page, int pageSize);
        Task UpdateBookAsync(int id, BookCreateViewModel dto);
        Task SetStatusAsync(int id, BookStatus status);
        Task<double> GetSellerAverageRatingAsync(int sellerId);
    }
}

