using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Medical.Models;
using Medical.BAL;
using System.Data;
using Newtonsoft.Json;

namespace Medical.Areas.Department.Controllers
{
    public class InspectionController : Controller
    {
        // GET: Department/Inspection
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TESTCROR()
        {
            return View();
        }
        public ActionResult TestInspection()
        {
            return View();
        }

       public string GetDeptUserInspectionQuestions(int TransactionId, int DepartmentUserId, int FacilityId)
       {
            DepartmentUserBAL objBAL = new DepartmentUserBAL();
            DataSet ds = objBAL.GetDeptUserInspectionQuestions(TransactionId, DepartmentUserId,FacilityId);
            DataTable dt = ds.Tables[0];
            return JsonConvert.SerializeObject(ds);
        }
        public ActionResult InspectionList(List<InspectionModel> Inspection, int TransactionId)
         {
            Inspection.ForEach(item => { item.DepartmentUserId = 1; });
            DepartmentUserBAL objBAL = new DepartmentUserBAL();
            bool status= objBAL.SaveInspectionFacilitiesQuestions(Inspection, TransactionId);
           
            NotificationModel Notification = new NotificationModel();
           if(status==true)
            {
                Notification.Title = "Success";
                Notification.NotificationType = NotificationType.Success;
                Notification.NotificationMessage = "Saved Successfully";
               
            }
          
            else
            {
                Notification.Title = "Error";
                Notification.NotificationType = NotificationType.Danger;
                Notification.NotificationMessage = "Oops! something went wrong. Please contact helpdesk";
               
            }
            return Json(Notification);

        }



       
    }
}