using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Medical.BAL;
using Newtonsoft.Json;
using Medical.Framework;
using Medical.Models;
using System.Data;

namespace Medical.Areas.Department.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Department/Dashboard
        #region Global
        DashboardBAL objBAL;
        #endregion
        public ActionResult Dashboard()
        {
            return View();
        }
        public string BindCounts()
        {
            objBAL = new DashboardBAL();
            DataTable dt = objBAL.GetDeptUserDashboadCounts(Session.GetDataFromSession<UserModel>("User"));
            return JsonConvert.SerializeObject(objBAL.GetDeptUserDashboadCounts(Session.GetDataFromSession<UserModel>("User")));
        }
        public ActionResult DepartmentAdmin()
        {
            return View();
        }
    }
}