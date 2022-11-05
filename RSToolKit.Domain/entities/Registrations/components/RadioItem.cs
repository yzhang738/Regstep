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
    public class RadioItem : Component, IComponentItem, IComparable<RadioItem>
    {
        public Guid RadioGroupKey { get; set; }
        [ForeignKey("RadioGroupKey")]
        public virtual RadioGroup RadioGroup { get; set; }

        [NotMapped]
        public Component Parent { get { return RadioGroup; } }

        [NotMapped]
        public Guid ParentKey { get { return RadioGroupKey; } }


        public RadioItem()
        {
        }

        public static RadioItem New(FormsRepository repository, RadioGroup component, Company company, User user, Guid? owner = null, Guid? group = null, string name = null, string permission = "770", string description = "")
        {
            var node = new RadioItem()
            {
                UId = Guid.NewGuid(),
                Name = name == null ? "New radio item - " + DateTime.UtcNow.ToString("MM/dd/yyyy h:mm tt") + "." : name,
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
            var item = new RadioItem();
            DeepCopyStuff(item, panel, audiences, seatings);
            item.Panel = null;
            item.PanelKey = null;
            return item;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType().Equals(this.GetType()))
            {
                return ((RadioItem)obj).UId.Equals(UId);
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

        public int CompareTo(RadioItem other)
        {
            return this.Order - other.Order;
        }

    }
}
