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
    public class CategoryRepository : Repository<Category>, ICategoryRepository 
    {
        private readonly QuickCartDbContext _db;
        public CategoryRepository(QuickCartDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Category obj)
        {   
            _db.Categories.Update(obj);
        }
    }
}
