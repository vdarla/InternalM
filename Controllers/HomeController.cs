using Medical.BAL;
using Medical.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medical.Controllers
{
    public class HomeController : Controller
    {
        #region Global
        MasterBAL masterBal;

        #endregion
        public ActionResult Index()
        {
            return View("Home");
        }
        public ActionResult Home()
        {
            return View();
        }
        public ActionResult Registration()
        {
            return View();
        }
        //POST: /Home/Registration
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Registration(UserModel objuser)
        {
            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult EODB()
        {
            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}