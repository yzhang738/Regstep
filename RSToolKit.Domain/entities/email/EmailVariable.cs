using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities.Email
{
    public class EmailVariable : INodeItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        public Guid UId { get; set; }
        [MaxLength(100)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Variable { get; set; }
        [MaxLength(150)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }
        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }
        [JsonIgnore]
        [ForeignKey("UId")]
        public RSEmail RSEmail { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Type { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Value { get; set; }

        public EmailVariable()
        {
            Variable = Name = Description = Value = "";
            Type = "measurement";
            RSEmail = null;
            SortingId = -1;
        }

        public EmailVariable DeepCopy(RSEmail email)
        {
            var eav = new EmailVariable();
            email.Variables.Add(eav);
            eav.RSEmail = email;
            eav.Description = Description;
            eav.Name = Name;
            eav.Type = Type;
            eav.Variable = Variable;
            eav.Value = Value;
            return eav;
        }

        public INode GetNode()
        {
            return RSEmail.GetNode();
        }

    }

    public class EmailTemplateVariable : IAdminOnly
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        public Guid UId { get; set; }
        [MaxLength(100)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Variable { get; set; }
        [MaxLength(150)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }
        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }
        [MaxLength(25)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Type { get; set; }
        [JsonIgnore]
        [ForeignKey("UId")]
        public virtual EmailTemplate EmailTemplate { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Value { get; set; }

        public EmailTemplateVariable()
        {
            Variable = Name = Description = Value = "";
            Type = "measurement";
            EmailTemplate = null;
            SortingId = -1;
        }

        public static EmailTemplateVariable New(EmailTemplate template, string variable, string name, string description, string value, string type)
        {
            var var = new EmailTemplateVariable()
            {
                Name = name,
                Description = description,
                Variable = variable,
                Value = value,
                Type = type
            };
            template.Variables.Add(var);
            return var;
        }
    }

    public class TemplateEmailAreaVariable : IAdminOnly
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [MaxLength(100)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Variable { get; set; }
        [MaxLength(150)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }
        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }
        [MaxLength(25)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Type { get; set; }
        [JsonIgnore]
        [ForeignKey("EmailTemplateAreaSortingId")]
        public virtual EmailTemplateArea EmailTemplateArea { get; set; }
        public long EmailTemplateAreaSortingId { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Value { get; set; }

        public TemplateEmailAreaVariable()
        {
            Variable = Name = Description = Value = "";
            Type = "measurement";
            EmailTemplateArea = null;
            SortingId = -1;
            EmailTemplateAreaSortingId = -1;
        }

        public static TemplateEmailAreaVariable New(EmailTemplateArea area, string variable, string name, string description, string value, string type)
        {
            var var = new TemplateEmailAreaVariable()
            {
                Name = name,
                Description = description,
                Variable = variable,
                Value = value,
                Type = type
            };
            area.Variables.Add(var);
            return var;
        }

    }

    public class EmailAreaVariable : INodeItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [MaxLength(100)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Variable { get; set; }

        [MaxLength(150)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }

        [MaxLength(25)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Type { get; set; }

        [JsonIgnore]
        [ForeignKey("EmailAreaSortingId")]
        public virtual EmailArea EmailArea { get; set; }
        public long EmailAreaSortingId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Value { get; set; }

        public EmailAreaVariable()
        {
            Variable = Name = Description = Value = "";
            Type = "measurement";
            EmailArea = null;
            SortingId = -1;
            EmailAreaSortingId = -1;
        }

        public EmailAreaVariable DeepCopy(EmailArea area)
        {
            var eav = new EmailAreaVariable();
            area.Variables.Add(eav);
            eav.EmailArea = area;
            eav.Description = Description;
            eav.Name = Name;
            eav.Type = Type;
            eav.Variable = Variable;
            eav.Value = Value;
            return eav;
        }

        public INode GetNode()
        {
            return EmailArea.GetNode();
        }

    }

}
