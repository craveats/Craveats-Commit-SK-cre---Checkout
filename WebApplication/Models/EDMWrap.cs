namespace WebApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public partial class Settings
    {
        public int Id { get; set; }
        public string SMTPHost { get; set; }
        public bool? SMTPRequiresSSL { get; set; }
        public bool? SMTPRequiresTLS { get; set; }
        public bool? SMTPRequiresAuth { get; set; }
        public int? SMTPSSLPort { get; set; }
        public int? SMTPTLSPort { get; set; }
        public string SMTPUserName { get; set; }
        public string SMTPPassword { get; set; }
    }
}

namespace WebApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public partial class SessionTracking
    {
        public int Id { get; set; }
        public string SessionId { get; set; }
        public string IPAddress { get; set; }
        public int? UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LoggedInAt { get; set; }
        public DateTime? LoggedOutAt { get; set; }
    }
}

namespace WebApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public partial class Search
    {
        public int Id;
        public int? UseId;
        public string SearchTerm;
        public DateTime? DateCreated;
    }
}

namespace WebApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public partial class Enquiry
    {
        public int Id;

        public DateTime? DateCreated;
        public string EmailAddress;
        public int? UserId;
        public int? ReviewedBy;
        public DateTime? ReviewDate;
        public int EnquiryStatus;
        public string Notes;
    }
}

namespace WebApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Web;

    [Table("Country")]
    public partial class Country
    {
        public string ISO2;
        public string Name;
        public string ISO3;
        public int Id;
    }
}

/*
namespace WebApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
}
*/
