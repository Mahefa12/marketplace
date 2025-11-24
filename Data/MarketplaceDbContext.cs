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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Book>()
                .HasMany(b => b.PurchaseRequests)
                .WithOne(pr => pr.Book!)
                .HasForeignKey(pr => pr.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

