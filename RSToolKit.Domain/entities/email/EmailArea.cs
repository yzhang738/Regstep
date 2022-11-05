using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Email;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Web.Mvc;

// Complete
namespace RSToolKit.Domain.Entities.Email
{
    public class EmailArea
        : IEmailArea, INodeItem
    {
        [CascadeDelete]
        public virtual List<EmailAreaVariable> Variables { get; set; }
        [JsonIgnore]
        [ForeignKey("UId")]
        public virtual RSEmail Email { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        public Guid UId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Type { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [AllowHtml]
        public string Html { get; set; }
        public int Order { get; set; }


        public EmailArea()
        {
            Html = "";
            Type = "";
            Order = 0;
            Variables = new List<EmailAreaVariable>();
        }

        public static EmailArea New(RSEmail email, string type, string html = "", int order = 0)
        {
            var area = new EmailArea()
            {
                UId = Guid.NewGuid(),
                Type = type,
                Html = html,
                Order = order
            };
            email.EmailAreas.Add(area);
            return area;
        }

        public EmailArea DeepCopy(RSEmail email)
        {
            var area = new EmailArea();
            email.EmailAreas.Add(area);
            area.Email = email;
            area.Html = Html;
            area.Order = Order;
            area.Type = Type;
            foreach (var variable in Variables)
            {
                variable.DeepCopy(area);
            }
            return area;
        }

        public INode GetNode()
        {
            return Email.EmailCampaign as INode ?? Email.Form as INode;
        }

        #region Methods

        public string Render(string body)
        {
            var str = Regex.Replace(body, @"@render_body", Html, RegexOptions.IgnoreCase);
            foreach (var variable in Variables)
            {
                str = Regex.Replace(str, @"@render_var_" + variable.Variable, String.IsNullOrWhiteSpace(variable.Value) ? "" : variable.Value, RegexOptions.IgnoreCase);
            }
            return str;
        }

        #endregion
    }
}
