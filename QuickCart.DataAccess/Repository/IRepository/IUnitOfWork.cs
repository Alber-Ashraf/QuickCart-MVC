using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace QuickCart.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IApplicationUserRepository ApplicationUsers { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IOrderDetailRepository OrderDetails { get; }
        IProductImageRepository ProductImage { get; }
        IRepository<IdentityRole> Roles { get; }
        IRepository<IdentityUserRole<string>> UserRoles { get; }

        void Save();
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
    }
}
