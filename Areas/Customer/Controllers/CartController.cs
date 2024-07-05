using System.Runtime.CompilerServices;
using System.Security.Claims;
using Bulky.DataAccess;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace MyApp.Namespace
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        // GET: CartController
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM? ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitofwork)
        {
            _unitOfWork = unitofwork;
        }
        public ActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity?) User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ShoppingCartVM  = new() {
               ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                    includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            foreach (ShoppingCart cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity?) User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ShoppingCartVM  = new() {
               ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                    includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            //This is to display for summary page

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser?.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser?.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser?.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser?.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser?.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser?.PostalCode;

           
            foreach (ShoppingCart cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity?) User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(ShoppingCartVM != null)
            {
                 ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                 includeProperties: "Product");

                ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

                ApplicationUser? applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
                       
                foreach (ShoppingCart cart in ShoppingCartVM.ShoppingCartList)
                {
                    cart.Price = GetPriceBasedOnQuantity(cart);
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                if(applicationUser?.CompanyId.GetValueOrDefault() == 0)
                {
                    //doesn't have company user access and is a regular customer
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                }
                else
                {
                    //30 days payment window to be given
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayement;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                }

                _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
                _unitOfWork.Save();

                foreach(var cart in ShoppingCartVM.ShoppingCartList)
                {
                    OrderDetail orderDetail = new() {
                        ProductId = cart.ProductId,
                        OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                        Price = cart.Price,
                        Count = cart.Count
                    };

                    _unitOfWork.OrderDetail.Add(orderDetail);
                    _unitOfWork.Save();
                }

                if(applicationUser?.CompanyId.GetValueOrDefault() == 0)
                {
                    //it is a regular customer account and we need to capture payment
                    // stripe LOGIC
                    string domain = "http://localhost:5217/";

                     
                    var options = new SessionCreateOptions
                    {
                        SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                        CancelUrl = domain + "Customer/Cart/Index",
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                    };

                    foreach(var item in ShoppingCartVM.ShoppingCartList)
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
                    var service = new SessionService();
                    Session session = service.Create(options);

                    _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();

                    //Redirect to Stripe for making payments
                    Response.Headers.Append("Location", session.Url);
                    return new StatusCodeResult(303);

                }

                
            }

            return RedirectToAction(nameof(OrderConfirmation), new {id=ShoppingCartVM?.OrderHeader.Id});    
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader? orderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if(orderFromDb != null)
            {
                if(orderFromDb.PaymentStatus != SD.PaymentStatusDelayedPayement)
                {
                    //this is the order by the customer
                    var service = new SessionService();
                    var session = service.Get(orderFromDb.SessionId);

                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        _unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                        _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                        _unitOfWork.Save();
                    }

                }

                List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderFromDb.ApplicationUserId).ToList();
                _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                _unitOfWork.Save();
            }

            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if(cartFromDb != null)
            {
                cartFromDb.Count++;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if(cartFromDb is not null)
            {
               if(cartFromDb.Count <= 1)
               {
                    _unitOfWork.ShoppingCart.Remove(cartFromDb);
                    HttpContext.Session.SetInt32(SD.SessionCart,
                        _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
               }
               else
               {
                    cartFromDb.Count--;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
               }

               _unitOfWork.Save(); 
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if(cartFromDb is not null)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);

                HttpContext.Session.SetInt32(SD.SessionCart, 
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

                _unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if(shoppingCart.Product != null)
            {
                if(shoppingCart.Count <= 50)
                {
                    return shoppingCart.Product.Price;
                }
                else if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
            else 
            {
                return 0.0;
            } 
            
        }

    }
}
