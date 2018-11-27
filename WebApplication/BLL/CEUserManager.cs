using Generic.Obfuscation.SHA1;
using Generic.Obfuscation.TripleDES;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using WebApplication.Common;
using WebApplication.DAL;
using WebApplication.DAL.DBCommon;
using WebApplication.Models;
using WebApplication.Models.ViewModel;

namespace WebApplication
{
    public class CEUserManager
    {
        private bool UserExistsForEmail(string emailAddress)
        {
            try
            {
                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    return craveatsDbContext.GetUserByEmail(emailAddress).Count() > 0;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool IsRegistered(string emailAddress)
        {
            return UserExistsForEmail(emailAddress);
        }

        internal int? RegisterNew(string email, string hashedPassword, string role)
        {
            try {
                Common.UserTypeEnum registeringRole = Common.CommonUtility.GetEnumValueFromDescription<Common.UserTypeEnum>(role);

                if (!(registeringRole.HasFlag(Common.UserTypeEnum.CraveatsDiner) ||
                    registeringRole.HasFlag(Common.UserTypeEnum.PartnerRestaurant)))
                {
                    throw new InvalidOperationException("Requested role could not be used in this context.");
                }

                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext()) {
                    DBCommonUtility dBCommonUtility = new DBCommonUtility();
                    string sqlCmdParamString = "";
                    SqlParameter[] sqlParameters = dBCommonUtility.GetSqlParameters(
                        new object[] {
                                        email,
                                        hashedPassword,
                                        (int) registeringRole},
                        out sqlCmdParamString
                        , true);
                    StringBuilder sbRawSQL = new StringBuilder("exec RegisterNewActiveUser");
                    sbRawSQL.AppendFormat(" {0}", sqlCmdParamString.Trim());

                    User newUser = craveatsDbContext.User.SqlQuery(
                        sql: sbRawSQL.ToString(),
                        parameters: sqlParameters
                    ).FirstOrDefault();

                    return newUser?.Id;
                }
            }
            catch (Exception e) {
                throw e;
            }
        }

        internal async Task<UserDTO> FindByIdAsync(int loggedUserId)
        {
            UserDTO userDTO = null;
            try
            {
                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    User user = await craveatsDbContext.User.FindAsync(loggedUserId);
                    userDTO = EntityDTOHelper.GetEntityDTO<User, UserDTO>(user);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            return userDTO;
        }

        internal async Task<IdentityResult> ChangePasswordAsync(int loggedUserId, string oldPassword, string newPassword)
        {
            bool returnSuccess = false;
            try
            {
                SHA1HashProvider sHA1HashProvider = new SHA1HashProvider();

                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext()) {
                    User thisUser = craveatsDbContext.User.FirstOrDefault(u => u.Id == loggedUserId);
                    if (thisUser != null && sHA1HashProvider.CheckHashSHA1(oldPassword, thisUser.Password, 8)) {

                        thisUser.Password = sHA1HashProvider.SecureSHA1(newPassword);
                        thisUser.LastUpdated = DateTime.Now;

                        await craveatsDbContext.SaveChangesAsync();
                        returnSuccess = true;
                    }
                }
            }
            catch (Exception e) {
                Trace.WriteLine(e);
            }

            return returnSuccess ? IdentityResult.Success : IdentityResult.Failed("Change password failed");
        }

        internal void SaveUserDetail(UserDTO userDTO)
        {
            try
            {
                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    int userId = int.Parse(DataSecurityTripleDES.GetPlainText(userDTO.Id));
                    User anUser = craveatsDbContext.User.FirstOrDefault(u => u.Id == userId);

                    anUser = EntityDTOHelper.MapToEntity<UserDTO, User>(userDTO, anUser);

                    anUser.LastUpdated = DateTime.Now;
                    craveatsDbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal object AddPasswordAsync(string v, string newPassword)
        {
            throw new NotImplementedException();
        }

        public User GetSigningUserByEmail(string emailAddress)
        {
            try
            {
                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    DBCommonUtility dBCommonUtility = new DBCommonUtility();
                    string sqlCmdParamString = "";
                    SqlParameter[] sqlParameters = dBCommonUtility.GetSqlParameters(
                        new object[] {
                            emailAddress
                        },
                        out sqlCmdParamString
                        , true);
                    StringBuilder sbRawSQL = new StringBuilder("exec GetSigningUserByEmail");
                    sbRawSQL.AppendFormat(" {0}", sqlCmdParamString.Trim());

                    User newUser = craveatsDbContext.User.SqlQuery(
                        sql: sbRawSQL.ToString(),
                        parameters: sqlParameters
                    ).FirstOrDefault();

                    return newUser;
                }
            }
            catch (Exception e) {
                throw e;
            }
        }

        internal User FindByCriteria(string email, List<int> userStatusEnums = null, UserTypeEnum? userTypeEnums = null)
        {
            User result = null;

            userStatusEnums = (userStatusEnums == null || userStatusEnums.Count == 0)
                ? new List<int> { (int)UserStatusEnum.Active, (int)UserStatusEnum.Blocked }
                : userStatusEnums;

            try
            {
                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    List<User> results = craveatsDbContext.User.Where(u => (u.UserStatus.HasValue &&
                        userStatusEnums.Contains(u.UserStatus.Value) &&
                        (u.EmailAddress == email))).OrderBy(u => u.UserStatus).ThenBy(v => v.Id).ToList();

                    result = results.Any() 
                        ? (userTypeEnums == null) 
                            ? results.FirstOrDefault()
                            : results.Where(u => 
                                u.UserTypeFlag.HasValue && 
                                ((UserTypeEnum)u.UserTypeFlag.Value).HasFlag(
                                    userTypeEnums.Value)).FirstOrDefault()
                        : null
                        ;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }

        public User FindById(int id)
        {
            User user = null;
            try
            {
                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    user = craveatsDbContext.User.Find(id);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            return user;

        }

        public bool FindByIdAndCheckIfPasswordIsMissing(int loggedUserId) {
            return FindById(loggedUserId)?.Password?.Length > 0 ? false : true;
        }

        public bool FindByIdAndCheckIfContactNumberIsMissing(int loggedUserId)
        {
            return FindById(loggedUserId)?.ContactNumber?.Length > 0 ? false : true;
        }

        public string FindByIdAndGetContactNumber(int loggedUserId)
        {
            return FindById(loggedUserId)?.ContactNumber;
        }

        //public static bool RegisterSessionActivity(int? userID = null, DateTime? loggedInAt = null, DateTime? loggedOffAt = null)
        //{
        //    if (EditSessionTracking(new SessionTracking()
        //    {
        //        SessionID = HttpContext.Current.Session.SessionID,
        //        IPAddress = (HttpContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"] ?? string.Empty).Trim() == string.Empty
        //        ? HttpContext.Current.Request.Headers["REMOTE_ADDR"]?.Trim()
        //        : HttpContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"]?.Trim(),
        //        UserId = userID.HasValue ? userID : HttpContext.Current.Session["loggeduser"] != null
        //            ? (int?)int.Parse(DataSecurityTripleDES.GetPlainText(((AuthenticatedUserInfo)HttpContext.Current.Session["loggeduser"]).UserId))
        //            : null,
        //        LoggedInAt = loggedInAt,
        //        LoggedOutAt = loggedOffAt
        //    }) != null) {
        //        return true;
        //    };
        //    return false;
        //}

        //private static SessionTracking EditSessionTracking(SessionTracking sessionTracking)
        //{
        //    SessionTracking savedSessionTracking = null;
        //    try
        //    {
        //        using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext()) {
        //            DBCommonUtility dBCommonUtility = new DBCommonUtility();
        //            string sqlCmdParamString = "";
        //            SqlParameter[] sqlParameters = dBCommonUtility.GetSqlParameters(
        //                new object[] {
        //                    sessionTracking.SessionID,
        //                    sessionTracking.IPAddress,
        //                    sessionTracking.UserId,
        //                    null,
        //                    sessionTracking.LoggedInAt,
        //                    sessionTracking.LoggedOutAt },
        //                out sqlCmdParamString
        //                , true);
        //            StringBuilder sbRawSQL = new StringBuilder("exec EditSessionTracking");
        //            sbRawSQL.AppendFormat(" {0}", sqlCmdParamString.Trim());

        //            savedSessionTracking = craveatsDbContext.Database.SqlQuery<SessionTracking>(
        //                sql: sbRawSQL.ToString(), 
        //                parameters: sqlParameters
        //            ).FirstOrDefault();
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        throw exception;
        //    }

        //    return savedSessionTracking;
        //}


        #region Sign In and Out method.    
        internal async Task<IdentityResult> SignIn(string username, bool isPersistent, bool rememberBrowser)
        {
            // Initialization.    
            var claims = new List<Claim>();
            try
            {
                if (username != null)
                {
                    // Setting    
                    claims.Add(new Claim(ClaimTypes.Name, username));
                    var claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                    var ctx = HttpContext.Current.Request.GetOwinContext();
                    var authenticationManager = ctx.Authentication;
                    // Sign In.    
                    authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, claimIdenties);

                    return IdentityResult.Success;
                }
            }
            catch (Exception ex)
            {
                // Info    
                throw ex;
            }
            return IdentityResult.Failed("Signin failed");
        }

        /// <summary>  
        /// Sign In User method.    
        /// </summary>  
        /// <param name="username">Username parameter.</param>  
        /// <param name="isPersistent">Is persistent parameter.</param>  
        internal void SignInUser(HttpContextBase context, string username, bool isPersistent)
        {
            // Initialization.    
            var claims = new List<Claim>();
            try
            {
                // Setting    
                claims.Add(new Claim(ClaimTypes.Name, username));
                var claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                var ctx = context.Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;
                // Sign In.    
                authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, claimIdenties);
            }
            catch (Exception ex)
            {
                // Info    
                throw ex;
            }
        }

        internal void SignOut(HttpContextBase context)
        {
            // Setting.    
            var ctx = context.Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            // Sign Out.    
            authenticationManager.SignOut();
        }

        #endregion

    }
}