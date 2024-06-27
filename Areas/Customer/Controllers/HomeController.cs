using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Bulky.Models;
using Bulky.DataAccess;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BulkyWeb.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
        return View(productList);
    }

    public IActionResult Details(int ProductId)
    {
        ShoppingCart cart = new ShoppingCart() {
            ProductId = ProductId,
            Product = _unitOfWork.Product.Get(u => u.Id == ProductId, includeProperties: "Category"),
            Count = 1
        };
        return View(cart);
    }
    [HttpPost]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        var claimsIdentity = (ClaimsIdentity?) User.Identity;
        var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        shoppingCart.ApplicationUserId = userId;


        //we have to find if the shopping cart exist or not 
        ShoppingCart? cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId
        && u.ProductId == shoppingCart.ProductId);

        if (cartFromDb != null)
        {
            //shopping cart exist
            cartFromDb.Count += shoppingCart.Count;
            // _unitOfWork.ShoppingCart.Update(cartFromDb);
        }
        else
        {
            //shopping cart does not exist
            _unitOfWork.ShoppingCart.Add(shoppingCart);
        }
        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
