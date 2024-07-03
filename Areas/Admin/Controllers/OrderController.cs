using Bulky.DataAccess;
using Bulky.Models;
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
            public IActionResult GetAll()
            {
                List<OrderHeader> objOrderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
                return Json(new {data = objOrderList});
            }

        #endregion

    }
}
