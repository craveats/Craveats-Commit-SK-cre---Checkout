using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication.DAL;

namespace WebApplication.Models
{   
    [Serializable]
    public class Item
    {
        private CraveatsDbContext db = new CraveatsDbContext();

        public RestaurantMenu Menu
        {
            get;
            set;
        }

        public int Quantity
        {
            get;
            set;
        }

    }
}