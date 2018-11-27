using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication.DAL;
using Generic.Obfuscation.TripleDES;
using WebApplication.Models.ViewModel;

namespace WebApplication.Models
{
    [Serializable]
    public class AuthenticatedUserInfo
    {
        private UserDTO _seed;

        public AuthenticatedUserInfo(UserDTO authenticateUser_Result)
        {
            _seed = authenticateUser_Result; 
        }

        public string FullName {
            get {
                return string.Format("{0}{1}{2}", _seed?.FirstName, " ", _seed?.Surname).Trim();
            }
        }

        public string UserId {
            get {
                return _seed?.Id;// DataSecurityTripleDES.GetEncryptedText(_seed?.Id);
            }
        }

        public int UserRoleEnum {
            get {
                return (_seed?.UserTypeFlag ?? (int?)Common.UserTypeEnum.Unspecified).Value;
            }
        }
    }
}