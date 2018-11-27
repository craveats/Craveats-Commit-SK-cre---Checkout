using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplication.DAL;
using WebApplication.Models;

namespace WebApplication.Tests.DAL
{
    [TestClass]
    public class CraveatsDbContextTest
    {
        [TestMethod]
        public void GetCountry()
        {
            using (CraveatsDbContext craveatsDbContext = new CraveatsDbContext())
            {
                var iso2Param = new SqlParameter {
                    ParameterName = "@iso2",
                    Value = "CA"
                };
                //var country = craveatsContext.Database.SqlQuery<object>("select c.* from [Country] c where c.iso2 = @iso2", iso2Param).SingleOrDefaultAsync();
                var country = craveatsDbContext.Database.SqlQuery<Country>("select c.* from [Country] c where c.iso2 = @iso2", iso2Param).SingleOrDefaultAsync().Result;
            }
        }
    }
}
