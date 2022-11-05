using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Entities.Components
{
    public class NumberSelector : Component, IVariableHolder
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public NumberSelector()
            : base()
        {
        }

        public static NumberSelector New(FormsRepository repository, Panel panel, Company company, User user, int min = 0, int max = 10, Guid? owner = null, Guid? group = null, string name = null, string permission = "770", string description = "", int row = int.MaxValue, int order = int.MaxValue)
        {
            var node = new NumberSelector()
            {
                UId = Guid.NewGuid(),
                Name = name == null ? "New numberselector - " + DateTime.UtcNow.ToString("MM/dd/yyyy h:mm tt") + "." : name,
                LabelText = name == null ? "" : name,
                Description = description,
                Row = row,
                Order = order,
                Company = company,
                CompanyKey = company.UId,
                Min = min,
                Max = max
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
            var comp = new NumberSelector();
            comp.Min = Min;
            comp.Max = Max;
            DeepCopyStuff(comp, panel, audiences, seatings);
            return comp;
        }
    }
}
