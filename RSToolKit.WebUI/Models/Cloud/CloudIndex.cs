using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;

namespace RSToolKit.WebUI.Models
{
    public class CloudIndex
    {
        public List<Notification> Notifications { get; set; }
        public IEnumerable<Tuple<Form, INamedNode>> FavoriteReports { get; set; }
        public List<Form> LiveForms { get; set; }
        public List<Domain.Entities.Reports.GlobalReport> GlobalReports { get; set; }

        public CloudIndex()
        {
            Notifications = new List<Notification>();
            FavoriteReports = new List<Tuple<Form, INamedNode>>();
            LiveForms = new List<Form>();
            GlobalReports = new List<Domain.Entities.Reports.GlobalReport>();
        }
    }
}