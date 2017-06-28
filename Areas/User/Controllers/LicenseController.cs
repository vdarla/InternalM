using Medical.BAL;
using Medical.Framework;
using Medical.Models;
//using RazorPDF;
//using Rotativa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medical.Areas.User.Controllers
{

    public class LicenseController : Controller
    {
        LicenseBAL objBAL;
        ApplicationBAL applicationBAL;
        PCPNDTBAL pcpndtBAL;
        APMCEBAL apmceBAL;
        // GET: User/License
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult Questionnaire()
        {
            return View(new LicenseQuestionnaireModel());
        }

        [HttpPost]
        public ActionResult Questionnaire(LicenseQuestionnaireModel model)
        {
            ClearData();

            objBAL = new LicenseBAL();
            ViewBag.DistrictList = objBAL.GetDistrictList();

            if (model.HasAppliedforAPMCE)
            {
                ViewBag.OfferedServices = objBAL.GetOfferedServices();
            }

            if (model.HasAppliedforPCPNDT)
            {
                List<OwnershipTypeModel> ownershipTypeList = new List<OwnershipTypeModel>();
                List<InstitutionTypeModel> institutionTypeList = new List<InstitutionTypeModel>();
                objBAL.GetOwnershipMasterData(ref ownershipTypeList, ref institutionTypeList);
                ViewBag.OwnershipTypeList = ownershipTypeList;
                ViewBag.InstitutionTypeList = institutionTypeList;
                ViewBag.FacilityMaster = objBAL.GetFacilityList();
                TempData["DoctorSpecialityList"] = objBAL.GetDoctorSpecialityList();
            }
            return View("ApplicationForm", model);
        }


        #region Applications
        public ViewResult ApplicationForm()
        {
            ClearData();

            objBAL = new LicenseBAL();
            ViewBag.DistrictList = objBAL.GetDistrictList();
            LicenseQuestionnaireModel model = new LicenseQuestionnaireModel();

            
            if (model.HasAppliedforPCPNDT)
            {
                List<OwnershipTypeModel> ownershipTypeList = new List<OwnershipTypeModel>();
                List<InstitutionTypeModel> institutionTypeList = new List<InstitutionTypeModel>();
                objBAL.GetOwnershipMasterData(ref ownershipTypeList, ref institutionTypeList);
                ViewBag.OwnershipTypeList = ownershipTypeList;
                ViewBag.InstitutionTypeList = institutionTypeList;
                ViewBag.FacilityMaster = objBAL.GetFacilityList();
                ViewBag.OfferedServices = objBAL.GetOfferedServices();
                TempData["DoctorSpecialityList"] = objBAL.GetDoctorSpecialityList();
            }
            return View(model);
        }
        public ViewResult ApplicationView(int id)
        {
            objBAL = new LicenseBAL();
            ApplicationModel model = objBAL.GetApplication(id);
            return View(model);
        }
        public ViewResult Edit(int id)
        {
            objBAL = new LicenseBAL();
            ViewBag.DistrictList = objBAL.GetDistrictList();

            LicenseQuestionnaireModel model = new LicenseQuestionnaireModel();

            model.ApplicationModel = objBAL.GetApplication(id);
            if (model.ApplicationModel.APMCEModel != null)
                model.HasAppliedforAPMCE = true;
            if (model.ApplicationModel.PCPNDTModel != null)
                model.HasAppliedforPCPNDT = true;

            Session["ApplicationId"] = id;
            if (model.ApplicationModel.APMCEModel != null)
            {
                ViewBag.OfferedServices = objBAL.GetOfferedServices();
                Session["APMCETransactionId"] = model.ApplicationModel.APMCEModel.TransactionId;

                // Set file paths
                if(model.ApplicationModel.APMCEModel.Accommadation != null && 
                    model.ApplicationModel.APMCEModel.Accommadation.UploadedFilePath != null)
                    Session["AccommodationFilePath"] = model.ApplicationModel.APMCEModel.Accommadation.UploadedFilePath;
            }
                
            if (model.ApplicationModel.PCPNDTModel != null)
            {
                List<OwnershipTypeModel> ownershipTypeList = new List<OwnershipTypeModel>();
                List<InstitutionTypeModel> institutionTypeList = new List<InstitutionTypeModel>();
                objBAL.GetOwnershipMasterData(ref ownershipTypeList, ref institutionTypeList);
                ViewBag.OwnershipTypeList = ownershipTypeList;
                ViewBag.InstitutionTypeList = institutionTypeList;
                ViewBag.FacilityMaster = objBAL.GetFacilityList();
                TempData["DoctorSpecialityList"] = objBAL.GetDoctorSpecialityList();

                Session["PCPNDTTransactionId"] = model.ApplicationModel.PCPNDTModel.TransactionId;
                Session["EquipmentsList"] = model.ApplicationModel.PCPNDTModel.EquipmentList;
                Session["EmployeeList"] = model.ApplicationModel.PCPNDTModel.EmployeeList;

                // Set file paths
                Session["AddressProofPath"] = model.ApplicationModel.PCPNDTModel.FacilityModel.AddressProofPath;
                Session["BuildingLayoutPath"] = model.ApplicationModel.PCPNDTModel.FacilityModel.BuildingLayoutPath;
                Session["AffidavitDocPath"] = model.ApplicationModel.PCPNDTModel.InstitutionModel.AffidavitDocPath;
                Session["ArticleFilePath"] = model.ApplicationModel.PCPNDTModel.InstitutionModel.ArticleDocPath;
                Session["StudyCertificates"] = model.ApplicationModel.PCPNDTModel.InstitutionModel.StudyCertificateDocPaths;                
            }

            return View("ApplicationForm", model);
        }
        public ActionResult Applications()
        {
            objBAL = new LicenseBAL();
            int _userId = Session.GetDataFromSession<UserModel>("User").Id;
            List<ApplicationModel> objList = objBAL.GetApplicationList(_userId);
            return View(objList);
        }
        public PartialViewResult PreviewApplication()
        {
            int _applicationId = Session.GetDataFromSession<int>("ApplicationId");
            objBAL = new LicenseBAL();
            ApplicationModel application = objBAL.GetApplication(_applicationId);
            return PartialView("_ApplicationPreview", application);
        }
        public JsonResult SubmitApplication()
        {
            int _applicationId = Session.GetDataFromSession<int>("ApplicationId");
            string _applicationNumber = string.Empty;

            applicationBAL = new ApplicationBAL();
            int result = applicationBAL.SubmitApplication(_applicationId, ref _applicationNumber);
            NotificationModel notificationModel = new NotificationModel();
            if (result > 0)
            {
                notificationModel.Title = "Success";
                notificationModel.NotificationType = NotificationType.Success;
                notificationModel.NotificationMessage = "Application has been sumbitted successfully"
                    + "<br>Your application number is <b>" + _applicationNumber + "</b>"
                    + "<br>Please note this for your future reference";
                notificationModel.ShowActionButton = true;
                notificationModel.ActionButtonClassType = PopupButtonClass.Success;
                notificationModel.ActionButtonText = "View Applications";
                notificationModel.ActionName = "Submitted";
                notificationModel.ControllerName = "Dashboard";
                notificationModel.AreaName = "User";
                ClearData();
            }
            else
            {
                notificationModel.Title = "Error";
                notificationModel.NotificationType = NotificationType.Danger;
                notificationModel.NotificationMessage = "Oops! something went wrong. Please contact helpdesk";
                notificationModel.ShowNonActionButton = true;
                notificationModel.NonActionButtonClassType = PopupButtonClass.Danger;
                notificationModel.NonActionButtonText = "Okay";
            }
            return Json(notificationModel);
        }
        public ActionResult Print()
        {


            objBAL = new LicenseBAL();
            ViewBag.DistrictList = objBAL.GetDistrictList();

            LicenseQuestionnaireModel model = new LicenseQuestionnaireModel();
            model.HasAppliedforPCPNDT = true;

            if (model.HasAppliedforPCPNDT)
            {
                List<OwnershipTypeModel> ownershipTypeList = new List<OwnershipTypeModel>();
                List<InstitutionTypeModel> institutionTypeList = new List<InstitutionTypeModel>();
                objBAL.GetOwnershipMasterData(ref ownershipTypeList, ref institutionTypeList);
                ViewBag.OwnershipTypeList = ownershipTypeList;
                ViewBag.InstitutionTypeList = institutionTypeList;
            }

            model.ApplicationModel = objBAL.GetApplication(175);
            //return new RazorPDF.PdfResult(model, "_ApplicationPreview");

            //var pdf = new PdfResult(model, "_ApplicationPreview");
            //pdf.ViewBag.Title = "Report Title";
            //return pdf;
            return null;
        }
        #endregion

        #region PCPNDT Form
        public JsonResult SaveApplicantDetails(ApplicantModel model)
        {
            NotificationModel notification = new NotificationModel();

            if (ModelState.IsValid)
            {
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");
                FormStatus formStatus = model.FormStatus;
                string _applicationNumber = string.Empty;

                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                PCPNDTBAL objPCPNDTBAL = new PCPNDTBAL();

                int result = objPCPNDTBAL.SaveApplicantDetails(model, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);
                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("PCPNDTTransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Applicant details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
                    notification.ReturnData = result.ToString() + "," + formStatus;
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

                return Json(notification);
            }
            else
            {
                // TODO: Return model validations       - Raj, 12-05-2017
                return Json(notification);
            }
        }
        //[HttpPost]
        //public ActionResult SaveEstablishmentDetails(EstablishmentModel model, HttpPostedFileBase AddressProof, HttpPostedFileBase BuildingLayout)
        //{
        //    NotificationModel notification = new NotificationModel();

        //    if (ModelState.IsValid)
        //    {
        //        PCPNDTBAL objPCPNDTBAL = new PCPNDTBAL();

        //        int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
        //        int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");
        //        FormStatus formStatus = model.FormStatus;
        //        string _applicationNumber = string.Empty;

        //        int _serviceId = 1;  // TODO: Set this value from m_service table        - Raj, 10-05-2017
        //        model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

        //        #region File Saving
        //        var uploadsPath = Path.Combine("Applicant", model.CreatedUserId.ToString()
        //            , _serviceId.ToString(), "Establishment");

        //        if (!Directory.Exists(Server.MapPath("~/Uploads/" + uploadsPath)))
        //            Directory.CreateDirectory(Server.MapPath("~/Uploads/" + uploadsPath));

        //        if (AddressProof != null)
        //        {
        //            string _addressProofPath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(AddressProof.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        //            model.AddressProofPath = _addressProofPath + Path.GetExtension(AddressProof.FileName);

        //            AddressProof.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _addressProofPath));
        //            string addressfilepath = Path.Combine(Server.MapPath("~/Uploads"), _addressProofPath);
        //            System.IO.File.Move(addressfilepath, addressfilepath + Path.GetExtension(AddressProof.FileName));
        //        }
        //        else if (Session["AddressProofPath"] != null)
        //        {
        //            model.AddressProofPath = Session.GetDataFromSession<string>("AddressProofPath");
        //        }

        //        if (BuildingLayout != null)
        //        {
        //            string _buildingLayoutPath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(BuildingLayout.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        //            model.BuildingLayoutPath = _buildingLayoutPath + Path.GetExtension(BuildingLayout.FileName);

        //            BuildingLayout.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _buildingLayoutPath));
        //            string layoutfilepath = Path.Combine(Server.MapPath("~/Uploads"), _buildingLayoutPath);
        //            System.IO.File.Move(layoutfilepath, layoutfilepath + Path.GetExtension(BuildingLayout.FileName));
        //        }
        //        else if (Session["BuildingLayoutPath"] != null)
        //        {
        //            model.AddressProofPath = Session.GetDataFromSession<string>("BuildingLayoutPath");
        //        }
        //        #endregion

        //        int result = objPCPNDTBAL.SaveEstablishmentDetails(model, ref _applicationId, ref _transactionId,
        //            ref formStatus, ref _applicationNumber);

        //        if (result > 0)
        //        {
        //            Session.SetDataToSession<int>("ApplicationId", _applicationId);
        //            Session.SetDataToSession<int>("PCPNDTTransactionId", _transactionId);

        //            notification.Title = "Success";
        //            notification.NotificationType = NotificationType.Success;
        //            notification.NotificationMessage = "Establishment details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
        //            notification.ShowNonActionButton = true;
        //            notification.NonActionButtonClassType = PopupButtonClass.Success;
        //            notification.NonActionButtonText = "Okay";
        //            notification.ReturnData = result.ToString() + "," + formStatus;
        //        }
        //        else
        //        {
        //            notification.Title = "Error";
        //            notification.NotificationType = NotificationType.Danger;
        //            notification.NotificationMessage = "Something went wrong! Please contact Help desk";
        //            notification.ShowNonActionButton = true;
        //            notification.NonActionButtonClassType = PopupButtonClass.Danger;
        //            notification.NonActionButtonText = "Okay";
        //            notification.ReturnData = "0," + FormStatus.Empty;
        //        }

        //        return Json(notification);
        //    }
        //    else
        //    {
        //        // TODO: Return model validations       - Raj, 10-05-2017
        //        return Json(notification);
        //    }
        //}
        [HttpPost]
        public ActionResult SaveFacilityDetails(FacilityModel model, HttpPostedFileBase AddressProof, HttpPostedFileBase BuildingLayout)
        {
            NotificationModel notification = new NotificationModel();

            if (ModelState.IsValid)
            {
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");
                FormStatus formStatus = model.FormStatus;
                string _applicationNumber = string.Empty;
                int _serviceId = 1;  // TODO: Set this value from m_service table        - Raj, 10-05-2017
                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                #region File Saving
                var uploadsPath = Path.Combine("Applicant", model.CreatedUserId.ToString()
                    , _serviceId.ToString(), "FacilityDetails");

                if (!Directory.Exists(Server.MapPath("~/Uploads/" + uploadsPath)))
                    Directory.CreateDirectory(Server.MapPath("~/Uploads/" + uploadsPath));

                if (AddressProof != null)
                {
                    string _addressProofPath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(AddressProof.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    model.AddressProofPath = _addressProofPath + Path.GetExtension(AddressProof.FileName);

                    AddressProof.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _addressProofPath));
                    string addressfilepath = Path.Combine(Server.MapPath("~/Uploads"), _addressProofPath);
                    System.IO.File.Move(addressfilepath, addressfilepath + Path.GetExtension(AddressProof.FileName));
                }
                else if (Session["AddressProofPath"] != null)
                {
                    model.AddressProofPath = Session.GetDataFromSession<string>("AddressProofPath");
                }

                if (BuildingLayout != null)
                {
                    string _buildingLayoutPath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(BuildingLayout.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    model.BuildingLayoutPath = _buildingLayoutPath + Path.GetExtension(BuildingLayout.FileName);

                    BuildingLayout.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _buildingLayoutPath));
                    string layoutfilepath = Path.Combine(Server.MapPath("~/Uploads"), _buildingLayoutPath);
                    System.IO.File.Move(layoutfilepath, layoutfilepath + Path.GetExtension(BuildingLayout.FileName));
                }
                else if (Session["BuildingLayoutPath"] != null)
                {
                    model.AddressProofPath = Session.GetDataFromSession<string>("BuildingLayoutPath");
                }
                #endregion

                PCPNDTBAL objPCPNDTBAL = new PCPNDTBAL();
                int result = objPCPNDTBAL.SaveFacilityDetails(model, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);
                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("PCPNDTTransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Facility details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
                    notification.ReturnData = result.ToString() + "," + formStatus;
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

                return Json(notification);
            }
            else
            {
                // TODO: Return model validations       - Raj, 12-05-2017
                return Json(notification);
            }
        }
        public JsonResult SaveTests(TestsModel model)
        {
            NotificationModel notification = new NotificationModel();

            if (ModelState.IsValid)
            {
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");
                FormStatus formStatus = model.FormStatus;
                string _applicationNumber = string.Empty;

                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                PCPNDTBAL objPCPNDTBAL = new PCPNDTBAL();

                int result = objPCPNDTBAL.SaveTests(model, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);
                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("PCPNDTTransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Test details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
                    notification.ReturnData = result.ToString() + "," + formStatus;
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

                return Json(notification);
            }
            else
            {
                // TODO: Return model validations       - Raj, 12-05-2017
                return Json(notification);
            }
        }
        [HttpPost]
        public ActionResult AddEquipment(EquipmentModel model)
        {
            if (ModelState.IsValid)
            {
                HttpPostedFileBase _uploadedFile = Request.Files[0];

                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");

                int _serviceId = 1;  // TODO: Set this value from m_service table        - Raj, 10-05-2017
                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;


                var uploadsPath = Path.Combine("Applicant", model.CreatedUserId.ToString()
                    , _serviceId.ToString(), "Equipment");

                string _uploadedFilePath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(_uploadedFile.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                model.UploadedFilePath = _uploadedFilePath + Path.GetExtension(_uploadedFile.FileName);

                if (!Directory.Exists(Server.MapPath("~/Uploads/" + uploadsPath)))
                    Directory.CreateDirectory(Server.MapPath("~/Uploads/" + uploadsPath));

                #region Saving files physically
                _uploadedFile.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _uploadedFilePath));

                string oldfilepath = Path.Combine(Server.MapPath("~/Uploads"), _uploadedFilePath);
                System.IO.File.Move(oldfilepath, oldfilepath + Path.GetExtension(_uploadedFile.FileName));
                #endregion

                List<EquipmentModel> objEquipmentsList;
                if (Session["EquipmentsList"] != null)
                    objEquipmentsList = Session.GetDataFromSession<List<EquipmentModel>>("EquipmentsList");
                //TempData["EquipmentsList"] as List<EquipmentModel>;
                else
                    objEquipmentsList = new List<EquipmentModel>();
                objEquipmentsList.Add(model);
                Session.SetDataToSession<List<EquipmentModel>>("EquipmentsList", objEquipmentsList);
                //TempData["EquipmentsList"] = objEquipmentsList;

                return Json(objEquipmentsList);
            }
            else
            {
                return Json("Invalid data");
            }

        }
        public JsonResult DeleteEquipment(int index)
        {
            if (Session["EquipmentsList"] != null)
            {
                List<EquipmentModel> objEquipmentsList = Session.GetDataFromSession<List<EquipmentModel>>("EquipmentsList");
                //TempData["EquipmentsList"] as List<EquipmentModel>;
                if (objEquipmentsList[index].Id == 0)
                    objEquipmentsList.RemoveAt(index);
                else
                    objEquipmentsList[index].IsDeleted = true;
                Session.SetDataToSession<List<EquipmentModel>>("EquipmentsList", objEquipmentsList);
                //TempData["EquipmentsList"] = objEquipmentsList;
                return Json(objEquipmentsList.Where(item => item.IsDeleted == false).ToList());
            }
            return Json(null);
        }
        public ActionResult SaveEquipments()
        {
            NotificationModel notification = new NotificationModel();
            if (Session["EquipmentsList"] != null)
            {
                List<EquipmentModel> objEquipmentsList = Session.GetDataFromSession<List<EquipmentModel>>("EquipmentsList");
                //TempData.Peek("EquipmentsList") as List<EquipmentModel>;
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");
                FormStatus formStatus = FormStatus.Empty;
                string _applicationNumber = string.Empty;

                int _userId = Session.GetDataFromSession<UserModel>("User").Id;
                objEquipmentsList
                    .ForEach(e =>
                    {
                        e.CreatedUserId = _userId;
                    });


                PCPNDTBAL objPCPNDTBAL = new PCPNDTBAL();
                int result = objPCPNDTBAL.SaveEquipments(objEquipmentsList, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);

                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("PCPNDTTransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Equipment details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
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
                notification.NotificationMessage = "Please clear error validations";
                notification.ShowNonActionButton = true;
                notification.NonActionButtonClassType = PopupButtonClass.Warning;
                notification.NonActionButtonText = "Okay";
            }
            return Json(notification);
        }
        public JsonResult GetEquipments(int transactionId)
        {
            pcpndtBAL = new PCPNDTBAL();
            List<EquipmentModel> equipmentList = pcpndtBAL.GetEquipments(transactionId);
            Session.SetDataToSession<List<EquipmentModel>>("EquipmentsList", equipmentList);
            return Json(equipmentList);
        }
        public JsonResult SaveFacilities(FacilitesModel model)
        {
            NotificationModel notification = new NotificationModel();

            if (ModelState.IsValid)
            {
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");
                FormStatus formStatus = model.FormStatus;
                string _applicationNumber = string.Empty;

                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                PCPNDTBAL objPCPNDTBAL = new PCPNDTBAL();
                int result = objPCPNDTBAL.SaveFacilities(model, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);
                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("PCPNDTTransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Facilities details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
                    notification.ReturnData = result.ToString() + "," + formStatus;
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

                return Json(notification);
            }
            else
            {
                // TODO: Return model validations       - Raj, 12-05-2017
                return Json(notification);
            }
        }
        [HttpPost]
        public JsonResult AddEmployee(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                HttpPostedFileBase _uploadedFile = Request.Files[0];

                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");

                int _serviceId = 1;  // TODO: Set this value from m_service table        - Raj, 10-05-2017
                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;


                var uploadsPath = Path.Combine("Applicant", model.CreatedUserId.ToString()
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
            pcpndtBAL = new PCPNDTBAL();
            List<EmployeeViewModel> employeeList = pcpndtBAL.GetEmployees(transactionId);
            Session.SetDataToSession<List<EmployeeViewModel>>("EmployeeList", employeeList);
            return Json(employeeList);
        }
        public JsonResult SaveInstitution(InstitutionModel model, HttpPostedFileBase affidavitFile,
            HttpPostedFileBase articleFile, HttpPostedFileBase[] studyCertificateFiles)
        {
            NotificationModel notification = new NotificationModel();

            int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
            int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");
            FormStatus formStatus = model.FormStatus;
            string _applicationNumber = string.Empty;
            int _serviceId = 1;  // TODO: Set this value from m_service table        - Raj, 06-06-2017
            model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

            #region File Saving
            var uploadsPath = Path.Combine("Applicant", model.CreatedUserId.ToString()
                , _serviceId.ToString(), "InstitutionDetails");

            if (!Directory.Exists(Server.MapPath("~/Uploads/" + uploadsPath)))
                Directory.CreateDirectory(Server.MapPath("~/Uploads/" + uploadsPath));

            if (model.OwnershipTypeId > 0)
            {
                // Saving Affidavit Doc
                if (affidavitFile != null)
                {
                    string _affidavitDocPath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(affidavitFile.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    model.AffidavitDocPath = _affidavitDocPath + Path.GetExtension(affidavitFile.FileName);

                    affidavitFile.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _affidavitDocPath));
                    string affidavitfilepath = Path.Combine(Server.MapPath("~/Uploads"), _affidavitDocPath);
                    System.IO.File.Move(affidavitfilepath, affidavitfilepath + Path.GetExtension(affidavitFile.FileName));
                }
                else if (Session["AffidavitDocPath"] != null)
                {
                    model.AffidavitDocPath = Session.GetDataFromSession<string>("AffidavitDocPath");
                }

                // Saving Study Certificates
                if (studyCertificateFiles != null && studyCertificateFiles.Length > 0)
                {
                    model.StudyCertificateDocPaths = new List<DocumentUploadModel>();
                    DocumentUploadModel docModel;
                    foreach (var file in studyCertificateFiles)
                    {
                        string _certificateDocPath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(file.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                        docModel = new DocumentUploadModel();
                        docModel.ReferenceTable = "t_institution";
                        docModel.DocumentPath = _certificateDocPath + Path.GetExtension(file.FileName);
                        docModel.UploadedUserId = Session.GetDataFromSession<UserModel>("User").Id;
                        model.StudyCertificateDocPaths.Add(docModel);

                        file.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _certificateDocPath));
                        string certificatefilepath = Path.Combine(Server.MapPath("~/Uploads"), _certificateDocPath);
                        System.IO.File.Move(certificatefilepath, certificatefilepath + Path.GetExtension(file.FileName));
                    }

                    // Check for any Study Certificates in Session, if so, add them to model
                    if(Session["StudyCertificates"] != null)
                        model.StudyCertificateDocPaths.AddRange(Session.GetDataFromSession<List<DocumentUploadModel>>("StudyCertificates"));
                }
                else
                {
                    model.StudyCertificateDocPaths = Session.GetDataFromSession<List<DocumentUploadModel>>("StudyCertificates");
                }

            }

            if (model.OwnershipTypeId == 2 || model.OwnershipTypeId == 3 || model.OwnershipTypeId == 4 ||
                model.OwnershipTypeId == 5)
            {
                // Saving Article Doc
                if (articleFile != null)
                {
                    string _articleDocPath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(articleFile.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    model.ArticleDocPath = _articleDocPath + Path.GetExtension(articleFile.FileName);

                    articleFile.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _articleDocPath));
                    string articlefilepath = Path.Combine(Server.MapPath("~/Uploads"), _articleDocPath);
                    System.IO.File.Move(articlefilepath, articlefilepath + Path.GetExtension(articleFile.FileName));
                }
                else if (Session["ArticleFilePath"] != null)
                {
                    model.ArticleDocPath = Session.GetDataFromSession<string>("ArticleFilePath");
                }
            }

            #endregion



            PCPNDTBAL objPCPNDTBAL = new PCPNDTBAL();
            int result = objPCPNDTBAL.SaveInstitutionDetails(model, ref _applicationId, ref _transactionId,
                ref formStatus, ref _applicationNumber);
            if (result > 0)
            {
                Session.SetDataToSession<int>("ApplicationId", _applicationId);
                Session.SetDataToSession<int>("PCPNDTTransactionId", _transactionId);

                notification.Title = "Success";
                notification.NotificationType = NotificationType.Success;
                notification.NotificationMessage = "Institution details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
                notification.ShowNonActionButton = true;
                notification.NonActionButtonClassType = PopupButtonClass.Success;
                notification.NonActionButtonText = "Okay";
                notification.ReturnData = result.ToString() + "," + formStatus;
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

            return Json(notification);

        }
        public JsonResult SaveDeclarationDetails(DeclarationModel model)
        {
            NotificationModel notification = new NotificationModel();

            if (ModelState.IsValid)
            {
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["PCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("PCPNDTTransactionId");
                FormStatus formStatus = model.FormStatus;
                string _applicationNumber = string.Empty;

                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                PCPNDTBAL objPCPNDTBAL = new PCPNDTBAL();

                int result = objPCPNDTBAL.SaveDeclarationDetails(model, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);
                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("PCPNDTTransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Declaration details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
                    notification.ReturnData = result.ToString() + "," + formStatus;
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

                return Json(notification);
            }
            else
            {
                // TODO: Return model validations       - Raj, 20-05-2017
                return Json(notification);
            }
        }
        #endregion

        #region APMCE Form
        public JsonResult SaveRegistrationDetails(RegistrationModel model)
        {
            NotificationModel notification = new NotificationModel();
            if (ModelState.IsValid)
            {
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["APMCETransactionId"] == null ? 0 : Session.GetDataFromSession<int>("APMCETransactionId");
                FormStatus formStatus = model.FormStatus;
                string _applicationNumber = string.Empty;

                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                APMCEBAL objAPMCEBAL = new APMCEBAL();
                int result = objAPMCEBAL.SaveRegistrationDetails(model, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);
                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("APMCETransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Registration details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
                    notification.ReturnData = result.ToString() + "," + formStatus;
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

                return Json(notification);
            }
            return null;
        }
        public JsonResult SaveCorrespondingAddressDetails(CorrespondingAddressModel model)
        {
            NotificationModel notification = new NotificationModel();
            if (ModelState.IsValid)
            {
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["APMCETransactionId"] == null ? 0 : Session.GetDataFromSession<int>("APMCETransactionId");
                FormStatus formStatus = model.FormStatus;
                string _applicationNumber = string.Empty;

                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                APMCEBAL objAPMCEBAL = new APMCEBAL();

                int result = objAPMCEBAL.SaveCorrespondingAddressDetails(model, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);

                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("APMCETransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Corresponding details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
                    notification.ReturnData = result.ToString() + "," + formStatus;
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

                return Json(notification);
            }
            else
            {
                // TODO: Return model validations       - Raj, 31-05-2017
                return Json(notification);
            }
        }
        public JsonResult SaveTrustDetails(TrustModel model)
        {
            NotificationModel notification = new NotificationModel();
            if (ModelState.IsValid)
            {
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["APMCETransactionId"] == null ? 0 : Session.GetDataFromSession<int>("APMCETransactionId");
                FormStatus formStatus = model.FormStatus;
                string _applicationNumber = string.Empty;

                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                APMCEBAL objAPMCEBAL = new APMCEBAL();
                int result = objAPMCEBAL.SaveTrustDetails(model, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);
                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("APMCETransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Trust details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
                    notification.ReturnData = result.ToString() + "," + formStatus;
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

                return Json(notification);
            }
            return null;
        }
        public JsonResult SaveAccommodationDetails(AccommodationModel model, HttpPostedFileBase uploadedFile)
        {
            NotificationModel notification = new NotificationModel();

            if (ModelState.IsValid)
            {
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["APMCETransactionId"] == null ? 0 : Session.GetDataFromSession<int>("APMCETransactionId");
                FormStatus formStatus = model.FormStatus;
                string _applicationNumber = string.Empty;
                int _serviceId = 1;  // TODO: Set this value from m_service table        - Raj, 10-05-2017
                model.UserId = Session.GetDataFromSession<UserModel>("User").Id;

                #region File Saving
                var uploadsPath = Path.Combine("Applicant", model.UserId.ToString()
                    , _serviceId.ToString(), "AccommodationDetails");

                if (!Directory.Exists(Server.MapPath("~/Uploads/" + uploadsPath)))
                    Directory.CreateDirectory(Server.MapPath("~/Uploads/" + uploadsPath));

                if (uploadedFile != null)
                {
                    string _uploadedfilePath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(uploadedFile.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    model.UploadedFilePath = _uploadedfilePath + Path.GetExtension(uploadedFile.FileName);

                    uploadedFile.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _uploadedfilePath));
                    string uploadedfilepath = Path.Combine(Server.MapPath("~/Uploads"), _uploadedfilePath);
                    System.IO.File.Move(uploadedfilepath, uploadedfilepath + Path.GetExtension(uploadedFile.FileName));
                }
                else if (Session["AddressProofPath"] != null)
                {
                    model.UploadedFilePath = Session.GetDataFromSession<string>("AccommodationFilePath");
                }

                #endregion

                APMCEBAL objAPMCEBAL = new APMCEBAL();
                int result = objAPMCEBAL.SaveAccommodationDetails(model, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);
                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("APMCETransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Accommodation details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
                    notification.ReturnData = result.ToString() + "," + formStatus;
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

                return Json(notification);
            }
            else
            {
                // TODO: Return model validations       - Raj, 22-06-2017
                return Json(notification);
            }
        }        
        public JsonResult AddInfraStructure(InfraStructureModel model)
        {
            if (ModelState.IsValid)
            {
                HttpPostedFileBase _uploadedFile = Request.Files[0];

                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["APMCETransactionId"] == null ? 0 : Session.GetDataFromSession<int>("APMCETransactionId");

                int _serviceId = 1;  // TODO: Set this value from m_service table        - Raj, 01-06-2017
                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                #region Saving files physically
                var uploadsPath = Path.Combine("Applicant", model.CreatedUserId.ToString()
                    , _serviceId.ToString(), "InfraStructure");

                string _uploadedFilePath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(_uploadedFile.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                model.UploadedFilePath = _uploadedFilePath + Path.GetExtension(_uploadedFile.FileName);

                if (!Directory.Exists(Server.MapPath("~/Uploads/" + uploadsPath)))
                    Directory.CreateDirectory(Server.MapPath("~/Uploads/" + uploadsPath));

                _uploadedFile.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _uploadedFilePath));

                string oldfilepath = Path.Combine(Server.MapPath("~/Uploads"), _uploadedFilePath);
                System.IO.File.Move(oldfilepath, oldfilepath + Path.GetExtension(_uploadedFile.FileName));
                #endregion

                List<InfraStructureModel> objInfraStructureList;
                if (Session["InfraStructureList"] != null)
                    objInfraStructureList = Session.GetDataFromSession<List<InfraStructureModel>>("InfraStructureList");
                else
                    objInfraStructureList = new List<InfraStructureModel>();
                objInfraStructureList.Add(model);
                Session.SetDataToSession<List<InfraStructureModel>>("InfraStructureList", objInfraStructureList);

                return Json(objInfraStructureList);
            }
            else
            {
                return Json("Invalid data");
            }
        }
        public JsonResult DeleteInfraStructure(int index)
        {
            if (Session["InfraStructureList"] != null)
            {
                List<InfraStructureModel> objInfraStructureList = Session.GetDataFromSession<List<InfraStructureModel>>("InfraStructureList");
                if (objInfraStructureList[index].Id == 0)
                    objInfraStructureList.RemoveAt(index);
                else
                    objInfraStructureList[index].IsDeleted = true;
                Session.SetDataToSession<List<InfraStructureModel>>("InfraStructureList", objInfraStructureList);
                //TempData["EmployeeList"] = objEmployeeList;
                return Json(objInfraStructureList.Where(item => item.IsDeleted == false).ToList());
            }
            return Json(null);
        }
        public JsonResult SaveInfraStructures()
        {
            NotificationModel notification = new NotificationModel();
            if (Session["InfraStructureList"] != null)
            {
                List<InfraStructureModel> objInfraStructureList = Session.GetDataFromSession<List<InfraStructureModel>>("InfraStructureList");
                int _applicationId = Session["ApplicationId"] == null ? 0 : Session.GetDataFromSession<int>("ApplicationId");
                int _transactionId = Session["APMCETransactionId"] == null ? 0 : Session.GetDataFromSession<int>("APMCETransactionId");
                FormStatus formStatus = FormStatus.Empty;
                string _applicationNumber = string.Empty;

                int _userId = Session.GetDataFromSession<UserModel>("User").Id;
                objInfraStructureList
                    .ForEach(e =>
                    {
                        e.CreatedUserId = _userId;
                    });


                APMCEBAL objAPMCEBAL = new APMCEBAL();
                int result = objAPMCEBAL.SaveInfraStructure(objInfraStructureList, ref _applicationId, ref _transactionId,
                    ref formStatus, ref _applicationNumber);

                if (result > 0)
                {
                    Session.SetDataToSession<int>("ApplicationId", _applicationId);
                    Session.SetDataToSession<int>("APMCETransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Equipments & Furniture details saved.<br>Your application is <b>" + _applicationNumber + "</b>";
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
        public JsonResult GetInfraStructures(int transactionId)
        {
            apmceBAL = new APMCEBAL();
            List<InfraStructureModel> infraStructureList = apmceBAL.GetInfraStructures(transactionId);
            Session.SetDataToSession<List<EquipmentModel>>("InfraStructureList", infraStructureList);
            return Json(infraStructureList);
        }
        #endregion

        #region Query & Response

        public ViewResult Queries()
        {
            return View();
        }

        public JsonResult GetQueryData()
        {
            objBAL = new LicenseBAL();
            int userId = Session.GetDataFromSession<UserModel>("User").Id;
            QueryResponseViewModel model = objBAL.GetQueryResponseData(userId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRaisedQueryData(int id)
        {
            objBAL = new LicenseBAL();
            QueryModel model = objBAL.GetRaisedQueryData(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SubmitResponse(string response, int queryId)
        {
            QueryModel model = new QueryModel();
            model.Description = response;
            model.UserId = Session.GetDataFromSession<UserModel>("User").Id;
            model.Type = "Response";
            model.QueryId = queryId;

            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase _uploadedFile = Request.Files[0];
                var uploadsPath = Path.Combine("Applicant", model.UserId.ToString()
                    , "Responses");

                if (!Directory.Exists(Server.MapPath("~/Uploads/" + uploadsPath)))
                    Directory.CreateDirectory(Server.MapPath("~/Uploads/" + uploadsPath));

                string _filePath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(_uploadedFile.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                _uploadedFile.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _filePath));
                model.UploadedFilePath = _filePath + Path.GetExtension(_uploadedFile.FileName);

                string _uploadedfilepath = Path.Combine(Server.MapPath("~/Uploads"), _filePath);
                System.IO.File.Move(_uploadedfilepath, _uploadedfilepath + Path.GetExtension(_uploadedFile.FileName));
            }

            objBAL = new LicenseBAL();
            bool result = objBAL.SubmitResponse(model);
            NotificationModel notification = new NotificationModel();
            if (result)
            {
                notification.Title = "Success";
                notification.NotificationType = NotificationType.Success;
                notification.NotificationMessage = "Your response has been submitted successfully";
                notification.ShowNonActionButton = true;
                notification.NonActionButtonClassType = PopupButtonClass.Success;
                notification.NonActionButtonText = "Close";
            }
            else
            {
                notification.Title = "Error";
                notification.NotificationType = NotificationType.Danger;
                notification.NotificationMessage = "Oops! Something went wrong... <br>Your response was not submitted, please contact technical support.";
                notification.ShowNonActionButton = true;
                notification.NonActionButtonClassType = PopupButtonClass.Danger;
                notification.NonActionButtonText = "Close";
            }
            return Json(notification);
        }

        #endregion

        #region Master Data
        public JsonResult GetMandals(int id)
        {
            objBAL = new LicenseBAL();
            return Json(objBAL.GetMandalList(id));
        }

        public JsonResult GetVillages(int id)
        {
            objBAL = new LicenseBAL();
            return Json(objBAL.GetVillageList(id));
        }
        #endregion

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

        public PartialViewResult GetPCPNDTLicense(int transactionId)
        {
            objBAL = new LicenseBAL();
            PCPNDTLicenseInfoModel model = objBAL.GetPCPNDTLicenseDetails(transactionId);
            return PartialView("_PCPNDTLicense", model);
        }

    }
}