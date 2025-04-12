using Microsoft.EntityFrameworkCore;

namespace QuickCart.Data
{
    public class QuickCartDbContext : DbContext
    {
        public QuickCartDbContext(DbContextOptions<QuickCartDbContext> options) : base(options)
        {
        }

        //Add DbSet property for Category model
        public DbSet<QuickCart.Models.Category> Categories { get; set; } 
    }
}
