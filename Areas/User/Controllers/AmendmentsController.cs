using Medical.BAL;
using Medical.Framework;
using Medical.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medical.Areas.User.Controllers
{
    public class AmendmentsController : Controller
    {
        // GET: User/Amendments
        public ActionResult Index()
        {
            return View();
        }
        // GET: User/AmendmentQuestionnaire

        public ActionResult AmendmentQuestionnaire()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AmendmentQuestionnaire(AmendmentModel model)
        {
            int SID = 2;
            ViewBag.serviceID = SID;
            int TransectionId = 190;
            return RedirectToAction("Amendment", model); 
        }
        public ActionResult Amendment(PCPNDTViewModel model) 
        {
            int SID = 2;
            LicenseBAL objLicenseBAL = new LicenseBAL();
            ViewBag.DistrictList = objLicenseBAL.GetDistrictList();
            int TransectionId = 190; 
            AmendmentBAL objBAL = new AmendmentBAL();
            if(SID==1)
            {

            }
            if(SID==2)
            {
                List<OwnershipTypeModel> ownershipTypeList = new List<OwnershipTypeModel>();
                List<InstitutionTypeModel> institutionTypeList = new List<InstitutionTypeModel>();
                objLicenseBAL.GetOwnershipMasterData(ref ownershipTypeList, ref institutionTypeList);
                ViewBag.OwnershipTypeList = ownershipTypeList;
                ViewBag.InstitutionTypeList = institutionTypeList;
                ViewBag.FacilityMaster = objLicenseBAL.GetFacilityList();
                TempData["DoctorSpecialityList"] = objLicenseBAL.GetDoctorSpecialityList();
                model = objBAL.GetPCPNDTData(TransectionId, model);
                if (model.HasAppliedforEmployeeDetails)
                {
                    ViewBag.EmployeeTab = "true";
                    model = objBAL.GetEmployeeDetails(TransectionId, model);
                }
                if (model.HasAppliedforFacilityAddress)
                {
                    ViewBag.FacilityTab = "true";
                    // model = objBAL.GetCorrespondentDetails(TransectionId, model);
                    //  model = objBAL.GetTrustDetails(TransectionId, model);
                    //  model = objBAL.GetEquipmentDetails(TransectionId, model);

                    model = objBAL.GetFacilityDetails(TransectionId, model);
                  //  model = objBAL.GetInstitutionDetails(TransectionId, model);
                  //  model = objBAL.GetEquipmentDetails(TransectionId, model);
                   
                  //  model = objBAL.GetFacilitiesDetails(TransectionId, model);
                    // ViewBag.FacilityMaster = objLicenseBAL.GetFacilityList();
                   
                }
            }
           

            //if (model.HasAppliedforEmployeeDetails)
            //{
            //    TempData["DoctorSpecialityList"] = objLicenseBAL.GetDoctorSpecialityList();
            //}
            return View("Amendment", model);
        }
        public JsonResult AddEmployee(EmployeeViewModel model)
        {
           
           if (ModelState.IsValid)
            {
                HttpPostedFileBase _uploadedFile = Request.Files[0];

                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");

                int _serviceId = 1;  // TODO: Set this value from m_service table        
                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;


                var uploadsPath = Path.Combine("Employee", model.CreatedUserId.ToString()
                    , _serviceId.ToString(), "Employee");

                string _uploadedFilePath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(_uploadedFile.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                model.UploadedFilePath = _uploadedFilePath + Path.GetExtension(_uploadedFile.FileName);

                if (!Directory.Exists(Server.MapPath("~/Uploads/" + uploadsPath)))
                    Directory.CreateDirectory(Server.MapPath("~/Uploads/" + uploadsPath));

                #region Saving files physically
                _uploadedFile.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _uploadedFilePath));

                string oldfilepath = Path.Combine(Server.MapPath("~/Uploads"), _uploadedFilePath);
                System.IO.File.Move(oldfilepath, oldfilepath + Path.GetExtension(_uploadedFile.FileName));
                #endregion

                List<EmployeeViewModel> objEmployeeList;
                if (Session["EmployeeList"] != null)
                    objEmployeeList = Session.GetDataFromSession<List<EmployeeViewModel>>("EmployeeList");
                // TempData["EmployeeList"] as List<EmployeeViewModel>;
                else
                    objEmployeeList = new List<EmployeeViewModel>();
                objEmployeeList.Add(model);
                Session.SetDataToSession<List<EmployeeViewModel>>("EmployeeList", objEmployeeList);
                //TempData["EmployeeList"] = objEmployeeList;

                return Json(objEmployeeList);
            }
            else
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
                return Json("Invalid data");
            }
        }
        public JsonResult DeleteEmployee(int index)
        {
            if (Session["EmployeeList"] != null)
            {
                List<EmployeeViewModel> objEmployeeList = Session.GetDataFromSession<List<EmployeeViewModel>>("EmployeeList");
                if (objEmployeeList[index].Id == 0)
                    objEmployeeList.RemoveAt(index);
                else
                    objEmployeeList[index].IsDeleted = true;
                Session.SetDataToSession<List<EmployeeViewModel>>("EmployeeList", objEmployeeList);
                //TempData["EmployeeList"] = objEmployeeList;
                return Json(objEmployeeList.Where(item => item.IsDeleted == false).ToList());
            }
            return Json(null);
        }
        public ActionResult SaveEmployees()
        {
            NotificationModel notification = new NotificationModel();
            if (Session["EmployeeList"] != null)
            {
                List<EmployeeViewModel> objEmployeeList = Session.GetDataFromSession<List<EmployeeViewModel>>("EmployeeList");
                //TempData.Peek("EmployeeList") as List<EmployeeViewModel>;
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");
                FormStatus formStatus = FormStatus.Empty;
                string _applicationNumber = string.Empty;

                int _userId = Session.GetDataFromSession<UserModel>("User").Id;
                objEmployeeList
                    .ForEach(e =>
                    {
                        e.CreatedUserId = _userId;
                    });


                PCPNDTBAL objPCPNDTBAL = new PCPNDTBAL();
                int result = objPCPNDTBAL.SaveEmployees(objEmployeeList, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);

                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("PCPNDTTransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Employee details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
                    notification.ReturnData = _transactionId + "," + formStatus;
                }
                else
                {
                    notification.Title = "Error";
                    notification.NotificationType = NotificationType.Danger;
                    notification.NotificationMessage = "Something went wrong! Please contact Help desk";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Danger;
                    notification.NonActionButtonText = "Okay";
                    notification.ReturnData = "0," + FormStatus.Empty;
                }
            }
            else
            {
                // TODO: Implement this neatly      - Raj, 19-05-2017
                notification.Title = "Warning";
                notification.NotificationType = NotificationType.Warning;
                notification.NotificationMessage = "Please clear error validations";
                notification.ShowNonActionButton = true;
                notification.NonActionButtonClassType = PopupButtonClass.Warning;
                notification.NonActionButtonText = "Okay";
            }
            return Json(notification);
        }
        public JsonResult GetEmployees(int transactionId)
        {
            PCPNDTBAL pcpndtBAL = new PCPNDTBAL();
            List<EmployeeViewModel> employeeList = pcpndtBAL.GetEmployees(transactionId);
            Session.SetDataToSession<List<EmployeeViewModel>>("EmployeeList", employeeList);
            return Json(employeeList);
        }
       

    }
}