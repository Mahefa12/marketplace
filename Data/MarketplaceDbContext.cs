using Marketplace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Data
{
    public class MarketplaceDbContext : IdentityDbContext<IdentityUser>
    {
        public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
        public DbSet<BookImage> BookImages => Set<BookImage>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public DbSet<SavedSearch> SavedSearches => Set<SavedSearch>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<SellerRating> SellerRatings => Set<SellerRating>();
        public DbSet<Bid> Bids => Set<Bid>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.PurchaseRequests)
                .WithOne(pr => pr.Book!)
                .HasForeignKey(pr => pr.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Images)
                .WithOne(i => i.Book!)
                .HasForeignKey(i => i.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>().HasIndex(b => b.Title);
            modelBuilder.Entity<Book>().HasIndex(b => b.Author);
            modelBuilder.Entity<Book>().HasIndex(b => b.CreatedAt);
            modelBuilder.Entity<PurchaseRequest>().HasIndex(pr => pr.BookId);
            modelBuilder.Entity<Message>().HasIndex(m => m.BookId);

            // Bid relationships
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Bids)
                .WithOne(bid => bid.Book!)
                .HasForeignKey(bid => bid.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bid>().HasIndex(b => b.BookId);
        }
    }
}
