using System.Security.Claims;
using Bulky.DataAccess;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace MyApp.Namespace
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        // GET: OrderController
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public  OrderVM? OrderVM {get; set;}
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
            OrderVM  = new OrderVM() {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

            if(OrderVM.OrderHeader is not null)
            {
                OrderVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == OrderVM.OrderHeader.ApplicationUserId);
            }
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles=SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetails()
        {
            if(OrderVM?.OrderHeader is not null)
            {
                OrderHeader? orderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id); 
                if(orderFromDb is not null)
                {
                    orderFromDb.Name = OrderVM.OrderHeader.Name;
                    orderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
                    orderFromDb.State = OrderVM.OrderHeader.State;
                    orderFromDb.City = OrderVM.OrderHeader.City;
                    orderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
                    orderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;

                    if(!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
                    {
                        orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
                    }

                    if(!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingStatus))
                    {
                        orderFromDb.TrackingStatus = OrderVM.OrderHeader.TrackingStatus;
                    }

                    _unitOfWork.OrderHeader.Update(orderFromDb);
                    _unitOfWork.Save();
                    TempData["success"] = "Order Details Updated successfully!";
                }
            }
            return RedirectToAction(nameof(Details), new {orderId = OrderVM?.OrderHeader?.Id});
        }

        [HttpPost]
        [Authorize(Roles=SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            if(OrderVM?.OrderHeader is not null)
            {
                _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
                _unitOfWork.Save();
                TempData["success"] = "Order Details Update sucessfully!";
            }
            return RedirectToAction(nameof(Details), new{orderId = OrderVM?.OrderHeader?.Id});
        }

        [HttpPost]
        [Authorize(Roles=SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            if(OrderVM?.OrderHeader is not null)
            {
               var orderFromDb = _unitOfWork.OrderHeader.Get(o => o.Id == OrderVM.OrderHeader.Id);
               
               if(orderFromDb != null)
               {
                    orderFromDb.TrackingStatus = OrderVM.OrderHeader.TrackingStatus;
                    orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
                    orderFromDb.OrderStatus = SD.StatusShipped;
                    orderFromDb.ShippingDate = DateTime.Now;

                    if(orderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayement)
                    {
                        orderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)); 
                    }

                    _unitOfWork.OrderHeader.Update(orderFromDb);
                    _unitOfWork.Save();
                    TempData["success"] = "Order Shipped Sucessfully";      
               }  
            }
            return RedirectToAction(nameof(Details), new{orderId = OrderVM?.OrderHeader?.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            if(OrderVM?.OrderHeader is not null)
            {
                var orderFromDb = _unitOfWork.OrderHeader.Get(o => o.Id == OrderVM.OrderHeader.Id);
                if(orderFromDb != null)
                {
                    if(orderFromDb.PaymentStatus == SD.PaymentStatusApproved)
                    {
                        var options = new RefundCreateOptions{
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = orderFromDb.PaymentIntentId
                        };

                        var service = new RefundService();
                        Refund refund = service.Create(options);
                        _unitOfWork.OrderHeader.UpdateStatus(orderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
                    }
                    else
                    {
                        _unitOfWork.OrderHeader.UpdateStatus(orderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
                    }

                    _unitOfWork.Save();
                    TempData["success"] = "Order Cancelled Successfully!";
                }
            }
            return RedirectToAction(nameof(Details), new {orderId = OrderVM?.OrderHeader?.Id});
        }
        [ActionName("Details")]
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult Details_PAY_NOW()
        {
            if(OrderVM?.OrderHeader != null)
            {
                OrderVM.OrderHeader = _unitOfWork.OrderHeader
                    .Get(o => o.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                OrderVM.OrderDetail = _unitOfWork.OrderDetail
                    .GetAll(o => o.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    //it is a regular customer account and we need to capture payment
                    // stripe LOGIC
                    string domain = "http://localhost:5217/";

                     
                    var options = new SessionCreateOptions
                    {
                        SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?orderHeaderid={OrderVM?.OrderHeader?.Id}",
                        CancelUrl = domain + $"Admin/Order/Details?orderId={OrderVM?.OrderHeader?.Id}",
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                    };

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                foreach (var item in OrderVM.OrderDetail)
                    {
                        var sessionLineItem = new SessionLineItemOptions {
                            PriceData = new SessionLineItemPriceDataOptions {
                                UnitAmount = (long)(item.Price * 100), //$20.50 ==> 2050
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions {
                                    Name = item?.Product?.Title
                                }
                            },
                            
                            Quantity = item?.Count
                        };

                        options.LineItems.Add(sessionLineItem);
                    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                var service = new SessionService();
                Session session = service.Create(options);

#pragma warning disable CS8602 // Dereference of a possibly null reference.

                _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);

#pragma warning restore CS8602 // Dereference of a possibly null reference.

                _unitOfWork.Save();

                    //Redirect to Stripe for making payments
                    Response.Headers.Append("Location", session.Url);
                    return new StatusCodeResult(303);

            }
             
            return RedirectToAction(nameof(Details), new {orderId = OrderVM?.OrderHeader?.Id});
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader? orderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if(orderFromDb != null)
            {
                if(orderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayement)
                {
                    //this is the order by the Company
                    var service = new SessionService();
                    var session = service.Get(orderFromDb.SessionId);

                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                        if(orderFromDb.OrderStatus is not null)
                        {
                            _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderFromDb.OrderStatus, SD.PaymentStatusApproved);
                        }
                        _unitOfWork.Save();
                    }

                }

                _unitOfWork.Save();
            }

            return View(orderHeaderId);
        }   

         #region API CALLS

            [HttpGet]
            public IActionResult GetAll(string status)
            {
                IEnumerable<OrderHeader> objOrderList; 

                if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
                {
                   objOrderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
                }
                else
                {
                    var claimsIdentity = (ClaimsIdentity?) User.Identity;
                    var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    objOrderList = _unitOfWork.OrderHeader.GetAll(o => o.ApplicationUserId == userId, includeProperties: "ApplicationUser");
                }

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
