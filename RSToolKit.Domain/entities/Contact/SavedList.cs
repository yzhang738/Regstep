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
using RSToolKit.Domain.Entities.Clients;
using Newtonsoft.Json;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Security;

namespace RSToolKit.Domain.Entities
{
    public class SavedList
        : IPointerTarget, ICompanyHolder, IEmailList
    {
        #region INode

        [JsonIgnore]
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(3)]
        public string Permission { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid Owner { get; set; }
        public Guid Group { get; set; }


        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        #endregion

        [JsonIgnore]
        public virtual List<Contact> Contacts { get; set; }
        [JsonIgnore]
        public virtual List<Form> Forms { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public SavedList()
        {
            Contacts = new List<Contact>();
            Description = "";
        }

        public List<ContactHeader> GetHeaders(FormsRepository repository)
        {
            var headers = repository.Search<ContactHeader>(h => h.CompanyKey == Company.UId && (h.SavedListKey == null || h.SavedListKey == UId)).ToList();
            return headers;
        }

        public static SavedList New(FormsRepository repository, Company company, User user, Guid? owner = null, Guid? group = null, string permission = "775", string name = "New Saved List", string description = "")
        {
            var node = new SavedList()
            {
                UId = Guid.NewGuid(),
                Permission = permission,
                Owner = owner.HasValue ? owner.Value : user.UId,
                Group = group.HasValue ? group.Value : company.UId,
                CompanyKey = company.UId,
                Company = company,
                Name = name,
                Description = description
            };
            PermissionSet.CreateDefaultPermissions(repository, node, company.UId);
            repository.Add(node);
            repository.Commit();
            return node;
        }

        public IEnumerable<IPerson> GetAllEmails(FormsRepository repository)
        {
            return Contacts;
        }

        public IEnumerable<string> GetAllEmailAddresses()
        {
            return GetAllEmails(null).Select(c => c.Email);
        }

        public List<Contact> LoadPartial(Paging paging, FormsRepository repository, SortingInformation sorting = null)
        {
            var page = new Paging()
            {
                Page = paging.Page,
                RecordsPerPage = paging.RecordsPerPage
            };
            if (sorting == null)
                sorting = new SortingInformation()
                {
                    ActingOn = "Email",
                    Descending = false
                };

            // Now we sort the information


            switch (sorting.ActingOn.ToLower())
            {
                case "email":
                    if (!sorting.Descending)
                        Contacts = Contacts.OrderBy(r => r.Email).ToList();
                    else
                        Contacts = Contacts.OrderByDescending(r => r.Email).ToList();
                    break;
                case "datecreated":
                    if (!sorting.Descending)
                        Contacts = Contacts.OrderBy(r => r.DateCreated).ToList().ToList();
                    else
                        Contacts = Contacts.OrderByDescending(r => r.DateCreated).ToList();
                    break;
                default:
                    Guid h_key;
                    if (Guid.TryParse(sorting.ActingOn, out h_key))
                    {
                        var header = repository.Search<ContactHeader>(c => c.UId == h_key).FirstOrDefault();
                        if (header == null)
                            break;
                        if (sorting.Descending)
                        {
                            #region Descending
                            Contacts.Sort((b, a) =>
                            {
                                var a_data = a.Data.Where(i => i.HeaderKey == header.UId).FirstOrDefault();
                                var b_data = b.Data.Where(i => i.HeaderKey == header.UId).FirstOrDefault();
                                if (a_data == null)
                                    return -1;
                                if (b_data == null)
                                    return 1;
                                switch (header.Descriminator)
                                {
                                    case ContactDataType.Integer:
                                        var int_a = a_data.GetTypedValue<int>();
                                        var int_b = b_data.GetTypedValue<int>();
                                        return int_a.CompareTo(int_b);
                                    case ContactDataType.Number:
                                        var float_a = a_data.GetTypedValue<float>();
                                        var float_b = b_data.GetTypedValue<float>();
                                        return float_a.CompareTo(float_b);
                                    case ContactDataType.Money:
                                        var decimal_a = a_data.GetTypedValue<decimal>();
                                        var decimal_b = b_data.GetTypedValue<decimal>();
                                        return decimal_a.CompareTo(decimal_b);
                                    case ContactDataType.DateTime:
                                        var dto_a = a_data.GetTypedValue<DateTimeOffset>();
                                        var dto_b = b_data.GetTypedValue<DateTimeOffset>();
                                        return dto_a.CompareTo(dto_b);
                                    case ContactDataType.Time:
                                        var ts_a = a_data.GetTypedValue<TimeSpan>();
                                        var ts_b = b_data.GetTypedValue<TimeSpan>();
                                        return ts_a.CompareTo(ts_b);
                                    default:
                                        return a_data.GetTypedValue<string>().CompareTo(b_data.GetTypedValue<string>());
                                }
                            });
                            #endregion
                        }
                        else
                        {
                            #region Descending
                            Contacts.Sort((a, b) =>
                            {
                                var a_data = a.Data.Where(i => i.HeaderKey == header.UId).FirstOrDefault();
                                var b_data = b.Data.Where(i => i.HeaderKey == header.UId).FirstOrDefault();
                                if (a_data == null)
                                    return -1;
                                if (b_data == null)
                                    return 1;
                                switch (header.Descriminator)
                                {
                                    case ContactDataType.Integer:
                                        var int_a = a_data.GetTypedValue<int>();
                                        var int_b = b_data.GetTypedValue<int>();
                                        return int_a.CompareTo(int_b);
                                    case ContactDataType.Number:
                                        var float_a = a_data.GetTypedValue<float>();
                                        var float_b = b_data.GetTypedValue<float>();
                                        return float_a.CompareTo(float_b);
                                    case ContactDataType.Money:
                                        var decimal_a = a_data.GetTypedValue<decimal>();
                                        var decimal_b = b_data.GetTypedValue<decimal>();
                                        return decimal_a.CompareTo(decimal_b);
                                    case ContactDataType.DateTime:
                                        var dto_a = a_data.GetTypedValue<DateTimeOffset>();
                                        var dto_b = b_data.GetTypedValue<DateTimeOffset>();
                                        return dto_a.CompareTo(dto_b);
                                    case ContactDataType.Time:
                                        var ts_a = a_data.GetTypedValue<TimeSpan>();
                                        var ts_b = b_data.GetTypedValue<TimeSpan>();
                                        return ts_a.CompareTo(ts_b);
                                    default:
                                        return a_data.GetTypedValue<string>().CompareTo(b_data.GetTypedValue<string>());
                                }
                            });
                            #endregion
                        }
                    }
                    break;
            }


            if (page.RecordsPerPage != -1)
                return Contacts.Skip((page.Page - 1) * page.RecordsPerPage).Take(page.RecordsPerPage).ToList();
            return Contacts;
        }

        public JTable FillJTable(JTable table = null, bool admin = false, Dictionary<string, string> options = null, User user = null, bool noHtml = false)
        {
            table = table ?? new JTable() { Sortings = new List<JTableSorting>() { new JTableSorting() { ActingOn = "email" } } };
            table.Options["savedList"] = "yes";
            table.Options["type"] = "email list";
            table.Name = Name;
            table.Parent = Company.Name;
            table.SavedId = UId.ToString();
            using (var repository = new FormsRepository())
            {
                table.AddHeader(new JTableHeader() { Id = "email", Label = "Email", Editable = true, Type = "text" });
                var headers = GetHeaders(repository);
                foreach (var header in headers)
                {
                    var t_type = "text";
                    if (header.Descriminator == ContactDataType.DateTime)
                        t_type = "date";
                    if (header.Descriminator == ContactDataType.Date)
                        t_type = "date";
                    if (header.Descriminator == ContactDataType.Time)
                        t_type = "time";
                    if (header.Descriminator == ContactDataType.Integer || header.Descriminator == ContactDataType.Money)
                        t_type = "number";
                    table.AddHeader(new JTableHeader() { Id = header.UId.ToString(), Label = header.Name, Type = t_type, Editable = true });
                }
                foreach (var contact in Contacts)
                {
                    var row = new JTableRow()
                    {
                        Id = contact.UId.ToString()
                    };
                    row.Columns.Add(new JTableColumn() { HeaderId = "email", Id = contact.UId.ToString() + "_email", Value = contact.Email, PrettyValue = contact.Email, Editable = true });
                    foreach (var e_data in contact.Data)
                    {
                        row.Columns.Add(new JTableColumn() { HeaderId = e_data.Header.UId.ToString(), Id = e_data.UId.ToString() + "_" + e_data.Header.UId.ToString(), PrettyValue = e_data.PrettyValue, Value = e_data.Value, Editable = true });
                    }
                    table.AddRow(row);
                }
            }
            return table;
        }

        [NotMapped]
        public System.Globalization.CultureInfo Culture
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
