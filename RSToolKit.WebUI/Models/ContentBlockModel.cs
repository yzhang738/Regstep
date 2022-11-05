using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RSToolKit.WebUI.Models
{
    public class ContentBlockModel
    {
        public Guid UId { get; set; }
        public List<LogicModel> Logics { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        public ContentBlockModel()
        {
            UId = Guid.Empty;
            Logics = new List<LogicModel>();
            Name = "";
        }
    }

    public class LogicModel
    {
        public Guid UId { get; set; }
        public int Order { get; set; }

        public LogicModel()
        {
            UId = Guid.Empty;
            Order = 0;
        }
    }
}