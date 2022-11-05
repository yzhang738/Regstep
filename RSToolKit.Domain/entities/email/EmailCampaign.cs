using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Security;

// Complete
namespace RSToolKit.Domain.Entities.Email
{
    public class EmailCampaign : IPointerTarget, IEmailHolder, ICustomTextHolder, IBaseItem
    {
        [NotMapped]
        public string[] roles
        {
            get
            {
                return new string[] { "Email Builders" };
            }
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        [MaxLength(3)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Permission { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid Owner { get; set; }
        public Guid Group { get; set; }
        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        [ForeignKey("TypeKey")]
        public virtual NodeType Type { get; set; }
        public Guid? TypeKey { get; set; }

        [CascadeDelete]
        public virtual List<RSEmail> Emails { get; set; }
        [CascadeDelete]
        public virtual List<RSHtmlEmail> HtmlEmails { get; set; }
        [CascadeDelete]
        public virtual List<CustomText> CustomTexts { get; set; }
        [CascadeDelete]
        public virtual List<LogicBlock> ContentLogic { get; set; }
        [CascadeDelete]
        public virtual List<Tag> Tags { get; set; }

        [NotMapped]
        public IEnumerable<IEmail> AllEmails
        {
            get
            {
                var t_emails = new List<IEmail>();
                t_emails.AddRange(Emails);
                t_emails.AddRange(HtmlEmails);
                return t_emails;
            }
        }


        [MaxLength(5000)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }

        [NotMapped]
        public IEnumerable<IContentItem> ContentItems
        {
            get
            {
                var t_contents = new List<IContentItem>();
                t_contents.AddRange(CustomTexts);
                return t_contents;
            }
        }

        public EmailCampaign()
        {
            Emails = new List<RSEmail>();
            Description = "";
            CustomTexts = new List<CustomText>();
            Tags = new List<Tag>();
            Name = "New Email Campaign";
            HtmlEmails = new List<RSHtmlEmail>();
        }

        public static EmailCampaign New(FormsRepository repository, Company company, User user, Guid? owner = null, Guid? group = null, string permission = "770", string name = "New Email Campaign", string description = "")
        {
            var date = DateTimeOffset.UtcNow;
            var campaign = new EmailCampaign()
            {
                Company = company,
                CompanyKey = company.UId,
                Owner = owner.HasValue ? owner.Value : user.UId,
                Group = group.HasValue ? group.Value : company.UId,
                Permission = permission,
                Name = name + " created on " + date.ToString("d/m/yyyy h:mm tt") + " by " + user.UserName,
                Description = description,
                DateCreated = date,
                DateModified = date
            };
            PermissionSet.CreateDefaultPermissions(repository, campaign, company.UId);
            repository.Add(campaign);
            return campaign;
        }

    }
}
