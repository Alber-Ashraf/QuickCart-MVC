using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using QuickCart.Data;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;

namespace QuickCart.DataAccess.Repository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private readonly QuickCartDbContext _db;
        public ProductImageRepository(QuickCartDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProductImage obj)
        {
            throw new NotImplementedException();
        }
    }
}
