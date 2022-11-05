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
    public class EmailTemplate : IAdminOnly
    {
        [CascadeDelete]
        public virtual List<EmailTemplateArea> EmailAreas { get; set; }
        [CascadeDelete]
        public virtual List<EmailTemplateVariable> Variables { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [Key]
        public Guid UId { get; set; }

        [MaxLength(5000)]
        public string Description { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        public EmailTemplate()
        {
            Description = "";
            EmailAreas = new List<EmailTemplateArea>();
            Variables = new List<EmailTemplateVariable>();
        }

        public static EmailTemplate New(string name, string description = "")
        {
            var template = new EmailTemplate()
            {
                UId = Guid.NewGuid(),
                Name = name,
                Description = description
            };
            return template;
        }

    }
}
