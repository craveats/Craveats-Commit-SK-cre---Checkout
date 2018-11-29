using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication.DAL;
using WebApplication.Models.ViewModel;

namespace WebApplication.Models
{   
    //[Serializable]
    //public class Item
    //{
    //    //private CraveatsDbContext db = new CraveatsDbContext();
    //    public RestaurantMenuCartDTO Menu
    //    {
    //        get;
    //        set;
    //    }
    //}


    [Serializable]
    public class CraveatsCart
    {
        private string _ownerId = null;
        public CraveatsCart(string ownerId) {
            _ownerId = ownerId;
        }

        public string OwnerId {
            get {
                return _ownerId;
            }
        }

        public int ItemCount { get {
                if (_Items == null || Items.Count == 0) return 0;
                return _Items.Count;
            } }

        private decimal total = 0.00m;
        private decimal totalTax = 0.00m;

        private void CalculateCartTotalAndTaxes()
        {
            {
                object xLock = new object();

                total = 0.00m;
                totalTax = 0.00m;

                lock (xLock)
                {
                    decimal unitPriceTotal = 0.00m, taxOnUnitPrice = 0.00m;

                    if (ItemCount > 0)
                    {
                        foreach (RestaurantMenuCartDTO anItem in _Items)
                        {
                            unitPriceTotal = (anItem.UnitPrice??0) * (anItem.Quantity);
                            taxOnUnitPrice = unitPriceTotal * ((!(anItem.IsTaxable??false) ? 0.00m : (anItem.TaxRate ??  13)) / 100);

                            totalTax += taxOnUnitPrice;
                            total += unitPriceTotal;// + taxOnUnitPrice;
                        }
                    }
                }
            }
        }

        public decimal CartTotalBeforeTax
        {
            get
            {
                CalculateCartTotalAndTaxes();
                return total;
            }
        }

        public decimal CartTotalTax {
            get {
                CalculateCartTotalAndTaxes();
                return totalTax;
            }
        }

        private List<RestaurantMenuCartDTO> _Items = new List<RestaurantMenuCartDTO>();
        public List<RestaurantMenuCartDTO> Items { get {
                return _Items;
            } }

        public bool AddToCart(RestaurantMenuCartDTO anItem) {
            object xLock = new object();
            bool isSuccess = false;
            lock (xLock)
            {
                if (_Items.Any(u => u.Id == anItem.Id))
                {
                    _Items.Find(u => u.Id == anItem.Id).Quantity += anItem.Quantity;
                    isSuccess = true;
                }
                else
                {
                    _Items.Add(anItem);
                    isSuccess = true;
                }
                
            }
            return isSuccess;
        }

        public int UpdateItemQuantity(string id, int newQuantity) {
            object xLock = new object();
            int curCount = int.MinValue;

            lock (xLock)
            {
                if (_Items.Any(u => u.Id == id))
                {
                    curCount = _Items.Find(u => u.Id == id).Quantity;

                    if (newQuantity > 0) {
                        curCount = curCount + newQuantity; 
                    }
                    else if (newQuantity < 0) 
                    {
                        curCount = (curCount + newQuantity) > 0 ? (curCount + newQuantity) : 0;
                    };

                    _Items.Find(u => u.Id == id).Quantity = curCount;
                }
            }
            ReviewCart();
            return curCount;
        }

        private void ReviewCart() {
            object xLock = new object();
            lock (xLock)
            {
                do
                {
                    _Items.Remove(_Items.Find(u => u.Quantity == 0));

                } while (_Items.Any(u => u.Quantity == 0));
            }
        }

        public bool RemoveItem(string id) {
            object xLock = new object();
            bool isSuccess = false;
            lock (xLock)
            {
                if (_Items.Any(u => u.Id == id))
                {
                    _Items.Remove(_Items.Find(u => u.Id == id));
                    isSuccess = true;
                }
            }
            return isSuccess;
        }

    }
}