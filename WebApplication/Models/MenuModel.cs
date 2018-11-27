using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication.DAL;

namespace WebApplication.Models
{
    public class MenuModel
    {
        private CraveatsDbContext db = new CraveatsDbContext();

        private List<RestaurantMenu> menuItems;

        public MenuModel()
        {
            menuItems = db.RestaurantMenu.ToList();
        }

        public RestaurantMenu find(int id)
        {
            
            return menuItems.Single(m => m.Id.Equals(id));
        }
     }
}