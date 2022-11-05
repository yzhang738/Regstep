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
    public class CheckboxGroup : Component, IComponentMultipleSelection, IVariableHolder, IComponentSurveyMappable
    {
        public bool TimeExclusion { get; set; }
        [MaxLength(1000)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DialogText { get; set; }
        public int ItemsPerRow { get; set; }
        public AgendaDisplay AgendaDisplay { get; set; }

        [JsonIgnore]
        [CascadeDelete]
        public virtual List<CheckboxItem> Items { get; set; }

        [NotMapped]
        [JsonIgnore]
        public IEnumerable<IComponentItem> Children
        {
            get
            {
                return Items.AsEnumerable<IComponentItem>();
            }
        }


        public CheckboxGroup()
            : base()
        {
            TimeExclusion = false;
            DialogText = "";
            ItemsPerRow = 1;
            Items = new List<CheckboxItem>();
        }

        public static CheckboxGroup New(FormsRepository repository, Panel panel, User user, Company company, Guid? owner = null, Guid? group = null, string permission = "770", string name = null, string description = null, int row = int.MaxValue, int order = int.MaxValue)
        {
            var node = new CheckboxGroup()
            {
                UId = Guid.NewGuid(),
                Name = name == null ? "New checkbox group - " + DateTimeOffset.UtcNow.ToString("MM/dd/yyyy h:mm tt") + "." : name,
                LabelText = name == null ? "" : name,
                Description = description == null ? "New checkbox group created by " + user.UserName + " on " + DateTimeOffset.UtcNow.ToString("mm/dd/yyy h:mm tt") : description,
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
            audiences = audiences ?? new List<Audience>();
            seatings = seatings ?? new List<Seating>();
            var comp = new CheckboxGroup();
            DeepCopyStuff(comp, panel, audiences, seatings);
            comp.TimeExclusion = TimeExclusion;
            comp.DialogText = DialogText;
            comp.ItemsPerRow = ItemsPerRow;
            comp.AgendaDisplay = AgendaDisplay;
            foreach (var item in Items)
            {
                var itm = item.DeepCopy(panel, audiences, seatings) as CheckboxItem;
                comp.Items.Add(itm);
                itm.CheckboxGroup = comp;
            }
            return comp;
        }
    }

    public enum AgendaDisplay
    {
        [StringValue("Date & Time")]
        DateAndTime = 0,
        [StringValue("Date Only")]
        Date = 1,
        [StringValue("Time Only")]
        Time = 2
    }
}
