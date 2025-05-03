using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCart.Models;

namespace QuickCart.DataAccess.Repository.IRepository
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        // Define methods specific to OrderDetail repository
        void Update(OrderDetail obj);
    }
}
