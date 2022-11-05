using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Security;
using System.Globalization;

namespace RSToolKit.Domain.Entities.Clients
{
    public class User
        : IdentityUser, IApiLogin
    {

        private string _permission;

        //Not Mapped
        /// <summary>
        /// This simply grabs the Id of the IdentityUser and converts it to a UId. This value is not stored in the database.
        /// </summary>
        [NotMapped]
        public Guid UId
        {
            get
            {
                return Guid.Parse(Id);
            }
            set
            {
                Id = value.ToString();
            }
        }
        /// <summary>
        /// This is just a representation of the timezone the user is in. This value is not stored directly in the database, but the offset is.
        /// </summary>
        [NotMapped]
        public TimeZoneInfo TimeZone { get; set; }
        /// <summary>
        /// This is the breadcrumbs of the user.  It is not directly stored in the database. It is serialized as a Json Object and then stored (<code>RawBreadCrumbs</code>).
        /// </summary>
        [NotMapped]
        public BreadCrumbs BreadCrumbs { get; set; }

        /// <summary>
        /// The list of <code>CustomGroups</code> the user belongs too.
        /// </summary>
        public virtual List<CustomGroup> CustomGroups { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }
        public DateTimeOffset PasswordResetTokenExpiration { get; set; }
        public DateTimeOffset LastPasswordFailureDate { get; set; }
        public DateTimeOffset PasswordChangeDate { get; set; }
        public DateTimeOffset LockedDate { get; set; }

        public DateTime Birthdate { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }
        public Guid ValidationToken { get; set; }
        public Guid PasswordResetToken { get; set; }
        [ForeignKey("WorkingCompanyKey")]
        public virtual Company WorkingCompany { get; set; }
        public Guid? WorkingCompanyKey { get; set; }
        public string ApiToken { get; set; }
        public DateTimeOffset? ApiTokenExpiration { get; set; }

        public string Crumbs
        {
            get
            {
                return JsonConvert.SerializeObject(BreadCrumbs.ToArray());
            }
            set
            {
                try
                {
                    Crumb[] c = JsonConvert.DeserializeObject<Crumb[]>(value);
                    BreadCrumbs = new BreadCrumbs();
                    foreach (var cr in c)
                    {
                        if (cr == null)
                            continue;
                        BreadCrumbs.Enqueue(cr);
                    }
                }
                catch (Exception)
                {
                    BreadCrumbs = new BreadCrumbs();
                }
            }
        }
        public string LockReason { get; set; }
        public string UTCOffset
        {
            get
            {
                return TimeZone.Id;
            }
            set
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById(value);
            }
        }
        public string Comment { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LastIPA { get; set; }

        public bool IsLocked { get; set; }
        public bool IsConfirmed { get; set; }

        public int PasswordFailuresSinceLastSuccess { get; set; }

        public User()
            : base()
        {
            var cD = DateTimeOffset.UtcNow;
            BreadCrumbs = new BreadCrumbs();
            DateCreated = cD;
            DateModified = cD;
            ValidationToken = Guid.Empty;
            PasswordResetToken = Guid.Empty;
            PasswordResetTokenExpiration = cD;
            LastPasswordFailureDate = cD;
            PasswordChangeDate = cD;
            Birthdate = new DateTime(1945, 6, 4);
            IsLocked = false;
            IsConfirmed = false;
            LockedDate = cD;
            LockReason = "";
            UTCOffset = "Central Standard Time";
            Comment = "New User";
            PasswordQuestion = "";
            PasswordAnswer = "";
            FirstName = "";
            LastName = "";
            PasswordFailuresSinceLastSuccess = 0;
            IsConfirmed = false;
            LastIPA = "";
            WorkingCompanyKey = null;
            EmailConfirmed = false;
            PhoneNumber = "";
            CustomGroups = new List<CustomGroup>();
            ApiToken = null;
            ApiTokenExpiration = null;
        }

        /// <summary>
        /// Gets the key of the company the user is working under.
        /// </summary>
        /// <returns>The Guid of the company being used for work.</returns>
        public Guid GetCurrentCompanyKey()
        {
            if (WorkingCompanyKey.HasValue)
                return WorkingCompanyKey.Value;
            return CompanyKey;
        }

        /// <summary>
        /// Gets the current company the user is working under.
        /// </summary>
        /// <returns>The <code>Company</code> that is being referenced for work.</returns>
        public Company GetCurrentCompany()
        {
            if (WorkingCompany != null)
                return WorkingCompany;
            return Company;
        }

    }
}
