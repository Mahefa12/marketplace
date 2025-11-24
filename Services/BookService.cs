using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Marketplace.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Services
{
    public class BookService : IBookService
    {
        private readonly MarketplaceDbContext _db;
        public BookService(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<Book> CreateBookAsync(BookCreateViewModel dto)
        {
            var seller = new Seller { Contact = dto.SellerContact };
            _db.Sellers.Add(seller);
            await _db.SaveChangesAsync();

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Version = dto.Version,
                Price = dto.Price,
                Condition = dto.Condition,
                Category = dto.Category,
                Location = dto.Location,
                Description = dto.Description,
                SellerId = seller.Id,
                Status = BookStatus.Active
            };
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            if (dto.ImageUrls != null)
            {
                if (dto.ImageUrls.Count == 1 && dto.ImageUrls[0].Contains('\n'))
                {
                    dto.ImageUrls = dto.ImageUrls[0].Split('\n').ToList();
                }
                foreach (var url in dto.ImageUrls)
                {
                    if (!string.IsNullOrWhiteSpace(url))
                        _db.BookImages.Add(new BookImage { BookId = book.Id, Url = url });
                }
                await _db.SaveChangesAsync();
            }
            return book;
        }

        public async Task<Book?> GetDetailsAsync(int id)
        {
            return await _db.Books.Include(b => b.Seller).ThenInclude(s => s.Contact).Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<(IEnumerable<Book> items, int total)> GetBooksAsync(string? q, string? sort, string? category, int page, int pageSize)
        {
            sort ??= "newest";
            var baseQuery = _db.Books.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                baseQuery = baseQuery.Where(b => EF.Functions.Like(b.Title, $"%{q}%") || EF.Functions.Like(b.Author, $"%{q}%"));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                baseQuery = baseQuery.Where(b => b.Category == category);
            }
            baseQuery = sort switch
            {
                "price_asc" => baseQuery.OrderBy(b => b.Price),
                "price_desc" => baseQuery.OrderByDescending(b => b.Price),
                _ => baseQuery.OrderByDescending(b => b.CreatedAt)
            };
            var total = await baseQuery.CountAsync();
            var items = await baseQuery.Include(b => b.Images).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task UpdateBookAsync(int id, BookCreateViewModel dto)
        {
            var book = await _db.Books.Include(b => b.Seller).ThenInclude(s => s.Contact).Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) throw new InvalidOperationException("Book not found");
            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Version = dto.Version;
            book.Price = dto.Price;
            book.Condition = dto.Condition;
            book.Category = dto.Category;
            book.Location = dto.Location;
            book.Description = dto.Description;
            if (book.Seller != null) book.Seller.Contact = dto.SellerContact;

            _db.BookImages.RemoveRange(book.Images);
            if (dto.ImageUrls != null)
            {
                if (dto.ImageUrls.Count == 1 && dto.ImageUrls[0].Contains('\n'))
                {
                    dto.ImageUrls = dto.ImageUrls[0].Split('\n').ToList();
                }
                foreach (var url in dto.ImageUrls)
                {
                    if (!string.IsNullOrWhiteSpace(url))
                        _db.BookImages.Add(new BookImage { BookId = book.Id, Url = url });
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task SetStatusAsync(int id, BookStatus status)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) throw new InvalidOperationException("Book not found");
            book.Status = status;
            await _db.SaveChangesAsync();
        }

        public async Task<double> GetSellerAverageRatingAsync(int sellerId)
        {
            var ratings = await _db.SellerRatings.Where(r => r.SellerId == sellerId).Select(r => (int?)r.Stars).ToListAsync();
            if (ratings.Count == 0) return 0;
            return ratings.Average(x => x!.Value);
        }
    }
}

