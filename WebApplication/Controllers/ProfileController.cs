using Generic.Obfuscation.TripleDES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.BLL;
using WebApplication.DAL;
using WebApplication.Common;
using WebApplication.Models;
using WebApplication.Models.ViewModel;
using PagedList;

namespace WebApplication.Controllers
{
    [Authorize, Tls]
    public class ProfileController : Controller
    {
        public ProfileController()
        {
            //SessionManager.RegisterSessionActivity();
        }

        // GET: Profile
        public ActionResult Index()
        {
            ProfileViewModel model = new ProfileViewModel();
            return View("ProfileView", model);
        }

        public ActionResult ProfileView(ProfileViewModel model)
        {
            model = new ProfileViewModel();

            if (Session != null && Session.Contents != null)
            {
                AuthenticatedUserInfo authenticatedUserInfo = Session["loggeduser"] as AuthenticatedUserInfo;

                if (authenticatedUserInfo != null)
                {
                    UserDTO userDTO = EntityDTOHelper.GetEntityDTO<DAL.User, UserDTO>(new CEUserManager().FindById(
                        int.Parse(DataSecurityTripleDES.GetPlainText(authenticatedUserInfo.UserId))));

                    model.ModelUserType = (Common.UserTypeEnum)userDTO.UserTypeFlag;

                    return View(model);
                }
            }

            ModelState.AddModelError(string.Empty, "Session has expired");
            return View("ProfileView", null);
        }

        public ActionResult CraveatsDiner(CraveatsDinerViewModel model)
        {
            if (Session != null && Session.Contents != null)
            {
                AuthenticatedUserInfo authenticatedUserInfo = Session["loggeduser"] as AuthenticatedUserInfo;

                if (authenticatedUserInfo != null)
                {
                    UserDTO userDTO = EntityDTOHelper.GetEntityDTO<DAL.User, UserDTO>(new CEUserManager().FindById(
                        int.Parse(DataSecurityTripleDES.GetPlainText(authenticatedUserInfo.UserId))));

                    CraveatsDinerViewModel craveatsDinerViewModel = null;

                    if (((Common.UserTypeEnum)userDTO.UserTypeFlag).HasFlag(Common.UserTypeEnum.CraveatsDiner))
                    {
                        craveatsDinerViewModel = new CraveatsDinerViewModel()
                        {
                            Id = userDTO.Id,
                            ContactNumber = userDTO.ContactNumber,
                            Email = userDTO.EmailAddress,
                            FirstName = userDTO.FirstName,
                            Surname = userDTO.Surname,
                            Role = Common.UserTypeEnum.CraveatsDiner.GetDescription()
                        };
                    }

                    DataProvider dataProvider = new DataProvider();
                    if (userDTO.AddressId?.Length > 0)
                    {
                        DAL.Address anAddress = dataProvider.FindAddressById(
                            int.Parse(DataSecurityTripleDES.GetPlainText(userDTO.AddressId)));

                        AddressViewModel addressViewModel = EntityDTOHelper.GetEntityDTO<DAL.Address, AddressViewModel>(anAddress);

                        if (anAddress != null)
                        {
                            DAL.Region region = dataProvider.FindRegionById(anAddress.RegionId ?? 0);

                            if (region != null)
                            {
                                addressViewModel.RegionAlias = region.RegionAlias;
                                addressViewModel.RegionId = DataSecurityTripleDES.GetEncryptedText(region.Id);
                            }

                            craveatsDinerViewModel.Addresses = new List<AddressViewModel>() { addressViewModel };
                        }
                    }
                    else {
                        craveatsDinerViewModel.Addresses = new List<AddressViewModel>() {};
                    }

                    return View("CraveatsDiner", craveatsDinerViewModel);
                }

            }

            return View("Error");
        }

