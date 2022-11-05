using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities
{
    public class RegistrantNote : IRegistrantItem
    {
        [ForeignKey("Registrant")]
        public Guid UId { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        public virtual Registrant Registrant { get; set; }

        [Required]
        public string Note { get; set; }

        public Guid ModifiedBy { get; set; }
        public DateTimeOffset Date { get; set; }

        public RegistrantNote()
        {
            Note = "";
            ModifiedBy = Guid.Empty;
            Date = DateTimeOffset.UtcNow;
        }

        public Form GetForm()
        {
            return Registrant.GetForm();
        }

        public INode GetNode()
        {
            return Registrant.Form;
        }
    }
}
