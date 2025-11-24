using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Models;

namespace Marketplace.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(MarketplaceDbContext db)
        {
            if (db.Books.Any()) return;

            var samples = new List<(string Title, string Author, string? Version, string Category, string Location, decimal Price, BookCondition Condition, string Description)>
            {
                ("The Pragmatic Programmer", "Andrew Hunt", "2nd", "Programming", "New York", 45m, BookCondition.VeryGood, "Classic programming book."),
                ("Clean Code", "Robert C. Martin", null, "Programming", "San Francisco", 40m, BookCondition.Good, "Guidelines for writing clean code."),
                ("Design Patterns", "Erich Gamma", "1st", "Software", "Chicago", 55m, BookCondition.LikeNew, "Gang of Four patterns."),
                ("Introduction to Algorithms", "Cormen", "3rd", "Algorithms", "Boston", 60m, BookCondition.Good, "Comprehensive algorithms."),
                ("Deep Learning", "Ian Goodfellow", null, "AI", "Seattle", 58m, BookCondition.Acceptable, "Neural networks and deep learning."),
                ("Artificial Intelligence", "Stuart Russell", "4th", "AI", "Austin", 50m, BookCondition.Good, "Modern AI overview."),
                ("Database System Concepts", "Abraham Silberschatz", "6th", "Databases", "Denver", 48m, BookCondition.VeryGood, "Database fundamentals."),
                ("Operating System Concepts", "Abraham Silberschatz", "9th", "Systems", "Portland", 46m, BookCondition.Good, "OS principles."),
                ("Computer Networks", "Andrew S. Tanenbaum", "5th", "Networks", "Miami", 42m, BookCondition.Good, "Networking basics."),
                ("Refactoring", "Martin Fowler", "2nd", "Programming", "Los Angeles", 44m, BookCondition.LikeNew, "Improving design of code."),
                ("You Don't Know JS", "Kyle Simpson", null, "Web", "Toronto", 30m, BookCondition.Good, "In-depth JavaScript."),
                ("Eloquent JavaScript", "Marijn Haverbeke", "3rd", "Web", "London", 28m, BookCondition.VeryGood, "Modern JavaScript guide."),
            };

            var rnd = new Random(1234);

            for (int i = 0; i < samples.Count; i++)
            {
                var s = samples[i];
                var seller = new Seller
                {
                    Contact = new ContactInfo
                    {
                        Name = $"Seller {i + 1}",
                        Email = $"seller{i + 1}@example.com",
                        Phone = $"555-000{i + 1:D2}"
                    }
                };
                db.Sellers.Add(seller);
                await db.SaveChangesAsync();

                var book = new Book
                {
                    Title = s.Title,
                    Author = s.Author,
                    Version = s.Version,
                    Price = s.Price,
                    Condition = s.Condition,
                    Category = s.Category,
                    Location = s.Location,
                    Description = s.Description,
                    SellerId = seller.Id,
                    Status = BookStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(0, 60))
                };
                db.Books.Add(book);
                await db.SaveChangesAsync();

                var img1 = new BookImage { BookId = book.Id, Url = $"https://picsum.photos/seed/{book.Id}a/300/400" };
                var img2 = new BookImage { BookId = book.Id, Url = $"https://picsum.photos/seed/{book.Id}b/300/400" };
                db.BookImages.AddRange(img1, img2);
                await db.SaveChangesAsync();

                if (i % 4 == 0)
                {
                    var pr = new PurchaseRequest
                    {
                        BookId = book.Id,
                        BuyerId = await EnsureBuyerAsync(db, $"buyer{i + 1}@example.com", $"Buyer {i + 1}", $"555-100{i + 1:D2}"),
                        OfferPrice = book.Price - rnd.Next(1, 5),
                        Status = PurchaseRequestStatus.Completed
                    };
                    db.PurchaseRequests.Add(pr);
                    await db.SaveChangesAsync();

                    var rating = new SellerRating
                    {
                        SellerId = seller.Id,
                        Stars = rnd.Next(3, 5),
                        Comment = "Great seller",
                        PurchaseRequestId = pr.Id
                    };
                    db.SellerRatings.Add(rating);
                    await db.SaveChangesAsync();
                }
            }
        }

        private static async Task<int> EnsureBuyerAsync(MarketplaceDbContext db, string email, string name, string phone)
        {
            var existing = db.Buyers.FirstOrDefault(b => b.Contact.Email == email);
            if (existing != null) return existing.Id;
            var buyer = new Buyer { Contact = new ContactInfo { Email = email, Name = name, Phone = phone } };
            db.Buyers.Add(buyer);
            await db.SaveChangesAsync();
            return buyer.Id;
        }
    }
}

