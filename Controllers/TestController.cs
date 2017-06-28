using Medical.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medical.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PostHtmlFormData()
        {
            return View(new UserModel());
        }

        [HttpPost]
        public ActionResult PostHtmlFormData(UserModel model)
        {
            return Content("hello");
        }
    }
}