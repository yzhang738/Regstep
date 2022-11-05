using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Entities.Email;
using System.Text.RegularExpressions;
using RSToolKit.Domain;

namespace RSToolKit.Domain.Entities.Clients
{
    public class Company
        : IPersonHolder, INamedNode
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(5000)]
        public string Description { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        public long ContactLimit { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company ParentCompany { get; set; }
        public Guid? CompanyKey { get; set; }

        public string BillingAddressLine1 { get; set; }
        public string BillingAddressLine2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingCountry { get; set; }
        public string BillingZip { get; set; }

        public string ShippingAddressLine1 { get; set; }
        public string ShippingAddressLine2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingZip { get; set; }

        public string RegistrationAddressLine1 { get; set; }
        public string RegistrationAddressLine2 { get; set; }
        public string RegistrationCity { get; set; }
        public string RegistrationState { get; set; }
        public string RegistrationCountry { get; set; }
        public string RegistrationZip { get; set; }
        public string RegistrationPhone { get; set; }
        public string RegistrationFax { get; set; }
        public string RegistrationEmail { get; set; }



        [CascadeDelete]
        public virtual List<TinyUrl> TinyUrls { get; set; }
        [CascadeDelete]
        public virtual List<Tag> Tags { get; set; }
        [CascadeDelete]
        public virtual List<NodeType> Types { get; set; }
        [CascadeDelete]
        public virtual List<Form> Forms { get; set; }
        [CascadeDelete]
        public virtual List<EmailCampaign> EmailCampaigns { get; set; }
        [CascadeDelete]
        public virtual List<User> Users { get; set; }
        [CascadeDelete]
        public virtual List<SavedList> SavedLists { get; set; }
        [CascadeDelete]
        public virtual List<ContactReport> DynamicList { get; set; }
        [CascadeDelete]
        public virtual List<Folder> Folders { get; set; }
        [CascadeDelete]
        public virtual List<DefaultFormStyle> FormStyles { get; set; }
        [CascadeDelete]
        public virtual List<MerchantAccountInfo> MerchantAccounts { get; set; }
        [CascadeDelete]
        public virtual List<CustomGroup> CustomGroups { get; set; }
        [CascadeDelete]
        public virtual List<AvailableRoles> AvailableRoles { get; set; }
        [CascadeDelete]
        public virtual List<AdvancedInventoryReport> AdvancedInventoryReports { get; set; }
        [CascadeDelete]
        public virtual List<Contact> Contacts { get; set; }
        [ClearKeyOnDelete("WorkingCompanyKey")]
        public virtual List<User> WorkingUsers { get; set; }
        [CascadeDelete]
        public virtual List<ContactHeader> ContactHeaders { get; set; }
        /// <summary>
        /// The logo of the company.
        /// </summary>
        public virtual List<CompanyLogo> Logo { get; set; }

        [NotMapped]
        public IEnumerable<IPerson> Persons
        {
            get
            {
                return Contacts.AsEnumerable();
            }
        }

        public Company()
        {
            DateCreated = DateModified = DateTimeOffset.Now;
            Name = "New Company";
            Description = "";
            ContactLimit = 100;
            Tags = new List<Tag>();
            Types = new List<NodeType>();
            Users = new List<User>();
            SavedLists = new List<SavedList>();
            Folders = new List<Folder>();
            FormStyles = new List<DefaultFormStyle>();
            MerchantAccounts = new List<MerchantAccountInfo>();
            CustomGroups = new List<CustomGroup>();
            AvailableRoles = new List<AvailableRoles>();
            AdvancedInventoryReports = new List<AdvancedInventoryReport>();
            Contacts = new List<Contact>();
            WorkingUsers = new List<User>();
            CompanyKey = null;
            Logo = new List<CompanyLogo>();
        }

        public static Company New(FormsRepository repository, User user, string name = "New Company", int contactLimit = 100, string database = null, string description = null)
        {
            var company = new Company()
            {
                Name = name,
                ContactLimit = contactLimit,
                Description = description == null ? "New company created by " + user.UserName + "." : description,
                UId = Guid.NewGuid()
            };
            PermissionSet.CreateDefaultPermissions(repository, company, company.UId);
            foreach (var style in repository.Search<DefaultFormStyle>(s => s.CompanyKey == null))
            {
                var item = new DefaultFormStyle()
                {
                    GroupName = style.GroupName,
                    Name = style.Name,
                    Sort = style.Sort,
                    SubSort = style.SubSort,
                    Type = style.Type,
                    Value = style.Value,
                    Variable = style.Variable,
                    CompanyKey = company.UId
                };
                company.FormStyles.Add(item);
            }
            repository.Add(company);
            repository.Commit();
            return company;
        }

    }
}
