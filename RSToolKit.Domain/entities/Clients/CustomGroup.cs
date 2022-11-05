using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities.Clients
{
    public class CustomGroup : INodeItem
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [JsonIgnore]
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        [JsonIgnore]
        public virtual List<User> Users { get; set; }

        public string Name { get; set; }

        public CustomGroup()
        {
            Name = "";
        }

        public static CustomGroup New(Company company)
        {
            var group = new CustomGroup();
            group.UId = Guid.NewGuid();
            group.Company = company;
            group.CompanyKey = company.UId;
            return group;
        }

        public static CustomGroup New(Guid companyKey)
        {
            var group = new CustomGroup();
            group.UId = Guid.NewGuid();
            group.CompanyKey = companyKey;
            return group;
        }

        public INode GetNode()
        {
            return Company;
        }
    }
}
