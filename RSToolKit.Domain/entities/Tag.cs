using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities.Email;

namespace RSToolKit.Domain.Entities
{
    public class Tag : IRSData, INodeItem
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }

        public Guid ModifiedBy { get; set; }

        [MaxLength(1000)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }
        public virtual List<Form> Forms { get; set; }
        public virtual List<EmailCampaign> EmailCampaigns { get; set; }
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        public Tag()
            : base()
        {
            Description = "";
            Forms = new List<Form>();
            EmailCampaigns = new List<EmailCampaign>();
        }
        
        public static Tag New(FormsRepository repository, Company company, User user, string name, string description = "", Guid? owner = null, Guid? group = null)
        {
            var node = new Tag()
            {
                UId = Guid.NewGuid(),
                Name = name ?? "New Tag - " + DateTimeOffset.UtcNow.ToString("MM/dd/yyyy h:mm tt") + ".",
                Description = description
            };
            company.Tags.Add(node);
            repository.Commit();
            return node;
        }

        public INode GetNode()
        {
            return Company as INode;
        }

        public override bool Equals(object obj)
        {
            if (obj is Tag)
                if (((Tag)obj).UId == UId)
                    return true;
            return false;
        }

        public override int GetHashCode()
        {
            return UId.GetHashCode();
        }

    }
}
