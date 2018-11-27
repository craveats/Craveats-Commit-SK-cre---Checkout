using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.DAL;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class CartController : Controller
    {
        private CraveatsDbContext db = new CraveatsDbContext();
        MenuModel menuModel = new MenuModel();

        // GET: Cart
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddToCart(int id)
        {
            if (Session["cart"] == null)
            {
                List<Item> cart = new List<Item>();
                cart.Add(new Item { Menu = menuModel.find(id)});
                Session["cart"] = cart;
            }
            else
            {
                List<Item> cart = (List<Item>)Session["cart"];
                int index = isExist(id);

                if (index != -1)
                {
                    
                }
                else
                {
                    cart.Add(new Item { Menu = menuModel.find(id) });
                }
                Session["cart"] = cart;
            }
            return RedirectToAction("Index");
        }

        public ActionResult Remove(int id)
        {
            List<Item> cart = (List<Item>)Session["cart"];
            int index = isExist(id);
            cart.RemoveAt(index);
            Session["cart"] = cart;
            return RedirectToAction("Index");
        }

        private int isExist(int id)
        {
            List<Item> cart = (List<Item>)Session["cart"];
            for (int i = 0; i < cart.Count; i++)
                if (cart[i].Menu.Id.Equals(id))
                    return i;
            return -1;
        }
    }
}