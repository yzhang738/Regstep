using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.Components;

namespace RSToolKit.WebUI.Models
{
    public class FormEmailsModel
    {
        public Form Form { get; set; }
        public List<RSEmail> Emails { get; set; }

        public FormEmailsModel()
        {
            Emails = new List<RSEmail>();
        }
    }

    public class EmailCampaignEmailsModel
    {
        public EmailCampaign Form { get; set; }
        public List<RSEmail> Emails { get; set; }

        public EmailCampaignEmailsModel()
        {
            Emails = new List<RSEmail>();
        }
    }

}