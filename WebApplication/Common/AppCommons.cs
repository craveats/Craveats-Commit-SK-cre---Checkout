namespace WebApplication.Common
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;

    internal enum CommonStatusEnum
    {
        Inactive = 0,
        Active = 1,
        Pending = 2, 
        Deleted = 3,
    }

    internal enum AddressStatusEnum 
    {
        Inactive = CommonStatusEnum.Inactive,
        Active = CommonStatusEnum.Active,
        Deleted = CommonStatusEnum.Deleted,
    }

    internal enum CategoryStatusEnum
    {
        Inactive = CommonStatusEnum.Inactive,
        Active = CommonStatusEnum.Active,
        Deleted = CommonStatusEnum.Deleted,
    }

    internal enum CategoryRelationStatusEnum
    {
        Inactive = CommonStatusEnum.Inactive,
        Active = CommonStatusEnum.Active,
        Deleted = CommonStatusEnum.Deleted,
    }

    public enum OwnerTypeEnum
    {
        Unknown = 0,
        User = 1,
        ServiceProvider = 2,
        Vendor = 3,
        
    }

    internal enum ServiceStatusEnum
    {
        Inactive = CommonStatusEnum.Inactive,
        Active = CommonStatusEnum.Active,
        Retired = CommonStatusEnum.Deleted,
    }

    internal enum ServiceProviderStatusEnum
    {
        Inactive = CommonStatusEnum.Inactive,
        Active = CommonStatusEnum.Active,
        Blocked = 2,
        Deleted = CommonStatusEnum.Deleted,
        PendingReview = 4
    }

    [Flags]
    public enum UserTypeEnum
    {
        /// <summary>
        /// Unspecified 
        /// </summary>
        [Description("Unspecified")]
        Unspecified = 0,
        /// <summary>
        /// Craveats admin 
        /// </summary>
        [Description("Craveats admin")]
        CraveatsAdmin = 1,
        /// <summary>
        /// Partner restaurant
        /// </summary>
        [Description("Partner restaurant")]
        PartnerRestaurant = CraveatsAdmin << 1,
        /// <summary>
        /// Craveats diner
        /// </summary>
        [Description("Craveats diner")]
        CraveatsDiner = PartnerRestaurant << 1
    }

    internal enum UserStatusEnum {
        Inactive = CommonStatusEnum.Inactive,
        Active = CommonStatusEnum.Active,
        Blocked = 2,
        Deleted = CommonStatusEnum.Deleted, 
        MustResetPassword = 4
    }

    public static class CommonUtility
    {
        // src :: https://stackoverflow.com/questions/4367723/get-enum-from-description-attribute
        public static string GetDescription(this Enum value, bool inherit = false)
        {
            MemberInfo member = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute
                    = Attribute.GetCustomAttribute(member, typeof(DescriptionAttribute), inherit)
                        as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static T GetEnumValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException();
            FieldInfo[] fields = type.GetFields();
            var field = fields
                            .SelectMany(f => f.GetCustomAttributes(
                                typeof(DescriptionAttribute), false), (
                                    f, a) => new { Field = f, Att = a })
                            .Where(a => ((DescriptionAttribute)a.Att)
                                .Description == description).SingleOrDefault();
            return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
        }
    }
}