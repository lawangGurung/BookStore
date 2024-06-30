using System.Runtime.CompilerServices;
using System.Security.Claims;
using Bulky.DataAccess;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        // GET: CartController
        private readonly IUnitOfWork _unitOfWork;
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
            };

            foreach (ShoppingCart cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            return View();
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
