using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Generic.Obfuscation.TripleDES;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [Authorize, Tls]
    public class ManageController : Controller
    {
        CEUserManager ceUserManager = new CEUserManager();
        int loggedUserId = 0;

        public ManageController()
        {
            if (Session != null && Session.Contents != null && Session["loggeduser"] != null)
            {
                AuthenticatedUserInfo authenticatedUserInfo = Session["loggeduser"] as AuthenticatedUserInfo;
                loggedUserId = int.Parse(DataSecurityTripleDES.GetPlainText(authenticatedUserInfo.UserId));
            }
        }
        
        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var model = new IndexViewModel
            {
                HasPassword = HasPassword()
            };
            return View(model);
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel();
            if (Session["loggeduser"] != null) {
                AuthenticatedUserInfo authenticatedUserInfo = Session["loggeduser"] as AuthenticatedUserInfo;
                model.UserId = authenticatedUserInfo.UserId;
            }
            return View(model);
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            loggedUserId = int.Parse(DataSecurityTripleDES.GetPlainText(model.UserId));
            var result = await ceUserManager.ChangePasswordAsync(loggedUserId, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var userDTO = await ceUserManager.FindByIdAsync(loggedUserId);
                if (userDTO != null)
                {
                    AuthenticatedUserInfo authenticatedUserInfo = new AuthenticatedUserInfo(userDTO);

                    Session["loggeduser"] = authenticatedUserInfo;

                    SessionManager.RegisterSessionActivity(userID: loggedUserId, loggedInAt: DateTime.Now);

                    await ceUserManager.SignIn(username: authenticatedUserInfo.FullName, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && ceUserManager != null)
            {
                ceUserManager = null;
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

        private bool HasPassword()
        {
            return ceUserManager.FindByIdAndCheckIfPasswordIsMissing(loggedUserId);
        }

        private bool HasPhoneNumber()
        {
            return ceUserManager.FindByIdAndCheckIfContactNumberIsMissing(loggedUserId);
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}