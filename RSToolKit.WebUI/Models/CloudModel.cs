using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;
using Newtonsoft.Json;
using RSToolKit.Domain;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities.MerchantAccount;
using System.Text.RegularExpressions;

namespace RSToolKit.WebUI.Models
{

    #region Upload Status

    public class RectifyUploadListModel
    {
        public Guid Id { get; set; }
        public List<ContactUploadHeaderModel> Headers { get; set; }
        public bool Overwrite { get; set; }

        public RectifyUploadListModel()
        {
            Headers = new List<ContactUploadHeaderModel>();
            Overwrite = false;
        }
    }

    public class ContactUploadListModel
    {
        [JsonIgnore]
        public List<ContactUploadHeaderModel> Headers { get; set; }
        [JsonIgnore]
        public List<ContactUploadModel> Contacts { get; set; }
        [JsonIgnore]
        public string FilePath { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public Guid CompanyKey { get; set; }
        public Guid? SavedListKey { get; set; }
        public float ProcessingPercent { get; set; }
        public bool ProcessingComplete { get; set; }
        public float UpdatePercent { get; set; }
        public bool UpdateComplete { get; set; }
        public bool NeedsRectified { get; set; }
        public string RectifyLocation { get; set; }
        public string SheetLocation { get; set; }
        public bool NeedsSheetSelection { get; set; }
        public bool CriticalFailure { get; set; }
        public string Message { get; set; }
        public int SheetSelection { get; set; }
        public Dictionary<int, string> Sheets { get; set; }

        public ContactUploadListModel()
        {
            Headers = new List<ContactUploadHeaderModel>();
            Contacts = new List<ContactUploadModel>();
            FilePath = null;
            ProcessingPercent = UpdatePercent = 0.00f;
            ProcessingComplete = UpdateComplete = false;
            SavedListKey = null;
            CriticalFailure = false;
            Message = "";
            SheetSelection = -1;
            Sheets = new Dictionary<int, string>();
        }
    }

    public class ContactUploadModel
    {
        public string Email { get; set; }
        public Guid? ContactKey { get; set; }
        public bool Override { get; set; }
        public List<ContactDataUploadModel> Data { get; set; }

        public ContactUploadModel()
        {
            Email = null;
            Data = new List<ContactDataUploadModel>();
            ContactKey = null;
            Override = false;
        }
    }

    public class ContactUploadHeaderModel
    {
        public Guid? HeaderKey { get; set; }
        public string HeaderName { get; set; }
        [JsonIgnore]
        public int CellIndex { get; set; }

        public ContactUploadHeaderModel()
        {
            HeaderKey = null;
            HeaderName = null;
            CellIndex = 1;
        }
    }

    public class ContactDataUploadModel
    {
        public Guid? HeaderKey { get; set; }
        public string HeaderName { get; set; }
        public string Value { get; set; }

        public ContactDataUploadModel()
        {
            HeaderKey = null;
            Value = null;
            HeaderName = null;
        }
    }

    #endregion

    public class RegistrantDataModel
    {
        public Guid RegistrantKey { get; set; }
        public string Value { get; set; }
        public string ComponentKey { get; set; }
        public Dictionary<Guid, bool> Waitlistings { get; set; }

        public RegistrantDataModel()
        {
            Waitlistings = new Dictionary<Guid, bool>();
        }
    }

    public class PagingRequest
    {
        public Guid UId { get; set; }
        public List<QueryFilter> Filters { get; set; }
        public SortingInformation Sorting { get; set; }
        public Paging Paging { get; set; }

        public PagingRequest()
        {
            Filters = new List<QueryFilter>();
            Sorting = new SortingInformation();
            Paging = new Paging();
        }

        public PagingRequest(Guid id)
            :this()
        {
            UId = id;
        }

        public PagingRequest(Guid id, Paging paging)
            :this(id)
        {
            Paging = paging;
        }
    }

