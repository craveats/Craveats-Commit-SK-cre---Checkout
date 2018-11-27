using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebApplication.BLL;

namespace WebApplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Fixing claim issue. https://stack247.wordpress.com/2013/02/22/antiforgerytoken-a-claim-of-type-nameidentifier-or-identityprovider-was-not-present-on-provided-claimsidentity/   
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;

        }

        protected void Application_Error()
        {
            try
            {
                Exception lastError = Server.GetLastError();

                if (!HttpContext.Current.IsDebuggingEnabled)
                {
                    StringBuilder stringBuilder = new StringBuilder(GenUtil.GetExceptionStringForReporting(lastError));

                    Server.ClearError();

                    CommunicationServiceProvider.SendAdminNotification(
                        new System.Net.Mail.MailAddress("craveats@gmail.com", "Craveats Notification"),
                        "An application error occurred.",
                        stringBuilder.ToString()
                        );
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e);
            }
        }
    }
}
