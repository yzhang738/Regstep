using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Security;

namespace RSToolKit.Domain.Entities
{
    public class ContactReport : IPointerTarget, IEmailList
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [Key]
        public Guid UId { get; set; }

        [JsonIgnore]
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }
        [MaxLength(5000)]
        public string Description { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        [CascadeDelete]
        public virtual List<QueryFilter> Filters { get; set; }

        public string RawHeaders
        {
            get
            {
                return JsonConvert.SerializeObject(Headers);
            }
            set
            {
                Headers = JsonConvert.DeserializeObject<List<long>>(value);
            }
        }

        [NotMapped]
        public List<long> Headers { get; set; }

        [NotMapped]
        [JsonIgnore]
        public List<Contact> Contacts { get; set; }

        public ContactReport()
        {
            Filters = new List<QueryFilter>();
            Name = "New Contact Report";
        }

        public static ContactReport New(FormsRepository repository, Company company, User user, Guid? owner = null, Guid? group = null, string permission = "770", string name = "", string description = "", List<QueryFilter> filters = null)
        {
            var node = new ContactReport()
            {
                UId = Guid.NewGuid(),
                Company = company,
                CompanyKey = company.UId,
                Name = name,
                Description = description,
                Filters = filters == null ? new List<QueryFilter>() : filters
            };
            PermissionSet.CreateDefaultPermissions(repository, node, company.UId);
            repository.Add(node);
            repository.Commit();
            return node;
        }

        public IEnumerable<IPerson> GetAllEmails(FormsRepository repository)
        {
            var paging = new Paging()
            {
                Page = -1,
                RecordsPerPage = -1
            };
            LoadPartial(paging, repository);
            return Contacts;
        }

        public IEnumerable<string> GetAllEmailAddresses()
        {
            using (var repository = new FormsRepository())
            {
                return GetAllEmails(repository).Select(c => c.Email);
            }
        }

        public List<ContactHeader> GetHeaders(FormsRepository repository)
        {
            var list = new List<ContactHeader>();
            foreach (var id in Headers)
            {
                var header = repository.Search<ContactHeader>(h => h.SortingId == id).FirstOrDefault();
                if (header == null)
                    continue;
                list.Add(header);
            }
            return list;
        }

        public Paging LoadPartial(Paging paging, FormsRepository repository, SortingInformation sorting = null)
        {
            var page = new Paging()
            {
                Page = paging.Page,
                RecordsPerPage = paging.RecordsPerPage
            };
            sorting = sorting ?? new SortingInformation() { ActingOn = "Email", Descending = false };
            var uf_contacts = repository.Search<Contact>(c => c.CompanyKey == CompanyKey).ToList();
            page.TotalRecords = uf_contacts.Count;
            switch (sorting.ActingOn.ToLower())
            {
                case "email":
                    if (!sorting.Descending)
                        uf_contacts = uf_contacts.OrderBy(r => r.Email).ToList();
                    else
                        uf_contacts = uf_contacts.OrderByDescending(r => r.Email).ToList();
                    break;
                case "datecreated":
                    if (!sorting.Descending)
                        uf_contacts = uf_contacts.OrderBy(r => r.DateCreated).ToList().ToList();
                    else
                        uf_contacts = uf_contacts.OrderByDescending(r => r.DateCreated).ToList();
                    break;
                default:
                    Guid h_key;
                    if (Guid.TryParse(sorting.ActingOn, out h_key))
                    {
                        var header = repository.Search<ContactHeader>(c => c.UId == h_key).FirstOrDefault();
                        if (header == null)
                            break;
                        uf_contacts.Sort((b, a) =>
                        {
                            var a_data = a.Data.Where(i => i.HeaderKey == header.UId).FirstOrDefault();
                            var b_data = b.Data.Where(i => i.HeaderKey == header.UId).FirstOrDefault();
                            if (a_data == null)
                                return 1;
                            if (b_data == null)
                                return -1;
                            return a_data.Value.CompareTo(b_data.Value);
                        });
                    }
                    break;
            }

            var list = new List<Contact>();

            // Now we must filter out the contacts
            foreach (var contact in uf_contacts)
            {
                var take = true;
                //if (taken == (pageSize * page))
                //    break;
                var tests = new List<Tuple<bool, FilterLink>>();
                Filters.Sort((a, b) => a.Order - b.Order);
                var items = new List<Guid>();
                for (var i = 0; i < Filters.Count; i++)
                {
                    var filter = Filters[i];
                    var groupTest = true;
                    var first = true;
                    var grouping = filter.GroupNext;
                    var test = false;
                    do
                    {
                        if (!filter.GroupNext)
                        {
                            grouping = false;
                        }
                        test = contact.CompareData(filter.ActingOn, filter.Value, filter.Test.GetTestValue(), ref items);
                        switch (filter.Link)
                        {
                            case FilterLink.And:
                                groupTest = groupTest && test;
                                break;
                            case FilterLink.Or:
                                if (first)
                                    groupTest = test;
                                else
                                    groupTest = groupTest || test;
                                break;
                            case FilterLink.None:
                            default:
                                groupTest = test;
                                break;
                        }
                        first = false;
                        if (!grouping)
                            break;
                        i++;
                        if (i < Filters.Count)
                            filter = Filters[i];
                        else
                            break;
                    } while (grouping);
                    tests.Add(new Tuple<bool, FilterLink>(groupTest, (i + 1) < Filters.Count ? Filters[(i + 1)].Link : FilterLink.None));
                }
                take = tests.Count > 0 ? tests[0].Item1 : true;
                for (int i = 1; i < tests.Count; i++)
                {
                    switch (tests[i - 1].Item2)
                    {
                        case FilterLink.And:
                            take = take && tests[i].Item1;
                            break;
                        case FilterLink.Or:
                            take = take || tests[i].Item1;
                            break;
                        case FilterLink.None:
                        default:
                            take = tests[i].Item1;
                            break;
                    }
                }
                if (take)
                {
                    list.Add(contact);
                }
            }
            if (page.RecordsPerPage != -1)
                list = list.Skip((page.Page - 1) * page.RecordsPerPage).Take(page.RecordsPerPage).ToList();
            Contacts = list;
            return page;
        }

        public JTable FillJTable(JTable table = null, bool admin = false, Dictionary<string, string> options = null, User user = null, bool noHtml = false)
        {
            table = table ?? new JTable() { Sortings = new List<JTableSorting>() { new JTableSorting() { ActingOn = "email" } } };
            table.Name = Name;
            table.Parent = Company.Name;
            using (var repository = new FormsRepository())
            {
                var contacts = GetAllEmails(repository).OfType<Contact>();
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
                    table.AddHeader(new JTableHeader() { Id = "email", Label = "Email", Editable = true, Type = "text" });
                    table.AddHeader(new JTableHeader() { Id = header.UId.ToString(), Label = header.Name, Type = t_type, Editable = true });
                }
                foreach (var contact in contacts)
                {
                    var row = new JTableRow()
                    {
                        Id = contact.UId.ToString()
                    };
                    row.Columns.Add(new JTableColumn() { Id = contact.UId.ToString() + "_email", Value = contact.Email, PrettyValue = contact.Email, Editable = true });
                    foreach (var e_data in contact.Data)
                    {
                        row.Columns.Add(new JTableColumn() { Id = e_data.UId.ToString(), PrettyValue = e_data.PrettyValue, Value = e_data.Value, Editable = true });
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