    public class ContactReportRequest
    {
        public string Name { get; set; }
        public bool Overwrite { get; set; }
        public List<QueryFilter> Filters { get; set; }

        public ContactReportRequest()
        {
            Filters = new List<QueryFilter>();
            Name = "";
            Overwrite = false;
        }
    }

    public class SavedListRequest
    {
        public Guid UId { get; set; }
        public int Page { get; set; }
        public int RecordsPerPage { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public Guid CompanyKey { get; set; }
        public SortingInformation Sorting { get; set; }
        public List<QueryFilter> Filters { get; set; }
        public List<Contact> Contacts { get; set; }

        public SavedListRequest()
        {
            Sorting = new SortingInformation();
            Filters = new List<QueryFilter>();
            Contacts = new List<Contact>();
        }
    }

    public class ContactListRequest
    {
        public int Page { get; set; }
        public int RecordsPerPage { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public Guid CompanyKey { get; set; }
        public SortingInformation Sorting { get; set; }
        public List<QueryFilter> Filters { get; set; }
        public List<Contact> Contacts { get; set; }

        public ContactListRequest()
        {
            Sorting = new SortingInformation();
            Filters = new List<QueryFilter>();
            Contacts = new List<Contact>();
        }
    }

    public class CloudModel
    {
        public List<Notification> Notifications { get; set; }
        public IEnumerable<Tuple<Form, INamedNode>> FavoriteReports { get; set; }
        public List<Form> LiveForms { get; set; }
        public FormsRepository Repository { get; set; }

        public CloudModel()
        {
            Notifications = new List<Notification>();
            FavoriteReports = new List<Tuple<Form, INamedNode>>();
            LiveForms = new List<Form>();
        }
    }

    public class Notification
    {
        public string Message { get; set; }
        public string Url { get; set; }
        public string ToolTip { get; set; }
    }

    public class ReportModel
    {
        public Form Form { get; set; }
        public List<FilterInformation> Filters { get; set; }
        public List<SortingInformation> Sortings { get; set; }
        public List<string> Fields { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<Registrant> Registrants { get; set; }
        public FormsRepository Repository { get; set; }
        public int TotalPages { get; set; }
        public string ReportType { get; set; }

        public ReportModel()
        {
            Fields = new List<string>();
            Page = 1;
            PageSize = 25;
            Registrants = new List<Registrant>();
            Filters = new List<FilterInformation>();
            Sortings = new List<SortingInformation>();
            TotalPages = 1;
            ReportType = "all";
        }
    }

    public class FormInfo
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public int Registrations { get; set; }

        public FormInfo()
        {
        }

        public FormInfo(Form form)
        {
            Name = form.Name;
            Id = form.UId;
            DateCreated = form.DateCreated;
            var testing = form.Status == FormStatus.Developement;
            Registrations = form.Registrations();
        }
    }

    public class FormsModel
    {
        public Guid Company { get; set; }
        public List<FilterInformation> Filters { get; set; }
        public List<SortingInformation> Sortings { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<FormInfo> Forms { get; set; }
        public string FormType { get; set; }
        public bool Success { get; set; }

        public FormsModel()
        {
            Page = 1;
            PageSize = 25;
            Forms = new List<FormInfo>();
            Filters = new List<FilterInformation>();
            Sortings = new List<SortingInformation>();
            TotalPages = 1;
            FormType = "all";
            Success = true;
        }

        public FormsModel(List<Form> forms)
            : this()
        {
            Forms = new List<FormInfo>();
            foreach (var form in forms)
            {
                Forms.Add(new FormInfo(form));
            }
        }
    }

    public class HeaderJson
    {
        public string id { get; set; }
        public string html { get; set; }
        public string name { get; set; }
    }


