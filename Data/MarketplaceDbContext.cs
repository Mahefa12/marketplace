using Marketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Data
{
    public class MarketplaceDbContext : DbContext
    {
        public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
        public DbSet<Seller> Sellers => Set<Seller>();
        public DbSet<Buyer> Buyers => Set<Buyer>();
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

            modelBuilder.Entity<Seller>().OwnsOne(s => s.Contact);
            modelBuilder.Entity<Buyer>().OwnsOne(b => b.Contact);

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

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Seller)
                .WithMany()
                .HasForeignKey(b => b.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseRequest>()
                .HasOne(pr => pr.Buyer)
                .WithMany()
                .HasForeignKey(pr => pr.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>().HasIndex(b => b.Title);
            modelBuilder.Entity<Book>().HasIndex(b => b.Author);
            modelBuilder.Entity<Book>().HasIndex(b => b.CreatedAt);
            modelBuilder.Entity<PurchaseRequest>().HasIndex(pr => pr.BookId);
            modelBuilder.Entity<WishlistItem>().HasIndex(w => new { w.BuyerId, w.BookId }).IsUnique();
            modelBuilder.Entity<SavedSearch>().HasIndex(s => s.BuyerId);
            modelBuilder.Entity<Message>().HasIndex(m => m.BookId);
            modelBuilder.Entity<SellerRating>().HasIndex(r => r.SellerId);

            // Bid relationships
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Bids)
                .WithOne(bid => bid.Book!)
                .HasForeignKey(bid => bid.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bid>()
                .HasOne(bid => bid.Bidder)
                .WithMany()
                .HasForeignKey(bid => bid.BidderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bid>().HasIndex(b => b.BookId);
        }
    }
}
