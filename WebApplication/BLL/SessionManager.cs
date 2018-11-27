using Generic.Obfuscation.TripleDES;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using WebApplication.Common;
using WebApplication.DAL;
using WebApplication.DAL.DBCommon;
using WebApplication.Models;

namespace WebApplication
{
    public class SessionManager
    {
        public static bool RegisterSessionActivity(int? userID = null, DateTime? loggedInAt = null, DateTime? loggedOffAt = null)
        {
            object xLock = new object();
            lock (xLock) {
                AuthenticatedUserInfo authenticatedUserInfo = HttpContext.Current.Session["loggeduser"] as AuthenticatedUserInfo;
                if (EditSessionTracking(new SessionTracking()
                {
                    SessionID = HttpContext.Current.Session.SessionID,
                    IPAddress = (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? string.Empty).Trim() == string.Empty
                    ? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]?.Trim()
                    : HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]?.Trim(),
                    UserId = userID.HasValue ? userID : authenticatedUserInfo != null
                        ? (int?)int.Parse(DataSecurityTripleDES.GetPlainText(authenticatedUserInfo.UserId))
                        : null,
                    LoggedInAt = loggedInAt,
                    LoggedOutAt = loggedOffAt
                }) != null) {
                        return true;
                    };
                    return false;
            }
        }

        private static SessionTracking EditSessionTracking(SessionTracking sessionTracking)
        {
            SessionTracking savedSessionTracking = null;
            try
            {
                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext()) {
                    DBCommonUtility dBCommonUtility = new DBCommonUtility();
                    string sqlCmdParamString = "";
                    SqlParameter[] sqlParameters = dBCommonUtility.GetSqlParameters(
                        new object[] {
                            sessionTracking.SessionID,
                            sessionTracking.IPAddress,
                            sessionTracking.UserId,
                            null,
                            sessionTracking.LoggedInAt,
                            sessionTracking.LoggedOutAt },
                        out sqlCmdParamString
                        , true);
                    StringBuilder sbRawSQL = new StringBuilder("exec EditSessionTracking");
                    sbRawSQL.AppendFormat(" {0}", sqlCmdParamString.Trim());

                    savedSessionTracking = craveatsDbContext.Database.SqlQuery<SessionTracking>(
                        sql: sbRawSQL.ToString(), 
                        parameters: sqlParameters
                    ).FirstOrDefault();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }

            return savedSessionTracking;
        }

        internal static UserTypeEnum GetContextSessionOwnerType()
        {
            object xLock = new object();
            lock (xLock) {
                AuthenticatedUserInfo authenticatedUserInfo = 
                    HttpContext.Current.Session["loggeduser"] as AuthenticatedUserInfo;
                return (UserTypeEnum)authenticatedUserInfo.UserRoleEnum;
            }
        }

        internal static string GetContextSessionLoggedUserID()
        {
            object xLock = new object();
            lock (xLock)
            {
                AuthenticatedUserInfo authenticatedUserInfo =
                    HttpContext.Current.Session["loggeduser"] as AuthenticatedUserInfo;
                return authenticatedUserInfo?.UserId;
            }
        }
    }
}