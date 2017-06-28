using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Medical.BAL;
using Medical.Models;
using System.IO;
using Medical.Framework;

namespace Medical.Areas.Department.Controllers
{

    public class DepartmentUserController : Controller
    {
        // GET: Department/DepartmentUser
        DepartmentUserBAL objBAL;
        public ActionResult ListofApplications()
        {
            objBAL = new DepartmentUserBAL();
            UserModel user = Session.GetDataFromSession<UserModel>("User");
            List<TransactionViewModel> ApplicationList = objBAL.GetListofApplications(user.DesignationId, user.DistrictId, user.MandalId, user.VillageId);
            return View(ApplicationList);
        }
        //public JsonResult GetApplications()
        //{
        //    objBAL = new DepartmentUserBAL();
        //    UserModel user = Session.GetDataFromSession<UserModel>("User");
        //    return Json(objBAL.GetListofApplications(user.DesignationId,user.DistrictId, user.MandalId,user.VillageId), JsonRequestBehavior.AllowGet);//TODO : change this after user managment
        //}
        public ActionResult Approval(int TransactionId, int ServiceId)
        {
            //Clear sessions
            Session["UploadList"] = null;
            objBAL = new DepartmentUserBAL();           
            LicenseBAL LBAL = new LicenseBAL();
            UserModel user = Session.GetDataFromSession<UserModel>("User");
            ApprovalComplexViewModel Approval = objBAL.ApprovalSceenOnloadData(TransactionId, user.DesignationId, ServiceId); // //TODO : change transactionid,designationid (get this login user session)            
            Approval.Approval = new ApprovalsModel();
            Approval.Approval.TransactionId = TransactionId;
            Approval.ServiceId = ServiceId;
            if (ServiceId == 2) //PCPNDT
                Approval.PCPNDTModel = LBAL.GetPCPNDTData(TransactionId);
            else if (ServiceId == 1)
                Approval.APMCEModel = LBAL.GetAPMCEData(TransactionId);

            return View(Approval); ;
            //HACK NEED TO CHECK ONCE      
        }
        [HttpPost]
        public JsonResult Approval(ApprovalsModel approval, string Submit, List<InspectionModel> InspectionList) //
        {
            
            objBAL = new DepartmentUserBAL();           

            UserModel user = Session.GetDataFromSession<UserModel>("User");
            if (Submit == "Forward")
                approval.status = Status.Forward;
            else if (Submit == "Return")
                approval.status = Status.Return;
            else if (Submit == "Approve")
                approval.status = Status.Approv;
            else if (Submit == "Reject")
                approval.status = Status.Reject;
            approval.UserId = user.Id;
            List<DocumentUploadModel> UploadList = new List<DocumentUploadModel>();
            if (Session["UploadList"] != null)
                UploadList = Session["UploadList"] as List<DocumentUploadModel>;
            bool Result = objBAL.SaveApprovals(approval, user.DesignationId,InspectionList,UploadList);
            NotificationModel Notification = new NotificationModel();
            if (Result)
            {
                Notification.Title = "Success";
                Notification.NotificationType = NotificationType.Success;
                Notification.NotificationMessage = "Application " + approval.status + "ed Successfully";
                Notification.ShowActionButton = true;
                Notification.ActionButtonText = "Goto List";
                Notification.ActionName = "ListofApplications";
                Notification.ControllerName = "DepartmentUser";
                Notification.AreaName = "Department";
            }
            else
            {
                Notification.Title = "Error";
                Notification.NotificationType = NotificationType.Danger;
                Notification.NotificationMessage = "Oops! something went wrong. Please contact helpdesk";
            }
            return Json(Notification);
        }
        public JsonResult SubmitQuery(string Query, int TransactionId,string ApplicationType)
        {
            UserModel user = Session.GetDataFromSession<UserModel>("User");
            LicenseBAL objLicenseBal = new LicenseBAL();
            QueryModel QueryModel = new QueryModel();
            QueryModel.Description = Query;
            QueryModel.TransactionId = TransactionId;
            QueryModel.DepartmentId = user.DepartmentId;
            QueryModel.UserId = user.Id;
            QueryModel.Type = "Query";
            QueryModel.ApplicationType = ApplicationType;
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase _uploadedFile = Request.Files[0];
                var uploadsPath = Path.Combine("Department", "9", "QueryUploads"); //TODO Change DesignationID  Mounika -23-05-2017

                if (!Directory.Exists(Server.MapPath("~/Uploads/" + uploadsPath)))
                    Directory.CreateDirectory(Server.MapPath("~/Uploads/" + uploadsPath));

                string _filePath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(_uploadedFile.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                _uploadedFile.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _filePath));
                QueryModel.UploadedFilePath = _filePath + Path.GetExtension(_uploadedFile.FileName);

