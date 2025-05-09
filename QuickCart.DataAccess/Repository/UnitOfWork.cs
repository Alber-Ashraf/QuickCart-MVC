using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using QuickCart.Data;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;

namespace QuickCart.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QuickCartDbContext _db;
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUsers { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetails { get; private set; }
        public IProductImageRepository ProductImage { get; private set; }
        public IRepository<IdentityRole> Roles { get; private set; }
        public IRepository<IdentityUserRole<string>> UserRoles { get; private set; }

        public UnitOfWork(QuickCartDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
            ApplicationUsers = new ApplicationUserRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            OrderDetails = new OrderDetailRepository(_db);
            ProductImage = new ProductImageRepository(_db);
            Roles = new Repository<IdentityRole>(_db);
            UserRoles = new Repository<IdentityUserRole<string>>(_db);
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
