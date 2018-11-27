using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Generic.Obfuscation.TripleDES;
using PagedList;

using WebApplication.BLL;
using WebApplication.Common;
using WebApplication.DAL;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class RestaurantMenuController : Controller
    {
        private CraveatsDbContext db = new CraveatsDbContext();

        // GET: RestaurantMenu
        public ViewResult Index(

         string ownerType, string ownerId, 
         string sortOrder, string currentFilter, string searchString, int? page)
        {
            if (ownerType != null && ownerId != null)
            {
                ViewBag.ownerType = ownerType;
                ViewBag.ownerId = ownerId;
            }
            else
            {
                ownerType = DataSecurityTripleDES.GetEncryptedText((int) SessionManager.GetContextSessionOwnerType());
                ownerId = SessionManager.GetContextSessionLoggedUserID();

                ViewBag.ownerType = ownerType;
                ViewBag.ownerId = ownerId;
            }


            ViewBag.CurrentSort = sortOrder;

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            int? filterId = int.Parse(DataSecurityTripleDES.GetPlainText(ownerId));
            var RestaurantMenus = SessionManager.GetContextSessionOwnerType() == UserTypeEnum.PartnerRestaurant 
                ? from s in db.RestaurantMenu 
                  where s.OwnerId == filterId && s.OwnerType == 2 && s.ServiceStatus == 1
                  select s
                : from s in db.RestaurantMenu
                  select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                RestaurantMenus = RestaurantMenus.Where(s => s.Name.Contains(searchString)
                                       || s.Detail.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    RestaurantMenus = RestaurantMenus.OrderByDescending(s => s.Name);
                    break;
                case "Date":
                    RestaurantMenus = RestaurantMenus.OrderBy(s => s.LastUpdated);
                    break;
                case "date_desc":
                    RestaurantMenus = RestaurantMenus.OrderByDescending(s => s.LastUpdated);
                    break;
                default:  // Name ascending 
                    RestaurantMenus = RestaurantMenus.OrderBy(s => s.Name);
                    break;
            }

            int pageSize = 25;
            int pageNumber = (page ?? 1);
            return View(RestaurantMenus.ToPagedList(pageNumber, pageSize));
        }

        // GET: RestaurantMenu/Create
        public ActionResult Create(string ownerType = null, string ownerId = null)
        {
            if (ownerType != null && ownerId != null)
            {
                ViewBag.ownerType = ownerType;
                ViewBag.ownerId = ownerId;
            }
            else
            {
                ownerType = DataSecurityTripleDES.GetEncryptedText((int)SessionManager.GetContextSessionOwnerType());
                ownerId = SessionManager.GetContextSessionLoggedUserID();

                ViewBag.ownerType = ownerType;
                ViewBag.ownerId = ownerId;
            }
            return View();
        }

        // POST: RestaurantMenu/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name, Brief, Detail, UnitPrice")]RestaurantMenu restaurantMenu, string ownerType = null, string ownerId = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //int.Parse(DataSecurityTripleDES.GetPlainText(SessionManager.GetContextSessionLoggedUserID()));
                    db.RestaurantMenu.Add(restaurantMenu);
                    db.SaveChanges();

                    restaurantMenu.DateAdded = DateTime.Now;
                    db.SaveChanges();

                    restaurantMenu.ServiceStatus = (int?)Common.ServiceStatusEnum.Active;
                    restaurantMenu.OwnerId = int.Parse(
                        DataSecurityTripleDES.GetPlainText(
                            ownerId));
                    restaurantMenu.OwnerType = (int)Common.OwnerTypeEnum.ServiceProvider;
                    restaurantMenu.IsTaxable = true;
                    restaurantMenu.TaxRate = 13m;
                    restaurantMenu.LastUpdated = DateTime.Now;
                    db.SaveChanges();

                    Restaurant ownerRestaurant = db.Restaurant.FirstOrDefault(u => u.Id == restaurantMenu.OwnerId &&
                    (u.ServiceProviderStatus.HasValue &&
                    u.ServiceProviderStatus.Value == (int)Common.ServiceProviderStatusEnum.Inactive) &&
                    u.AddressId.HasValue);
                    if (ownerRestaurant != null)
                    {
                        ownerRestaurant.ServiceProviderStatus = (int)Common.ServiceProviderStatusEnum.Active;
                        db.SaveChanges();
                    }

                    return RedirectToAction("Index", new { ownerType = ownerType, ownerId = ownerId }); 
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(restaurantMenu);
        }

        // GET: RestaurantMenu/Details/5
        public ActionResult Details(string id, string ownerType = null, string ownerId = null)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RestaurantMenu restaurantMenu = db.RestaurantMenu.Find(
                int.Parse(DataSecurityTripleDES.GetPlainText(id)));

            if (restaurantMenu == null)
            {
                return HttpNotFound();
            }

            return View(restaurantMenu);
        }

        // GET: RestaurantMenu/Edit/5
        public ActionResult Edit(string id, string ownerType = null, string ownerId = null)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RestaurantMenu restaurantMenu = db.RestaurantMenu.Find(int.Parse(DataSecurityTripleDES.GetPlainText(id)));
            if (restaurantMenu == null)
            {
                return HttpNotFound();
            }
            return View(restaurantMenu);
        }

        // POST: RestaurantMenu/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(string id, string ownerType = null, string ownerId = null)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var RestaurantMenuToUpdate = db.RestaurantMenu.Find(int.Parse(DataSecurityTripleDES.GetPlainText(id)));
            if (TryUpdateModel(RestaurantMenuToUpdate, "",
               new string[] { "Name, Brief, Detail, UnitPrice" }))
            {
                try
                {
                    db.SaveChanges();

                    return RedirectToAction("Index", new { ownerType = ownerType, ownerId = ownerId });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(RestaurantMenuToUpdate);
        }

        // GET: RestaurantMenu/Delete/5
        public ActionResult Delete(string id, bool? saveChangesError = false, string ownerType = null, string ownerId = null)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }
            var RestaurantMenuToUpdate = db.RestaurantMenu.Find(int.Parse(DataSecurityTripleDES.GetPlainText(id)));
            if (RestaurantMenuToUpdate == null)
            {
                return HttpNotFound();
            }
            return View(RestaurantMenuToUpdate);
        }

        // POST: RestaurantMenu/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id, string ownerType = null, string ownerId = null)
        {
            try
            {
                RestaurantMenu restaurantMenu = db.RestaurantMenu.Find(int.Parse(DataSecurityTripleDES.GetPlainText(id)));
                db.RestaurantMenu.Remove(restaurantMenu);
                db.SaveChanges();
            }
            catch (RetryLimitExceededException/* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new {
                    id = DataSecurityTripleDES.GetEncryptedText(id),
                    saveChangesError = true,
                    ownerType = ownerType, 
                    ownerId = ownerId
                });
            }
            return RedirectToAction("Index", new { ownerType = ownerType, ownerId = ownerId });
        }
    }
}