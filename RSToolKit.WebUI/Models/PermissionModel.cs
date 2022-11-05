using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models
{
    public class PermissionModel
    {
        public Guid Group { get; set; }
        public bool GroupRead { get; set; }
        public bool GroupWrite { get; set; }
        public bool GroupExecute { get; set; }
        public Guid Owner { get; set; }
        public bool OwnerRead { get; set; }
        public bool OwnerWrite { get; set; }
        public bool OwnerExecute { get; set; }
        public bool AnonymousRead { get; set; }
        public bool AnonymousWrite { get; set; }
        public bool AnonymousExecute { get; set; }
        public Guid UId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }
}