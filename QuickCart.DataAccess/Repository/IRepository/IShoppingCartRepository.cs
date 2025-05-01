using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCart.Models;

namespace QuickCart.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        // Define methods specific to ShoppingCart repository
        void Update(ShoppingCart obj);
    }
}