        public ActionResult PartnerRestaurant(PartnerRestaurantViewModel model)
        {
            if (Session != null && Session.Contents != null)
            {
                AuthenticatedUserInfo authenticatedUserInfo = Session["loggeduser"] as AuthenticatedUserInfo;

                if (authenticatedUserInfo != null)
                {
                    UserDTO userDTO = EntityDTOHelper.GetEntityDTO<DAL.User, UserDTO>(new CEUserManager().FindById(
                        int.Parse(DataSecurityTripleDES.GetPlainText(authenticatedUserInfo.UserId))));

                    PartnerRestaurantViewModel partnerRestaurantViewModel = null;

                    if (((Common.UserTypeEnum)userDTO.UserTypeFlag).HasFlag(Common.UserTypeEnum.PartnerRestaurant))
                    {
                        partnerRestaurantViewModel = new PartnerRestaurantViewModel()
                        {
                            Id = userDTO.Id,
                            ContactNumber = userDTO.ContactNumber,
                            Email = userDTO.EmailAddress,
                            FirstName = userDTO.FirstName,
                            Surname = userDTO.Surname,
                            Role = Common.UserTypeEnum.PartnerRestaurant.GetDescription()
                        };
                    }


                    DataProvider dataProvider = new DataProvider();

                    DAL.Address anAddress = dataProvider.FindAddressById(
                        int.Parse(DataSecurityTripleDES.GetPlainText(userDTO.AddressId)));

                    AddressViewModel addressViewModel = EntityDTOHelper.GetEntityDTO<DAL.Address, AddressViewModel>(anAddress);

                    if (anAddress != null)
                    {
                        DAL.Region region = dataProvider.FindRegionById(anAddress.RegionId ?? 0);

                        if (region != null)
                        {
                            addressViewModel.RegionAlias = region.RegionAlias;
                            addressViewModel.RegionId = DataSecurityTripleDES.GetEncryptedText(region.Id);
                        }

                        partnerRestaurantViewModel.Addresses = new List<AddressViewModel>() { addressViewModel };
                    }

                    return View("PartnerRestaurant", partnerRestaurantViewModel);

                }

            }

            return View("Error");
        }

        public ActionResult EditPartnerProfile(string identifier)
        {
            if (Session != null && Session.Contents != null)
            {
                AuthenticatedUserInfo authenticatedUserInfo = Session["loggeduser"] as AuthenticatedUserInfo;

                if (authenticatedUserInfo != null)
                {
                    UserDTO userDTO = EntityDTOHelper.GetEntityDTO<DAL.User, UserDTO>(new CEUserManager().FindById(
                        int.Parse(DataSecurityTripleDES.GetPlainText(authenticatedUserInfo.UserId))));

                    PartnerRestaurantViewModel partnerRestaurantViewModel = null;

                    if (((Common.UserTypeEnum)userDTO.UserTypeFlag).HasFlag(Common.UserTypeEnum.PartnerRestaurant))
                    {
                        partnerRestaurantViewModel = new PartnerRestaurantViewModel()
                        {
                            Id = userDTO.Id,
                            ContactNumber = userDTO.ContactNumber,
                            Email = userDTO.EmailAddress,
                            FirstName = userDTO.FirstName,
                            Surname = userDTO.Surname,
                            Role = Common.UserTypeEnum.PartnerRestaurant.GetDescription()
                        };
                    }
                    return View("EditPartnerProfile", partnerRestaurantViewModel);
                }
            }
            return View("Error");
        }

