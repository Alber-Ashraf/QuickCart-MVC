using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models.ViewModels;
using QuickCart.Models;
using QuickCart.Utility;
using System.Diagnostics;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("admin")]
    public class OrderController : Controller
    {


        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == orderId, includedProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetails.GetAll(u => u.OrderHeaderId == orderId, includedProperties: "Product")
            };

            return View(orderVM);
        }



        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includedProperties: "ApplicationUser").ToList();


            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;

            }


            return Json(new { data = objOrderHeaders });
        }


        #endregion
    }
}