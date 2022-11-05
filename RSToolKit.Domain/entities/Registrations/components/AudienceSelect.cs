using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

//COMPLETE
namespace RSToolKit.Domain.Entities.Components
{
    public class AudienceSelect : Component
    {
              
        public AudienceStyle Style { get; set; }
        
        public AudienceSelect() : base()
        {
            Style = AudienceStyle.LinearButton;
        }

        public override IComponent DeepCopy(Panel panel, IEnumerable<Audience> audiences = null, IEnumerable<Seating> seatings = null)
        {
            var comp = new AudienceSelect();
            comp.Style = Style;
            DeepCopyStuff(comp, panel, audiences, seatings);
            return comp;
        }

    }
}
