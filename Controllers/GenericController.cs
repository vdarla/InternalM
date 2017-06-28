using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medical.Controllers
{
    public class GenericController : Controller
    {
        // GET: Generic
        public ActionResult Index()
        {
            return View();
        }

        public FileResult DownloadFile(string path, string downloadName)
        {
            return File(Server.MapPath("~/Uploads/" + path), "multipart/form-data", downloadName);
        }

        
    }
}