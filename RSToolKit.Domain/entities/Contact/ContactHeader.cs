using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Clients;
using Newtonsoft.Json;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Components;

namespace RSToolKit.Domain.Entities
{
    public class ContactHeader : INodeItem
    {
        [Key]
        public Guid UId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        [JsonIgnore]
        [ForeignKey("SavedListKey")]
        public virtual SavedList SavedList { get; set; }
        public Guid? SavedListKey { get; set; }

        [ClearKeyOnDelete("MappedTo")]
        public virtual List<Component> Components { get; set; }

        public ContactDataType Descriminator { get; set; }

        [JsonIgnore]
        public string RawDescriminatorOptions
        {
            get
            {
                return JsonConvert.SerializeObject(DescriminatorOptions);
            }
            set
            {
                DescriminatorOptions = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
            }
        }

        [CascadeDelete]
        public virtual List<ContactData> ContactDatas { get; set; }

        [NotMapped]
        public Dictionary<string, string> DescriminatorOptions { get; set; }

        [NotMapped]
        public List<ContactHeaderSelectionValue> Values { get; set; }
        public string RawValues
        {
            get
            {
                return JsonConvert.SerializeObject(Values);
            }
            set
            {
                Values = JsonConvert.DeserializeObject<List<ContactHeaderSelectionValue>>(value ?? "[]");
            }
        }

        public ContactHeader()
        {
            Name = "";
            DescriminatorOptions = new Dictionary<string, string>();
            Values = new List<ContactHeaderSelectionValue>();
        }

        public static ContactHeader New(FormsRepository repository, Company company, ContactDataType descriminator = ContactDataType.Text, SavedList list = null)
        {
            var header = new ContactHeader();
            header.CompanyKey = company.UId;
            header.Company = company;
            header.Descriminator = descriminator;
            header.SavedList = list;
            header.UId = Guid.NewGuid();
            repository.Add(header);
            repository.Commit();
            return header;
        }

        public INode GetNode()
        {
            return Company;
        }
    }

    public class ContactHeaderSelectionValue
    {
        public string Value { get; set; }
        public long Id { get; set; }

        public ContactHeaderSelectionValue()
        {
            Value = "New Value";
            Id = 0L;
        }
    }

    public enum ContactDataType
    {
        [StringValue("Email")]
        [JTableType("text")]
        Email = 0,
        [StringValue("Date")]
        [JTableType("date")]
        Date = 2,
        [StringValue("DateTime")]
        [JTableType("datetime")]
        DateTime = 3,
        [StringValue("Number")]
        [JTableType("number")]
        Number = 4,
        [StringValue("Money")]
        [JTableType("number")]
        Money = 5,
        [StringValue("Text")]
        [JTableType("text")]
        Text = 6,
        [ISecure]
        [StringValue("Secure Text")]
        [JTableType("secure")]
        SecureText = 7,
        [StringValue("Integer")]
        [JTableType("integer")]
        Integer = 8,
        [StringValue("Time")]
        [JTableType("time")]
        Time = 9,
        [StringValue("Multiple Selection")]
        [JTableType("multipleSelection")]
        MultipleSelection = 10,
        [StringValue("Single Selection")]
        [JTableType("itemParent")]
        SingleSelection = 11
    }
}
