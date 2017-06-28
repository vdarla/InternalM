using Medical.BAL;
using Medical.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medical.Areas.Admin.Controllers
{
    public class DepartmentController : Controller
    {
        DepartmentBAL departmentBAL;
        // GET: Admin/Department
        public ActionResult Index()
        {
            return View();
        }

        #region Create
        public ViewResult Create()
        {
            DepartmentAdminViewModel model = new DepartmentAdminViewModel();
            departmentBAL = new DepartmentBAL();
            ViewBag.DistrictList = departmentBAL.GetDistrictList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(DepartmentAdminViewModel model)
        {
            
            if (ModelState.IsValid)
            {
                HttpPostedFileBase uploadedFile = Request.Files["Department.Logo"];
                departmentBAL = new DepartmentBAL();
                var path = Path.Combine(Server.MapPath("~/App_Data/"), uploadedFile.FileName);
                uploadedFile.SaveAs(path);
                model.Department.Logo = "~/App_Data/" + uploadedFile.FileName;

                return Json(departmentBAL.SaveDepartmentandAdmin(model));
            }
            else
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        var x = error;
                    }
                }

                RedirectToAction("Create");
                return null;
            }
        }

        public JsonResult GetMandals(int districtId)
        {
            departmentBAL = new DepartmentBAL();
            return Json(departmentBAL.GetMandalList(districtId));
            //List<MandalModel> MandalsList = departmentBAL.GetMandalList(districtId);
            //return Json(MandalsList);
        }

        public JsonResult GetVillages(int mandalId)
        {
            departmentBAL = new DepartmentBAL();
            return Json(departmentBAL.GetVillageList(mandalId));
        }
        #endregion

        #region Edit & Update
        public JsonResult Edit(int id)
        {
            List<DepartmentAdminViewModel> objList = GetDummyData();
            DepartmentAdminViewModel department = objList
                .Where(item => item.Department.Id == id).First();
            return Json(department, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public JsonResult GetDepartments()
        {
            departmentBAL = new DepartmentBAL();
            List<DepartmentAdminViewModel> objList = departmentBAL.GetDepartmentandAdmins();
            return Json(objList, JsonRequestBehavior.AllowGet);
        }

        private List<DepartmentAdminViewModel> GetDummyData()
        {
            List<DepartmentAdminViewModel> objList = new List<DepartmentAdminViewModel>();
            DepartmentAdminViewModel department = new DepartmentAdminViewModel();
            department.Department = new DepartmentViewModel();
            department.Department.Id = 1;
            department.Department.Name = "Medical Department";
            department.Department.DistrictName = "Yadhadhri";
            department.Department.MandalName = "Aler";
            department.Department.VillageName = "Kolanpak";

            department.DepartmentAdmin = new UserViewModel();
            department.DepartmentAdmin.FirstName = "Raj K";
            department.DepartmentAdmin.DesginationName = "Some Desgination";
            department.DepartmentAdmin.UserName = "Raj09";

            objList.Add(department);

            department = new DepartmentAdminViewModel();
            department.Department = new DepartmentViewModel();
            department.Department.Id = 2;
            department.Department.Name = "Hospital Department";
            department.Department.DistrictName = "Hyderabad";
            department.Department.MandalName = "Hyderabad";
            department.Department.VillageName = "Kachiguda";

            department.DepartmentAdmin = new UserViewModel();
            department.DepartmentAdmin.FirstName = "Hulk";
            department.DepartmentAdmin.DesginationName = "Angry Desgination";
            department.DepartmentAdmin.UserName = "Hulk777";

            objList.Add(department);
            return objList;
        }
    }
}