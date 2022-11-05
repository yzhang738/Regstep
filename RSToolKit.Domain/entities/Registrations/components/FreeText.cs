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
using System.Web.Mvc;
using RSToolKit.Domain.Entities.Clients;

// Complete
namespace RSToolKit.Domain.Entities.Components
{
    public class FreeText : Component
    {

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [AllowHtml]
        public string Html { get; set; }

        public bool HideReview { get; set; }

        public FreeText() : base()
        {
            Html = "";
            HideReview = false;
        }

        public static FreeText New(FormsRepository repository, Panel panel, Company company, User user, Guid? owner = null, Guid? group = null, string name = null, string description = "", int row = int.MaxValue, int order = int.MaxValue, string permission = "770")
        {
            var node = new FreeText()
            {
                UId = Guid.NewGuid(),
                Name = name == null ? "New free text - " + DateTime.UtcNow.ToString("MM/dd/yyyy h:mm tt") + "." : name,
                Description = description,
                Row = row,
                Order = order,
                Company = company,
                CompanyKey = company.UId
            };
            panel.Components.Add(node);
            repository.Commit();
            return node;
        }

        public override IComponent DeepCopy(Panel panel, IEnumerable<Audience> audiences = null, IEnumerable<Seating> seatings = null)
        {
            var comp = new FreeText();
            DeepCopyStuff(comp, panel, audiences, seatings);
            comp.Html = Html;
            return comp;
        }

        #region IHtmlView


        public string JavascriptHeader(Registrant reg)
        {
            return "";
        }



        #endregion

    }
}