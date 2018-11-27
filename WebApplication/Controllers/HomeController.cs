using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.DAL;

namespace WebApplication.Controllers
{
    [Authorize, Tls]
    public class HomeController : Controller
    {
        private CraveatsDbContext db = new CraveatsDbContext();

        // GET: RestaurntMenu
        public ViewResult LocalMenuSearch(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            int sortOption = ViewBag.NameSortParm == "" ? 1 : 2;


            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var mixedBag = db.GetMenuItem(searchString, sortOption);
            //if (!String.IsNullOrEmpty(searchString))
            //{
            //    mixedBag = mixedBag.Where(s => s.Brief.Contains(searchString));
            //}
            //switch (sortOrder)
            //{
            //    case "name_desc":
            //        mixedBag = mixedBag.OrderByDescending(s => s.Name);
            //        break;
            //    default:  // Name ascending 
            //        mixedBag = mixedBag.OrderBy(s => s.Name);
            //        break;
            //}

            //int pageSize = 3000;
            //int pageNumber = (page ?? 1);
            return View(mixedBag);
        }

        public ActionResult Index()
        {
            SessionManager.RegisterSessionActivity();
            ViewBag.Message = (Session["AreaVisited"] != null ?
                ("Last visited: " + Session["AreaVisited"].ToString() + " ") : "");
            Session["AreaVisited"] = "Home/Index";
            return View();
        }

        public ActionResult About()
        {
            SessionManager.RegisterSessionActivity();
            ViewBag.Message = (Session["AreaVisited"]!=null ? 
                ("Last visited: " + Session["AreaVisited"].ToString()+ " ") : "") + "Your application description page.";
            Session["AreaVisited"] = "Home/About";
            return View();
        }

        public ActionResult Contact()
        {
            SessionManager.RegisterSessionActivity();
            ViewBag.Message = (Session["AreaVisited"] != null ?
                ("Last visited: " + Session["AreaVisited"].ToString() + " ") : "") + "Your contact page.";
            Session["AreaVisited"] = "Home/Contact";
            return View();
        }
    }
}