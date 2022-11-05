using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Entities.Email
{
    public class ClickEvent
    {
        public string url { get; set; }
        public string aId { get; set; }

        public ClickEvent()
        {
            url = aId = "";
        }
    }
}
