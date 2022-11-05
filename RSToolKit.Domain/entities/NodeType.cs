using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities.Email;

namespace RSToolKit.Domain.Entities
{
    public class NodeType : INode
    {

        #region INode

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(3)]
        public string Permission { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid Owner { get; set; }
        public Guid Group { get; set; }
        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        #endregion

        public virtual List<Form> Forms { get; set; }
        public virtual List<EmailCampaign> EmailCampaigns { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        [MaxLength(1000)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }


        public NodeType()
        {
            UId = Guid.NewGuid();
            Description = "";
            Forms = new List<Form>();
            EmailCampaigns = new List<EmailCampaign>();
        }

    }
}
