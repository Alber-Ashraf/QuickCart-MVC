using Microsoft.EntityFrameworkCore;

namespace QuickCart.Data
{
    public class QuickCartDbContext : DbContext
    {
        public QuickCartDbContext(DbContextOptions<QuickCartDbContext> options) : base(options)
        {
        }
    }
}
