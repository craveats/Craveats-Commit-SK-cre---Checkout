﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApplication.DAL
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    [Serializable]
    public partial class CraveatsDbContext : DbContext
    {
        public CraveatsDbContext()
            : base("name=EFDMConnSettings")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Address> Address { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<CategoryRelation> CategoryRelation { get; set; }
        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<Country> Country { get; set; }
        public virtual DbSet<Enquiry> Enquiry { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderDetail> OrderDetail { get; set; }
        public virtual DbSet<OrderFreight> OrderFreight { get; set; }
        public virtual DbSet<OrderPayment> OrderPayment { get; set; }
        public virtual DbSet<Region> Region { get; set; }
        public virtual DbSet<Search> Search { get; set; }
        public virtual DbSet<SessionTracking> SessionTracking { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Vendor> Vendor { get; set; }
        public virtual DbSet<Restaurant> Restaurant { get; set; }
        public virtual DbSet<RestaurantMenu> RestaurantMenu { get; set; }
    
        public virtual ObjectResult<AuthenticateUser_Result> AuthenticateUser(string emailAddress, string password)
        {
            var emailAddressParameter = emailAddress != null ?
                new ObjectParameter("emailAddress", emailAddress) :
                new ObjectParameter("emailAddress", typeof(string));
    
            var passwordParameter = password != null ?
                new ObjectParameter("password", password) :
                new ObjectParameter("password", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<AuthenticateUser_Result>("AuthenticateUser", emailAddressParameter, passwordParameter);
        }
    
        public virtual ObjectResult<Nullable<bool>> DoesRegistrantExist(string emailAddress)
        {
            var emailAddressParameter = emailAddress != null ?
                new ObjectParameter("emailAddress", emailAddress) :
                new ObjectParameter("emailAddress", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<bool>>("DoesRegistrantExist", emailAddressParameter);
        }
    
        public virtual ObjectResult<GetUserByEmail_Result> GetUserByEmail(string emailAddress)
        {
            var emailAddressParameter = emailAddress != null ?
                new ObjectParameter("emailAddress", emailAddress) :
                new ObjectParameter("emailAddress", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetUserByEmail_Result>("GetUserByEmail", emailAddressParameter);
        }
    
        public virtual ObjectResult<GetMenuItem_Result> GetMenuItem(string searchterm, Nullable<int> orderby)
        {
            var searchtermParameter = searchterm != null ?
                new ObjectParameter("searchterm", searchterm) :
                new ObjectParameter("searchterm", typeof(string));
    
            var orderbyParameter = orderby.HasValue ?
                new ObjectParameter("orderby", orderby) :
                new ObjectParameter("orderby", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetMenuItem_Result>("GetMenuItem", searchtermParameter, orderbyParameter);
        }

        public System.Data.Entity.DbSet<WebApplication.DAL.GetMenuItem_Result> GetMenuItem_Result { get; set; }
    }
}