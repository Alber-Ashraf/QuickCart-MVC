using Microsoft.EntityFrameworkCore;
using QuickCart.Models;

namespace QuickCart.Data
{
    public class QuickCartDbContext : DbContext
    {
        public QuickCartDbContext(DbContextOptions<QuickCartDbContext> options) : base(options)
        {
        }

        // Add DbSet property for Category model
        public DbSet<QuickCart.Models.Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Horror", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 }
            );
        }
    }
}
