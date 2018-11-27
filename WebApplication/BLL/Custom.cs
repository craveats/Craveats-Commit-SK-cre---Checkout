using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.BLL
{
    public static class GenUtil
    {
        public static string GetExceptionStringForReporting(Exception exception)
        {
            object xLock = new object();
            lock(xLock)
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append("Please find exception detail listed below.");
                stringBuilder.AppendFormat("<br/><br/><em>Exception FullName: {0}</em><br/><em>{1}</em>", exception.GetType().FullName, exception.ToString());
                stringBuilder.Replace(Environment.NewLine, "<br/>");

                return stringBuilder.ToString();
            }
        }

        // src:: https://nimblegecko.com/using-simple-drop-down-lists-in-ASP-NET-MVC/
        // visited :: 2018 11 18
        // This is one of the most important parts in the whole example.
        // This function takes a list of strings and returns a list of SelectListItem objects.
        // These objects are going to be used later in the SignUp.html template to render the
        // DropDownList.
        public static IEnumerable<SelectListItem> GetSelectListItems(IEnumerable<string> elements)
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

    }

}