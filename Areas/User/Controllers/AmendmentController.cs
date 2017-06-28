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
    public class AmendmentController : Controller
    {
        // GET: User/Amendments
        public ActionResult Index()
        {
            return View();
        }
        // GET: User/AmendmentQuestionnaire

        public ActionResult Questionnaire(int id, int serviceId)
        {
            AmendmentModel model = new AmendmentModel();
            model.TransactionId = id;
            if (serviceId == 1) // APMCE Amendments
                model.APMCEAmendments = new APMCEAmendmentModel();
            else if(serviceId == 2)  // PCPNDT Amendments
                model.PCPNDTAmendments = new PCPNDTAmendmentModel();
            return View(model);
        }
        //[HttpPost]
        //public ActionResult Questionnaire(AmendmentModel model)
        //{
        //    int SID = 2;
        //    ViewBag.serviceID = SID;
        //    TempData["AmendmentsModel"] = model;
        //    return RedirectToAction("Amendment"); 
        //}

        #region Get PCPNDT,APMCE Amendment Data 

        [HttpPost]
        public ActionResult Amendment(AmendmentModel model)
        {
            if(model.PCPNDTAmendments != null)
            {
                AmendmentBAL oblamendmentBAL=new AmendmentBAL();
                LicenseBAL objBAL = new LicenseBAL();
                List<OwnershipTypeModel> ownershipTypeList = new List<OwnershipTypeModel>();
                List<InstitutionTypeModel> institutionTypeList = new List<InstitutionTypeModel>();
                objBAL.GetOwnershipMasterData(ref ownershipTypeList, ref institutionTypeList);
                ViewBag.OwnershipTypeList = ownershipTypeList;
                ViewBag.InstitutionTypeList = institutionTypeList;
                ViewBag.FacilityMaster = objBAL.GetFacilityList();
                TempData["DoctorSpecialityList"] = objBAL.GetDoctorSpecialityList();
                model.PCPNDTAmendments.PCPNDTModel = oblamendmentBAL.GetPCPNDTData(model.TransactionId);
                if (model.PCPNDTAmendments.PCPNDTModel != null)
                {
                    Session["amtPCPNDTTransactionId"] = model.PCPNDTAmendments.PCPNDTModel.TransactionId;
                    Session["amtEquipmentsList"] = model.PCPNDTAmendments.PCPNDTModel.EquipmentList;
                    Session["amtEmployeeList"] = model.PCPNDTAmendments.PCPNDTModel.EmployeeList;

                    // Set file paths
                    Session["amtAddressProofPath"] = model.PCPNDTAmendments.PCPNDTModel.FacilityModel.AddressProofPath;
                    Session["amtBuildingLayoutPath"] = model.PCPNDTAmendments.PCPNDTModel.FacilityModel.BuildingLayoutPath;
                    Session["amtAffidavitDocPath"] = model.PCPNDTAmendments.PCPNDTModel.InstitutionModel.AffidavitDocPath;
                    Session["amtArticleFilePath"] = model.PCPNDTAmendments.PCPNDTModel.InstitutionModel.ArticleDocPath;
                    Session["amtStudyCertificates"] = model.PCPNDTAmendments.PCPNDTModel.InstitutionModel.StudyCertificateDocPaths;
                }

            }

          if( model.PCPNDTAmendments.InstitutionAmendment == true)
            {
                Session["InstitutionAmendment"] = 20;  //Institution Amendment  Used By Saving    --kishore 21-06-17
            }
            if (model.PCPNDTAmendments.OwnershipTypeAmendment == true)
            {
                Session["OwnershipTypeAmendment"] = 19;  //OwnershipType Amendment  Used By Saving    --kishore 21-06-17
            }
           
            return View(model);
        }

        #endregion
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

        #region  Facility Details Saving for PCPNDT Amendment
        public ActionResult SaveFacilityAmendment(FacilityViewModel model, HttpPostedFileBase AddressProof, HttpPostedFileBase BuildingLayout)
        {
            NotificationModel notification = new NotificationModel();
            try
            {

                int _transactionId = Session["amtPCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("amtPCPNDTTransactionId");
               // int _serviceId = 18; 
                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                #region File Saving
                var uploadsPath = Path.Combine("Applicant", "FacilityDetails");


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
                else if (Session["amtAddressProofPath"] != null)
                {
                    model.AddressProofPath = Session.GetDataFromSession<string>("amtAddressProofPath");
                }

                if (BuildingLayout != null)
                {
                    string _buildingLayoutPath = uploadsPath + "/" + Path.GetFileNameWithoutExtension(BuildingLayout.FileName) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    model.BuildingLayoutPath = _buildingLayoutPath + Path.GetExtension(BuildingLayout.FileName);

                    BuildingLayout.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), _buildingLayoutPath));
                    string layoutfilepath = Path.Combine(Server.MapPath("~/Uploads"), _buildingLayoutPath);
                    System.IO.File.Move(layoutfilepath, layoutfilepath + Path.GetExtension(BuildingLayout.FileName));
                }
                else if (Session["amtBuildingLayoutPath"] != null)
                {
                    model.AddressProofPath = Session.GetDataFromSession<string>("amtBuildingLayoutPath");
                }
                #endregion

                AmendmentBAL objPCPNDTBAL = new AmendmentBAL();
                int result = objPCPNDTBAL.SaveFacilityAmendment(model, ref _transactionId);

                if (result > 0)
                {

                    Session.SetDataToSession<int>("amtPCPNDTTransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Facility details saved.";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";

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
            catch
            {
                // TODO: Return model validations       - kishore, 15-06-2017
                return Json(notification);
            }
        }

        #endregion

        #region  Employee Details  Saving for PCPNDT Amendment
        public JsonResult AddEmployeeAmendment(EmployeeViewModel model)
        {

            if (ModelState.IsValid)
            {
                HttpPostedFileBase _uploadedFile = Request.Files[0];
                int _transactionId = Session["amtPCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("amtPCPNDTTransactionId");
                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;


                var uploadsPath = Path.Combine("Applicant", "Employee");


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
                if (Session["amtEmployeeList"] != null)
                    objEmployeeList = Session.GetDataFromSession<List<EmployeeViewModel>>("amtEmployeeList");
                // TempData["EmployeeList"] as List<EmployeeViewModel>;
                else
                    objEmployeeList = new List<EmployeeViewModel>();
                objEmployeeList.Add(model);
                Session.SetDataToSession<List<EmployeeViewModel>>("amtEmployeeList", objEmployeeList);
                //TempData["EmployeeList"] = objEmployeeList;

                return Json(objEmployeeList);
            }
            else
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
                return Json("Invalid data");
            }
        }
        public JsonResult DeleteEmployeeAmendment(int index)   //using this one
        {
            if (Session["amtEmployeeList"] != null)
            {
                List<EmployeeViewModel> objEmployeeList = Session.GetDataFromSession<List<EmployeeViewModel>>("amtEmployeeList");
                if (objEmployeeList[index].Id == 0)
                    objEmployeeList.RemoveAt(index);
                else
                    objEmployeeList[index].IsDeleted = true;
                Session.SetDataToSession<List<EmployeeViewModel>>("amtEmployeeList", objEmployeeList);
                return Json(objEmployeeList);
            }
            return Json(null);
        }
        public ActionResult SaveEmployeeDetails(EmployeeViewModel model) // using this one
        {
            NotificationModel notification = new NotificationModel();
            if (Session["amtEmployeeList"] != null)
            {
                List<EmployeeViewModel> objEmployeeList = Session.GetDataFromSession<List<EmployeeViewModel>>("amtEmployeeList");
                int _transactionId = Session["amtPCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("amtPCPNDTTransactionId");
                int _userId = Session.GetDataFromSession<UserModel>("User").Id;
                objEmployeeList
                    .ForEach(e =>
                    {
                        e.CreatedUserId = _userId;
                    });
                AmendmentBAL objBAL = new AmendmentBAL();
                int result = objBAL.SaveEmployeeDetails(objEmployeeList, _transactionId, _userId);

                if (result > 0)
                {
                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = " Employee details saved.";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
                    // notification.ReturnData = result.ToString() + "," + formStatus;
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

            return Json(notification);
        }
        public JsonResult GetEmployees(int transactionId)
        {
            AmendmentBAL objBAL = new AmendmentBAL();
            List<EmployeeViewModel> employeeList = objBAL.GetEmployees(transactionId);
            Session.SetDataToSession<List<EmployeeViewModel>>("EmployeeList", employeeList);
            return Json(employeeList);
        }
        #endregion 

        #region  Equipment  Details Saving for PCPNDT Amendment
        [HttpPost]
        public ActionResult AddEquipment(EquipmentModel model)
        {
            if (ModelState.IsValid)
            {
                HttpPostedFileBase _uploadedFile = Request.Files[0];


                int _transactionId = Session["amtPCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("amtPCPNDTTransactionId");
                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;
                var uploadsPath = Path.Combine("Applicant", "Equipment");


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
                if (Session["amtEquipmentsList"] != null)
                    objEquipmentsList = Session.GetDataFromSession<List<EquipmentModel>>("amtEquipmentsList");
                //TempData["EquipmentsList"] as List<EquipmentModel>;
                else
                    objEquipmentsList = new List<EquipmentModel>();
                objEquipmentsList.Add(model);
                Session.SetDataToSession<List<EquipmentModel>>("amtEquipmentsList", objEquipmentsList);
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
            if (Session["amtEquipmentsList"] != null)
            {
                List<EquipmentModel> objEquipmentsList = Session.GetDataFromSession<List<EquipmentModel>>("amtEquipmentsList");
                //TempData["EquipmentsList"] as List<EquipmentModel>;
                if (objEquipmentsList[index].Id == 0)
                    objEquipmentsList.RemoveAt(index);
                else
                    objEquipmentsList[index].IsDeleted = true;
                Session.SetDataToSession<List<EquipmentModel>>("amtEquipmentsList", objEquipmentsList);
                return Json(objEquipmentsList);
            }
            return Json(null);
        }
        public ActionResult SaveEquipments()
        {
            NotificationModel notification = new NotificationModel();
            if (Session["amtEquipmentsList"] != null)
            {
                int _transactionId = Session["amtPCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("amtPCPNDTTransactionId");

                List<EquipmentModel> objEquipmentsList = Session.GetDataFromSession<List<EquipmentModel>>("amtEquipmentsList");
                int _userId = Session.GetDataFromSession<UserModel>("User").Id;
                objEquipmentsList
                    .ForEach(e =>
                    {
                        e.CreatedUserId = _userId;
                    });
                AmendmentBAL objbal = new AmendmentBAL();
                int result = objbal.SaveEquipments(objEquipmentsList, _transactionId, _userId);

                if (result > 0)
                {
                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Equipment details saved.";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";

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
            AmendmentBAL objBAL = new AmendmentBAL();
            List<EquipmentModel> equipmentList = objBAL.GetEquipments(transactionId);
            Session.SetDataToSession<List<EquipmentModel>>("EquipmentsList", equipmentList);
            return Json(equipmentList);
        }

        #endregion

        #region  Institution Details Saving for PCPNDT Amendment
        public JsonResult SaveInstitutionAmendment(InstitutionModel model, HttpPostedFileBase affidavitFile,
             HttpPostedFileBase articleFile, HttpPostedFileBase[] studyCertificateFiles)
        {

            NotificationModel notification = new NotificationModel();
            int _transactionId = Session["amtPCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("amtPCPNDTTransactionId");
            model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;
          
          
            AmendmentBAL objamendmentbal = new AmendmentBAL();
         
            if (Session["InstitutionAmendment"]!=null)
            {
              //  int institutionid = (int)Session["InstitutionAmendment"];
                int result = objamendmentbal.SaveInstitutionDetails(model, ref _transactionId);
                if (result > 0)
                {

                    Session.SetDataToSession<int>("amtPCPNDTTransactionId", _transactionId);
                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Your Amendment is saved Successfully.";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
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

                //clear Seassion Data 
                Session["InstitutionAmendment"] = null;
               
              // Json(notification);
            }
            if (Session["OwnershipTypeAmendment"] != null)
            {

                #region File Saving

                var uploadsPath = Path.Combine("Applicant", "OwnershipDetails");
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
                    else if (Session["amtAffidavitDocPath"] != null)
                    {
                        model.AffidavitDocPath = Session.GetDataFromSession<string>("amtAffidavitDocPath");
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
                        if (Session["amtStudyCertificates"] != null)
                            model.StudyCertificateDocPaths.AddRange(Session.GetDataFromSession<List<DocumentUploadModel>>("amtStudyCertificates"));
                    }
                    else
                    {
                        model.StudyCertificateDocPaths = Session.GetDataFromSession<List<DocumentUploadModel>>("amtStudyCertificates");
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
                        //System.IO.File.Move(articlefilepath, articlefilepath + Path.GetExtension(articleFile.FileName));
                    }
                    else if (Session["amtArticleFilePath"] != null)
                    {
                        model.ArticleDocPath = Session.GetDataFromSession<string>("amtArticleFilePath");
                    }
                }

                #endregion
                int result = objamendmentbal.SaveOwnershipDetails(model, ref _transactionId);
                if (result > 0)
                {

                    Session.SetDataToSession<int>("amtPCPNDTTransactionId", _transactionId);
                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Your Amendment is saved Successfully.";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";
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
                //return Json(notification);
            }
            //clear Seassion Data 
           
            Session["OwnershipTypeAmendment"] = null;
            return Json(notification);

        }
        public JsonResult DeleteStudyCertificate(int id)
        {
            var studyCertificates = Session.GetDataFromSession<List<DocumentUploadModel>>("amtStudyCertificates");
            studyCertificates.Where(item => item.Id == id).First().IsDeleted = true;
            studyCertificates.Where(item => item.Id == id).First().
                LastModifiedUserId = Session.GetDataFromSession<UserModel>("User").Id;
            Session.SetDataToSession<List<DocumentUploadModel>>("amtStudyCertificates", studyCertificates);

            return Json(studyCertificates.Where(item => item.IsDeleted == false).ToList());
        }
        #endregion
        

        #region Tests Details Saving for PCPNDT Amendment
        public JsonResult SaveTestsAmendment(TestsModel model)
        {
            NotificationModel notification = new NotificationModel();

            if (ModelState.IsValid)
            {

                int _transactionId = Session["amtPCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("amtPCPNDTTransactionId");
                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                AmendmentBAL objAmendmentBAL = new AmendmentBAL();

                int result = objAmendmentBAL.SaveTestsAmendment(model, ref _transactionId);

                if (result > 0)
                {

                    Session.SetDataToSession<int>("amtPCPNDTTransactionId", _transactionId);

                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Test details saved.";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";

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
        #endregion

        #region Facilities Details Saving for PCPNDT Amendment 
        public JsonResult SaveFacilitiesamendment(FacilitesModel model)
        {
            NotificationModel notification = new NotificationModel();

            if (ModelState.IsValid)
            {

                int _transactionId = Session["amtPCPNDTTransactionId"] == null ? 0 : Session.GetDataFromSession<int>("amtPCPNDTTransactionId");
                model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;

                AmendmentBAL objAmendmentBAL = new AmendmentBAL();
                int result = objAmendmentBAL.SaveFacilitiesAmendment(model, ref _transactionId);
                if (result > 0)
                {
                    Session.SetDataToSession<int>("amtPCPNDTTransactionId", _transactionId);
                    notification.Title = "Success";
                    notification.NotificationType = NotificationType.Success;
                    notification.NotificationMessage = "Facilities details saved.";
                    notification.ShowNonActionButton = true;
                    notification.NonActionButtonClassType = PopupButtonClass.Success;
                    notification.NonActionButtonText = "Okay";

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
        #endregion

   
        #region License Data Search and  Cancel
        public ActionResult SearchLicenseByLicenseNo(CancelLicenseModel model)
        {
            NotificationModel notification = new NotificationModel();
          //  CancelLicenseModel objlicensecancel = new CancelLicenseModel();
            AmendmentBAL objAmendmentBAL = new AmendmentBAL();
            model.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;
            //   int licenseno = 12345;   //Static license number by testing purpose    --kishore 23-06-17
            model = objAmendmentBAL.GetLicenseSearch(model);
            if(model != null)
            {

                //notification.Title = "Success";
                //notification.NotificationType = NotificationType.Success;
                //notification.NotificationMessage = "Check Your License Details.";
                //notification.ShowNonActionButton = true;
                //notification.NonActionButtonClassType = PopupButtonClass.Success;
                //notification.NonActionButtonText = "Okay";
                return Json(model);
            }
            else
            {
                notification.Title = "Error";
                notification.NotificationType = NotificationType.Danger;
                notification.NotificationMessage = "License Number Does Not Exist.";
                notification.ShowNonActionButton = true;
                notification.NonActionButtonClassType = PopupButtonClass.Danger;
                notification.NonActionButtonText = "Okay";
                return Json(notification);
            }
           // return View(notification);
        }
        #endregion
    }
}