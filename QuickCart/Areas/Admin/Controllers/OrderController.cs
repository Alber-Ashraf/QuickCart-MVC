using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using QuickCart.Utility;

namespace QuickCart.Areas.Admin.Controllers
{
    [Area("Admin")]
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

        // This method is used to get the list of Products in JSON format for DataTables
        #region ApI Calls
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            // Fetch the list of Products from the database
            IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includedProperties: "ApplicationUser").ToList();

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment).ToList();
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess).ToList();
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped).ToList();
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved).ToList();
                    break;
                default:
                    break;
            }

            // Return the list as JSON
            return Json(new { data = objOrderHeaders });
        }

        #endregion
    }
}
