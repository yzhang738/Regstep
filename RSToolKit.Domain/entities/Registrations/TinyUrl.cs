using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities
{
    public class TinyUrl
        : IFormItem
    {
        [Key]
        [ForeignKey("Form")]
        [Index(IsClustered = false)]
        public Guid UId { get; set; }
        [JsonIgnore]
        public virtual Form Form { get; set; }

        [Index(IsClustered = true)]
        public long SortingId { get; set; }

        [ForeignKey("CompanyKey")]
        [JsonIgnore]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }


        [Required]
        [Index(IsClustered = false, IsUnique = true)]
        [MaxLength(25)]
        public string Url { get; set; }

        public TinyUrl()
        {
            Url = null;
        }

        public Form GetForm()
        {
            return Form;
        }

        public INode GetNode()
        {
            return Form as INode;
        }
    }
}
