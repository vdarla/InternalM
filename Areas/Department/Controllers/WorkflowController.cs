using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Medical.BAL;
using Medical.Models;
using System.Data;
using Medical.Framework;

namespace Medical.Areas.Department.Controllers
{
    public class WorkflowController : Controller
    {
        #region Global
        WorkflowBAL objBAL;
        List<DesignationModel> DesignationList;
        List<WorkFlowViewModel> workFlowList;
        #endregion
        // GET: Department/Workflow
        public ActionResult Workflow()
        {
            Session["WorkflowList"] = null;
            Session["DesignationList"] = null;
            Session["RevWorkflowList"] = null;
            objBAL = new WorkflowBAL();
            List<MasterServiceModel> ServicesList; List<DocumentModel> DocumentList; 
            objBAL.GetMasterdata(1, out ServicesList, out DocumentList, out DesignationList);
            ViewData["MasterServices"] = ServicesList;
            ViewData["RequiredDocs"] = DocumentList.Where(item => item.DocumentTypeId ==DocumentType.Required).ToList();
            ViewData["ApprovalDocs"] = DocumentList.Where(item => item.DocumentTypeId == DocumentType.Approval).ToList();
            ViewData["FromDesignation"] = DesignationList;
            Session["DesignationList"] = DesignationList;
            return View();
        }
        public JsonResult FillToDesignation(int RoleId)
        {
            DesignationList = new List<DesignationModel>();
            if (Session["DesignationList"] != null)
                DesignationList = Session["DesignationList"] as List<DesignationModel>;
            workFlowList = new List<WorkFlowViewModel>();
            if (Session["WorkflowList"] != null)
            {
                workFlowList = Session["WorkflowList"] as List<WorkFlowViewModel>;
                DesignationList = DesignationList.Where(r => !workFlowList.Any(w => w.FromRoleId == r.Id)).ToList().Where(item => item.Id != RoleId).ToList();
            }
            else
            {
                DesignationList = DesignationList.Where(item => item.Id != RoleId).ToList();
            }
            return Json(DesignationList);
        }
        public JsonResult FillRevToDesignationList(int RoleId)
        {
            DesignationList = new List<DesignationModel>();
            if (Session["DesignationList"] != null)
            {
                DesignationList = Session["DesignationList"] as List<DesignationModel>;
                DesignationList = DesignationList.Where(item => item.Id != RoleId).ToList();
            }
            return Json(DesignationList);

        }
        public JsonResult FillRevFromRoleList()
        {
            workFlowList = new List<WorkFlowViewModel>();
            if (Session["RevWorkflowList"] != null)
                workFlowList = Session["RevWorkflowList"] as List<WorkFlowViewModel>;
            DesignationList = new List<DesignationModel>();
            if (Session["RoleList"] != null)
                DesignationList = Session["RoleList"] as List<DesignationModel>;
            //Session["RoleList"] = Rolelist.Where(r => !workFlowList.Any(w => w.FromOfficerId == r.Id)).ToList();
            return Json(DesignationList.Where(r => !workFlowList.Any(w => w.FromRoleId == r.Id)).ToList());
        }
        public ActionResult AddRevWorkflowList(WorkFlowViewModel workFlow)
        {
            workFlowList = new List<WorkFlowViewModel>();
            if (Session["RevWorkflowList"] != null)
                workFlowList = Session["RevWorkflowList"] as List<WorkFlowViewModel>;
            workFlowList.Add(workFlow);
            Session["RevWorkflowList"] = workFlowList;

            return Json(workFlowList);
        }
        public ActionResult SaveWorkFlow(ServiceModel service)
        {

            objBAL = new WorkflowBAL();
            DataTable dtForward = ConvertWorkFlowtoDatatable();
            DataTable dtRevWorkflow = ConvertRevWorkFlowtoDatatable();
            service.DepartmentId =Session.GetDataFromSession<UserModel>("User").DepartmentId ;
            service.CreatedUserId = Session.GetDataFromSession<UserModel>("User").Id;
            bool result = objBAL.SaveWorkFlow(dtForward, dtRevWorkflow, service);
            //Clear All Sessions
             Session["DesignationList"] = null;
            Session["WorkflowList"] = null;
            NotificationModel notification = new NotificationModel();
            if (result)
            {
                notification.Title = "Success";
                notification.NotificationType = NotificationType.Success;
                notification.NotificationMessage = "Workflow has been submitted successfully";
                notification.ShowActionButton = true;
                notification.ActionButtonText = "OK";
                notification.ActionName = "Workflow";
                notification.ControllerName = "Workflow";
                notification.AreaName = "Department";
            }
            else
            {
                notification.Title = "Error";
                notification.NotificationType = NotificationType.Danger;
                notification.NotificationMessage = "Oops! Something went wrong... <br>Workflow not submitted, please contact technical support.";               
            }
            return Json(notification);
            //return Json(Url.Action("DepartmentAdmin"));

        }
        private DataTable ConvertWorkFlowtoDatatable()
        {            
          
                if (Session["WorkflowList"] != null)
                    workFlowList = Session["WorkflowList"] as List<WorkFlowViewModel>;
                DataTable dtWorkflow = new DataTable();
                dtWorkflow.Columns.Add("DeptId", typeof(int));
                dtWorkflow.Columns.Add("FromOfficerId", typeof(int));
                dtWorkflow.Columns.Add("ToOfficerId", typeof(int));
                dtWorkflow.Columns.Add("InspectionReport", typeof(bool));
                dtWorkflow.Columns.Add("AutoSlide", typeof(bool));
                dtWorkflow.Columns.Add("SLA", typeof(int));
                dtWorkflow.Columns.Add("Id", typeof(int));
                dtWorkflow.Columns.Add("CreatedBy", typeof(int));
                dtWorkflow.Columns.Add("IsApprovalAuthority", typeof(bool));
                dtWorkflow.Columns.Add("HasRaiseQuery", typeof(bool));
                dtWorkflow.Columns.Add("HasReturn", typeof(bool));
                foreach (WorkFlowViewModel item in workFlowList)
                {
                    DataRow dr = dtWorkflow.NewRow();
                    dr[0] = Session.GetDataFromSession<UserModel>("User").DepartmentId;
                dr[1] = item.FromRoleId;
                    dr[2] = item.ToRoleId;
                    dr[3] = item.HasInspectionPrevilege;
                    dr[4] = item.HasAutoSlidePrevilege;
                    dr[5] = item.SLA;
                    dr[6] = item.Id;
                    dr[7] = Session.GetDataFromSession<UserModel>("User").Id; 
                    dr[8] = item.HasApprovalPrevilege;
                    dr[9] = item.HasRaisedQueryPrevilege;
                    dr[10] = item.HasReturnPrevilege;
                    dtWorkflow.Rows.Add(dr);
                }
                return dtWorkflow;    
        }
        private DataTable ConvertRevWorkFlowtoDatatable()
        {
            workFlowList = new List<WorkFlowViewModel>();
            if (Session["RevWorkflowList"] != null)
                workFlowList = Session["RevWorkflowList"] as List<WorkFlowViewModel>;
            DataTable dtRewWorkflow = new DataTable();
            dtRewWorkflow.Columns.Add("Id", typeof(int));
            dtRewWorkflow.Columns.Add("FromOfficerId", typeof(int));
            dtRewWorkflow.Columns.Add("ToOfficerId", typeof(int));
            dtRewWorkflow.Columns.Add("CreatedBy", typeof(int));

            foreach (WorkFlowViewModel item in workFlowList)
            {
                DataRow dr = dtRewWorkflow.NewRow();
                dr[0] = item.Id;
                dr[1] = item.FromRoleId;
                dr[2] = item.ToRoleId;
                dr[3] = Session.GetDataFromSession<UserModel>("User").Id;

                dtRewWorkflow.Rows.Add(dr);
            }
            return dtRewWorkflow;
        }
        public ActionResult AddWorkflowList(WorkFlowViewModel workFlow)
        {
            workFlowList = new List<WorkFlowViewModel>();
            if (Session["WorkflowList"] != null)
                workFlowList = Session["WorkflowList"] as List<WorkFlowViewModel>;
            workFlowList.Add(workFlow);
            Session["WorkflowList"] = workFlowList;

            return Json(workFlowList);
        }
        public JsonResult FillFromRoleList()
        {
            workFlowList = new List<WorkFlowViewModel>();
            if (Session["WorkflowList"] != null)
                workFlowList = Session["WorkflowList"] as List<WorkFlowViewModel>;
            DesignationList = new List<DesignationModel>();
            if ( Session["DesignationList"] != null)
                DesignationList =  Session["DesignationList"] as List<DesignationModel>;
            // Session["DesignationList"] = Rolelist.Where(r => !workFlowList.Any(w => w.FromOfficerId == r.Id)).ToList();
            return Json(DesignationList.Where(r => !workFlowList.Any(w => w.FromRoleId == r.Id)).ToList());
        }
    }
}