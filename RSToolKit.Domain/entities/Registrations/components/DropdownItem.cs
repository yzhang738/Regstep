using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Clients;

// Complete
namespace RSToolKit.Domain.Entities.Components
{
    public class DropdownItem : Component, IComponentItem, IComparable<DropdownItem>
    {
        public Guid DropdownGroupKey { get; set; }

        [ForeignKey("DropdownGroupKey")]
        public virtual DropdownGroup DropdownGroup { get; set; }

        [NotMapped]
        public Component Parent { get { return DropdownGroup; } }

        [NotMapped]
        public Guid ParentKey { get { return DropdownGroupKey; } }

        public DropdownItem() : base()
        {
        }

        public static DropdownItem New(FormsRepository repository, DropdownGroup component, Company company, User user, Guid? owner = null, Guid? group = null, string name = null, string permission = "770", string description = "")
        {
            var node = new DropdownItem()
            {
                UId = Guid.NewGuid(),
                Name = name == null ? "New dropdown item - " + DateTime.UtcNow.ToString("MM/dd/yyyy h:mm tt") + "." : name,
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
            var item = new DropdownItem();
            DeepCopyStuff(item, panel, audiences, seatings);
            item.Panel = null;
            item.PanelKey = null;
            return item;

        }

        public override bool Equals(object obj)
        {
            if (obj.GetType().Equals(this.GetType()))
            {
                return ((DropdownItem)obj).UId.Equals(UId);
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

        public int CompareTo(DropdownItem other)
        {
            return this.Order - other.Order;
        }

    }
}
