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
    public class CompanyController : Controller
    {
        // GET: CompanyController
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
           _unitOfWork = unitOfWork; 
        }
        public IActionResult Index()
        {
            List<Company> CompanyList = _unitOfWork.Company.GetAll().ToList();
            
            return View(CompanyList);
        }

        public IActionResult Upsert(int? id)
        {
            

            // just a key value pair where ViewBag.CategoryList is key and CategoryList is the value.
            // ViewBag.CategoryList = CategoryList;

            

            if(id == null || id == 0)
            {
                //Create
                return View(new Company());
            }
            else 
            {
                //Update
                Company? Company = _unitOfWork.Company.Get(u => u.Id == id);

                return View(Company); 
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {
            if (ModelState.IsValid)
            {
                // this is to save the image and here "file" is the image
                

                if(companyObj.Id == 0)
                {
                    _unitOfWork.Company.Add(companyObj);
                    TempData["success"] = "Company created successfully";
                }
                else
                {
                    _unitOfWork.Company.Update(companyObj);
                    TempData["success"] = "Company updated successfully";
                }
                
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                return View(companyObj);
            }

        }


        // public IActionResult Delete(int? id)
        // {
        //     if (id == null || id == 0)
        //     {
        //         return NotFound();
        //     }

        //     Company? CompanyFromDb = _unitOfWork.Company.Get(u => u.Id == id);

        //     if(CompanyFromDb == null)
        //     {
        //         return NotFound();
        //     }

        //     return View(CompanyFromDb);
        // }

        // [HttpPost]
        // public IActionResult Delete(Company obj)
        // {
        //     if(obj == null)
        //     {
        //         return NotFound();
        //     }

        //     _unitOfWork.Company.Remove(obj);
        //     _unitOfWork.Save();
        //     TempData["success"] = "Company removed successfully";
        //     return RedirectToAction("Index");
        // }

        #region API CALLS

            [HttpGet]
            public IActionResult GetAll()
            {
                List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
                return Json(new {data = objCompanyList});
            }


            [HttpDelete]
            public IActionResult Delete(int? id)
            {
                Company? CompanyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);

                if (CompanyToBeDeleted == null)
                {
                    return Json(new {success = false, message = "Error While Deleting"});
                }

                _unitOfWork.Company.Remove(CompanyToBeDeleted);
                _unitOfWork.Save();

                return Json(new {success = true, message = "Delete Successful"});
            }

        #endregion

    }
}