                string _uploadedfilepath = Path.Combine(Server.MapPath("~/Uploads"), _filePath);
                System.IO.File.Move(_uploadedfilepath, _uploadedfilepath + Path.GetExtension(_uploadedFile.FileName));
            }


            bool result = objLicenseBal.SubmitResponse(QueryModel);
            NotificationModel notification = new NotificationModel();
            if (result)
            {
                notification.Title = "Success";
                notification.NotificationType = NotificationType.Success;
                notification.NotificationMessage = "Your Query has been submitted successfully";
                notification.ShowNonActionButton = true;
                notification.NonActionButtonClassType = PopupButtonClass.Success;
                notification.NonActionButtonText = "Close";
            }
            else
            {
                notification.Title = "Error";
                notification.NotificationType = NotificationType.Danger;
                notification.NotificationMessage = "Oops! Something went wrong... <br>Your Query was not submitted, please contact technical support.";
                notification.ShowNonActionButton = true;
                notification.NonActionButtonClassType = PopupButtonClass.Danger;
                notification.NonActionButtonText = "Close";
            }
            return Json(notification);

        }
        public JsonResult GetQureyResponsebyTransactionId(int TransactionId)
        {
            objBAL = new DepartmentUserBAL();
            return Json(objBAL.GetQureyResponsebyTransactionId(TransactionId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddDocuments(DocumentUploadModel Upload)
        {
            HttpPostedFileBase _uploadedFile = Request.Files[0];
            var uploadsPath = Path.Combine("Deparment", "InspectionReports");

            string _uploadedFilePath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(_uploadedFile.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            Upload.DocumentPath = _uploadedFilePath + Path.GetExtension(_uploadedFile.FileName);

            if (!Directory.Exists(Server.MapPath("~/Uploads/" + uploadsPath)))
                Directory.CreateDirectory(Server.MapPath("~/Uploads/" + uploadsPath));
            #region Saving files physically
            _uploadedFile.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _uploadedFilePath));

            string oldfilepath = Path.Combine(Server.MapPath("~/Uploads"), _uploadedFilePath);
            System.IO.File.Move(oldfilepath, oldfilepath + Path.GetExtension(_uploadedFile.FileName));
            #endregion
            List<DocumentUploadModel> UploadList = new List<DocumentUploadModel>();
            if (Session["UploadList"]!=null)
                UploadList= Session["UploadList"] as List<DocumentUploadModel>;
            Upload.UploadedUserId = Session.GetDataFromSession<UserModel>("User").Id;
            UploadList.Add(Upload);
            Session["UploadList"] = UploadList;
            return Json(UploadList);
        }
        

        #region Amendment Approval 
        public ActionResult ListofAmendment()
        {
            objBAL = new DepartmentUserBAL();
            UserModel user = Session.GetDataFromSession<UserModel>("User");
            List<TransactionViewModel> ApplicationList = objBAL.GetListofAmendments(user.DesignationId, user.DistrictId, user.MandalId, user.VillageId);
            return View(ApplicationList);
        }
        public ActionResult AmendmentApproval(int TId, int SId, int AId, int TSId) // AId= AmendmentId,TId=TransactionId,SId=ServiceId
        {
            objBAL = new DepartmentUserBAL();
            LicenseBAL LBAL = new LicenseBAL();
            UserModel user = Session.GetDataFromSession<UserModel>("User");
            ApprovalComplexViewModel Approval = objBAL.AmendmentApprovalOnloadData(AId, user.DesignationId, SId);
            Approval.Approval = new ApprovalsModel();
            Approval.Approval.TransactionId = TId;
            Approval.Approval.AmendmentId = AId;
            Approval.ServiceId = SId;
            Approval.TranServiceId = TSId;
            if (TSId == 2) //PCPNDT
                Approval.PCPNDTModel = LBAL.GetPCPNDTData(TId);
            else if (TSId == 1)
                Approval.APMCEModel = LBAL.GetAPMCEData(TId);
            switch (SId)
            {
                case 18: //Add/delete Facility
                    Approval.PCPNDTModel.FacilityLogModel = objBAL.GetFacility(AId);
                    Approval.PCPNDTModel.TestsModelLog = objBAL.GetPCPNDTTests(AId, "InDirect");
                    Approval.PCPNDTModel.FacilitiesModellog = objBAL.GetFacilitiesforTests(AId, "InDirect");
                    break;
                case 21: //PCPNDT Lists of Tests/Procedures
                    Approval.PCPNDTModel.TestsModelLog = objBAL.GetPCPNDTTests(AId, "Direct");
                    break;
                case 23: //PCPNDT Facilities available
                    Approval.PCPNDTModel.FacilitiesModellog = objBAL.GetFacilitiesforTests(AId, "Direct");
                    break;
                case 22://PCPNDT Equipment details
                    Approval.PCPNDTModel.EquipmentListLog = objBAL.GetEquipments(AId);
                    break;
                case 24: // PCPNDT Employee Details
                    Approval.PCPNDTModel.EmployeeListLog = objBAL.GetEmployees(AId);
                    break;
                case 19://PCPNDT Type of Ownership
                    Approval.PCPNDTModel.InstitutionModelLog = objBAL.GetOwnership(AId);
                    break;
                case 20://  PCPNDT Type of Institution
                    Approval.PCPNDTModel.InstitutionModelLog = objBAL.GetInstitution(AId);
                    break;
                case 28: //PCPNDT Cancellation of License
                    Approval.PCPNDTModel.cancelLiceseModel = objBAL.GetCancelLicenseDetails(TId);
                    break;


            }
            return View(Approval); ;
        }
        [HttpPost]
        public JsonResult AmendmentApproval(ApprovalsModel approval, string Submit)
        {
            objBAL = new DepartmentUserBAL();
            UserModel user = Session.GetDataFromSession<UserModel>("User");
            if (Submit == "Forward")
                approval.status = Status.Forward;
            else if (Submit == "Return")
                approval.status = Status.Return;
            else if (Submit == "Approve")
                approval.status = Status.Approv;
            else if (Submit == "Reject")
                approval.status = Status.Reject;
            approval.UserId = user.Id;
            bool Result = objBAL.SaveAmedmentApprovals(approval, user.DesignationId);
            NotificationModel Notification = new NotificationModel();
            if (Result)
            {
                Notification.Title = "Success";
                Notification.NotificationType = NotificationType.Success;
                Notification.NotificationMessage = "Application " + approval.status + "ed Successfully";
                Notification.ShowActionButton = true;
                Notification.ActionButtonText = "Goto List";
                Notification.ActionName = "ListofAmendment";
                Notification.ControllerName = "DepartmentUser";
                Notification.AreaName = "Department";
            }
            else
            {
                Notification.Title = "Error";
                Notification.NotificationType = NotificationType.Danger;
                Notification.NotificationMessage = "Oops! something went wrong. Please contact helpdesk";
            }
            return Json(Notification);
        }
        #endregion


    }
}