using Medical.BAL;
using Medical.Framework;
using Medical.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medical.Areas.User.Controllers
{
  
    public class ApplicationController : Controller
    {
        LicenseBAL licenseBAL;
        PCPNDTBAL pcpndtBAL;
        ApplicationBAL applicationBAL;
        // GET: User/Application
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult GetAcknowledge(int applicationId)
        {
            applicationBAL = new ApplicationBAL();
            AcknowledgeModel model = applicationBAL.GetAcknowledgeDetails(applicationId);            
            return PartialView("_Acknowledgement", model);
        }
        public void ClearData()
        {
            Session["ApplicationId"] = null;
            Session["PCPNDTTransactionId"] = null;
            Session["EquipmentsList"] = null;
            Session["EmployeeList"] = null;
            Session["APMCETransactionId"] = null;
            Session["AddressProofPath"] = null;
            Session["BuildingLayoutPath"] = null;
        }

        public JsonResult DeleteStudyCertificate(int id)
        {
            var studyCertificates = Session.GetDataFromSession<List<DocumentUploadModel>>("StudyCertificates");
            studyCertificates.Where(item => item.Id == id).First().IsDeleted = true;
            studyCertificates.Where(item => item.Id == id).First().
                LastModifiedUserId = Session.GetDataFromSession<UserModel>("User").Id;
            Session.SetDataToSession<List<DocumentUploadModel>>("StudyCertificates", studyCertificates);
            
            return Json(studyCertificates.Where(item => item.IsDeleted == false).ToList());
        }
    }
}