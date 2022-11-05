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
using System.Text.RegularExpressions;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Entities.Components
{
    public class Input : Component, IVariableHolder
    {
        private RegexPattern _regexPattern = RegexPattern.None;

        public RegexPattern RegexPattern
        {
            get
            {
                return _regexPattern;
            }
            set
            {
                _regexPattern = value;
                RegexString = _regexPattern.GetRgxValue();
                RegexErrorString = _regexPattern.GetRgxErrorValue();
                RegexHumanString = _regexPattern.GetRgxPatternValue();
            }
        }

        public ValueType ValueType { get; set; }

        [NotMapped]
        public Regex RegexSearch { get; set; }

        public bool ComfirmField { get; set; }
        public InputType Type { get; set; }
        [MaxLength(500)]
        public string RegexString
        {
            get
            {
                return RegexSearch.ToString();
            }
            set
            {
                if (value == null)
                {
                    RegexSearch = null;
                    return;
                }
                RegexSearch = new Regex(value);
            }
        }
        [MaxLength(500)]
        public string RegexHumanString { get; set; }
        [MaxLength(250)]
        public string RegexErrorString { get; set; }
        public int? Length { get; set; }
        public int Height { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public string FileType { get; set; }
        public Formatting Formatting { get; set; }

        public Input()
            : base()
        {
            RegexString = null;
            RegexSearch = null;
            RegexHumanString = null;
            RegexErrorString = null;
            Length = null;
            Height = 1;
            RegexPattern = RegexPattern.None;
            ComfirmField = false;
            Type = InputType.Default;
            MinDate = null;
            MaxDate = null;
            ValueType = ValueType.Default;
            Formatting = Formatting.None;
        }

        public static Input New(FormsRepository repository, Panel panel, Company company, User user, Guid? owner = null, Guid? group = null, string name = null, string permission = "770", string description = "", int row = int.MaxValue, int order = int.MaxValue)
        {
            var node = new Input()
            {
                UId = Guid.NewGuid(),
                Name = name == null ? "New input - " + DateTime.UtcNow.ToString("MM/dd/yyyy h:mm tt") + "." : name,
                LabelText = name == null ? "" : name,
                Description = description,
                Row = row,
                Order = order,
                Company = company,
                CompanyKey = company.UId
            };
            node.Variable = new Variable()
            {
                Form = panel.Page.Form,
                FormKey = panel.Page.FormKey,
                Value = node.UId.ToString()
            };
            panel.Components.Add(node);
            repository.Commit();
            return node;
        }

        public override IComponent DeepCopy(Panel panel, IEnumerable<Audience> audiences = null, IEnumerable<Seating> seatings = null)
        {
            var comp = new Input();
            DeepCopyStuff(comp, panel, audiences, seatings);
            comp.UId = Guid.NewGuid();
            comp.RegexString = RegexString;
            comp.RegexSearch = RegexSearch;
            comp.RegexHumanString = RegexHumanString;
            comp.RegexErrorString = RegexErrorString;
            comp.Length = Length;
            comp.Height = Height;
            comp.RegexPattern = RegexPattern;
            comp.ComfirmField = ComfirmField;
            comp.Type = Type;
            comp.MinDate = MinDate;
            comp.MaxDate = MaxDate;
            comp.ValueType = ValueType;
            comp.Formatting = Formatting;
            return comp;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType().Equals(this.GetType()))
            {
                return ((Input)obj).UId.Equals(UId);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return UId.GetHashCode();
        }

    }

    public enum ValueType
    {
        [StringValue("Default")]
        [TagValue("input")]
        Default = -1,
        [StringValue("Number")]
        [TagValue("int")]
        Number = 0,
        [StringValue("Money")]
        [TagValue("decimal")]
        Decimal = 1,
        [StringValue("Date Time")]
        [TagValue("datetime")]
        DateTime = 2,
        [StringValue("Email")]
        [TagValue("email")]
        Email = 3,
        [StringValue("Phone Number (U.S.)")]
        [TagValue("usphone")]
        USPhone = 4,
        [StringValue("Zip Code (U.S.)")]
        [TagValue("zipcode")]
        ZipCode = 5
    }

    public enum InputType
    {
        [StringValue("Default")]
        Default = 0,
        [StringValue("Password")]
        Password = 1,
        [StringValue("Multiline")]
        Multiline = 2,
        [StringValue("Credit Card")]
        UniversalCreditCard = 3,
        [StringValue("Social Security")]
        [TagValue("ssn")]
        SSN = 4,
        [StringValue("Date Time")]
        [TagValue("datetime")]
        DateTime = 5,
        [TagValue("date")]
        [StringValue("Date")]
        Date = 6,
        [TagValue("time")]
        [StringValue("Time")]
        Time = 7,
        [StringValue("File")]
        File = 8
    }

    public enum RegexPattern
    {
        [RgxValue(@"^.*$")]
        [RgxPattern(@"*")]
        [RgxErrorValue(@"None")]
        [StringValue("None")]
        None = 0,
        [RgxValue(@"^.+\@(\[?)[a-zA-Z0-9\-\.]+\.([a-zA-Z]{2,3}|[0-9]{1,3})(\]?)$")]
        [RgxPattern(@"somthing@domain.suffix")]
        [RgxErrorValue(@"Your email must conform to specifications of email.")]
        [StringValue("Email")]
        [TagValue("email")]
        Email = 1,
        [RgxValue(@"^(?:\+1)?[ -.]?\(?\d{3}\)?[ -.]?\d{3}[ -.]?\d{4}$")]
        [RgxPattern(@"###-###-#### or (###) ###-####")]
        [RgxErrorValue(@"You must provide a valid phone number.")]
        [StringValue("US Phone")]
        [TagValue("phone")]
        USPhone = 2,
        [RgxValue(@"^\d{5}(?:-\d{4})?$")]
        [RgxPattern(@"##### or #####-####")]
        [RgxErrorValue(@"You must provide a valid zip code.")]
        [StringValue("Zip Code")]
        ZipCode = 4,
    }

    public enum Formatting
    {
        [StringValue("None")]
        None = 0,
        [StringValue("Lowercase")]
        Lowercase = 1,
        [StringValue("All Caps")]
        Caps = 2,
        [StringValue("Sentence Case")]
        SentenceCase = 3,
        [StringValue("Title Case")]
        TitleCase = 4,
        [StringValue("(###) ###-####")]
        GenericPhone = 5,
        [StringValue("###.###.####")]
        DotPhone = 6,
        [StringValue("### ### ####")]
        SpacePhone = 7,
        [StringValue("###-###-####")]
        DashPhone = 8
    }
}
