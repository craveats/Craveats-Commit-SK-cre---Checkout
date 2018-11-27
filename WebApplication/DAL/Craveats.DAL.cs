using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApplication.Models;

namespace WebApplication.DAL
{
    public class CraveatsContext : DbContext
    {
        private CraveatsContext(EntityConnection entityConnection) : base(existingConnection: entityConnection, contextOwnsConnection: false)
        {
            
        }

        private static string GetSqlConnectionProviderString()
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
            sqlConnectionStringBuilder.DataSource = Properties.Settings.Default.DataServer;
            sqlConnectionStringBuilder.PersistSecurityInfo = false;
            if (Properties.Settings.Default.UseIntegratedSecurity)
            {
                sqlConnectionStringBuilder.IntegratedSecurity = true;
            }
            else
            {
                sqlConnectionStringBuilder.UserID = Properties.Settings.Default.DSUserID;
                sqlConnectionStringBuilder.Password = Properties.Settings.Default.DSPassword;
            }
            sqlConnectionStringBuilder.InitialCatalog = Properties.Settings.Default.InitialCatalog;
            sqlConnectionStringBuilder.MultipleActiveResultSets = Properties.Settings.Default.MultipleActiveResultSets;

            return sqlConnectionStringBuilder.ToString();
        }

        private static string GetEntityConnectionString()
        {
            EntityConnectionStringBuilder entityBuilder =
                new EntityConnectionStringBuilder();
            entityBuilder.Provider = "System.Data.SqlClient";
            entityBuilder.ProviderConnectionString = GetSqlConnectionProviderString();
            entityBuilder.Metadata = @"res://*/"; 
            /* string.Format(@"res://*\/{0}Model.csdl|
                            res://*\/{0}Model.ssdl|
                            res://*\/{0}Model.msl", Properties.Settings.Default.InitialCatalog)*/;
            return entityBuilder.ToString();
        }

        private static EntityConnection GetEntityConnection()
        {
            return new EntityConnection()
            {
                ConnectionString = GetEntityConnectionString()
            };    
        }

        public static CraveatsContext CraveatsDbContext()
        {
            return new CraveatsContext(GetEntityConnection());
        }

        public virtual DbSet<Country> Countries { get; set; }
    }
}