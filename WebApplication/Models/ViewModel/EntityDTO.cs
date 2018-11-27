using Generic.Obfuscation.TripleDES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace WebApplication.Models.ViewModel
{
    public class EntityDTOHelper
    {
        public static U GetEntityDTO<T, U>(T t)
        {
            string sPropName = string.Empty, tPropName = string.Empty;
            try
            {
                if (t != null)
                {
                    U uDTO = Activator.CreateInstance<U>();

                    PropertyInfo[] uProps = uDTO.GetType().GetProperties(),
                        tProps = t.GetType().GetProperties();

                    foreach (PropertyInfo propertyInfo in tProps)
                    {
                        if (propertyInfo.CanRead)
                        {
                            PropertyInfo uProp = uProps.FirstOrDefault(u => u.Name == propertyInfo.Name && u.CanWrite);

                            if (uProp != null)
                            {
                                sPropName = $"T.{propertyInfo.Name}:{propertyInfo.PropertyType.Name}";
                                tPropName = $"U.{uProp.Name}:{uProp.PropertyType.Name}";

                                if (!(propertyInfo.Name.ToLower().EndsWith("id") && 
                                    ((propertyInfo.PropertyType == typeof(System.Int32)) || 
                                    ((propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) && 
                                    Nullable.GetUnderlyingType(propertyInfo.PropertyType) == typeof(System.Int32)))))
                                {
                                    if (uProp.Name == "OwnerType" && (uProp.ReflectedType.FullName == "WebApplication.Models.AddressViewModelDTO" || 
                                        uProp.ReflectedType.FullName == "WebApplication.Models.AddressViewModel"))
                                    {
                                        int? tPropVal = (int?)propertyInfo.GetValue(t, null);

                                        uProp.SetValue(
                                            uDTO,
                                            tPropVal == null
                                                ? null
                                                : DataSecurityTripleDES.GetEncryptedText(tPropVal.Value)); 
                                    }
                                    else
                                    {
                                        uProp.SetValue(uDTO, propertyInfo.GetValue(t, null));
                                    }
                                }
                                else
                                {
                                    int? tPropVal = (int?)propertyInfo.GetValue(t, null);

                                    uProp.SetValue(
                                        uDTO,
                                        tPropVal == null 
                                            ? null 
                                            : DataSecurityTripleDES.GetEncryptedText(tPropVal.Value));
                                }
                            }
                        }
                    }

                    return uDTO;
                }

                return default(U);
            }
            catch (Exception e)
            {
                string issueWith = $"{sPropName} -> {tPropName}";
                throw e;
            }
        }

        internal static T2 MapToEntity<T1, T2>(T1 sourceDTO, T2 targetEntity, bool createInstance = false)
        {
            try
            {
                if (sourceDTO != null)
                {
                    if (targetEntity == null && createInstance)
                    {
                        targetEntity = Activator.CreateInstance<T2>();
                    }

                    PropertyInfo[] sourceProps = sourceDTO.GetType().GetProperties(),
                        targetProps = targetEntity?.GetType().GetProperties();

                    if (targetProps?.Length > 0)
                    {
                        foreach (PropertyInfo propertyInfo in targetProps)
                        {
                            if (propertyInfo.CanWrite)
                            {
                                PropertyInfo uProp = sourceProps.FirstOrDefault(u => u.Name == propertyInfo.Name && u.CanRead);
                                if (uProp != null)
                                {
                                    if (!(propertyInfo.Name.ToLower().EndsWith("id") &&
                                        ((propertyInfo.PropertyType == typeof(System.Int32)) ||
                                        ((propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) &&
                                        Nullable.GetUnderlyingType(propertyInfo.PropertyType) == typeof(System.Int32)))))
                                    {
                                        object objValue = uProp.GetValue(
                                            sourceDTO,
                                            null);

                                        if (objValue != null)
                                        {
                                            if (propertyInfo.PropertyType == uProp.PropertyType)
                                            {
                                                propertyInfo.SetValue(
                                                    targetEntity,
                                                    objValue);
                                            }
                                            else
                                            {
                                                if (propertyInfo.Name == "OwnerType" &&
                                                    (propertyInfo.ReflectedType.FullName == "WebApplication.DAL.Address" || 
                                                    propertyInfo.ReflectedType.FullName == "WebApplication.Models.ViewModel.AddressDTO"))
                                                {
                                                    int? iVal = int.Parse(DataSecurityTripleDES.GetPlainText(
                                                        uProp.GetValue(
                                                            sourceDTO,
                                                            null)));

                                                    propertyInfo.SetValue(
                                                        targetEntity,
                                                        iVal);
                                                }
                                                else
                                                {
                                                    propertyInfo.SetValue(targetEntity,
                                                        Convert.ChangeType(
                                                            objValue,
                                                            propertyInfo.PropertyType));
                                                }
                                            }
                                        }
                                    }
                                    else 
                                    {
                                        object objValue = DataSecurityTripleDES.GetPlainText(
                                            uProp.GetValue(
                                                sourceDTO,
                                                null));

                                        if (objValue != null)
                                        {
                                            if (((propertyInfo.PropertyType.IsGenericType &&
                                                propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) &&
                                                Nullable.GetUnderlyingType(propertyInfo.PropertyType) == typeof(System.Int32)))
                                            {
                                                propertyInfo.SetValue(targetEntity,
                                                    (int?)int.Parse(objValue.ToString()));
                                            }
                                            else
                                            {
                                                propertyInfo.SetValue(targetEntity,
                                                    Convert.ChangeType(objValue,
                                                    propertyInfo.PropertyType));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return targetEntity;
                    }
                }

                return default(T2);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }

    [Serializable]
    public class UserDTO
    {
        public string Id { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string EmailAddress { get; set; }
        public string ContactNumber { get; set; }
        public Nullable<int> UserStatus { get; set; }
        public Nullable<int> UserTypeFlag { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public string ResetCode { get; set; }
        public Nullable<System.DateTime> ResetCodeSentAt { get; set; }
        public Nullable<System.DateTime> ResetCodeExpiry { get; set; }
        public string AddressId { get; set; }
        public string ProfileAssetUrl { get; set; }
    }

    [Serializable]
    public class AddressDTO
    {
        public string Id { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string RegionId { get; set; }
        public string Postcode { get; set; }
        public string CountryId { get; set; }
        public string OwnerType { get; set; }
        public string OwnerId { get; set; }
        public Nullable<int> AddressStatus { get; set; }

        public string RegionAlias { get; set; }
        public string RegionName { get; set; }

        public string CountryName { get; set; }
    }

    [Serializable]
    public class RegionDTO
    {
        public string Id { get; set; }
        public string RegionName { get; set; }
        public string CountryISO2 { get; set; }
        public string RegionAlias { get; set; }
    }

    [Serializable]
    public class CountryDTO
    {
        public string ISO2 { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }

    [Serializable]
    public class MixedBagDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brief { get; set; }
        public string Detail { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<bool> IsTaxable { get; set; }
        public Nullable<decimal> TaxRate { get; set; }

        public int r_Id { get; set; }
        public string r_Name { get; set; }

        public int ra_Id { get; set; }
        public string City { get; set; }
        public Nullable<int> RegionId { get; set; }
        public string Postcode { get; set; }
    }
}