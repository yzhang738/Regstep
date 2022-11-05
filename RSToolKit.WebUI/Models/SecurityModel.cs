using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Security;

namespace RSToolKit.WebUI.Models
{
    public class PermissionsModel
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public Guid UId { get; set; }
        public List<PermissionSetModel> Permissions { get; set; }
        public string Type { get; set; }
            
        public PermissionsModel()
        {
            Permissions = new List<PermissionSetModel>();
        }
    }

    public class PermissionSetModel
    {
        public Guid Target { get; set; }
        public Guid Owner { get; set; }
        public string OwnerName { get; set; }
        public bool Read { get; set; }
        public bool Write { get; set; }
        public bool Execute { get; set; }
        public string Type { get; set; }
    }
}