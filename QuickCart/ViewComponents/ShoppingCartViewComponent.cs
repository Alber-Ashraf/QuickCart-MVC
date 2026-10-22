using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Utility;

namespace QuickCart.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
                {
                    var userCartItems = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == claim.Value);
                    HttpContext.Session.SetInt32(SD.SessionCart, userCartItems.Count());
                }
                return View(HttpContext.Session.GetInt32(SD.SessionCart) ?? 0);
            }
            else
            {
                HttpContext.Session.Remove(SD.SessionCart);
                return View(0);
            }
        }
    }
}
