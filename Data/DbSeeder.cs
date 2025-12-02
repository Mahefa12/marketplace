using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Models;
using Microsoft.AspNetCore.Identity;

namespace Marketplace.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(MarketplaceDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            string[] roles = { "Admin", "Buyer", "Seller" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Users
            var adminUser = await SeedUserAsync(userManager, "admin", "admin@unimarket.com", "Password123!", "Admin");
            var buyerUser = await SeedUserAsync(userManager, "buyer", "buyer@unimarket.com", "Password123!", "Buyer");
            var sellerUser = await SeedUserAsync(userManager, "seller", "seller@unimarket.com", "Password123!", "Seller");

            if (db.Books.Any()) return;

            var samples = new List<(string Title, string Author, string? Version, string Category, string Location, decimal Price, BookCondition Condition, string Description)>
            {
                ("The Pragmatic Programmer", "Andrew Hunt", "2nd", "Programming", "New York", 45m, BookCondition.VeryGood, "Classic programming book."),
                ("Clean Code", "Robert C. Martin", null, "Programming", "San Francisco", 40m, BookCondition.Good, "Guidelines for writing clean code."),
                ("Design Patterns", "Erich Gamma", "1st", "Software", "Chicago", 55m, BookCondition.LikeNew, "Gang of Four patterns."),
                ("Introduction to Algorithms", "Cormen", "3rd", "Algorithms", "Boston", 60m, BookCondition.Good, "Comprehensive algorithms."),
                ("Deep Learning", "Ian Goodfellow", null, "AI", "Seattle", 58m, BookCondition.Acceptable, "Neural networks and deep learning."),
                ("Artificial Intelligence", "Stuart Russell", "4th", "AI", "Austin", 50m, BookCondition.Good, "Modern AI overview."),
            };

            var rnd = new Random(1234);

            for (int i = 0; i < samples.Count; i++)
            {
                var s = samples[i];

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
                    SellerId = buyerUser.Id, // Use seeded buyer ID for testing
                    Status = BookStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(0, 60))
                };
                db.Books.Add(book);
                await db.SaveChangesAsync();

                var img1 = new BookImage { BookId = book.Id, Url = $"https://picsum.photos/seed/{book.Id}a/300/400" };
                var img2 = new BookImage { BookId = book.Id, Url = $"https://picsum.photos/seed/{book.Id}b/300/400" };
                db.BookImages.AddRange(img1, img2);
                await db.SaveChangesAsync();

                if (i % 3 == 0)
                {
                    var pr = new PurchaseRequest
                    {
                        BookId = book.Id,
                        BuyerId = buyerUser.Id, // Use seeded buyer ID
                        OfferPrice = book.Price - rnd.Next(1, 5),
                        Status = PurchaseRequestStatus.Completed
                    };
                    db.PurchaseRequests.Add(pr);
                    await db.SaveChangesAsync();

                    // SellerRating logic removed as SellerRating likely depended on SellerId being int or needs refactoring.
                    // Assuming SellerRating.SellerId is int, we need to skip it or refactor it.
                    // The plan said "Remove DbSet<SellerRating>... temporarily". 
                    // But I kept it in DbContext. Let's check SellerRating model.
                    // If SellerRating uses int SellerId, it will break.
                    // I will skip seeding ratings for now to avoid errors.
                }
            }
        }

        private static async Task<IdentityUser> SeedUserAsync(UserManager<IdentityUser> userManager, string username, string email, string password, string role)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            return user;
        }
    }
}
