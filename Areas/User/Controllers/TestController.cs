using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rotativa;
using System.IO;

namespace Medical.Areas.User.Controllers
{
    public class TestController : Controller
    {
        // GET: User/Test
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetUsers()
        {
            return View("Index");
        }
        public ActionResult GeneratePDF()
        {
            return new Rotativa.ActionAsPdf("Index") { FileName = "somename" };
        }
    }
}