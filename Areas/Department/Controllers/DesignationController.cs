using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Medical.Models;
using Medical.BAL;
using Medical.Framework;

namespace Medical.Areas.Department.Controllers
{
    public class DesignationController : Controller
    {
        #region Global
        DesignationBAL DesignationBal;
        #endregion
        public ActionResult CreateDesignation()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateDesignation(DesignationModel model)
        {
            if (ModelState.ContainsKey("Id"))
                ModelState["Id"].Errors.Clear();

            if (ModelState.IsValid)
            {
                DesignationBal = new DesignationBAL();
                model.DepartmentId = Session.GetDataFromSession<UserModel>("User").DepartmentId  ;
                if(model.Id==0)
                    model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;
                else
                    model.LastModifiedUserId = Session.GetDataFromSession<UserModel>("User").CreatedUserId;
                DesignationComplexViewModel Designation = new DesignationComplexViewModel();
                List<DesignationModel> DesignationList = DesignationBal.SaveDesignation(model);
                NotificationModel Notification = new NotificationModel();
                if(DesignationList !=null && DesignationList.Count>0)
                {
                    Notification.Title = "Sucess";
                    Notification.NotificationType = NotificationType.Success;
                    Notification.NotificationMessage = model.Name +"Saved Successfully";
                }
                else
                {
                    Notification.Title = "Error";
                    Notification.NotificationType = NotificationType.Danger;
                    Notification.NotificationMessage = "Oops! something went wrong. Please contact helpdesk";
                }
                Designation.DesignationList = DesignationList;Designation.Notification = Notification;
                return Json(Designation);

            }
            else
            {
                return RedirectToAction("CreateDesignation");
            }
        }
        public JsonResult GetDesignations()
        {
            DesignationBal = new DesignationBAL();
            List<DesignationModel> List = DesignationBal.GetDesignationbyDeptId(Session.GetDataFromSession<UserModel>("User").DepartmentId);
            return Json(List, JsonRequestBehavior.AllowGet);
        }
    }
}