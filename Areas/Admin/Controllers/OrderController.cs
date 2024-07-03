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
        public ActionResult Index()
        {
            return View();
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
