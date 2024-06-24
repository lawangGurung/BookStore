using Bulky.DataAccess;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        // GET: CategoryController
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> categoryList = _unitOfWork.Category.GetAll().ToList();
            return View(categoryList);
        }

        public IActionResult Create()
        {
            return View();
        }


        // this create method will actually post the object
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            // if(obj.Name == obj.DisplayOrder.ToString())
            // {
            //     ModelState.AddModelError("name", "The Display Order cannot exactly match the Name.");
            // }
            // if(obj.Name?.ToLower() == "test")
            // {
            //     ModelState.AddModelError("", "\'test\' is an invalid value");
            // }

            if(ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category Created Successfully.";
                return RedirectToAction("Index");
            }
            return View();    
        }

        // IndexView passed @obj.Id -> EditViewAction method -> selected Category object back to -> Edit view page ->
        // EdiViewAction method (post) -> Database.
        public IActionResult Edit(int? Id)
        {
            if(Id == null || Id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == Id);
            // Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u => u.Id == Id);
            // Category? categoryFromDb2 = _db.Categories.Where(u => u.Id == Id).FirstOrDefault();

            if(categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category Updated Successfully.";
                return RedirectToAction("Index");
            }

            return View();

        }

        public IActionResult Delete(int? Id)
        {
            if(Id == null || Id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == Id);
            // Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u => u.Id == Id);
            // Category? categoryFromDb2 = _db.Categories.Where(u => u.Id == Id).FirstOrDefault();

            if(categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {   
            Category? obj = _unitOfWork.Category.Get(u => u.Id == id);
            if(obj == null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category Deleted Successfully.";

            return RedirectToAction("Index");

        }

    }
}
