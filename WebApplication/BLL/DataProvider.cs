using Generic.Obfuscation.SHA1;
using Generic.Obfuscation.TripleDES;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using WebApplication.Common;
using WebApplication.DAL;
using WebApplication.DAL.DBCommon;
using WebApplication.Models;
using WebApplication.Models.ViewModel;

namespace WebApplication
{
    public class DataProvider
    {
        public Address FindAddressById(int id)
        {
            Address result = null;
            try
            {
                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    result = craveatsDbContext.Address.Find(id);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            return result;
        }

        public DAL.Region FindRegionById(int id)
        {
            Region result = null;
            try
            {
                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    result = craveatsDbContext.Region.Find(id);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            return result;
        }

        public DAL.Country FindCountryById(int id)
        {
            DAL.Country result = null;
            try
            {
                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    result = craveatsDbContext.Country.Find(id);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            return result;
        }

        internal Address FindActiveByCriteria(int ownerId, int ownerType)
        {
            Address result = null;

            try
            {
                using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
                {
                    result = craveatsDbContext.Address.Where(u => (u.AddressStatus.HasValue &&
                        u.AddressStatus.Value == (int)Common.AddressStatusEnum.Active) &&
                        (u.OwnerType.HasValue && u.OwnerType.Value == ownerType) &&
                        (u.OwnerId.HasValue && u.OwnerId == ownerId)).OrderBy(u => u.Id).ThenBy(v => v.Id).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }
    }
}