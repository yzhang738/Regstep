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
    public class RadioGroup : Component, IComponentItemParent, IVariableHolder, IComponentSurveyMappable
    {
        public int ItemsPerRow { get; set; }

        [JsonIgnore]
        [CascadeDelete]
        public virtual List<RadioItem> Items { get; set; }

        [NotMapped]
        [JsonIgnore]
        public IEnumerable<IComponentItem> Children
        {
            get
            {
                return Items.AsEnumerable<IComponentItem>();
            }
        }


        public RadioGroup()
            : base()
        {
            ItemsPerRow = 1;
            Items = new List<RadioItem>();
        }

        public static RadioGroup New(FormsRepository repository, Panel panel, Company company, User user, Guid? owner = null, Guid? group = null, string name = null, string permission = "770", string description = "", int row = int.MaxValue, int order = int.MaxValue)
        {
            var node = new RadioGroup()
            {
                UId = Guid.NewGuid(),
                Name = name == null ? "New radio group - " + DateTime.UtcNow.ToString("MM/dd/yyyy h:mm tt") + "." : name,
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
            var comp = new RadioGroup();
            DeepCopyStuff(comp, panel, audiences, seatings);
            comp.ItemsPerRow = ItemsPerRow;
            foreach (var item in Items)
            {
                var itm = item.DeepCopy(panel, audiences, seatings) as RadioItem;
                comp.Items.Add(itm);
                itm.RadioGroup = comp;
            }
            return comp;

        }
    }
}
