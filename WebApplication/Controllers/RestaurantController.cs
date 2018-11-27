using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication.BLL;
using WebApplication.DAL;
using PagedList;
using System.Data.Entity.Infrastructure;
using Generic.Obfuscation.TripleDES;

namespace WebApplication.Controllers
{
    [Authorize, Tls]
    public class RestaurantController : Controller
    {
        private CraveatsDbContext db = new CraveatsDbContext();

        // GET: Restaurant
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
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


            Common.UserTypeEnum userTypeEnumFromSession = SessionManager.GetContextSessionOwnerType();

            int? sessionLoggedUserId = int.Parse(DataSecurityTripleDES.GetPlainText(
                SessionManager.GetContextSessionLoggedUserID()));
            var Restaurants = (userTypeEnumFromSession == Common.UserTypeEnum.CraveatsAdmin) 
                ? from s in db.Restaurant
                  select s
                : (userTypeEnumFromSession == Common.UserTypeEnum.PartnerRestaurant)
                    ? from s in db.Restaurant
                      where s.PartnerUserId == sessionLoggedUserId
                      select s
                    : from s in db.Restaurant
                      where s.PartnerUserId == 0
                      select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                Restaurants = Restaurants.Where(s => s.Name.Contains(searchString)
                                       || s.Detail.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    Restaurants = Restaurants.OrderByDescending(s => s.Name);
                    break;
                case "Date":
                    Restaurants = Restaurants.OrderBy(s => s.LastUpdated);
                    break;
                case "date_desc":
                    Restaurants = Restaurants.OrderByDescending(s => s.LastUpdated);
                    break;
                default:  // Name ascending 
                    Restaurants = Restaurants.OrderBy(s => s.Name);
                    break;
            }

            int pageSize = 25;
            int pageNumber = (page ?? 1);
            return View(Restaurants.ToPagedList(pageNumber, pageSize));
        }

        // GET: Restaurat/Details/xxx
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Restaurant Restaurant = db.Restaurant.Find(int.Parse(DataSecurityTripleDES.GetPlainText(id)));

            if (Restaurant == null)
            {
                return HttpNotFound();
            }

            return View(Restaurant);
        }

        // GET: Restaurant/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Restaurant/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name, Detail, ContactNumber, EmailAddress")]Restaurant restaurant)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    db.Restaurant.Add(restaurant);
                    db.SaveChanges();

                    restaurant.DateAdded = DateTime.Now;
                    db.SaveChanges();

                    restaurant.ServiceProviderStatus = (int?)Common.ServiceProviderStatusEnum.Inactive;
                    restaurant.PartnerUserId = int.Parse(DataSecurityTripleDES.GetPlainText(SessionManager.GetContextSessionLoggedUserID()));
                    restaurant.LastUpdated = DateTime.Now;
                    db.SaveChanges();

                    return RedirectToAction("AddAddress", "Profile", new
                    {
                        ownerType = DataSecurityTripleDES.GetEncryptedText((int)Common.OwnerTypeEnum.ServiceProvider),
                        ownerId = DataSecurityTripleDES.GetEncryptedText(restaurant.Id)
                    });
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(restaurant);
        }

        // GET: Restaurant/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant Restaurant = db.Restaurant.Find(int.Parse(DataSecurityTripleDES.GetPlainText(id)));
            if (Restaurant == null)
            {
                return HttpNotFound();
            }
            return View(Restaurant);
        }

        // POST: Restaurant/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var RestaurantToUpdate = db.Restaurant.Find(int.Parse(DataSecurityTripleDES.GetPlainText(id)));
            if (TryUpdateModel(RestaurantToUpdate, "",
               new string[] { "LastName", "FirstMidName", "EnrollmentDate" }))
            {
                try
                {
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(RestaurantToUpdate);
        }

        // GET: Restaurant/Delete/5
        public ActionResult Delete(string id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }
            Restaurant Restaurant = db.Restaurant.Find(int.Parse(DataSecurityTripleDES.GetPlainText(id)));
            if (Restaurant == null)
            {
                return HttpNotFound();
            }
            return View(Restaurant);
        }

        // POST: Restaurant/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            try
            {
                Restaurant Restaurant = db.Restaurant.Find(int.Parse(DataSecurityTripleDES.GetPlainText(id)));
                db.Restaurant.Remove(Restaurant);
                db.SaveChanges();
            }
            catch (RetryLimitExceededException/* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
