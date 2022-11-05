using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Clients;
using Newtonsoft.Json;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Security;

namespace RSToolKit.Domain.Entities
{
    public class SavedEmailTable : IReport, IEmailList
    {
        protected JTable pr_table;

        [Key]
        public Guid UId { get; set; }

        [NotMapped]
        public JTable Table
        {
            get
            {
                return pr_table;
            }
            set
            {
                pr_table = value;
                Favorite = value.Favorite;
                pr_table.SavedId = UId.ToString();
            }
        }

        public Guid ParentKey { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }

        public Guid ModifiedBy { get; set; }

        [Index(IsClustered = true)]
        public long SortingId { get; set; }

        [MaxLength]
        public string RawJtable
        {
            get
            {
                return JsonConvert.SerializeObject(Table);
            }
            set
            {
                Table = JsonConvert.DeserializeObject<JTable>(value);
            }
        }

        [NotMapped]
        public Form Form { get; set; }

        public bool Favorite { get; set; }

        public SavedEmailTable()
        {
            pr_table = new JTable();
        }

        public static SavedEmailTable New(FormsRepository repository, Company company)
        {
            var node = new SavedEmailTable()
            {
                Company = company
            };
            PermissionSet.CreateDefaultPermissions(repository, node, company.UId);
            repository.Add(node);
            return node;
        }

        public IEnumerable<IPerson> GetAllEmails(FormsRepository repository)
        {
            var contacts = new List<Contact>();
            var table = FillJTable();
            foreach (var row in table.Rows)
            {
                Guid t_id;
                if (Guid.TryParse(row.Id, out t_id))
                {
                    var t_contact = repository.Search<Contact>(c => c.UId == t_id).FirstOrDefault();
                    contacts.Add(t_contact);
                }
            }
            return contacts;
        }

        public IEnumerable<string> GetAllEmailAddresses()
        {
            var emails = new List<string>();
            foreach (var row in Table.Rows)
            {
                var emailData = row.Columns.FirstOrDefault(d => d.HeaderId == "email");
                if (emailData != null)
                    emails.Add(emailData.Value.ToLower());
            }
            return emails;
        }

        public JTable FillJTable(JTable table = null, bool admin = false, Dictionary<string, string> options = null, User user = null, bool noHtml = false)
        {
            var contacts = Company.Contacts;
            var rows = new List<JTableRow>();
            foreach (var contact in contacts)
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
                rows.Add(row);
            }
            for (var i = 0; i < rows.Count; i++)
            {
                var take = false;
                var grouping = true;
                var tests = new List<Tuple<bool, string>>();
                var row = rows[i];
                for (var j = 0; j < Table.Filters.Count; j++)
                {
                    var filter = Table.Filters[j];
                    var groupTest = true;
                    var first = true;
                    grouping = filter.GroupNext;
                    var test = false;
                    do
                    {
                        if (!filter.GroupNext)
                            grouping = false;
                        test = row.TestValue(filter, Table.Headers.FirstOrDefault(h => h.Id == filter.ActingOn));
                        switch (filter.Link)
                        {
                            case "and":
                                groupTest = groupTest && test;
                                break;
                            case "or":
                                if (first)
                                    groupTest = test;
                                else
                                    groupTest = groupTest || test;
                                break;
                            case "none":
                            default:
                                groupTest = test;
                                break;
                        }
                        first = false;
                        if (!grouping)
                            break;
                        j++;
                        if (j < Table.Filters.Count)
                            filter = Table.Filters[j];
                        else
                            break;
                    } while (grouping);
                    tests.Add(new Tuple<bool, string>(groupTest, j < (Table.Filters.Count - 1) ? Table.Filters[j + 1].Link : "none"));
                }
                take = tests.Count > 0 ? tests[0].Item1 : true;
                for (var j = 1; j < tests.Count; j++)
                {
                    switch (tests[j - 1].Item2)
                    {
                        case "and":
                            take = take && tests[j].Item1;
                            break;
                        case "or":
                            take = take || tests[j].Item1;
                            break;
                        case "none":
                        default:
                            take = tests[j].Item1;
                            break;
                    }
                }
                if (take)
                {
                    Table.AddRow(row);
                }
            }
            return Table;
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
