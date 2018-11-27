using Generic.Obfuscation.TripleDES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

    }
}