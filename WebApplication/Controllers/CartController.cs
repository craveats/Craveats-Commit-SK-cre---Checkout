using Generic.Obfuscation.TripleDES;
using Stripe;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.Common;
using WebApplication.DAL;
using WebApplication.Models;
using WebApplication.Models.ViewModel;

namespace WebApplication.Controllers
{
    [Authorize, Tls]
    public class CartController : Controller
    {
        private CraveatsDbContext db = new CraveatsDbContext();

        MenuModel menuModel = new MenuModel();

        // GET: Cart
        public ActionResult Index()
        {
            
            return View();
        }

        public ActionResult AddToCart(string id)
        {
            RestaurantMenuCartDTO thisMenuDTO = EntityDTOHelper.GetEntityDTO<RestaurantMenu, RestaurantMenuCartDTO>(menuModel.find(int.Parse(DataSecurityTripleDES.GetPlainText(id))));
            if (thisMenuDTO != null)
            {
                thisMenuDTO.Quantity = 1;

                CraveatsCart craveatsCart = (Session["cart"] == null) ? new CraveatsCart() : (Session["cart"] as CraveatsCart);
                craveatsCart.AddToCart(thisMenuDTO);

                Session["cart"] = craveatsCart;
            }

            return RedirectToAction("Index");
        }

        public ActionResult FinalisePayment(string stripeToken)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var cart = Session["cart"] as CraveatsCart;

                    //if (cart != null)
                    //{
                    //    db.Order.Add(order);
                    //    //order.Id =
                    //    //order.UserId =
                    //    //order.OrderStatus = 1;
                    //    //order.DateCreated = 
                    //    //order.OrderTotal =
                    //    db.SaveChanges();

                    //    db.OrderDetail.Add(orderDetail);
                    //    //orderDetail.OrderId = 
                    //    //orderDetail.ServiceId = 
                    //    //orderDetail.ServiceOwnerId = 
                    //    //orderDetail.UnitPrice =
                    //    db.SaveChanges();

                    //}

                    //long? total = (long)order.OrderTotal;
                    // Set your secret key: remember to change this to your live secret key in production
                    // See your keys here: https://dashboard.stripe.com/account/apikeys
                    StripeConfiguration.SetApiKey("sk_test_Rg2BSmdAQkVhLwSdOZyTqHGZ");

                    // Token is created using Checkout or Elements!
                    // Get the payment token submitted by the form:
                    //var token = CraveatsCart.Token; // Using ASP.NET MVC

                    long chargeAmount = (long)(decimal.Parse(CommonUtility.DoFormat((cart.CartTotalBeforeTax + cart.CartTotalTax))) * 100);
                    var options = new ChargeCreateOptions
                    {
                        Amount = chargeAmount,
                        Currency = "cad",
                        Description = "Order Payment 20181129",
                        SourceId = stripeToken
                    };
                    var service = new ChargeService();
                    Charge charge = service.Create(options);

                    Session["cart"] = null;

                    return RedirectToAction("Success");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View();
        }

        public ActionResult Remove(string id)
        {
            RestaurantMenuCartDTO thisMenuDTO = EntityDTOHelper.GetEntityDTO<RestaurantMenu, RestaurantMenuCartDTO>(menuModel.find(int.Parse(DataSecurityTripleDES.GetPlainText(id))));
            if (thisMenuDTO != null)
            {
                CraveatsCart craveatsCart = (Session["cart"] == null) ? new CraveatsCart() : (Session["cart"] as CraveatsCart);
                craveatsCart.RemoveItem(id);

                Session["cart"] = craveatsCart;
            }

            return RedirectToAction("Index");
        }

        public ActionResult Success() {
            return View();
        }

    }
}