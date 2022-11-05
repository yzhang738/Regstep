using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models
{
    public class PanelInfoModel
    {
        public Guid UId { get; set; }
        public int Order { get; set; }
        public bool Enabled { get; set; }
    }
}