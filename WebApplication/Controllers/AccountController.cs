using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Generic.Obfuscation.SHA1;
using Generic.Obfuscation.TripleDES;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebApplication.BLL;
using WebApplication.Common;
using WebApplication.DAL;
using WebApplication.Models;
using WebApplication.Models.ViewModel;

namespace WebApplication.Controllers
{
    [Authorize, Tls]
    public class AccountController : Controller
    {
        CEUserManager ceUserManager = new CEUserManager();

        public AccountController()
        {
            //SessionManager.RegisterSessionActivity();
        }

        //
        // GET: /Account/Register
        [AllowAnonymous, Tls]
        public ActionResult Register()
        {
            var roles = GetAllRoles();
            var model = new RegisterViewModel();
            model.Roles = GenUtil.GetSelectListItems(roles);
            return View(model);
        }

        private IEnumerable<string> GetAllRoles() {
            return new List<string> {
                /*CommonUtility.GetDescriptionFromEnumValue(*/UserTypeEnum.CraveatsDiner.GetDescription()/*)*/,
                /*CommonUtility.GetDescriptionFromEnumValue(*/UserTypeEnum.PartnerRestaurant.GetDescription()/*)*/
            };
        }

        

        //
        // Action method for handling user-entered data when 'Role' button is pressed.
        [HttpPost, Tls]
        public ActionResult Register(RegisterViewModel model)
        {
            SessionManager.RegisterSessionActivity();

            // Get all states again
            var roles = GetAllRoles();

            // Set these states on the model. We need to do this because
            // only the selected value from the DropDownList is posted back, not the whole
            // list of states.
            model.Roles = GenUtil.GetSelectListItems(roles);

            // In case everything is fine - i.e. both "Name" and "State" are entered/selected,
            // redirect user to the "Done" page, and pass the user object along via Session
            if (ModelState.IsValid)
            {
                SHA1HashProvider sHA1HashProvider = new SHA1HashProvider();
                if (!ceUserManager.IsRegistered(model.Email))
                {
                    string sha1HashText = sHA1HashProvider.SecureSHA1(model.Password.Trim());
                    int? newUserID = ceUserManager.RegisterNew(model.Email, sha1HashText, model.Role);
                    if (newUserID.HasValue)
                    {
                        UserDTO userDTO = new UserDTO() {
                            Id = DataSecurityTripleDES.GetEncryptedText(newUserID),
                            FirstName = model.FirstName,
                            Surname = model.Surname,
                            UserStatus = (int?) UserStatusEnum.Active
                        };

                        ceUserManager.SaveUserDetail(userDTO);

                        StringBuilder sbSubject = new StringBuilder("Craveats new registrant notification"),
                            sbEmailBody = new StringBuilder("<p>A new user with the following detail has been registered in the system. " +
                            $"<br/><em>FirstName            </em>: {model.FirstName}" +
                            $"<br/><em>Surname              </em>: {model.Surname}" +
                            $"<br/><em>Email                </em>: {model.Email}" +
                            $"<br/><em>Registration Type    </em>: {model.Role}" +
                            "</p><p>Thank you.</p><p>Craveats</p>");

                            CommunicationServiceProvider.SendOutgoingNotification(
                                new MailAddress(
                                    model.Email,
                                    string.Format("{0}{1}{2}", model.FirstName, " ", model?.Surname).Trim()),
                                sbSubject.ToString(),
                                sbEmailBody.ToString());

                        User result = ceUserManager.FindByCriteria(email: model.Email, userStatusEnums: new List<int> { (int)UserStatusEnum.Active, (int)UserStatusEnum.Blocked });
                        if (result != null)
                        {
                            userDTO = EntityDTOHelper.GetEntityDTO<User, UserDTO>(result);

                            AuthenticatedUserInfo authenticatedUserInfo = new AuthenticatedUserInfo(userDTO);
                            Session["loggeduser"] = authenticatedUserInfo;

                            SessionManager.RegisterSessionActivity(userID: result.Id, loggedInAt: DateTime.Now);

                            ceUserManager.SignInUser(HttpContext, string.Format("{0}", authenticatedUserInfo.FullName), false);

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "An error occurred in reading user data. Please review input and re-try.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred in registering new user. Please review input and re-try.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email is registered and cannot be used to create another account.");
                }
            }

            // Something is not right - so render the registration page again,
            // keeping the data user has entered by supplying the model.
            return View("Register", model);
        }

        //
        // 3. Action method for displaying 'Done' page
        public ActionResult Done()
        {
            // Get Sign Up information from the session
            var model = Session["RegisterViewModel"] as RegisterViewModel;

            // Display Done.html page that shows Name and selected state.
            return View(model);
        }


        //
        // GET: /Account/Login
        [AllowAnonymous, Tls]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        ////
        //// POST: /Account/Login
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
        // POST: /Account/Login
        [HttpPost, Tls]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                CEUserManager ceUserManager = new CEUserManager();
                SHA1HashProvider sHA1HashProvider = new SHA1HashProvider();
                User anActiveOrBlockedUser = ceUserManager.GetSigningUserByEmail(model.Email);

                if (anActiveOrBlockedUser != null && sHA1HashProvider.CheckHashSHA1(model.Password, anActiveOrBlockedUser.Password, 8))
                {
                    UserDTO userDTO = EntityDTOHelper.GetEntityDTO<User, UserDTO>(anActiveOrBlockedUser);
                    AuthenticatedUserInfo authenticatedUserInfo = new AuthenticatedUserInfo(userDTO);

                    ceUserManager.SignInUser(HttpContext, string.Format("{0}", authenticatedUserInfo.FullName), false);

                    Session["loggeduser"] = authenticatedUserInfo;

                    SessionManager.RegisterSessionActivity(loggedInAt: DateTime.Now);

                    return this.RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Login attempt failed.");
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e);
            }
            return this.View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost, Tls]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            try
            {
                new CEUserManager().SignOut(HttpContext);

                SessionManager.RegisterSessionActivity(loggedOffAt: DateTime.Now);

                Session["loggeduser"] = null;
                Session.Contents.Clear();
                Session.Clear();

                Session.RemoveAll();
                foreach (string key in HttpContext.Request.Cookies.Keys.Cast<string>().ToList())
                {
                    HttpContext.Response.Cookies.Add(new HttpCookie(key, string.Empty) { Expires = DateTime.Now.Subtract(new TimeSpan(4, 15, 30)), HttpOnly = true });
                }
                Session.Abandon();
                
            }
            catch (Exception e)
            {
                throw e;
            }
            return RedirectToAction("Login", "Login");
        }

        ////
        //// GET: /Account/VerifyCode
        //[AllowAnonymous]
        //public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        //{
        //    // Require that the user has already logged in via username/password or external login
        //    if (!await SignInManager.HasBeenVerifiedAsync())
        //    {
        //        return View("Error");
        //    }
        //    return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ////
        //// POST: /Account/VerifyCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // The following code protects for brute force attacks against the two factor codes. 
        //    // If a user enters incorrect codes for a specified amount of time then the user account 
        //    // will be locked out for a specified amount of time. 
        //    // You can configure the account lockout settings in IdentityConfig
        //    var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(model.ReturnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.Failure:
        //        default:
        //            ModelState.AddModelError("", "Invalid code.");
        //            return View(model);
        //    }
        //}

        

        ////
        //// POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
        //            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
        //            // Send an email with this link
        //            // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //            // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //            // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

        //            return RedirectToAction("Index", "Home");
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        ////
        //// GET: /Account/ConfirmEmail
        //[AllowAnonymous]
        //public async Task<ActionResult> ConfirmEmail(string userId, string code)
        //{
        //    if (userId == null || code == null)
        //    {
        //        return View("Error");
        //    }
        //    var result = await UserManager.ConfirmEmailAsync(userId, code);
        //    return View(result.Succeeded ? "ConfirmEmail" : "Error");
        //}

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous, Tls]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User anActiveOrBlockedUser = null;
                CEUserManager ceUserManager = new CEUserManager();
                anActiveOrBlockedUser = ceUserManager.GetSigningUserByEmail(model.Email);

                if (anActiveOrBlockedUser == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string longTicks = DateTime.Now.Ticks.ToString(), 
                    code = DataSecurityTripleDES.GetEncryptedText(longTicks);

                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    User anUser = craveatsDbContext.User.First(u => u.Id == anActiveOrBlockedUser.Id);

                    anUser.ResetCode = longTicks;
                    anUser.ResetCodeExpiry = DateTime.Now.AddDays(1);
                    anUser.ResetCodeSentAt = DateTime.Now;

                    anUser.LastUpdated = DateTime.Now;

                    craveatsDbContext.SaveChanges();
                }

                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = DataSecurityTripleDES.GetEncryptedText(anActiveOrBlockedUser.Id), code = code }, protocol: Request.Url.Scheme);

                StringBuilder sbSubject = new StringBuilder("Craveats reset password request"), 
                    sbEmailBody = new StringBuilder("<p>Dear [FullName],</p><p>We have received a request that you would like to reset your account password with us." + 
                    "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a></p><p>Thank you.</p><p>Craveats</p>");

                CommunicationServiceProvider.SendOutgoingNotification(
                    new MailAddress(
                        anActiveOrBlockedUser.EmailAddress, 
                        string.Format("{0}{1}{2}", anActiveOrBlockedUser?.FirstName, " ", anActiveOrBlockedUser?.Surname).Trim()),
                    sbSubject.ToString(),
                    sbEmailBody.Replace("[FullName]",
                        string.Format("{0}{1}{2}", anActiveOrBlockedUser?.FirstName, " ", anActiveOrBlockedUser?.Surname).Trim()).ToString());

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous, Tls]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous, Tls]
        public ActionResult ResetPassword(string code, string userId)
        {

            return (code == null || userId == null) 
                ? View("Error") : View();
            
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost, Tls]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User anActiveOrBlockedUser = null;
            CEUserManager ceUserManager = new CEUserManager();
            int userIDFromRequest = 0;
            string plainCode = null, errorInTranslation = string.Empty;

            try
            {
                userIDFromRequest = int.Parse(DataSecurityTripleDES.GetPlainText(model.UserId));
                plainCode = DataSecurityTripleDES.GetPlainText(model.Code);
            
                DateTime minExpiry = DateTime.Now;

                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    anActiveOrBlockedUser = craveatsDbContext.User.First(u => u.Id == userIDFromRequest && u.ResetCode == plainCode && (!u.ResetCodeExpiry.HasValue || u.ResetCodeExpiry >= minExpiry));
                    anActiveOrBlockedUser.ResetCodeExpiry = DateTime.Now;
                    anActiveOrBlockedUser.ResetCode = null;

                    anActiveOrBlockedUser.Password = new SHA1HashProvider().SecureSHA1(model.Password.Trim());

                    anActiveOrBlockedUser.LastUpdated = DateTime.Now;

                    craveatsDbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

            return RedirectToAction("ResetPasswordConfirmation", "Account");
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous, Tls]
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

        ////
        //// GET: /Account/SendCode
        //[AllowAnonymous]
        //public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        //{
        //    var userId = await SignInManager.GetVerifiedUserIdAsync();
        //    if (userId == null)
        //    {
        //        return View("Error");
        //    }
        //    var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
        //    var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
        //    return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ////
        //// POST: /Account/SendCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> SendCode(SendCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }

        //    // Generate the token and send it
        //    if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
        //    {
        //        return View("Error");
        //    }
        //    return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        //}

        ////
        //// GET: /Account/ExternalLoginCallback
        //[AllowAnonymous]
        //public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        //{
        //    var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
        //    if (loginInfo == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    // Sign in the user with this external login provider if the user already has a login
        //    var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(returnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.RequiresVerification:
        //            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
        //        case SignInStatus.Failure:
        //        default:
        //            // If the user does not have an account, then prompt the user to create an account
        //            ViewBag.ReturnUrl = returnUrl;
        //            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
        //            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
        //    }
        //}

        ////
        //// POST: /Account/ExternalLoginConfirmation
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("Index", "Manage");
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        // Get the information about the user from the external login provider
        //        var info = await AuthenticationManager.GetExternalLoginInfoAsync();
        //        if (info == null)
        //        {
        //            return View("ExternalLoginFailure");
        //        }
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //            if (result.Succeeded)
        //            {
        //                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                return RedirectToLocal(returnUrl);
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    ViewBag.ReturnUrl = returnUrl;
        //    return View(model);
        //}

        

        ////
        //// GET: /Account/ExternalLoginFailure
        //[AllowAnonymous]
        //public ActionResult ExternalLoginFailure()
        //{
        //    return View();
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //if (_userManager != null)
                //{
                //    _userManager.Dispose();
                //    _userManager = null;
                //}

                //if (_signInManager != null)
                //{
                //    _signInManager.Dispose();
                //    _signInManager = null;
                //}
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
        #region Not-In-Use
        //private ApplicationSignInManager _signInManager;
        //private ApplicationUserManager _userManager;

        //public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        //{
        //    UserManager = userManager;
        //    SignInManager = signInManager;
        //}

        //public ApplicationSignInManager SignInManager
        //{
        //    get
        //    {
        //        return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        //    }
        //    private set 
        //    { 
        //        _signInManager = value; 
        //    }
        //}

        //public ApplicationUserManager UserManager
        //{
        //    get
        //    {
        //        return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //    }
        //    private set
        //    {
        //        _userManager = value;
        //    }
        //}
        #endregion  
    }
}