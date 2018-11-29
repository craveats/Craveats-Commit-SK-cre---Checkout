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
            DAL.RestaurantMenu restaurantMenu = menuModel.find(int.Parse(DataSecurityTripleDES.GetPlainText(id)));
            RestaurantMenuCartDTO thisMenuDTO = EntityDTOHelper.GetEntityDTO<RestaurantMenu, RestaurantMenuCartDTO>(restaurantMenu);

            DAL.Restaurant restaurant = db.Restaurant.Find(restaurantMenu.OwnerId);

            thisMenuDTO.ServiceOwnerName = restaurant.Name;
            thisMenuDTO.ServiceOwnerId = DataSecurityTripleDES.GetEncryptedText(restaurant.Id);
            thisMenuDTO.ServiceOwnerType = DataSecurityTripleDES.GetEncryptedText((int)OwnerTypeEnum.ServiceProvider);

            DAL.Address address = db.Address.Find(restaurant.AddressId);
            if (address != null)
            {
                AddressDTO addressDTO = EntityDTOHelper.GetEntityDTO<DAL.Address, AddressDTO>(address);
                thisMenuDTO.ServiceOwnerAddressDetail = addressDTO.GetAddressString(true);
            }

            if (thisMenuDTO != null)
            {
                thisMenuDTO.Quantity = 1;

                CraveatsCart craveatsCart = (Session["cart"] == null) ? new CraveatsCart(SessionManager.GetContextSessionLoggedUserID()) : (Session["cart"] as CraveatsCart);
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

                    if (charge.Status == "succeeded")
                    {
                        DAL.Order newOrder = new DAL.Order()
                        {
                            DateCreated = DateTime.Now,
                            DiscountTotal = 0.0m,
                            OrderStatus = (int)OrderStatusEnum.Paid,
                            OrderTotal = cart.CartTotalBeforeTax,
                            SessionId = int.Parse(DataSecurityTripleDES.GetPlainText(SessionManager.GetContextSessionID())),
                            TaxTotal = cart.CartTotalTax,
                            UserId = int.Parse(DataSecurityTripleDES.GetPlainText(cart.OwnerId))
                        };
                        db.Order.Add(newOrder);
                        db.SaveChanges();

                        foreach (RestaurantMenuCartDTO restaurantMenuCartDTO in cart.Items)
                        {
                            db.OrderDetail.Add(new OrderDetail()
                            {
                                IsTaxable = restaurantMenuCartDTO.IsTaxable,
                                OrderId = newOrder.Id,
                                ServiceId = int.Parse(DataSecurityTripleDES.GetPlainText(restaurantMenuCartDTO.Id)),
                                ServiceOwnerId = int.Parse(DataSecurityTripleDES.GetPlainText(restaurantMenuCartDTO.ServiceOwnerId)),
                                ServiceOwnerType = int.Parse(DataSecurityTripleDES.GetPlainText(restaurantMenuCartDTO.ServiceOwnerType)),
                                TaxRate = restaurantMenuCartDTO.TaxRate,
                                UnitPrice = restaurantMenuCartDTO.UnitPrice,
                                Quantity = restaurantMenuCartDTO.Quantity,
                                Name = restaurantMenuCartDTO.Name,
                                Detail = restaurantMenuCartDTO.Detail
                            });
                            db.SaveChanges();
                        }

                        db.OrderPayment.Add(new OrderPayment()
                        {
                            DateProcessed = DateTime.Now,
                            GatewayResponseCode = charge.Id,
                            GatewayResponseVerbose = charge.Status,
                            TotalAmount = (decimal)(charge.Amount / 100.00)
                        });
                        db.SaveChanges();

                        cart = null;
                        Session["cart"] = null;

                        return View("Success", new WebApplication.Models.ViewModel.OrderConfirmationDTO() {
                            Id = DataSecurityTripleDES.GetEncryptedText(newOrder.Id),
                            StatusMessage = "success"
                        });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to accept charges. Try again, and if the problem persists please review your card detail with your bank.");
                    }
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
            if (thisMenuDTO != null && Session["cart"] != null)
            {
                CraveatsCart craveatsCart = Session["cart"] as CraveatsCart;
                craveatsCart.RemoveItem(id);

                Session["cart"] = craveatsCart;
            }

            return RedirectToAction("Index");
        }

        public ActionResult Success(OrderConfirmationDTO orderConfirmationDTO) {
            return View();
        }

    }
}