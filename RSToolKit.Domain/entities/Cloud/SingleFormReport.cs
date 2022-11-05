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
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Data.OldJsonData;

namespace RSToolKit.Domain.Entities
{
    public class SingleFormReport : IFormReport
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Index(IsClustered = true, IsUnique = true)]
        public long SortingId { get; set; }

        [Key]
        [Index(IsClustered = false)]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public string Permission { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }
        public Guid Owner { get; set; }
        public Guid Group { get; set; }

        public bool Favorite { get; set; }
        public bool Inventory { get; set; }

        // End INode

        [JsonIgnore]
        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid FormKey { get; set; }
        public Guid ParentKey
        {
            get
            {
                return Guid.Empty;
            }
            set
            {
                return;
            }
        }

        protected System.Web.Mvc.UrlHelper Url { get; set; }

        #region Report Headers

        [NotMapped]
        public List<string> Variables { get; set; }

        public string RawVariables
        {
            get
            {
                return JsonConvert.SerializeObject(Variables);
            }
            set
            {
                Variables = JsonConvert.DeserializeObject<List<string>>(value);
            }
        }

        #endregion

        public virtual List<QueryFilter> Filters { get; set; }

        public string SortOn { get; set; }
        public bool Ascending { get; set; }

        public SingleFormReport()
        {
            UId = Guid.NewGuid();
            Filters = new List<QueryFilter>();
            SortOn = "confirmation";
            Ascending = true;
            Favorite = false;
            Inventory = false;
            Variables = new List<string>();
            DateCreated = DateModified = DateTimeOffset.UtcNow;
            Url = new System.Web.Mvc.UrlHelper(System.Web.HttpContext.Current.Request.RequestContext);
        }

        public static SingleFormReport New(FormsRepository repository, User user, Company company, Form form, Guid? owner = null, Guid? group = null, string permission = "770", string name = null, string[] variables = null )
        {
            if (variables == null)
                variables = new string[] { "confirmation", "email", "status" };
            var node = new SingleFormReport()
            {
                UId = Guid.NewGuid(),
                Form = form,
                FormKey = form.UId,
                Company = company,
                CompanyKey = company.UId,
                Group = group.HasValue ? group.Value : company.UId,
                Owner = owner.HasValue ? owner.Value : user.UId,
                Name = name == null ? "New custom report." : name,
                Variables = variables.ToList(),
                Permission = permission
            };
            PermissionSet.CreateDefaultPermissions(repository, node, company.UId);
            repository.Add(node);
            repository.Commit();
            return node;
        }

        protected JsonTable GetNonInventyReport(FormsRepository repository, Paging paging, List<string> urlOn = null, string baseUrl = null)
        {
            List<Registrant> uf_list;
            var table = new JsonTable()
            {
                Name = Name,
                Id = UId.ToString()
            };
            switch (SortOn.ToLower())
            {
                case "email":
                    uf_list = Form.Registrants.OrderBy(r => r.Email).ToList();
                    break;
                case "confirmation":
                    uf_list = Form.Registrants.OrderBy(r => r.SortingId).ToList();
                    break;
                case "status":
                    uf_list = Form.Registrants.OrderBy(r => r.Status).ToList();
                    break;
                case "datecreated":
                case "dateregistered":
                    uf_list = Form.Registrants.OrderBy(r => r.DateCreated).ToList();
                    break;
                default:
                    uf_list = Form.Registrants.ToList();
                    Guid h_key;
                    if (Guid.TryParse(SortOn, out h_key))
                    {
                        uf_list.Sort((a, b) =>
                        {
                            var a_data = a.Data.Where(i => i.VariableUId == h_key).FirstOrDefault();
                            var b_data = b.Data.Where(i => i.VariableUId == h_key).FirstOrDefault();
                            if (a_data == null)
                                return 1;
                            if (b_data == null)
                                return -1;
                            return a_data.CompareTo(b_data);
                        });
                    }
                    break;
            }
            if (!Ascending)
                uf_list.Reverse();
            var f_list = FilterInformation.Filter(uf_list, Filters, Company, repository).ToList();
            paging.TotalRecords = f_list.Count();
            if (paging.RecordsPerPage != -1)
                f_list = f_list.Skip((paging.Page - 1) * paging.RecordsPerPage).Take(paging.RecordsPerPage).ToList();
            foreach (var variable in Variables)
            {
                var header = new JsonTableHeader();
                switch (variable.ToLower())
                {
                    case "email":
                        header.Label = "Email";
                        header.Id = "email";
                        break;
                    case "confirmation":
                        header.Label = "Confirmation";
                        header.Id = "confirmation";
                        break;
                    case "datecreated":
                    case "dateregistered":
                        header.Label = "Date Registered";
                        header.Id = "dateregistered";
                        break;
                    case "status":
                        header.Label = "Status";
                        header.Id = "status";
                        break;
                    case "audience":
                        header.Label = "Audience";
                        header.Id = "audience";
                        break;
                    default:
                        Guid c_key;
                        if (Guid.TryParse(variable, out c_key))
                        {
                            var component = repository.Search<Component>(c => c.UId == c_key).FirstOrDefault();
                            if (component != null)
                            {
                                header.Label = component.LabelText.GetElipse(75);
                                header.Id = component.UId.ToString();
                            }
                        }
                        break;
                }
                table.AddHeader(header);

            }
            foreach (var registrant in f_list)
            {
                var row = new JsonTableRow()
                {
                    Id = registrant.UId.ToString()
                };
                foreach (var header in table.Headers)
                {
                    var column = new JsonTableColumn();
                    column.HeaderId = header.Id;
                    switch (header.Id)
                    {
                        case "email":
                            column.PrettyValue = FormatValue(urlOn, baseUrl, registrant.Email, header.Id, registrant);
                            column.Value = registrant.Email;
                            break;
                        case "confirmation":
                            column.PrettyValue = FormatValue(urlOn, baseUrl, registrant.Confirmation, header.Id, registrant);
                            column.Value = registrant.Confirmation;
                            break;
                        case "dateregistered":
                        case "datecreated":
                            column.PrettyValue = FormatValue(urlOn, baseUrl, registrant.DateCreated.ToString("yyyy-M-d h:mm:ss tt"), header.Id, registrant);
                            column.Value = registrant.DateCreated.ToString("yyyy-M-d h:mm:ss tt");
                            break;
                        case "status":
                            column.PrettyValue = FormatValue(urlOn, baseUrl, registrant.Status.GetStringValue(), header.Id, registrant);
                            column.Value = registrant.Status.GetStringValue();
                            break;
                        case "type":
                            column.PrettyValue = FormatValue(urlOn, baseUrl, registrant.Type.GetStringValue(), header.Id, registrant);
                            column.Value = registrant.Type.GetStringValue();
                            break;
                        case "audience":
                            column.PrettyValue = FormatValue(urlOn, baseUrl, registrant.Audience != null ? registrant.Audience.Name : "", header.Id, registrant);
                            column.Value = registrant.Audience != null ? registrant.Audience.Name : "";
                            break;
                        default:
                            Guid c_key;
                            if (Guid.TryParse(header.Id, out c_key))
                            {
                                var data = registrant.SearchData(c_key.ToString());
                                if (data != null)
                                {
                                    column.PrettyValue = FormatValue(urlOn, baseUrl, data.GetFormattedValue(), header.Id, registrant);
                                    column.Value = data.GetFormattedValue();
                                    column.Id = header.Id;
                                }
                                else
                                {
                                    column.PrettyValue = FormatValue(urlOn, baseUrl, "", header.Id, registrant);
                                    column.Value = null;
                                    column.Id = header.Id;
                                }
                            }
                            break;
                    }
                    if (String.IsNullOrEmpty(column.Value))
                    {
                        column.Value = "";
                        column.PrettyValue = "<i>No Value</i>";
                    }
                    row.Columns.Add(column);
                }
                table.AddRow(row);
            }
            table.Paging = paging;
            return table;
        }

        public JsonTable GetInventoryReport(FormsRepository repository, Paging paging, List<string> urlOn = null, string baseUrl = null)
        {
            var uf_list = Form.Registrants.ToList();
            var table = new JsonTable()
            {
                Name = Name,
                Id = UId.ToString()
            };
            var f_list = FilterInformation.Filter(uf_list, Filters, Company, repository).ToList();
            paging.TotalRecords = f_list.Count();
            var row = new JsonTableRow();
            table.Rows.Add(row);
            foreach (var variable in Variables)
            {
                switch (variable.ToLower())
                {
                    case "status":
                        table.AddHeader(new JsonTableHeader() { Id = "submitted", Label = "Submitted" });
                        table.AddHeader(new JsonTableHeader() { Id = "incomplete", Label = "Incomplete" });
                        table.AddHeader(new JsonTableHeader() { Id = "canceled", Label = "Canceled" });
                        var count_submitted = f_list.Count(r => r.Status == RegistrationStatus.Submitted).ToString();
                        var count_incomplete = f_list.Count(r => r.Status == RegistrationStatus.Incomplete).ToString();
                        var count_canceled = f_list.Count(r => r.Status == RegistrationStatus.Canceled || r.Status == RegistrationStatus.CanceledByAdministrator || r.Status == RegistrationStatus.CanceledByCompany).ToString();
                        row.Columns.Add(new JsonTableColumn() { PrettyValue = count_submitted, Value = count_submitted, HeaderId = "submitted" });
                        row.Columns.Add(new JsonTableColumn() { PrettyValue = count_incomplete, Value = count_incomplete, HeaderId = "incomplete" });
                        row.Columns.Add(new JsonTableColumn() { PrettyValue = count_canceled, Value = count_canceled, HeaderId = "canceled" });
                        break;
                    case "rsvp":
                        table.AddHeader(new JsonTableHeader() { Id = "rsvpYes", Label = Form.RSVPAccept });
                        table.AddHeader(new JsonTableHeader() { Id = "rsvpNo", Label = Form.RSVPDecline });
                        var count_rsvpYes = f_list.Count(r => r.RSVP).ToString();
                        var count_rsvpNo = f_list.Count(r => !r.RSVP).ToString();
                        row.Columns.Add(new JsonTableColumn() { PrettyValue = count_rsvpYes, Value = count_rsvpYes, HeaderId = "rsvpYes" });
                        row.Columns.Add(new JsonTableColumn() { PrettyValue = count_rsvpNo, Value = count_rsvpNo, HeaderId = "rsvpNo" });
                        break;
                    case "audience":
                        foreach (var audience in Form.Audiences)
                        {
                            table.AddHeader(new JsonTableHeader() { Id = audience.UId.ToString(), Label = audience.Label.GetElipse(75) });
                            var count_aud = f_list.Count(r => r.AudienceKey.HasValue && r.AudienceKey == audience.UId).ToString();
                            row.Columns.Add(new JsonTableColumn() { PrettyValue = count_aud, Value = count_aud, HeaderId = audience.UId.ToString() });
                        }
                        break;
                    case "type":
                        table.AddHeader(new JsonTableHeader() { Id = "liveregistration", Label = "Live Registration" });
                        table.AddHeader(new JsonTableHeader() { Id = "testregistration", Label = "Test Registration" });
                        var count_live = f_list.Count(r => r.Type == RegistrationType.Live).ToString();
                        var count_test = f_list.Count(r => r.Type == RegistrationType.Test).ToString();
                        row.Columns.Add(new JsonTableColumn() { PrettyValue = count_live, Value = count_live, HeaderId = "liveregistration" });
                        row.Columns.Add(new JsonTableColumn() { PrettyValue = count_test, Value = count_test, HeaderId = "testregistration" });
                        break;
                    default:
                        Guid c_key;
                        if (Guid.TryParse(variable, out c_key))
                        {
                            var component = repository.Search<Component>(c => c.UId == c_key).FirstOrDefault();
                            if (component is CheckboxGroup)
                            {
                                var c_comp = (CheckboxGroup)component;
                                var c_items = c_comp.Items.OrderBy(i => i.Order).ToList();
                                var c_counts = new int[c_comp.Items.Count];
                                for (var ci = 0; ci < c_items.Count; ci++)
                                {
                                    c_counts[ci] = 0;
                                }
                                foreach (var reg in f_list)
                                {
                                    var data = reg.SearchData(c_comp.UId.ToString());
                                    if (data == null || String.IsNullOrWhiteSpace(data.Value))
                                        continue;
                                    var selections = JsonConvert.DeserializeObject<List<Guid>>(data.Value);
                                    for (var ci = 0; ci < c_items.Count; ci++)
                                    {
                                        if (selections.Contains(c_items[ci].UId))
                                            c_counts[ci]++;
                                    }
                                }
                                for (var ci = 0; ci < c_items.Count; ci++)
                                {
                                    table.AddHeader(new JsonTableHeader() { Id = c_items[ci].UId.ToString(), Label = c_items[ci].LabelText.GetElipse(75) });
                                    var count_compItem = c_counts[ci].ToString();
                                    row.Columns.Add(new JsonTableColumn() { PrettyValue = count_compItem, Value = count_compItem, HeaderId = c_items[ci].UId.ToString() });
                                }
                            }
                            else if (component is RadioGroup)
                            {
                                var c_comp = (RadioGroup)component;
                                foreach (var item in c_comp.Items.OrderBy(i => i.Order).ToList())
                                {
                                    var count = 0;
                                    table.AddHeader(new JsonTableHeader() { Id = item.UId.ToString(), Label = item.LabelText.GetElipse(75) });
                                    foreach (var reg in f_list)
                                    {
                                        var data = reg.SearchData(c_comp.UId.ToString());
                                        if (data == null || String.IsNullOrWhiteSpace(data.Value))
                                            continue;
                                        if (data.Value == item.UId.ToString())
                                            count++;
                                    }
                                    row.Columns.Add(new JsonTableColumn() { PrettyValue = count.ToString(), Value = count.ToString(), HeaderId = item.UId.ToString() });
                                }
                            }
                            else if (component is DropdownGroup)
                            {
                                var c_comp = (DropdownGroup)component;
                                foreach (var item in c_comp.Items.OrderBy(i => i.Order).ToList())
                                {
                                    var count = 0;
                                    table.AddHeader(new JsonTableHeader() { Id = item.UId.ToString(), Label = item.LabelText.GetElipse(75) });
                                    foreach (var reg in f_list)
                                    {
                                        var data = reg.SearchData(c_comp.UId.ToString());
                                        if (data == null || String.IsNullOrWhiteSpace(data.Value))
                                            continue;
                                        if (data.Value == item.UId.ToString())
                                            count++;
                                    }
                                    row.Columns.Add(new JsonTableColumn() { PrettyValue = count.ToString(), Value = count.ToString(), HeaderId = item.UId.ToString() });
                                }
                            }
                        }
                        break;
                }
            }
            table.Paging = paging;
            return table;
        }

        public JsonTable GetReportData(FormsRepository repository, Paging paging, List<string> urlOn = null)
        {
            var baseUrl = Url.Action("Registrant", "Cloud", null, System.Web.HttpContext.Current.Request.Url.Scheme);
            urlOn = urlOn ?? new List<string>();
            if (!Inventory)
                return GetNonInventyReport(repository, paging, urlOn, baseUrl);
            else
                return GetInventoryReport(repository, paging, urlOn);
        }

        protected string FormatValue(List<string> urlOn, string baseUrl, string value, string key, Registrant registrant)
        {
            if (urlOn.Contains(key))
                return "<a target=\"_blank\" href=\"" + baseUrl + "/" + registrant.UId + "\">" + value + "</a>";
            else
                return value;
        }
    }
}