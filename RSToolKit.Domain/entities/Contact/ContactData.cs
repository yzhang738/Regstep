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
using System.Text.RegularExpressions;
using System.Globalization;
using RSToolKit.Domain.Entities.Clients;
using Newtonsoft.Json;

// Complete
namespace RSToolKit.Domain.Entities
{
    [Table("ContactData")]
    public class ContactData : ISecureMap
    {
        [ForeignKey("UId")]
        [JsonIgnore]
        public virtual Contact Contact { get; set; }
        public Guid UId { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [MaxLength(5000)]
        public string Value { get; set; }

        [JsonIgnore]
        [ForeignKey("HeaderKey")]
        public virtual ContactHeader Header { get; set; }
        public Guid HeaderKey { get; set; }

        [NotMapped]
        public Guid ReferenceKey
        {
            get
            {
                return HeaderKey;
            }
        }

        [NotMapped]
        public string PrettyValue
        {
            get
            {
                return GetFormattedValue();
            }
        }

        [NotMapped]
        [JsonIgnore]
        public IPerson Parent
        {
            get
            {
                return Contact;
            }
            set
            {
                if (value is Contact)
                    Contact = value as Contact;
            }
        }
        [NotMapped]
        public Guid ParentKey
        {
            get
            {
                return UId;
            }
        }

        public ContactData()
        {
            Value = null;
        }

        public static ContactData New(Contact contact, ContactHeader header, string value = null)
        {
            var data = new ContactData()
            {
                Contact = contact,
                UId = contact.UId,
                Header = header,
                HeaderKey = header.UId,
                Value = value
            };
            contact.Data.Add(data);
            return data;
        }

        public INode GetNode()
        {
            return Contact.Company;
        }

        public string SetValue(string value)
        {
            if (String.IsNullOrEmpty(value))
                value = "";
            switch (Header.Descriminator)
            {
                case ContactDataType.Integer:
                    int i_var;
                    if (!int.TryParse(value, out i_var))
                        return "The value must be a whole number.";
                    break;
                case ContactDataType.Number:
                    double d_var;
                    if (!double.TryParse(value, out d_var))
                        return "The value must be a number.";
                    break;
                case ContactDataType.Money:
                    decimal dec_var;
                    if (!decimal.TryParse(Value, out dec_var))
                        return "The value must be a number";
                    break;
                case ContactDataType.DateTime:
                    DateTimeOffset dto_date;
                    if (DateTimeOffset.TryParse(Value, out dto_date))
                    {
                        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(Header.DescriminatorOptions["timezone"]);
                        value = TimeZoneInfo.ConvertTime(dto_date, timeZone).ToString();
                    }
                    else
                        return "The value must be a date and time.";
                    break;
                case ContactDataType.Time:
                    DateTime time;
                    if (!DateTime.TryParseExact(Value, "h:mm tt", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out time))
                        return "The value must be a time in format of 1:23 AM";
                    break;
                case ContactDataType.Email:
                    if (!value.IsEmail())
                        return "The value must be a valid email.";
                    value = value.ToLower();
                    break;
            }
            Value = value;
            return null;
        }

        public string GetFormattedValue()
        {
            if (String.IsNullOrWhiteSpace(Value))
                return "";
            switch (Header.Descriminator)
            {
                case ContactDataType.Number:
                case ContactDataType.Integer:
                    return Value;
                case ContactDataType.Money:
                    var moneyValue = decimal.Parse(Value);
                    return moneyValue.ToString("c", new CultureInfo(Header.DescriminatorOptions["culture"]));
                case ContactDataType.DateTime:
                    var date = DateTimeOffset.Parse(Value);
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(Header.DescriminatorOptions["timezone"]);
                    var retDate = TimeZoneInfo.ConvertTime(date, timeZone);
                    return retDate.ToString(new CultureInfo(Header.DescriminatorOptions["culture"]));
                case ContactDataType.MultipleSelection:
                    var t_values = JsonConvert.DeserializeObject<List<string>>(Value).ToArray();
                    var p_values = Header.Values.Where(h => h.Id.ToString().In(t_values)).Select(c => c.Value).ToArray();
                    return string.Concat(", ", p_values);
                case ContactDataType.SingleSelection:
                    var t_selection = Header.Values.FirstOrDefault(h => h.Id.ToString() == Value);
                    if (t_selection == null)
                        return "";
                    else
                        return t_selection.Value;
                default:
                    return Value;
            }
        }

        public object GetSoftTypedValue()
        {
            switch (Header.Descriminator)
            {
                case ContactDataType.Integer:
                    int i_var;
                    if (int.TryParse(Value, out i_var))
                        return i_var;
                    else
                        return int.MinValue;
                case ContactDataType.Number:
                    float f_var;
                    if (float.TryParse(Value, out f_var))
                        return f_var;
                    else
                        return float.MinValue;
                case ContactDataType.Money:
                    decimal dec_var;
                    if (decimal.TryParse(Value, out dec_var))
                        return dec_var;
                    else
                        return decimal.MinValue;
                case ContactDataType.DateTime:
                    DateTimeOffset dto_date;
                    if (DateTimeOffset.TryParse(Value, out dto_date))
                    {
                        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(Header.DescriminatorOptions["timezone"]);
                        return TimeZoneInfo.ConvertTime(dto_date, timeZone);
                    }
                    return DateTimeOffset.MinValue;
                case ContactDataType.Time:
                    DateTime time;
                    if(DateTime.TryParseExact(Value, "h:mm tt", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out time))
                    {
                        return time.TimeOfDay;
                    }
                    return TimeSpan.MinValue;
                default:
                    return Value == null ? String.Empty : Value;
            }
        }

        public T GetTypedValue<T>()
        {
            var obj = GetSoftTypedValue();
            return (T)obj;
        }
    }
}
