using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Controllers
{
    public class HoldingPageController : Controller
    {
        // GET: HoldingPage
        public ActionResult Index()
        {
            return View(HoldingProfile());
        }

        // GET: HoldingPage/Profile
        public ActionResult HoldingProfile()
        {
            return View();
        }

        // GET: HoldingPage/Messages
        public ActionResult HoldingMessages()
        {
            return View();
        }

        // GET: HoldingPage/Orders
        public ActionResult HoldingOrders()
        {
            return View();
        }

        // GET: HoldingPage/Checkout
        public ActionResult HoldingCheckout()
        {
            return View();
        }
    }
}