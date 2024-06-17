using Bulky.DataAccess;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyApp.Namespace
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        // GET: ProductController
        private readonly IUnitOfWork _unitOfWork;
        private IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
           _unitOfWork = unitOfWork; 
           _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            
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
                // this is to save the image and here "file" is the image
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images/products");

                    //this is for updating the image
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        // Delete the old image
                        string oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('/'));

                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"/images/products/" + fileName;
                }

                if(productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                    TempData["success"] = "Product created successfully";
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                    TempData["success"] = "Product updated successfully";
                }
                
                _unitOfWork.Save();
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


        // public IActionResult Delete(int? id)
        // {
        //     if (id == null || id == 0)
        //     {
        //         return NotFound();
        //     }

        //     Product? productFromDb = _unitOfWork.Product.Get(u => u.Id == id);

        //     if(productFromDb == null)
        //     {
        //         return NotFound();
        //     }

        //     return View(productFromDb);
        // }

        // [HttpPost]
        // public IActionResult Delete(Product obj)
        // {
        //     if(obj == null)
        //     {
        //         return NotFound();
        //     }

        //     _unitOfWork.Product.Remove(obj);
        //     _unitOfWork.Save();
        //     TempData["success"] = "Product removed successfully";
        //     return RedirectToAction("Index");
        // }

        #region API CALLS

            [HttpGet]
            public IActionResult GetAll()
            {
                List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
                return Json(new {data = objProductList});
            }


            [HttpDelete]
            public IActionResult Delete(int? id)
            {
                Product? productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);

                if (productToBeDeleted == null)
                {
                    return Json(new {success = false, message = "Error While Deleting"});
                }

            // image is also needed to be deleted before deleting the obj
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('/'));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _unitOfWork.Product.Remove(productToBeDeleted);
                _unitOfWork.Save();

                return Json(new {success = true, message = "Delete Successful"});
            }

        #endregion

    }
}
