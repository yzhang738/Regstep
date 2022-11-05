using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSToolKit.Domain.Entities
{
    public class FormTemplate : IRSData
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

        public List<Form> Forms { get; set; }

        public FormTemplate()
        {
            UId = Guid.NewGuid();
            Forms = new List<Form>();
        }
    }
}
