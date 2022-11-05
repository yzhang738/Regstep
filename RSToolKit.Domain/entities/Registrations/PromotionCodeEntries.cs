using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Entities
{
    public class PromotionCodeEntry : IRegistrantItem
    {
        [Key]
        public Guid UId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [ForeignKey("CodeKey")]
        public virtual PromotionCode Code { get; set; }
        public Guid CodeKey { get; set; }

        [ForeignKey("RegistrantKey")]
        public virtual Registrant Registrant { get; set; }
        public Guid RegistrantKey { get; set; }

        public PromotionCodeEntry()
        {
        }

        public static PromotionCodeEntry New(FormsRepository repository, PromotionCode code, Registrant registrant)
        {
            var node = new PromotionCodeEntry()
            {
                UId = Guid.NewGuid()
            };
            code.Entries.Add(node);
            registrant.PromotionalCodes.Add(node);
            repository.Commit();
            return node;
        }

        public Form GetForm()
        {
            return Registrant.GetForm();
        }

        public INode GetNode()
        {
            return Registrant.GetNode();
        }
    }
}
