using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCart.Models;

namespace QuickCart.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        // Define methods specific to OrderHeader repository
        void Update(OrderHeader obj);
    }
}
