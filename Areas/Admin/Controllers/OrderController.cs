using Bulky.DataAccess;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        // GET: OrderController
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
            OrderVM orderVM  = new OrderVM() {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

            if(orderVM.OrderHeader is not null)
            {
                orderVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == orderVM.OrderHeader.ApplicationUserId);
            }
            return View(orderVM);
        }

         #region API CALLS

            [HttpGet]
            public IActionResult GetAll(string status)
            {
                IEnumerable<OrderHeader> objOrderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

                switch(status) {
                    case "pending" :
                        objOrderList = objOrderList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayement);
                        break;
                    case "inprocess" :
                        objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusInProcess);
                        break;
                    case "completed" :
                        objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusShipped);
                        break;
                    case "approved" :
                        objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusApproved);
                        break;
                    default:
                        break;
                }

                return Json(new {data = objOrderList});
            }

        #endregion

    }
}
