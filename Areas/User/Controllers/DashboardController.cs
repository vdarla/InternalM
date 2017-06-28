using Medical.BAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Medical.Framework;
using Medical.Models;
using System.Data;
using Newtonsoft.Json;

namespace Medical.Areas.User.Controllers
{
    
    public class DashboardController : Controller
    {
        DashboardBAL dashboardBAL;

        // GET: User/Dashboard
        public ActionResult Index()
        {
            return View("Dashboard");
        }

        public string GetUserDashboard()
        {
            dashboardBAL = new DashboardBAL();
            int _userId = Session.GetDataFromSession<UserModel>("User").Id;
            DataSet ds = dashboardBAL.GetUserDashboard(_userId);
            return JsonConvert.SerializeObject(ds);
        }

        public ViewResult Drafts()
        {
            return View();
        }

        public string GetDraftApplications()
        {
            dashboardBAL = new DashboardBAL();
            int _userId = Session.GetDataFromSession<UserModel>("User").Id;
            DataTable dt = dashboardBAL.GetDraftApplications(_userId);
            return JsonConvert.SerializeObject(dt);
        }

        public ViewResult Submitted()
        {
            return View();
        }

        public string GetSubmittedApplications()
        {
            dashboardBAL = new DashboardBAL();
            int _userId = Session.GetDataFromSession<UserModel>("User").Id;
            DataTable dt = dashboardBAL.GetSubmittedApplications(_userId);
            return JsonConvert.SerializeObject(dt);
        }

        public ViewResult Licenses()
        {
            return View();
        }

        public string GetLicenses()
        {
            dashboardBAL = new DashboardBAL();
            int _userId = Session.GetDataFromSession<UserModel>("User").Id;
            DataTable dt = dashboardBAL.GetLicenses(_userId);
            return JsonConvert.SerializeObject(dt);
        }
    }
}