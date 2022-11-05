using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;

//COMPLETE
namespace RSToolKit.Domain.Entities
{
    [Table("Price")]
    public class Price : IComparable<Price>, IFormItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [ForeignKey("PricesKey")]
        public virtual Prices Prices { get; set; }
        public Guid PricesKey { get; set; }

        public decimal? Amount { get; set; }
        public DateTimeOffset Start { get; set; }

        public Price()
        {
            Start = DateTimeOffset.UtcNow;
            Amount = null;
        }
        
        public static Price New(FormsRepository repository, Prices prices, Company company, User user, DateTimeOffset? start = null, decimal? amount = null, Guid? owner = null, Guid? group = null)
        {
            var node = new Price()
            {
                UId = Guid.NewGuid(),
                Amount = amount,
                Start = start.HasValue ? start.Value : DateTimeOffset.UtcNow.AddDays(-5)
            };
            prices.Price.Add(node);
            repository.Commit();
            return node;
        }

        public Form GetForm()
        {
            return Prices.GetForm();
        }

        public INode GetNode()
        {
            return Prices.GetNode();
        }

        public int CompareTo(Price other)
        {
            return Start.CompareTo(other.Start);
        }

        public Price DeepCopy(Prices prices)
        {
            var price = new Price();
            prices.Price.Add(price);
            price.Prices = prices;
            price.PricesKey = prices.UId;
            price.Amount = Amount;
            price.Start = Start;
            price.UId = Guid.NewGuid();
            return price;
        }
    }
}
