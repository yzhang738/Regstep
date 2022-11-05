using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Components;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Clients;

// Complete
namespace RSToolKit.Domain.Entities.Components
{
    public class DropdownGroup : Component, IComponentItemParent, IVariableHolder, IComponentSurveyMappable
    {
        [JsonIgnore]
        [CascadeDelete]
        public virtual List<DropdownItem> Items { get; set; }

        [NotMapped]
        [JsonIgnore]
        public IEnumerable<IComponentItem> Children
        {
            get
            {
                return Items.AsEnumerable<IComponentItem>();
            }
        }

        public DropdownGroup() : base()
        {
            Items = new List<DropdownItem>();
        }

        public static DropdownGroup New(FormsRepository repository, Panel panel, Company company, User user, Guid? owner = null, Guid? group = null, string name = null, string permission = "770", string description = "", int row = int.MaxValue, int order = int.MaxValue)
        {
            var node = new DropdownGroup()
            {
                UId = Guid.NewGuid(),
                Name = name == null ? "New dropdown group - " + DateTime.UtcNow.ToString("MM/dd/yyyy h:mm tt") + "." : name,
                LabelText = name == null ? "" : name,
                Description = description,
                Row = row,
                Order = order,
                Company = company,
                CompanyKey = company.UId
            };
            node.Variable = new Variable()
            {
                Form = panel.Page.Form,
                FormKey = panel.Page.FormKey,
                Value = node.UId.ToString()
            };
            panel.Components.Add(node);
            repository.Commit();
            return node;
        }

        public override IComponent DeepCopy(Panel panel, IEnumerable<Audience> audiences = null, IEnumerable<Seating> seatings = null)
        {
            var comp = new DropdownGroup();
            DeepCopyStuff(comp, panel, audiences, seatings);
            foreach (var item in Items)
            {
                var itm = item.DeepCopy(panel, audiences, seatings) as DropdownItem;
                comp.Items.Add(itm);
                itm.DropdownGroup = comp;
            }
            return comp;

        }
    }
}
