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

// Complete
namespace RSToolKit.Domain.Entities.Components
{
    public class CheckboxItem : Component, IComponentItem, IComparable<CheckboxItem>
    {
        public Guid CheckboxGroupKey { get; set; }

        [ForeignKey("CheckboxGroupKey")]
        public virtual CheckboxGroup CheckboxGroup { get; set; }

        [NotMapped]
        public Component Parent { get { return CheckboxGroup; } }

        [NotMapped]
        public Guid ParentKey { get { return CheckboxGroupKey; } }


        public CheckboxItem() : base()
        {
        }

        public static CheckboxItem New(FormsRepository repository, CheckboxGroup component, Company company, User user, Guid? owner = null, Guid? group = null, string name = null, string permission = "770", string description = "")
        {
            var node = new CheckboxItem()
            {
                UId = Guid.NewGuid(),
                Name = name == null ? "New checkbox item - " + DateTime.UtcNow.ToString("MM/dd/yyyy h:mm tt") + "." : name,
                LabelText = name == null ? "" : name,
                Description = description,
                Company = company,
                CompanyKey = company.UId
            };
            component.Items.Sort((a, b) => a.Order - b.Order);
            var lastItem = component.Items.LastOrDefault();
            var order = 1;
            if (lastItem != null)
                order = lastItem.Order + 1;
            node.Order = order;
            component.Items.Add(node);
            repository.Commit();
            return node;
        }

        public override IComponent DeepCopy(Panel panel, IEnumerable<Audience> audiences = null, IEnumerable<Seating> seatings = null)
        {
            var item = new CheckboxItem();
            DeepCopyStuff(item, panel, audiences, seatings);
            item.Panel = null;
            item.PanelKey = null;
            return item;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType().Equals(this.GetType()))
            {
                return ((CheckboxItem)obj).UId.Equals(UId);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return UId.GetHashCode();
        }

        public int CompareTo(CheckboxItem other)
        {
            return this.Order - other.Order;
        }

    }
}