    public class ReportJson
    {
        public Guid Id { get; set; }
        public List<FilterInformation> Filters { get; set; }
        public SortingInformation Sortings { get; set; }
        public List<RegistrantModel> Registrants { get; set; }
        public LinkedList<ReportHeaderJson> Fields { get; set; }
        public List<HeaderJson> FilterHeaders { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalPages { get; set; }
        public string ReportType { get; set; }
        public long TotalRecords { get; set; }

        public ReportJson()
        {
            Filters = new List<FilterInformation>();
            FilterHeaders = new List<HeaderJson>();
            Sortings = new SortingInformation()
                {
                    ActingOn = "Confirmation",
                    Descending = false
                };
            Fields = new LinkedList<ReportHeaderJson>();
            Registrants = new List<RegistrantModel>();
            Page = TotalPages = 1;
            PageSize = 25;
            ReportType = "all";
        }
    }

    public class ReportHeaderJson
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Sortable { get; set; }

        public ReportHeaderJson()
        {
            Id = Guid.Empty;
            Name = "";
            Sortable = true;
        }
    }

    public class RegistrantModel : IFilterable
    {
        public List<RegistrantDataJson> Data { get; set; }
        public string Email { get; set; }
        public string Confirmation { get; set; }
        public string AudienceName { get; set; }
        public Guid? Audience { get; set; }
        public bool RSVP { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset LastEdit { get; set; }
        public string Status { get; set; }

        public RegistrantModel()
        {
            Data = new List<RegistrantDataJson>();
        }

        public RegistrantModel(Registrant registrant, FormsRepository repository, System.Web.Mvc.UrlHelper Url)
        {
            Data = new List<RegistrantDataJson>();
            Email = registrant.Email;
            Confirmation = registrant.Confirmation;
            Audience = registrant.AudienceKey;
            Status = registrant.Status.GetStringValue();
            if (Audience.HasValue)
                AudienceName = registrant.Audience.Name;
            else
                AudienceName = "";
            RSVP = registrant.RSVP;
            Id = registrant.UId;
            Date = registrant.DateCreated;
            LastEdit = registrant.DateModified;
            foreach (var dataPoint in registrant.Data)
            {
                var data = new RegistrantDataJson();
                data.Id = dataPoint.VariableUId.HasValue ? dataPoint.VariableUId.Value : Guid.Empty;
                data.Variable = dataPoint.Component.Variable.Value;
                data.Value = dataPoint.Value;
                data.Descriminator = Component.GetDescriminator(dataPoint.Component);
                data.IsSecure = false;
                var blanked = dataPoint.Empty();
                if (blanked)
                {
                    data.Value = "";
                    data.FormattedValue = "<i>No Value</i>";
                }
                else
                {
                    data.Value = dataPoint.Value ?? "";
                    data.FormattedValue = dataPoint.GetFormattedValue().GetElipse(50);
                }
                if (dataPoint.Component is Input && ((Input)dataPoint.Component).Type == InputType.File)
                {
                    if (!blanked)
                    {
                        var input = (Input)dataPoint.Component;
                        if (input.FileType != "picture")
                            data.FormattedValue = "<a href=" + Url.Action("RegistrantFile", "Cloud", new { id = dataPoint.UId }) + ">Download File</a>";
                        else
                            data.FormattedValue = "View Picture";
                    }
                    else
                    {
                        data.FormattedValue = "No File";
                    }
                }
                else if (dataPoint.Component is Input && ((Input)dataPoint.Component).Type == InputType.UniversalCreditCard)
                {
                    data.IsSecure = true;
                    Guid cardId;
                    if (Guid.TryParse(data.Value, out cardId))
                        data.FormattedValue = repository.SecurePeek<CreditCard>(c => c.UId == cardId).FirstOrDefault();
                    if (String.IsNullOrEmpty(data.FormattedValue))
                        data.FormattedValue = "";
                }
                Data.Add(data);
            }
        }

        public bool TestValue(string field, string testValue, string test, bool caseSensitive = false)
        {
            return false;
        }

        public bool TestValue(string field, string testValue, LogicTest test, Company company, FormsRepository repository, bool caseSensitive = false)
        {
            var value = "";
            testValue = (caseSensitive ? testValue : testValue.ToLower());
            switch (field)
            {
                case "Confirmation":
                    value = Confirmation;
                    break;
                case "Email":
                    value = Email;
                    break;
                case "DateRegistered":
                    value = Date.UtcDateTime.ToString();
                    break;
                case "LastEdit":
                    value = LastEdit.UtcDateTime.ToString();
                    break;
                case "RSVP":
                    value = RSVP.ToString().ToLower();
                    break;
                case "InContactList":
                    var t_email = company.Contacts.Where(c => c.Email.ToLower() == Email.ToLower()).Count() > 0;
                    if (!t_email)
                    {
                        foreach (var header in repository.Search<ContactHeader>(ch => ch.CompanyKey == company.UId && ch.Descriminator == ContactDataType.Email).ToList())
                        {
                            t_email = repository.Search<ContactData>(cd => cd.HeaderKey == header.UId && cd.Value.ToLower() == Email.ToLower()).Count() > 0;
                            if (t_email)
                                break;
                        }
                    }
                    if (testValue == "true")
                        return t_email;
                    else
                        return !t_email;
                default:
                    Guid t_id;
                    RegistrantDataJson t_data;
                    if (Guid.TryParse(field, out t_id))
                        t_data = Data.Where(d => d.Id == t_id).FirstOrDefault();
                    else
                        t_data = Data.Where(d => d.Variable == field).FirstOrDefault();
                    if (t_data != null)
                        value = t_data.Value;
                    break;
            }
            if (value == null)
            {
                decimal t_dec;
                DateTimeOffset t_dto;
                if (decimal.TryParse(testValue, out t_dec))
                    value = int.MinValue.ToString();
                else if (DateTimeOffset.TryParse(testValue, out t_dto))
                    value = DateTimeOffset.MinValue.ToString();
                else
                    value = "";
            }
            return TestIt((caseSensitive ? value : value.ToLower()), testValue.ToLower(), test);
        }
        
        protected bool TestIt(string value, string value2, LogicTest test)
        {
            switch (test)
            {
                case LogicTest.Contains:
                    return value.Contains(value2);
                case LogicTest.DoesNotContain:
                    return !value.Contains(value2);
                case LogicTest.EndsWith:
                    return value.EndsWith(value2);
                case LogicTest.Equal:
                    return value.Equals(value2);
                case LogicTest.GreaterThan:
                case LogicTest.GreaterThanOrEqual:
                case LogicTest.LessThan:
                case LogicTest.LessThanOrEqual:
                    decimal dec_v1, dec_v2;
                    DateTimeOffset dto_v1, dto_v2;
                    if (decimal.TryParse(value, out dec_v1) && decimal.TryParse(value2, out dec_v2))
                    {
                        if (test == LogicTest.GreaterThan)
                            return dec_v1 > dec_v2;
                        else if (test == LogicTest.GreaterThanOrEqual)
                            return dec_v1 >= dec_v2;
                        else if (test == LogicTest.LessThan)
                            return dec_v1 < dec_v2;
                        else
                            return dec_v1 <= dec_v2;
                    }
                    else if (DateTimeOffset.TryParse(value, out dto_v1) && DateTimeOffset.TryParse(value2, out dto_v2))
                    {
                        if (test == LogicTest.GreaterThan)
                            return dto_v1 > dto_v2;
                        else if (test == LogicTest.GreaterThanOrEqual)
                            return dto_v1 >= dto_v2;
                        else if (test == LogicTest.LessThan)
                            return dto_v1 < dto_v2;
                        else
                            return dto_v1 <= dto_v2;
                    }
                    else if (decimal.TryParse(value2, out dec_v2))
                    {
                        if (test == LogicTest.GreaterThan)
                            return value.Length > dec_v2;
                        else if (test == LogicTest.GreaterThanOrEqual)
                            return value.Length >= dec_v2;
                        else if (test == LogicTest.LessThan)
                            return value.Length < dec_v2;
                        else
                            return value.Length <= dec_v2;
                    }
                    else
                    {
                        if (test == LogicTest.GreaterThan)
                            return value.Length > value2.Length;
                        else if (test == LogicTest.GreaterThanOrEqual)
                            return value.Length >= value2.Length;
                        else if (test == LogicTest.LessThan)
                            return value.Length < value2.Length;
                        else
                            return value.Length <= value2.Length;
                    }
                case LogicTest.In:
                case LogicTest.NotIn:
                    try
                    {
                        IEnumerable<string> arr = JsonConvert.DeserializeObject<string[]>(value2);
                        var t_contains = arr.Contains(value);
                        if (test == LogicTest.In)
                            return t_contains;
                        else
                            return !t_contains;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                case LogicTest.NotEqual:
                    return !value.Equals(value2);
                case LogicTest.NotStartsWith:
                    return !value.StartsWith(value2);
                case LogicTest.RegexMatch:
                case LogicTest.RegexNotMatch:
                    var t_rgx = new Regex(value2);
                    var match = t_rgx.IsMatch(value);
                    if (test == LogicTest.RegexMatch)
                        return match;
                    else
                        return !match;
                case LogicTest.StartsWith:
                    return value.StartsWith(value2);
                default:
                    return false;
            }
        }
    }

    public class RegistrantDataJson : IComparable<RegistrantDataJson>
    {
        public Guid Id { get; set; }
        public string Variable { get; set; }
        public string Value { get; set; }
        public string Descriminator { get; set; }
        public string FormattedValue { get; set; }
        public bool IsSecure { get; set; }

        public RegistrantDataJson()
        {
        }

        public RegistrantDataJson(Guid id, string variable, string value, string descriminator, string formattedValue)
        {
            Id = id;
            Variable = variable;
            Value = value;
            Descriminator = descriminator;
            FormattedValue = formattedValue;
        }

        public int CompareValue(RegistrantDataJson other)
        {
            if (other == null)
                return -1;
            if (other.Value == null && Value == null)
                return 0;
            if (other.Value == null)
                return -1;
            if (Value == null)
                return 1;
            decimal dec_1, dec_2;
            if (decimal.TryParse(Value, out dec_1) && decimal.TryParse(other.Value, out dec_2))
                return dec_1.CompareTo(dec_2);
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(RegistrantDataJson other)
        {
            return Id.CompareTo(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (obj is RegistrantDataJson)
                return Id == ((RegistrantDataJson)obj).Id;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class ItemModel
    {
        public Guid UId { get; set; }
        public string Name { get; set; }

        public ItemModel()
        {
            UId = Guid.Empty;
            Name = "";
        }
    }

    public class ComponentModel
    {
        public Guid UId { get; set; }
        public string Type { get; set; }
        public List<ItemModel> Items { get; set; }
        public string Name { get; set; }

        public ComponentModel()
        {
            UId = Guid.Empty;
            Type = "";
            Items = new List<ItemModel>();
        }
    }
    
    public class EnumModel
    {
        public int Index { get; set; }
        public string Name { get; set; }

        public EnumModel()
        {
            Index = 0;
            Name = "";
        }
    }

    public class ContactDataModel
    {
        public Guid ContactKey { get; set; }
        public string Value { get; set; }
        public Guid HeaderKey { get; set; }

        public ContactDataModel()
        {
            Value = null;
        }

    }

    public class ContactEditModel
    {
        public Guid UId { get; set; }
        public string Email { get; set; }
        public Dictionary<Guid, string> Data { get; set; }

        public ContactEditModel()
        {
            Data = new Dictionary<Guid, string>();
        }
    }

}