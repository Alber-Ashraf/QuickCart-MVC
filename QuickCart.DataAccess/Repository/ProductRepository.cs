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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly QuickCartDbContext _db;
        public ProductRepository(QuickCartDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Product obj)
        {
            // Find the product in the database using its Id
            var objFromDb = _db.Products.Find(obj.Id);

            // If the product is found, update its properties
            if (objFromDb != null)
            {
                objFromDb.Title = obj.Title;
                objFromDb.Author = obj.Author;
                objFromDb.Description = obj.Description;
                objFromDb.ISBN = obj.ISBN;
                objFromDb.ListPrice = obj.ListPrice;
                objFromDb.Price = obj.Price;
                objFromDb.Price50 = obj.Price50;
                objFromDb.Price100 = obj.Price100;
                objFromDb.CategoryId = obj.CategoryId;

                // Only update the ImageURL if it is not null
                //if (obj.ImageURL != null)
                //{
                //    objFromDb.ImageURL = obj.ImageURL;
                //}
            }
        }
    }
}
