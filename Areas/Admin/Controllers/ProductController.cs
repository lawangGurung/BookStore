using Bulky.DataAccess;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyApp.Namespace
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        // GET: ProductController
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
           _unitOfWork = unitOfWork; 
        }
        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product.GetAll().ToList();
            
            return View(productList);
        }

        public IActionResult Upsert(int? id)
        {
            

            // just a key value pair where ViewBag.CategoryList is key and CategoryList is the value.
            // ViewBag.CategoryList = CategoryList;

            ProductVM productVM = new ProductVM() 
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };

            if(id == null || id == 0)
            {
                //Create
                return View(productVM);
            }
            else 
            {
                //Update
                Product? product = _unitOfWork.Product.Get(u => u.Id == id);

                if(product is not null)
                {
                    productVM.Product = product;
                }
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()

                });

                return View(productVM);
            }

        }


        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? productFromDb = _unitOfWork.Product.Get(u => u.Id == id);

            if(productFromDb == null)
            {
                return NotFound();
            }

            return View(productFromDb);
        }

        [HttpPost]
        public IActionResult Delete(Product obj)
        {
            if(obj == null)
            {
                return NotFound();
            }

            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Product removed successfully";
            return RedirectToAction("Index");
        }

    }
}
