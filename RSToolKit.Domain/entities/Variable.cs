using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Entities.Components;
using Newtonsoft.Json;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities
{
    public class Variable : IFormItem
    {
        [JsonIgnore]
        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid FormKey { get; set; }

        [JsonIgnore]
        public virtual Component Component { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [Key, ForeignKey("Component")]
        public Guid UId { get; set; }
        [MaxLength(150)]
        [Required]
        public string Value { get; set; }

        public Variable()
        {
            Value = "";
        }

        public Form GetForm()
        {
            return Component.GetForm();
        }

        public INode GetNode()
        {
            return Component.GetNode();
        }
    }
}
