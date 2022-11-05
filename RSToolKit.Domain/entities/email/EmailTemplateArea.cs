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

// Complete
namespace RSToolKit.Domain.Entities.Email
{
    public class EmailTemplateArea : IEmailArea, IAdminOnly
    {
        [CascadeDelete]
        public virtual List<TemplateEmailAreaVariable> Variables { get; set; }
        [JsonIgnore]
        [ForeignKey("UId")]
        public virtual EmailTemplate Template { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        public Guid UId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Type { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Html { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Default { get; set; }

        public EmailTemplateArea()
        {
            Html = "";
            Type = "";
            Default = "Default Area";
            Variables = new List<TemplateEmailAreaVariable>();
        }

        public static EmailTemplateArea New(EmailTemplate template, string type, string html = "", string def = "Default Area")
        {
            var area = new EmailTemplateArea()
            {
                Html = html,
                Type = type,
                Default = def
            };
            template.EmailAreas.Add(area);
            return area;
        }

        public INode GetNode()
        {
            return null;
        }

        #region Methods

        public string Render(string body)
        {
            var str = Regex.Replace(Html, @"@Render_Body", body, RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"@Render_Tags", "", RegexOptions.IgnoreCase);
            return str;
        }

        #endregion
    }
}
