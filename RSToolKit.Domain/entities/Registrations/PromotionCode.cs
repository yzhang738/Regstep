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
    public class PromotionCode : IFormItem
    {
        [Key]
        public Guid UId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid FormKey { get; set; }

        [CascadeDelete]
        public virtual List<PromotionCodeEntry> Entries { get; set; }

        public ShoppingCartAction Action { get; set; }
        public decimal Amount { get; set; }

        public string Code { get; set; }
        public string Description { get; set; }

        public int Limit { get; set; }

        public PromotionCode()
        {
            Action = ShoppingCartAction.Multiply;
            Amount = 1;
            Entries = new List<PromotionCodeEntry>();
            Limit = -1;
        }

        public static PromotionCode New(FormsRepository repository, Form form, User user, string code, string description = "", ShoppingCartAction action = ShoppingCartAction.Multiply, decimal amount = .9m)
        {
            var node = new PromotionCode()
            {
                UId = Guid.NewGuid(),
                Code = code ?? "New Promotion Code - " + DateTimeOffset.UtcNow.ToString("MM/dd/yyyy h:mm tt") + ".",
                Amount = amount,
                Action = action,
                Description = description
            };
            form.PromotionalCodes.Add(node);
            repository.Commit();
            return node;
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

    public enum ShoppingCartAction
    {
        [StringValue("Multiply")]
        Multiply = 0,
        [StringValue("Add")]
        Add = 1,
        [StringValue("Subtract")]
        Subtract = 2
    }
}
