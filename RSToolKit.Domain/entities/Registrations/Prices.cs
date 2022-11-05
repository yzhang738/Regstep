using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Clients;

//COMPLETE
namespace RSToolKit.Domain.Entities
{
    [Table("Prices")]
    public class Prices : IRSData, IFormItem
    {

        public virtual List<Audience> Audiences { get; set; }

        [CascadeDelete]
        public virtual List<Price> Price { get; set; }

        [ForeignKey("PriceGroupKey")]
        public virtual PriceGroup PriceGroup { get; set; }
        public Guid PriceGroupKey { get; set; }

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

        public Prices()
        {
            Price = new List<Price>();
            Audiences = new List<Audience>();
        }

        public static Prices New(FormsRepository repository, PriceGroup priceGroup, Company company, User user, string name, Guid? owner = null, Guid? group = null, string permission = "770")
        {
            var node = new Prices()
            {
                UId = Guid.NewGuid(),
                Name = name ?? "New Prices - " + DateTimeOffset.UtcNow.ToString("MM/dd/yyyy h:mm tt") + ".",
            };
            priceGroup.Prices.Add(node);
            repository.Commit();
            return node;
        }

        public Form GetForm()
        {
            return PriceGroup.GetForm();
        }

        public INode GetNode()
        {
            return PriceGroup.GetNode();
        }

        public Prices DeepCopy(PriceGroup priceGroup, IEnumerable<Audience> audiences = null)
        {
            audiences = audiences ?? new List<Audience>();
            var prices = new Prices();
            prices.UId = Guid.NewGuid();
            prices.Name = Name;
            prices.PriceGroupKey = priceGroup.UId;
            prices.PriceGroup = priceGroup;
            prices.DateCreated = DateTimeOffset.UtcNow;
            prices.DateModified = prices.DateCreated;
            priceGroup.Prices.Add(prices);
            foreach (var aud in Audiences)
            {
                var t_aud = audiences.FirstOrDefault(a => a.Label == aud.Label);
                if (t_aud != null)
                    prices.Audiences.Add(t_aud);
            }
            foreach (var price in Price)
            {
                var pr = price.DeepCopy(prices);
            }
            return prices;
        }

    }
}