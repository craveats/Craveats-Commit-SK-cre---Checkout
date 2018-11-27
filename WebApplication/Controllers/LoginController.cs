using Generic.Obfuscation.SHA1;
using Generic.Obfuscation.TripleDES;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApplication.Common;
using WebApplication.DAL;
using WebApplication.Models;
using WebApplication.Models.ViewModel;

namespace WebApplication.Controllers
{
    [Tls]
    public class LoginController : Controller
    {
        CEUserManager ceUserManager = new CEUserManager();

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

        //
        // GET: /Login/Register
        [AllowAnonymous, Tls]
        public ActionResult Register()
        {
            SessionManager.RegisterSessionActivity();

            var roles = GetAllRoles();
            var model = new RegisterViewModel();
            model.Roles = GetSelectListItems(roles);
            return View(model);
        }

        private IEnumerable<string> GetAllRoles()
        {
            return new List<string> {
                /*CommonUtility.GetDescriptionFromEnumValue(*/UserTypeEnum.CraveatsDiner.GetDescription()/*)*/,
                /*CommonUtility.GetDescriptionFromEnumValue(*/UserTypeEnum.PartnerRestaurant.GetDescription()/*)*/
            };
        }

        // src:: https://nimblegecko.com/using-simple-drop-down-lists-in-ASP-NET-MVC/
        // visited :: 2018 11 18
        // This is one of the most important parts in the whole example.
        // This function takes a list of strings and returns a list of SelectListItem objects.
        // These objects are going to be used later in the SignUp.html template to render the
        // DropDownList.
        private IEnumerable<SelectListItem> GetSelectListItems(IEnumerable<string> elements)
        {
            // Create an empty list to hold result of the operation
            var selectList = new List<SelectListItem>();

            // For each string in the 'elements' variable, create a new SelectListItem object
            // that has both its Value and Text properties set to a particular value.
            // This will result in MVC rendering each item as:
            //     <option value="State Name">State Name</option>
            foreach (var element in elements)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element,
                    Text = element
                });
            }

            return selectList;
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
            model.Roles = GetSelectListItems(roles);

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
                        UserDTO userDTO = new UserDTO()
                        {
                            Id = DataSecurityTripleDES.GetEncryptedText(newUserID),
                            FirstName = model.FirstName,
                            Surname = model.Surname,
                            UserStatus = (int?)UserStatusEnum.Active
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
        // GET: /Login/Login
        [AllowAnonymous, Tls]
        public ActionResult Login(string returnUrl)
        {
            SessionManager.RegisterSessionActivity();

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Login/Login
        [HttpPost, Tls]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            SessionManager.RegisterSessionActivity();

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
        // GET: /Login/ForgotPassword
        [AllowAnonymous, Tls]
        public ActionResult ForgotPassword()
        {
            SessionManager.RegisterSessionActivity();

            return View();
        }

        //
        // POST: /Login/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            SessionManager.RegisterSessionActivity();

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

                var callbackUrl = Url.Action("ResetPassword", "Login", new { userId = DataSecurityTripleDES.GetEncryptedText(anActiveOrBlockedUser.Id), code = code }, protocol: Request.Url.Scheme);

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

                return RedirectToAction("ForgotPasswordConfirmation", "Login");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Login/ForgotPasswordConfirmation
        [AllowAnonymous, Tls]
        public ActionResult ForgotPasswordConfirmation()
        {
            SessionManager.RegisterSessionActivity();

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

            return RedirectToAction("ResetPasswordConfirmation", "Login");
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous, Tls]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult NotFound()
        {
            SessionManager.RegisterSessionActivity();

            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;

            return View();
        }

    }
}