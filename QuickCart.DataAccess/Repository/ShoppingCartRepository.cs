using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCart.Data;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;

namespace QuickCart.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly QuickCartDbContext _db;
        public ShoppingCartRepository(QuickCartDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ShoppingCart obj)
        {   
            _db.ShoppingCarts.Update(obj);
        }
    }
}