        [HttpPost, Tls, ValidateAntiForgeryToken]
        public ActionResult EditPartnerProfile(PartnerRestaurantViewModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                CEUserManager ceUserManager = new CEUserManager();
                if (model.Id?.Length > 0)
                {
                    UserDTO userDTO = new UserDTO()
                    {
                        Id = model.Id,
                        FirstName = model.FirstName,
                        Surname = model.Surname,
                        ContactNumber = model.ContactNumber,
                        LastUpdated = DateTime.Now
                    };

                    ceUserManager.SaveUserDetail(userDTO);

                    return RedirectToAction("PartnerRestaurant", "Profile");
                }

                ModelState.AddModelError(string.Empty, "Save attempt failed.");

                //ModelState.AddModelError(string.Empty, "Login attempt failed.");
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e);
            }
            return this.View(model);
        }

        public ActionResult EditDinerProfile(string identifier)
        {
            if (Session != null && Session.Contents != null)
            {
                AuthenticatedUserInfo authenticatedUserInfo = Session["loggeduser"] as AuthenticatedUserInfo;

                if (authenticatedUserInfo != null)
                {
                    UserDTO userDTO = EntityDTOHelper.GetEntityDTO<DAL.User, UserDTO>(new CEUserManager().FindById(
                        int.Parse(DataSecurityTripleDES.GetPlainText(authenticatedUserInfo.UserId))));

                    CraveatsDinerViewModel craveatsDinerViewModel = null;

                    if (((Common.UserTypeEnum)userDTO.UserTypeFlag).HasFlag(Common.UserTypeEnum.CraveatsDiner))
                    {
                        craveatsDinerViewModel = new CraveatsDinerViewModel()
                        {
                            Id = userDTO.Id,
                            ContactNumber = userDTO.ContactNumber,
                            Email = userDTO.EmailAddress,
                            FirstName = userDTO.FirstName,
                            Surname = userDTO.Surname,
                            Role = Common.UserTypeEnum.CraveatsDiner.GetDescription()
                        };
                    }
                    return View("EditDinerProfile", craveatsDinerViewModel);
                }
            }
            return View("Error");
        }

        [HttpPost, Tls, ValidateAntiForgeryToken]
        public ActionResult EditDinerProfile(CraveatsDinerViewModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                CEUserManager ceUserManager = new CEUserManager();
                if (model.Id?.Length > 0)
                {
                    UserDTO userDTO = new UserDTO()
                    {
                        Id = model.Id,
                        FirstName = model.FirstName,
                        Surname = model.Surname,
                        ContactNumber = model.ContactNumber,
                        LastUpdated = DateTime.Now
                    };

                    ceUserManager.SaveUserDetail(userDTO);

                    return RedirectToAction("CraveatsDiner", "Profile");
                }

                ModelState.AddModelError(string.Empty, "Save attempt failed.");

                //ModelState.AddModelError(string.Empty, "Login attempt failed.");
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e);
            }
            return this.View(model);
        }

        [HttpGet(), Route("Profile/EditAddress/{id}")]
        public ActionResult EditAddress(string id)
        {
            SessionManager.RegisterSessionActivity();

            if (Session != null && Session.Contents != null)
            {
                AuthenticatedUserInfo authenticatedUserInfo = Session["loggeduser"] as AuthenticatedUserInfo;

                if (authenticatedUserInfo != null)
                {
                    UserDTO userDTO = EntityDTOHelper.GetEntityDTO<DAL.User, UserDTO>(new CEUserManager().FindById(
                        int.Parse(DataSecurityTripleDES.GetPlainText(authenticatedUserInfo.UserId))));

                    if (((Common.UserTypeEnum)userDTO.UserTypeFlag).HasFlag(Common.UserTypeEnum.CraveatsDiner) ||
                        ((Common.UserTypeEnum)userDTO.UserTypeFlag).HasFlag(Common.UserTypeEnum.PartnerRestaurant))
                    {
                        DataProvider dataProvider = new DataProvider();
                        AddressDTO addressDTO = EntityDTOHelper.GetEntityDTO<DAL.Address, AddressDTO>(
                            dataProvider.FindAddressById(int.Parse(DataSecurityTripleDES.GetPlainText(id))));

                        if (addressDTO != null)
                        {
                            RegionDTO regionDTO = addressDTO.RegionId?.Trim().Length <= 0 
                                ? null
                                : EntityDTOHelper.GetEntityDTO<DAL.Region, RegionDTO>(
                                dataProvider.FindRegionById(
                                    int.Parse(DataSecurityTripleDES.GetPlainText(addressDTO.RegionId))));

                            if (regionDTO != null) {
                                addressDTO.RegionAlias = regionDTO.RegionAlias;
                                addressDTO.RegionName = regionDTO.RegionName;
                            }

                            CountryDTO countryDTO = addressDTO.CountryId?.Trim().Length <= 0 
                                ? null 
                                : EntityDTOHelper.GetEntityDTO<DAL.Country, CountryDTO>(
                                dataProvider.FindCountryById(
                                    int.Parse(DataSecurityTripleDES.GetPlainText(addressDTO.CountryId))));

                            if (countryDTO != null) {
                                addressDTO.CountryName = countryDTO.Name;
                            }
                            
                        }

                        IEnumerable<string> regionAliases = GetAllRegionAliases();

                        AddressViewModel addressViewModel = new AddressViewModel()
                        {
                            Id = addressDTO.Id,
                            City = addressDTO.City,
                            Line1 = addressDTO.Line1,
                            Line2 = addressDTO.Line2,
                            Postcode = addressDTO.Postcode,
                            RegionAlias = addressDTO.RegionAlias,
                            RegionAliases = GenUtil.GetSelectListItems(regionAliases)
                        };

                        return View("EditAddress", addressViewModel);
                    }
                }
            }
            return View("Error");
        }

        [HttpPost, Tls]
        public ActionResult EditAddress(AddressViewModel model, string returnUrl)
        {
            SessionManager.RegisterSessionActivity();

            IEnumerable<string> regionAliases = GetAllRegionAliases();
            model.RegionAliases = GenUtil.GetSelectListItems(regionAliases);

            if (ModelState.IsValid)
            {

                DataProvider dataProvider = new DataProvider();

                DAL.Address address = dataProvider.FindAddressById(
                     int.Parse(DataSecurityTripleDES.GetPlainText(model.Id)));

                if (address != null)
                {
                    AddressDTO addressDTO = new AddressDTO()
                    {
                        Id = model.Id,
                        City = model.City,
                        Line1 = model.Line1,
                        Line2 = model.Line2,
                        Postcode = model.Postcode,
                        RegionAlias = model.RegionAlias
                    };

                    using (DAL.CraveatsDbContext c = new DAL.CraveatsDbContext())
                    {

                        addressDTO.RegionId = DataSecurityTripleDES.GetEncryptedText(
                            c.Region.FirstOrDefault(r => r.CountryISO2 == "CA" &&
                                r.RegionAlias == addressDTO.RegionAlias).Id);

                        addressDTO.CountryId = DataSecurityTripleDES.GetEncryptedText(
                            c.Country.FirstOrDefault(s => s.ISO2 == "CA").Id);

                        address = c.Address.FirstOrDefault(u => u.Id == address.Id);
                        address = EntityDTOHelper.MapToEntity<AddressDTO, DAL.Address>(addressDTO, address);

                        c.SaveChanges();

                        return View("ProfileView", new ProfileViewModel());
                    }
                }
            }

            // Something is not right - so render the registration page again,
            // keeping the data user has entered by supplying the model.
            return View("EditAddress", model);
        }

        [HttpGet(), Route("Profile/AddAddress"), Route("Profile/AddAddress?ownerType={ownerType}&ownerId={ownerId}")]
        public ActionResult AddAddress(string ownerType = null, string ownerId = null)
        {
            if ((ownerType ?? string.Empty).Length > 0 && (ownerId ?? string.Empty).Length > 0)
            {
                ViewBag.AlterButtonTitle = true;
                ViewBag.AlteredButtonName = "Next";
            }
            
            SessionManager.RegisterSessionActivity();

            if (Session != null && Session.Contents != null)
            {
                AuthenticatedUserInfo authenticatedUserInfo = Session["loggeduser"] as AuthenticatedUserInfo;

                if (authenticatedUserInfo != null)
                {
                    UserDTO userDTO = EntityDTOHelper.GetEntityDTO<DAL.User, UserDTO>(new CEUserManager().FindById(
                        int.Parse(DataSecurityTripleDES.GetPlainText(authenticatedUserInfo.UserId))));

                    if (((Common.UserTypeEnum)userDTO.UserTypeFlag).HasFlag(Common.UserTypeEnum.CraveatsDiner) ||
                        ((Common.UserTypeEnum)userDTO.UserTypeFlag).HasFlag(Common.UserTypeEnum.PartnerRestaurant))
                    {
                        IEnumerable<string> regionAliases = GetAllRegionAliases();

                        AddressViewModel addressViewModel = new AddressViewModel()
                        {
                            RegionAliases = GenUtil.GetSelectListItems(regionAliases),
                            OwnerId = ownerId,
                            OwnerType = ownerType
                        };

                        return View("AddAddress", addressViewModel);
                    }
                }
            }
            return View("Error");
        }

        [HttpPost, Tls]
        public ActionResult AddAddress(AddressViewModel model, string returnUrl)
        {
            SessionManager.RegisterSessionActivity();

            IEnumerable<string> regionAliases = GetAllRegionAliases();
            model.RegionAliases = GenUtil.GetSelectListItems(regionAliases);

            if (ModelState.IsValid)
            {
                AuthenticatedUserInfo authenticatedUserInfo = Session["loggeduser"] as AuthenticatedUserInfo;
                if (authenticatedUserInfo != null)
                {
                    int ownerType = model.OwnerType?.Length > 0
                        ? int.Parse(DataSecurityTripleDES.GetPlainText(model.OwnerType))
                        : -1;

                    int ownerId = model.OwnerId?.Length > 0
                        ? int.Parse(DataSecurityTripleDES.GetPlainText(model.OwnerType))
                        : -1;

                    DAL.User addressOwner = null;
                    if (!(ownerType > -1 && ownerId > 0))
                    {
                        addressOwner = new CEUserManager().FindById(
                            int.Parse(DataSecurityTripleDES.GetPlainText(authenticatedUserInfo.UserId)));
                    }

                    DataProvider dataProvider = new DataProvider();
                    AddressDTO addressDTO = new AddressDTO()
                    {
                        City = model.City,
                        Line1 = model.Line1,
                        Line2 = model.Line2,
                        Postcode = model.Postcode,
                        RegionAlias = model.RegionAlias
                    };

                    if (addressOwner != null && !addressOwner.AddressId.HasValue)
                    {
                        addressDTO.OwnerType = DataSecurityTripleDES.GetEncryptedText((int)Common.OwnerTypeEnum.User);
                        addressDTO.OwnerId = authenticatedUserInfo.UserId;

                        using (DAL.CraveatsDbContext c = new DAL.CraveatsDbContext())
                        {
                            addressDTO.RegionId = DataSecurityTripleDES.GetEncryptedText(
                                c.Region.FirstOrDefault(r => r.CountryISO2 == "CA" &&
                                    r.RegionAlias == addressDTO.RegionAlias).Id);

                            addressDTO.CountryId = DataSecurityTripleDES.GetEncryptedText(
                                c.Country.FirstOrDefault(s => s.ISO2 == "CA").Id);

                            DAL.Address newAddress = EntityDTOHelper.MapToEntity<AddressDTO, DAL.Address>(
                                addressDTO, null, true);
                            newAddress.AddressStatus = (int?)Common.AddressStatusEnum.Active;

                            c.Entry(newAddress).State = System.Data.Entity.EntityState.Added;

                            c.SaveChanges();

                            addressOwner = c.User.FirstOrDefault(u => u.Id == newAddress.OwnerId.Value);

                            addressOwner.AddressId = newAddress.Id;
                            addressOwner.LastUpdated = DateTime.Now;

                            c.SaveChanges();

                            return RedirectToAction("ProfileView", "Profile");
                        }
                    }
                    else if (ownerType > -1 && ownerId > 0) {
                        addressDTO.OwnerType = DataSecurityTripleDES.GetEncryptedText(ownerType);
                        addressDTO.OwnerId = model.OwnerId;

                        using (DAL.CraveatsDbContext c = new DAL.CraveatsDbContext())
                        {
                            addressDTO.RegionId = DataSecurityTripleDES.GetEncryptedText(
                                c.Region.FirstOrDefault(r => r.CountryISO2 == "CA" &&
                                    r.RegionAlias == addressDTO.RegionAlias).Id);

                            addressDTO.CountryId = DataSecurityTripleDES.GetEncryptedText(
                                c.Country.FirstOrDefault(s => s.ISO2 == "CA").Id);

                            DAL.Address newAddress = EntityDTOHelper.MapToEntity<AddressDTO, DAL.Address>(
                                addressDTO, null, true);
                            newAddress.AddressStatus = (int?)Common.AddressStatusEnum.Active;

                            c.Entry(newAddress).State = System.Data.Entity.EntityState.Added;

                            c.SaveChanges();

                            DAL.Restaurant restaurant = c.Restaurant.FirstOrDefault(u => u.Id == newAddress.OwnerId.Value);

                            restaurant.AddressId = newAddress.Id;
                            restaurant.LastUpdated = DateTime.Now;

                            c.SaveChanges();

                            return RedirectToAction("Index", "RestaurantMenu", new
                            {
                                ownerType = DataSecurityTripleDES.GetEncryptedText((int)Common.OwnerTypeEnum.ServiceProvider),
                                ownerId = DataSecurityTripleDES.GetEncryptedText(restaurant.Id)
                            });
                        }
                    }
                    ModelState.AddModelError("", "An address exists for this owner.");
                }
            }

            // Something is not right - so render the registration page again,
            // keeping the data user has entered by supplying the model.
            return View(model);
        }

        private IEnumerable<string> GetAllRegionAliases()
        {
            List<string> items = new List<string>();
            using (DAL.CraveatsDbContext c = new DAL.CraveatsDbContext())
            {
                var regions = c.Region.Where(u => u.CountryISO2 == "CA").OrderBy(v=>v.RegionAlias).ToList();
                foreach (var x in regions)
                {
                    items.Add(x.RegionAlias);
                }
            }

            return items;
        }
    }
}