using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Medical.Models;
using Medical.BAL;
using System.Net.Mail;
using System.Net;
using System.Data;
using System.Collections.Generic;
using Medical.Framework;

namespace Medical.Controllers
{
    
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        #region Global
        UserModel user;
        UserBAL userBAL;
        UserModel userModel;
        #endregion
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return this.RedirectToAction("Index","Home");
        }

        //POST: /Account/Login
        [HttpPost]
       [AllowAnonymous]       
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model); 
            
            UserBAL objBAL = new UserBAL();
            UserModel user = objBAL.ValidateUser(model);

            if (user == null)
            {
                return Json(new
                {
                    errormessage = "Invalid Username/Password"
                });
            }
            else
            {
                Session["User"] = user;
                return Json(new
                {
                    redirecturl = Url.Action(user.Role.DefaultAction, user.Role.DefaultController,
                    new { Area = user.Role.DefaultArea })
                });





                //return RedirectToAction(user.Role.DefaultAction, user.Role.DefaultController,
                //    new { Area = user.Role.DefaultArea });
            }
        }
        [AllowAnonymous]
        public ActionResult LoginLink()
        {
           
             
            UserModel user = Session.GetDataFromSession<UserModel>("User");
            return RedirectToAction(user.Role.DefaultAction, user.Role.DefaultController,
              new { Area = user.Role.DefaultArea });
           // return View();
        }
        // POST: /Account/Login
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // This doesn't count login failures towards account lockout
        //    // To enable password failures to trigger account lockout, change to shouldLockout: true
        //    var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(returnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.RequiresVerification:
        //            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        //        case SignInStatus.Failure:
        //        default:
        //            ModelState.AddModelError("", "Invalid login attempt.");
        //            return View(model);
        //    }
        //}

        //
        // GET: /Account/VerifyCode

        #region DeptAdminAddUser        
            [AllowAnonymous]        
        public ActionResult AddUser()
        {
            userBAL = new UserBAL();
            List<RoleModel> roleList = new List<RoleModel>();
            if (Session["User"] != null)
            {
                userModel = (UserModel)Session["User"];
                roleList = userBAL.GetDesignationList(userModel.DepartmentId);
                ViewData["RoleList"] = roleList;
                if (userModel.RoleType == RoleTypes.SuperAdmin)
                {
                    ViewBag.RoleType = userModel.RoleType;
                }

                else if (userModel.RoleType == RoleTypes.DepartmentAdmin)
                return View();
            }
            return View();
        }
          [HttpPost]
          [AllowAnonymous]
        public JsonResult AddDepartmentUser(UserModel user)
        {                 
            userBAL = new UserBAL();
            string status = "";
            if (Session["User"] != null)
                userModel = (UserModel)Session["User"];
            DataTable objDeptUser = new DataTable();
            //if (user.Id == 0)
            //{
            //    objDeptUser = userBAL.CheckUserExistOrNot(user);

            //}
            if (objDeptUser != null && objDeptUser.Rows.Count > 0)
            {
                status = "User Already Existed";
            }
            else
            {                
                user.Password = user.UserName;
                user.DepartmentId = userModel.DepartmentId;              
                userBAL = new UserBAL();
                bool result = userBAL.AddUser(user);
                if (result == true)
                {
                    if (user.Id == 0)
                    {
                        status = "Saved Successfully";
                    }
                    else
                        status = "Updated Successfully";


                }
                else
                    status = "Technical Problem While Saving";

            }

            return Json(status);
        }
        [AllowAnonymous]
        public JsonResult GetDepartmentUsersList()
        {
            List<DepartmentViewModel> DeptUsersList = new List<DepartmentViewModel>();

            UserModel User = Session["user"] as UserModel;
            userBAL = new UserBAL();
            DeptUsersList = userBAL.GetDepartmentUsers(4, User.DepartmentId);
            TempData["DeptUsersList"] = DeptUsersList;
            return Json(DeptUsersList);
        }
        #endregion

        public ActionResult Signout()
        {
            // Clearing the session        - Raj, 31-05-2017
            Session.Abandon();
            Session.Clear();
            Response.Cookies.Clear();
            Session.RemoveAll();

            AuthenticationManager.SignOut();
            //return Response.RedirectPermanent(Url.Action("Index", "Home"));
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            userBAL = new UserBAL();
            ViewBag.DistrictList = userBAL.GetDistrictList();
            return View("Register");
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserModel objuser)       //async Task<ActionResult>
        {            
            NotificationModel Notification = new NotificationModel();
            UserBAL userBAL = new UserBAL();
            string result = userBAL.ValidateUser(objuser);
            DataTable dt = new DataTable();
            if (result.Length != 0)
            {
                Notification.Title = "Information";
                Notification.NotificationType = NotificationType.Success;
                Notification.NotificationMessage = result;
                return Json(Notification);
            }
            else
            {
                try
                {
                    string mail = "aegisconsultingservice@gmail.com"; //<--Enter your gmail id here
                    string password = "aegis123";//<--Enter gmail password here
                    string FromMail = "aegisconsultingservice@gmail.com";
                    using (MailMessage mm = new MailMessage(FromMail, objuser.EmailId))
                    {
                        mm.Subject = "Medical Application : Your Login Credentials";
                        mm.Body = "Hi " + objuser.FirstName + "," + " <br/><br/> LoginName :  " + objuser.EmailId + "<br/><br/> Password :  " + "123" + "  .<br/><br/>Thanks & Regards,<br/>Medical Team.<br/> ";

                        mm.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com";
                        smtp.EnableSsl = true;
                        NetworkCredential NetworkCred = new NetworkCredential(mail, password);
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = NetworkCred;
                        smtp.Port = 587;
                        smtp.Send(mm);
                        //ClientScript.RegisterStartupScript(GetType(), "alert", "alert('Email sent.');", true);

                        bool regID = userBAL.SaveUserRegistratin(objuser); // modified by kishore 27-06-2017 
                        if (regID == true)
                        {
                            Notification.Title = "Success";
                            Notification.NotificationType = NotificationType.Success;
                            Notification.NotificationMessage = "Successfully Registered";
                            Notification.ShowActionButton = true;
                            Notification.ActionButtonText = "Login";
                            Notification.ActionName = "Login";
                            Notification.ControllerName = "Account";
                            Notification.AreaName = "";
                            return Json(Notification);
                        }
                        else
                        {
                            Notification.Title = "Error";
                            Notification.NotificationType = NotificationType.Danger;
                            Notification.NotificationMessage = "Something went wrong! Please contact Help desk";
                            Notification.ShowNonActionButton = true;
                            Notification.NonActionButtonClassType = PopupButtonClass.Danger;
                            Notification.NonActionButtonText = "Okay";
                            Notification.ReturnData = "0," + FormStatus.Empty;
                            return Json(Notification);
                        }
                    } // end using
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
         [AllowAnonymous]
        public ActionResult GetDistrictsList()
        {
            userBAL = new UserBAL();
            //ViewBag.DistrictList = userBAL.GetDistrictList();  
            List<DistrictModel> DistrictList = userBAL.GetDistrictList();
            return Json(DistrictList);

        }
        
    }
}