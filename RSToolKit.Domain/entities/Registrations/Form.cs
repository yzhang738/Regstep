using Newtonsoft.Json;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using RSToolKit.Domain.Collections;
using RSToolKit.Domain.Data.Info;

namespace RSToolKit.Domain.Entities
{
    public class Form
        : ICopyable, IJsonTableInformation, IPermissionHolder, INamedNode, IEmailHolder, IPersonHolder, IProtected
    {

        private int? p_registrations = null;
        private int? p_active = null;
        private int? p_canceled = null;
        private int? p_incompletes = null;
        private int? p_deleted = null;

        [NotMapped]
        public string[] roles
        {
            get
            {
                return new string[] { "Form Builders" };
            }
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        [CascadeDelete]
        public virtual TinyUrl TineyUrl { get; set; }
        [CascadeDelete]
        public virtual List<RSEmail> Emails { get; set; }
        [CascadeDelete]
        public virtual List<LogicBlock> LogicBlocks { get; set; }
        [CascadeDelete]
        public virtual List<Page> Pages { get; set; }
        [CascadeDelete]
        public virtual List<Audience> Audiences { get; set; }
        [CascadeDelete]
        public virtual List<Variable> Variables { get; set; }
        public virtual List<Tag> Tags { get; set; }
        [CascadeDelete]
        public virtual List<Seating> Seatings { get; set; }
        [CascadeDelete]
        public virtual List<CustomText> CustomTexts { get; set; }
        [CascadeDelete]
        public virtual List<FormStyle> FormStyles { get; set; }
        [CascadeDelete]
        public virtual List<DefaultComponentOrder> DefaultComponentOrders { get; set; }
        [CascadeDelete]
        public virtual List<PromotionCode> PromotionalCodes { get; set; }
        [CascadeDelete]
        public virtual List<Registrant> Registrants { get; set; }
        [CascadeDelete]
        public virtual List<RSHtmlEmail> HtmlEmails { get; set; }
        [CascadeDelete]
        public virtual List<SingleFormReport> CustomReports { get; set; }
        public virtual List<AdvancedInventoryReport> InventoryReports { get; set; }

        [NotMapped]
        public IEnumerable<IContentItem> ContentItems
        {
            get
            {
                var t_contents = new List<IContentItem>();
                t_contents.AddRange(CustomTexts);
                t_contents.AddRange(LogicBlocks);
                return t_contents;
            }
        }

        [NotMapped]
        public IEnumerable<IEmail> AllEmails
        {
            get
            {
                var t_emails = new List<IEmail>();
                t_emails.AddRange(Emails);
                t_emails.AddRange(HtmlEmails);
                return t_emails;
            }
        }

        [ForeignKey("MerchantAccountKey")]
        public virtual MerchantAccountInfo MerchantAccount { get; set; }
        public Guid? MerchantAccountKey { get; set; }

        [ForeignKey("FormTemplateKey")]
        public virtual FormTemplate Template { get; set; }
        public Guid? FormTemplateKey { get; set; }

        [ForeignKey("TypeKey")]
        public virtual NodeType Type { get; set; }
        public Guid? TypeKey { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        [ForeignKey("InvitationListKey")]
        public virtual SavedList InvitationList { get; set; }
        public Guid? InvitationListKey { get; set; }

        [ForeignKey("ContactReportKey")]
        public virtual ContactReport ContactReport { get; set; }
        public Guid? ContactReportKey { get; set; }

        public bool DisableShoppingCart { get; set; }

        [NotMapped]
        public IEmailList EmailList
        {
            get
            {
                if (InvitationList != null)
                    return InvitationList;
                else
                    return ContactReport;
            }
            set
            {
                if (value == null)
                {
                    ContactReport = null;
                    ContactReportKey = null;
                    InvitationList = null;
                    InvitationListKey = null;
                }
                else if (value is SavedList)
                {
                    ContactReport = null;
                    ContactReportKey = null;
                    InvitationList = (SavedList)value;
                    InvitationListKey = value.UId;
                }
                else if (value is ContactReport)
                {
                    InvitationList = null;
                    InvitationListKey = null;
                    ContactReport = (ContactReport)value;
                    ContactReportKey = value.UId;
                }
            }
        }

        [NotMapped]
        public Guid? EmailListKey { get; set; }
        [MaxLength(1000)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public string Answer { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LastCode { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public string Question { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Style { get; set; }

        [AllowHtml]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Header { get; set; }

        [AllowHtml]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Footer { get; set; }

        [AllowHtml]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Start { get; set; }

        [AllowHtml]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FormCloseMessage { get; set; }

        [AllowHtml]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string UnderMaintenance { get; set; }

        [AllowHtml]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FormClosed { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CoordinatorName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CoordinatorPhone { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CoordinatorEmail { get; set; }

        public string RSVPAccept { get; set; }
        public string RSVPDecline { get; set; }
        public string CultureString
        {
            get
            {
                return Culture.Name;
            }
            set
            {
                Culture = new CultureInfo(value);
            }
        }

        public decimal? Tax { get; set; }
        public string TaxDescription { get; set; }

        public FormStatus Status { get; set; }
        public Currency Currency { get; set; }
        public FormAccessType AccessType { get; set; }
        public BillingOptions BillingOption { get; set; }

        public bool Approval { get; set; }
        public bool Editable { get; set; }
        public bool Cancelable { get; set; }

        public int Year { get; set; }

        public decimal? Price { get; set; }

        public DateTimeOffset Open { get; set; }
        public DateTimeOffset Close { get; set; }
        public DateTime? EventStart { get; set; }
        public DateTime? EventEnd { get; set; }
        public string Location { get; set; }

        public string Badge { get; set; }

        public bool Survey { get; set; }
        [ClearRelationship("ParentFormKey")]
        [ForeignKey("ParentFormKey")]
        public virtual Form ParentForm { get; set; }
        public Guid? ParentFormKey { get; set; }

        public string RawLoginInformation
        {
            get
            {
                return JsonConvert.SerializeObject(LoginHeaders ?? new List<long>());
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    LoginHeaders = new List<long>();
                else
                    LoginHeaders = JsonConvert.DeserializeObject<List<long>>(value);
            }
        }
        [NotMapped]
        public List<long> LoginHeaders { get; set; }

        [NotMapped]
        public CultureInfo Culture { get; set; }

        [NotMapped]
        public IEnumerable<IPerson> Persons
        {
            get
            {
                return Registrants.AsEnumerable();
            }
        }

        public string EventTimeZone { get; set; }

        public Form()
        {
            DisableShoppingCart = false;
            Registrants = new List<Registrant>();
            UId = Guid.NewGuid();
            Emails = new List<RSEmail>();
            Pages = new List<Page>();
            Audiences = new List<Audience>();
            LogicBlocks = new List<LogicBlock>();
            Variables = new List<Variable>();
            CustomTexts = new List<CustomText>();
            Tags = new List<Tag>();
            Seatings = new List<Seating>();
            Description = "";
            var randGen = new Random();
            LastCode = randGen.Next(0, 1000000).ToString("X");
            Status = FormStatus.Developement;
            Currency = Currency.USD;
            Type = null;
            Question = Answer = null;
            Approval = false;
            Price = null;
            Open = DateTimeOffset.UtcNow.AddDays(30);
            Close = Open.AddDays(30);
            Year = Open.Year;
            Style = "";
            Header = "";
            Footer = "";
            Name = "New Form - " + DateTime.Now.ToShortDateString();
            DefaultComponentOrders = new List<DefaultComponentOrder>();
            Culture = CultureInfo.CreateSpecificCulture("en-US");
            FormStyles = new List<FormStyle>();
            AccessType = FormAccessType.Open;
            Editable = true;
            Cancelable = true;
            BillingOption = BillingOptions.CCandBM;
            RSVPAccept = "Accept";
            RSVPDecline = "Decline";
            Badge = null;
            PromotionalCodes = new List<PromotionCode>();
            HtmlEmails = new List<RSHtmlEmail>();
            Survey = false;
            EventTimeZone = "";
            EventStart = EventEnd = null;
            //Logic = new List<JTableFilter>();
        }

        public static Form New(FormsRepository repository, Company company, User user, string name, Guid? owner = null, Guid? group = null, string permission = "770", string description = null)
        {
            var node = new Form()
            {
                UId = Guid.NewGuid(),
                Name = name != null ? name : "New Form - " + DateTimeOffset.UtcNow.ToString("MM/dd/yyyy h:mm tt"),
                Company = company,
                CompanyKey = company.UId,
                Description = description == null ? "This is a newly created form by " + user.UserName + " working for " + company.Name + "." : description
            };
            PermissionSet.CreateDefaultPermissions(repository, node, company.UId);
            repository.Add(node);
            repository.Commit();
            return node;
        }

        public static Form NewSurvey(FormsRepository repository, Company company, User user, string name, Form form = null, Guid? owner = null, Guid? group = null, string permission = "770", string description = null)
        {
            var node = new Form()
            {
                UId = Guid.NewGuid(),
                Name = name != null ? name : "New Form - " + DateTimeOffset.UtcNow.ToString("MM/dd/yyyy h:mm tt"),
                Company = company,
                CompanyKey = company.UId,
                Survey = true,
                Description = description == null ? "This is a newly created form by " + user.UserName + " working for " + company.Name + "." : description
            };
            if (form != null)
            {
                var t_page = new Page()
                {
                    UId = Guid.NewGuid(),
                    Description = "First page of survey.",
                    Name = "Page 1",
                    Type = PageType.UserDefined,
                    PageNumber = 1,
                };
                node.Pages.Add(t_page);
                node.ParentForm = form;
                node.ParentFormKey = form.UId;
                var order = 0;
                // We need to iterate through all components by page and panel and create the components for the survey.
                foreach (var page in form.Pages.OrderBy(p => p.PageNumber).ToList())
                {
                    foreach (var panel in page.Panels.OrderBy(p => p.Order).ToList())
                    {
                        foreach (var component in panel.Components.OrderBy(p => p.Order).ToList())
                        {
                            if (!(component is IComponentSurveyMappable))
                                continue;
                            var t_panel = new Panel()
                            {
                                Page = t_page,
                                PageKey = t_page.UId,
                                Order = ++order,
                                Name = component.LabelText + " Panel",
                                UId = Guid.NewGuid()
                            };
                            var t_rating = new RatingSelect()
                            {
                                Name = component.LabelText + " rating component.",
                                LabelText = "Rating for " + component.LabelText,
                                MappedComponent = component,
                                MappedComponentKey = component.UId,
                                Company = company,
                                UId = Guid.NewGuid(),
                                Panel = t_panel,
                                Row = 1
                            };
                            t_rating.Variable = new Variable() { Value = component.Variable.Value, Component = t_rating, Form = node };
                            t_panel.Components.Add(t_rating);
                            t_page.Panels.Add(t_panel);
                        }
                    }
                }
            }
            var confPage = new Page()
            {
                Type = PageType.Confirmation,
                Name = "Confirmation Page",
                Locked = true,
                PageNumber = -1,
                UId = Guid.NewGuid()
            };
            var confPanelText = new Panel()
            {
                Name = "Confirmation Panel",
                Order = 1,
                Locked = true,
                UId = Guid.NewGuid()

            };
            var confText = new FreeText()
            {
                Name = "Confirmation Text",
                Row = 1,
                Order = 1,
                Locked = true,
                Html = "Thank you for your input.",
                Company = company,
                UId = Guid.NewGuid()
            };
            confPanelText.Components.Add(confText);
            confPage.Panels.Add(confPanelText);
            node.Pages.Add(confPage);

            PermissionSet.CreateDefaultPermissions(repository, node, company.UId);
            // Now we populate the styles
            var styles = repository.Search<DefaultFormStyle>(ds => ds.CompanyKey == company.UId).ToList();
            foreach (var style in styles)
            {
                var nStyle = new FormStyle() { Owner = user.UId, Group = company.UId, GroupName = style.GroupName, Name = style.Name, Variable = style.Variable, Value = style.Value, Sort = style.Sort, SubSort = style.SubSort, Type = style.Type };
                node.FormStyles.Add(nStyle);
            }

            node.DefaultComponentOrders.AddRange(new DefaultComponentOrder[]
                {
                    new DefaultComponentOrder(typeof(CheckboxGroup)),
                    new DefaultComponentOrder(typeof(CheckboxItem)),
                    new DefaultComponentOrder(typeof(RadioGroup)),
                    new DefaultComponentOrder(typeof(RadioItem)),
                    new DefaultComponentOrder(typeof(Input)),
                    new DefaultComponentOrder(typeof(DropdownGroup)),
                    new DefaultComponentOrder(typeof(RatingSelect))
                });
            if (form == null)
            {
                var folder = repository.Search<Folder>(f => f.CompanyKey == company.UId && f.ParentKey == null).FirstOrDefault();
                if (folder != null)
                {
                    var newPointer = new Pointer()
                    {
                        Folder = folder,
                        Target = node.UId
                    };
                    repository.Add(newPointer);
                }
            }
            else
            {
                var pointer = repository.Search<Pointer>(p => p.Target == form.UId).FirstOrDefault();
                if (pointer != null)
                {
                    var newPointer = new Pointer()
                    {
                        Folder = pointer.Folder,
                        Target = node.UId
                    };
                    repository.Add(newPointer);
                }
            }
            repository.Add(node);
            repository.Commit();
            return node;
        }

        public int Registrations()
        {
            var type = RegistrationType.Live;
            if (Status == FormStatus.Developement)
                type = RegistrationType.Test;
            if (!p_registrations.HasValue)
                p_registrations = Registrants.Where(r => r.Type == type && (r.Status == RegistrationStatus.Submitted || r.Status == RegistrationStatus.Canceled || r.Status == RegistrationStatus.CanceledByAdministrator || r.Status == RegistrationStatus.CanceledByCompany)).Count();
            return p_registrations.Value;
        }

        public int DeletedRegistrations()
        {
            var type = RegistrationType.Live;
            if (Status == FormStatus.Developement)
                type = RegistrationType.Test;
            if (!p_deleted.HasValue)
                p_deleted = Registrants.Where(r => r.Type == type && r.Status == RegistrationStatus.Deleted).Count();
            return p_deleted.Value;
        }

        public int ActiveRegistrations()
        {
            var type = RegistrationType.Live;
            if (Status == FormStatus.Developement)
                type = RegistrationType.Test;
            if (!p_active.HasValue)
                p_active = Registrants.Where(r => r.Type == type && r.Status == RegistrationStatus.Submitted).Count();
            return p_active.Value;
        }

        public int CanceledRegistrations()
        {
            var type = RegistrationType.Live;
            if (Status == FormStatus.Developement)
                type = RegistrationType.Test;
            if (!p_canceled.HasValue)
                p_canceled = Registrants.Where(r => r.Type == type && (r.Status == RegistrationStatus.Canceled || r.Status == RegistrationStatus.CanceledByAdministrator || r.Status == RegistrationStatus.CanceledByCompany)).Count();
            return p_canceled.Value;
        }

        public int IncompleteRegistrations()
        {
            var type = RegistrationType.Live;
            if (Status == FormStatus.Developement)
                type = RegistrationType.Test;
            if (!p_incompletes.HasValue)
                p_incompletes = Registrants.Where(r => r.Type == type && r.Status == RegistrationStatus.Incomplete).Count();
            return p_incompletes.Value;
        }

        public decimal MoneyRecieved()
        {
            var recieved = 0.00m;
            foreach (var reg in Registrants)
            {
                recieved += reg.Transactions;
            }
            return recieved;
        }

        public string GetVariableName(string key)
        {
            switch (key.ToLower())
            {
                case "email":
                    return "Email" ;
                case "confirmation":
                    return "Confirmation";
                case "datecreated":
                case "dateregistered":
                    return "Date Registered";
                case "status":
                    return "Status";
                case "type":
                    return "Type";
                case "audience":
                    return "Audience";
                case "rsvp":
                    return "RSVP";
            }
            var uid = Guid.Empty;
            if (Guid.TryParse(key, out uid))
            {
                var variable = Variables.FirstOrDefault(v => v.UId == uid);
                if (variable != null)
                {
                    return variable.Value;
                }
            }
            return "";
        }

        public string GetValueName(string key, string value)
        {
            using (var context = new EFDbContext())
            {
                var uid = Guid.Empty;
                if (!Guid.TryParse(key, out uid))
                    return key;
                var component = context.Components.Where(c => c.UId == uid).FirstOrDefault();
                if (component == null)
                    return value;
                if (component is CheckboxGroup || component is RadioGroup || component is DropdownGroup)
                {
                    var vUId = Guid.Empty;
                    if (!Guid.TryParse(value, out vUId))
                        return value;
                    var item = context.Components.Where(c => c.UId == vUId).FirstOrDefault();
                    if (item != null)
                        return item.LabelText;
                }
                return value;
            }
        }

        public string PrettyName(string key, FormsRepository repository)
        {
            switch (key.ToLower())
            {
                case "email":
                    return "Email";
                case "confirmation":
                    return "Confirmation";
                case "dateregisterd":
                    return "Date Registered";
                case "status":
                    return "Status";
                case "type":
                    return "Type";
                case "audience":
                    return "Audience";
            }
            Guid c_key;
            if (Guid.TryParse(key, out c_key))
            {
                var component = repository.Search<Component>(c => c.UId == c_key).FirstOrDefault();
                if (component != null)
                    return component.LabelText;
            }
            return "";
        }

        public async Task<IEnumerable<IComponent>> GetMappedComponentsAsync()
        {
            return await Task.Run(() =>
            {
                using (var context = new EFDbContext())
                {
                    return context.Components.Where(c => c.MappedToKey != null && c.Panel.Page.FormKey == UId).ToList();
                }
            });
        }

        public IEnumerable<IComponent> GetPricedUnorderedComponents()
        {
            var comps = Pages.SelectMany(p => p.Panels).SelectMany(p => p.Components).ToList();
            var retComps = new List<IComponent>();
            foreach (var comp in comps)
            {
                if (comp.PriceGroup.Prices.Count > 0)
                    retComps.Add(comp);
            }
            return retComps;
        }

        public IEnumerable<IComponent> GetUnorderedComponents()
        {
            return Pages.SelectMany(p => p.Panels).SelectMany(p => p.Components).ToList();
        }

        public IEnumerable<IComponent> GetComponents(bool priced = false)
        {
            var comps = new List<Component>();
            foreach (var page in Pages.OrderBy(p => p.PageNumber))
            {
                foreach (var panel in page.Panels.OrderBy(p => p.Order))
                {
                    foreach (var component in panel.Components.OrderBy(p => p.Row).ThenBy(p => p.Order))
                    {
                        if (priced)
                        {
                            if (component.PriceGroup.Prices.Count > 0)
                            {
                                comps.Add(component);
                            }
                        }
                        else
                        {
                            comps.Add(component);
                        }
                    }
                }
            }
            return comps.AsEnumerable();
        }

        public async Task<IEnumerable<IComponent>> GetComponentsAsync(bool priced = false)
        {
            return await Task.Run(() =>
            {
                using (var context = new EFDbContext())
                {
                    var comps = new List<Component>();
                    foreach (var page in context.Pages.Where(p => p.FormKey == UId).OrderBy(p => p.PageNumber).ToList())
                    {
                        foreach (var panel in page.Panels.OrderBy(p => p.Order).ToList())
                        {
                            foreach (var component in panel.Components.OrderBy(p => p.Row).ThenBy(p => p.Order).ToList())
                            {
                                if (priced)
                                {
                                    if (component.PriceGroup.Prices.Count > 0)
                                    {
                                        comps.Add(component);
                                    }
                                }
                                else
                                {
                                    comps.Add(component);
                                }
                            }
                        }
                    }
                    return comps.AsEnumerable();                    
                }
            });
        }

        public IEnumerable<IComponentItem> GetComponentItems()
        {
            var compItems = new List<IComponentItem>();
            foreach (var comp in GetComponents().OfType<IComponentItemParent>())
            {
                compItems.AddRange(comp.Children);
            }
            return compItems;
        }

        public ICopyable DeepCopy(string name, Company company, User owner, FormsRepository repository)
        {
            Form newForm = new Form();
            var newComponents = new Dictionary<Guid, Component>();
            newForm.Name = name ?? "Copy of " + Name;
            newForm.AccessType = AccessType;
            newForm.Answer = Answer;
            newForm.Approval = Approval;
            newForm.Badge = Badge;
            newForm.BillingOption = BillingOption;
            newForm.Cancelable = Cancelable;
            newForm.Close = Close;
            newForm.Company = company;
            newForm.CoordinatorEmail = CoordinatorEmail;
            newForm.CoordinatorName = CoordinatorName;
            newForm.CoordinatorPhone = CoordinatorPhone;
            newForm.Culture = Culture;
            newForm.CultureString = CultureString;
            newForm.Currency = Currency;
            newForm.DateCreated = DateTimeOffset.UtcNow;
            newForm.DateModified = newForm.DateCreated;
            newForm.Description = Description;
            newForm.DisableShoppingCart = DisableShoppingCart;
            newForm.Editable = Editable;
            newForm.Footer = Footer;
            newForm.FormClosed = FormClosed;
            newForm.FormCloseMessage = FormCloseMessage;
            newForm.Header = Header;
            newForm.InvitationList = InvitationList;
            newForm.InvitationListKey = InvitationListKey;
            newForm.MerchantAccount = MerchantAccount;
            newForm.MerchantAccountKey = MerchantAccountKey;
            newForm.Open = Open;
            newForm.Price = Price;
            newForm.Question = Question;
            newForm.RSVPAccept = RSVPAccept;
            newForm.RSVPDecline = RSVPDecline;
            newForm.Status = Status;
            newForm.Style = Style;
            newForm.Survey = Survey;
            newForm.ParentForm = ParentForm;
            newForm.ParentFormKey = ParentFormKey;
            newForm.Tags = Tags;
            newForm.Tax = Tax;
            newForm.TaxDescription = TaxDescription;
            newForm.Type = Type;
            newForm.UId = Guid.NewGuid();
            newForm.UnderMaintenance = UnderMaintenance;
            newForm.Year = Year;
            foreach (var seating in Seatings)
            {
                var newSeating = new Seating();
                newSeating.DateCreated = DateTimeOffset.UtcNow;
                newSeating.DateModified = newSeating.DateCreated;
                newSeating.End = seating.End;
                newSeating.Form = newForm;
                newSeating.FullLabel = seating.FullLabel;
                newSeating.MaxSeats = seating.MaxSeats;
                newSeating.Name = seating.Name;
                newSeating.SeatingType = seating.SeatingType;
                newSeating.Start = seating.Start;
                newSeating.UId = Guid.NewGuid();
                newSeating.Waitlistable = seating.Waitlistable;
                newSeating.WaitlistItemLabel = seating.WaitlistItemLabel;
                newSeating.WaitlistLabel = seating.WaitlistLabel;
                newForm.Seatings.Add(newSeating);
            }
            foreach (var audience in Audiences)
            {
                var newAudience = new Audience();
                newAudience.UId = Guid.NewGuid();
                newAudience.DateCreated = DateTimeOffset.UtcNow;
                newAudience.DateModified = newAudience.DateCreated;
                newAudience.Form = newForm;
                newAudience.Label = audience.Label;
                newAudience.Name = audience.Name;
                newForm.Audiences.Add(newAudience);
            }
            foreach (var page in Pages)
            {
                var newPage = new Page();
                newPage.AdminOnly = page.AdminOnly;
                newPage.DateCreated = DateTimeOffset.UtcNow;
                newPage.DateModified = newPage.DateCreated;
                newPage.Description = page.Description;
                newPage.Display = page.Display;
                newPage.Enabled = page.Enabled;
                newPage.Form = newForm;
                newPage.Locked = page.Locked;
                newPage.Name = page.Name;
                newPage.PageNumber = page.PageNumber;
                newPage.RSVP = page.RSVP;
                newPage.Type = page.Type;
                newPage.UId = Guid.NewGuid();
                newForm.Pages.Add(newPage);
                foreach (var audience in page.Audiences)
                {
                    var t_aud = newForm.Audiences.FirstOrDefault(a => a.Label == audience.Label);
                    if (t_aud != null)
                        newPage.Audiences.Add(t_aud);
                }
                foreach (var panel in page.Panels)
                {
                    var newPanel = new Panel();
                    newPanel.UId = Guid.NewGuid();
                    newPanel.AdminOnly = panel.AdminOnly;
                    newPanel.DateCreated = DateTimeOffset.UtcNow;
                    newPanel.DateModified = panel.DateCreated;
                    newPanel.Description = panel.Description;
                    newPanel.Display = panel.Display;
                    newPanel.Enabled = panel.Enabled;
                    newPanel.Name = panel.Name;
                    newPanel.Order = panel.Order;
                    newPanel.Page = newPage;
                    newPanel.RSVP = panel.RSVP;
                    newPage.Panels.Add(newPanel);
                    foreach (var audience in panel.Audiences)
                    {
                        var t_aud = newForm.Audiences.FirstOrDefault(a => a.Label == audience.Label);
                        if (t_aud != null)
                            newPage.Audiences.Add(t_aud);
                    }
                    foreach (var component in panel.Components)
                    {
                        var n_comp = component.DeepCopy(newPanel, newForm.Audiences, newForm.Seatings);
                        newComponents.Add(component.UId, n_comp as Component);
                    }
                }
            }
            foreach (var email in Emails)
            {
                var newEmail = email.DeepCopy(newForm);
            }
            foreach (var email in HtmlEmails)
            {
                var newEmail = email.DeepCopy(newForm);
            }
            foreach (var logicBlock in LogicBlocks)
                logicBlock.DeepCopy(newForm);
            foreach (var customText in CustomTexts)
                customText.DeepCopy(newForm);
            foreach (var style in FormStyles)
                style.DeepCopy(newForm);
            foreach (var dco in DefaultComponentOrders)
                dco.DeepCopy(newForm);
            var oldComponents = GetComponents();
            foreach (var kvp in newComponents)
            {
                var oldComp = oldComponents.FirstOrDefault(c => c.UId == kvp.Key);
                if (oldComp == null)
                    continue;
                foreach (var logic in oldComp.Logics)
                {
                    logic.DeepCopy(kvp.Value, newForm, this);
                }
            }
            foreach (var page in newForm.Pages)
            {
                var oldPage = Pages.FirstOrDefault(p => p.PageNumber == page.PageNumber);
                if (oldPage == null)
                    continue;
                foreach (var logic in oldPage.Logics)
                {
                    logic.DeepCopy(page, newForm, this);
                }
                foreach (var panel in page.Panels)
                {
                    var oldPanel = oldPage.Panels.FirstOrDefault(p => p.Order == panel.Order);
                    if (oldPanel == null)
                        continue;
                    foreach (var logic in oldPanel.Logics)
                    {
                        logic.DeepCopy(panel, newForm, this);
                    }
                }
            }
            if (company.UId == CompanyKey)
            {
                var pointer = repository.Search<Pointer>(p => p.Target == UId).FirstOrDefault();
                if (pointer != null)
                {
                    var newPointer = new Pointer()
                    {
                        Folder = pointer.Folder,
                        Target = newForm.UId
                    };
                    repository.Add(newPointer);
                }
            }
            else
            {
                var folder = repository.Search<Folder>(f => f.CompanyKey == company.UId && f.ParentKey == null).FirstOrDefault();
                if (folder != null)
                {
                    var newPointer = new Pointer()
                    {
                        Folder = folder,
                        Target = newForm.UId
                    };
                    repository.Add(newPointer);
                }
            }
            PermissionSet.CreateDefaultPermissions(repository, newForm, company.UId);
            repository.Add(newForm);
            return newForm;
        }

        protected JTable JTableSurvey(JTable table, bool admin = false, bool noHtml = false)
        {
            if (String.IsNullOrEmpty(table.Name))
                table.Name = "All Registrations";
            table.Id = UId.ToString();
            table.Parent = Name;
            table.Description = Description;
            var allComponents = GetComponents();
            var order = 0;
            // Create an initialize a variable to determin if using test records or live records.
            var testing = Status == FormStatus.Developement || Status == FormStatus.Ready;
            // Create a query of registrants.
            var regQuery = Registrants.Where(r => r.Type == (testing ? RegistrationType.Test : RegistrationType.Live));
            // If not an admin requesting table, we get rid of the deleted variables.
            if (!admin)
                regQuery = regQuery.Where(r => r.Status != RegistrationStatus.Deleted);
            if (table.Headers.Count == 0)
            {
                if (ParentForm != null)
                    table.Headers.Add(new JTableHeader() { Id = "email", Label = "Email", Order = ++order });
                foreach (var component in allComponents)
                {
                    if (component is FreeText)
                        continue;
                    if (component is RatingSelect)
                    {
                        var rs = component as RatingSelect;
                        if (rs.MappedComponent != null)
                        {
                            if (rs.MappedComponent is IComponentItemParent)
                            {
                                foreach (var item in (rs.MappedComponent as IComponentItemParent).Children.OrderBy(i => i.Order))
                                {
                                    table.Headers.Add(new JTableHeader() { Id = item.UId.ToString(), Label = item.LabelText, Order = ++order, Group = rs.MappedComponent.LabelText, Type = "rating=>" + JsonConvert.SerializeObject(new { min = rs.MinRating, max = rs.MaxRating, step = rs.Step.GetFloatValue(), type = rs.RatingSelectType.GetStringValue() }) });
                                }
                            }
                            else
                            {
                                table.Headers.Add(new JTableHeader() { Id = component.UId.ToString(), Label = component.Variable.Value, Order = ++order, Type = "rating=>" + JsonConvert.SerializeObject(new { min = rs.MinRating, max = rs.MaxRating, step = rs.Step.GetFloatValue(), type = rs.RatingSelectType.GetStringValue() }) });
                            }
                        }
                    }
                    else if (component is IComponentItemParent)
                    {
                        table.Headers.Add(new JTableHeader() { Id = component.UId.ToString(), Label = component.Variable.Value, Order = ++order, Type = (component is IComponentMultipleSelection ? "multipleSelection" : "itemParent"), PossibleValues = (component as IComponentItemParent).Children.OrderBy(i => i.Order).Select(i => new JTableHeaderPossibleValue() { Id = i.UId.ToString(), Label = i.LabelText }).ToList() });
                    }
                    else
                    {
                        var type = "string";
                        if (component is Input)
                        {
                            if (((Input)component).Type == Domain.Entities.Components.InputType.Date || ((Input)component).Type == Domain.Entities.Components.InputType.DateTime || ((Input)component).Type == Domain.Entities.Components.InputType.Time)
                                type = "date";
                        }
                        table.Headers.Add(new JTableHeader() { Id = component.UId.ToString(), Label = component.Variable.Value, Order = ++order, Type = type });
                    }
                }
            }
            else
            {
                foreach (var header in table.Headers.Where(h => h.Type == "itemParent" || h.Type == "multipleSelection"))
                {
                    var component = allComponents.FirstOrDefault(c => c.UId.ToString() == header.Id);
                    if (component == null || !(component is IComponentItemParent))
                        continue;
                    header.PossibleValues = (component as IComponentItemParent).Children.Select(i => new JTableHeaderPossibleValue() { Id = i.UId.ToString(), Label = i.LabelText }).ToList();
                }
            }
            table.Rows.Clear();
            IEnumerable<Registrant> registrants = regQuery;
            table.TotalRecords = registrants.Count();
            foreach (var registrant in registrants)
            {
                var row = new JTableRow() { Id = registrant.UId.ToString() };
                foreach (var header in table.Headers)
                {
                    var data = registrant.Data.FirstOrDefault(d => d.VariableUId.ToString() == header.Id);
                    if (data == null)
                        continue;
                    row.Columns.Add(new JTableColumn() { HeaderId = data.VariableUId.ToString(), Id = data.UId.ToString(), PrettyValue = data.GetFormattedValue(), Value = data.Value });
                }
                if (ParentForm != null)
                {
                    row.Columns.Add(new JTableColumn() { HeaderId = "email", Id = "email", PrettyValue = registrant.Email, Value = registrant.Email });
                }
                table.AddRow(row);
            }
            return table;
        }

        protected JTable JTableEmailReport(JTable table, bool admin = false, User user = null, bool noHtml = false)
        {
            if (String.IsNullOrEmpty(table.Name))
                table.Name = "Email Report";
            table.Id = UId.ToString();
            table.Parent = Name;
            table.Description = Description;
            table.Sortings.Add(new JTableSorting() { ActingOn = "email", Ascending = true, Order = 1 });
            if (table.Headers.Count == 0)
            {
                var order = 0;
                //Recipient
                table.Headers.Add(new JTableHeader() { Id = "recipient", Label = "Recipient", Order = ++order, Editable = false });
                //Email
                var t_emails = AllEmails.OrderBy(e => e.Name).Select(e => new JTableHeaderPossibleValue() { Id = e.UId.ToString(), Label = e.Name }).ToList();
                t_emails.Add(new JTableHeaderPossibleValue() { Id = "Generic Email", Label = "Generic Email" });
                table.Headers.Add(new JTableHeader() { Id = "email", Label = "Email", Order = ++order, Type = "itemParent", Editable = false, PossibleValues = t_emails });
                //Email Type
                table.Headers.Add(new JTableHeader() { Id = "type", Label = "Type", Order = ++order, Type = "itemParent", Editable = false, PossibleValues = Enum.GetValues(typeof(EmailType)).Cast<EmailType>().Select(e => new JTableHeaderPossibleValue() { Id = ((int)e).ToString(), Label = e.GetStringValue() }).ToList() });
                //Status
                var possibleStatus = new List<JTableHeaderPossibleValue>()
                {
                    new JTableHeaderPossibleValue() { Id = "0", Label = "Attempting to Send" },
                    new JTableHeaderPossibleValue() { Id = "1", Label = "Delivered" },
                    new JTableHeaderPossibleValue() { Id = "2", Label = "Opened" },
                    new JTableHeaderPossibleValue() { Id = "3", Label = "Clicked" },
                    new JTableHeaderPossibleValue() { Id = "4", Label = "Permanent Bounce" },
                    new JTableHeaderPossibleValue() { Id = "5", Label = "Temporary Bounce" }
                };
                table.Headers.Add(new JTableHeader() { Id = "status", Label = "Status", Order = ++order, Type = "multipleSelection", PossibleValues = possibleStatus, Editable = false });
                //Sent for processing
                table.Headers.Add(new JTableHeader() { Id = "sentForProcessing", Label = "Sent For Processing", Order = ++order, Type = "boolean", Editable = false, PossibleValues = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "1", Label = "Yes" }, new JTableHeaderPossibleValue() { Id = "0", Label = "No" } } });
                //Proccessed
                table.Headers.Add(new JTableHeader() { Id = "processed", Label = "Processed", Order = ++order, Type = "boolean", Editable = false, PossibleValues = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "1", Label = "Yes" }, new JTableHeaderPossibleValue() { Id = "0", Label = "No" } } });
                //Delivered
                table.Headers.Add(new JTableHeader() { Id = "delivered", Label = "Delivered", Order = ++order, Type = "boolean", Editable = false, PossibleValues = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "1", Label = "Yes" }, new JTableHeaderPossibleValue() { Id = "0", Label = "No" } } });
                //Opened
                table.Headers.Add(new JTableHeader() { Id = "opened", Label = "Opened", Order = ++order, Type = "boolean", Editable = false, PossibleValues = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "1", Label = "Yes" }, new JTableHeaderPossibleValue() { Id = "0", Label = "No" } } });
                //Clicked
                table.Headers.Add(new JTableHeader() { Id = "clicked", Label = "Clicked", Order = ++order, Type = "boolean", Editable = false, PossibleValues = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "1", Label = "Yes" }, new JTableHeaderPossibleValue() { Id = "0", Label = "No" } } });
                //Bounced
                table.Headers.Add(new JTableHeader() { Id = "bounced", Label = "Bounced", Order = ++order, Type = "boolean", Editable = false, PossibleValues = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "1", Label = "Yes" }, new JTableHeaderPossibleValue() { Id = "0", Label = "No" } } });
            }
            using (var repository = new FormsRepository())
            {
                foreach (var email in AllEmails)
                {
                    System.Linq.Expressions.Expression<Func<EmailSend, bool>> search;
                    if (email.EmailType != EmailType.Invitation)
                    {
                        if (Status == FormStatus.Developement || Status == FormStatus.Ready)
                        {
                            search = e => e.EmailKey == email.UId && e.Registrant != null && e.Registrant.Type == RegistrationType.Test;
                        }
                        else
                        {
                            search = e => e.EmailKey == email.UId && e.Registrant != null && e.Registrant.Type == RegistrationType.Live;
                        }
                    }
                    else
                    {
                        search = e => e.EmailKey == email.UId;
                    }
                    foreach (var emailSend in repository.Search<EmailSend>(search).OrderByDescending(e => e.DateSent).Distinct(new EmailSendComparer_Recipient()).ToList())
                    {
                        var t_eventObj = emailSend.EmailEvents.OrderByDescending(ev => ev.Event, Comparer<string>.Create((a, b) =>
                        {
                            var a_i = -1;
                            var b_i = -1;
                            if (EmailEvent.eventDepth.ContainsKey(a))
                            {
                                a_i = EmailEvent.eventDepth[a];
                            }
                            if (EmailEvent.eventDepth.ContainsKey(b))
                            {
                                b_i = EmailEvent.eventDepth[b];
                            }
                            return a_i.CompareTo(b_i);
                        })).FirstOrDefault();
                        var t_event_i = -1;
                        if (EmailEvent.eventDepth.ContainsKey(t_eventObj.Event))
                            t_event_i = EmailEvent.eventDepth[t_eventObj.Event];
                        var row = new JTableRow()
                        {
                            Id = emailSend.UId.ToString()
                        };
                        if (table.Headers.Count(h => h.Id == "recipient") > 0)
                            row.Columns.Add(new JTableColumn() { HeaderId = "recipient", Value = emailSend.Recipient, PrettyValue = (noHtml ? emailSend.Recipient : "<a href=\"#\" class=\"email-information\" data-id=\"" + emailSend.UId + "\">" + emailSend.Recipient + "</a>") });
                        if (table.Headers.Count(h => h.Id == "type") > 0)
                            row.Columns.Add(new JTableColumn() { HeaderId = "type", Value = ((int)email.EmailType).ToString(), PrettyValue = email.EmailType.GetStringValue() });
                        if (table.Headers.Count(h => h.Id == "email") > 0)
                            row.Columns.Add(new JTableColumn() { HeaderId = "email", PrettyValue = email.Name, Value = email.UId.ToString() });
                        if (table.Headers.Count(h => h.Id == "status") > 0)
                            row.Columns.Add(new JTableColumn() { HeaderId = "status", Value = t_event_i.ToString(), PrettyValue = t_eventObj.ToString(), Id = emailSend.UId.ToString() + "_status" });
                        var events = emailSend.EmailEvents.OrderByDescending(ev => ev.Date).ToList();
                        EmailEvent c_event = null;
                        if (table.Headers.Count(h => h.Id == "sentForProcessing") > 0)
                        {
                            c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "sent for processing");
                            row.Columns.Add(new JTableColumn() { HeaderId = "sentForProcessing", Id = emailSend.UId.ToString() + "sentForProcessing", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.ToString(@"d \b\y e", user) : "No") });
                        }
                        if (table.Headers.Count(h => h.Id == "processed") > 0)
                        {
                            c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "processed");
                            row.Columns.Add(new JTableColumn() { HeaderId = "processed", Id = emailSend.UId.ToString() + "processed", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.ToString(@"d \b\y e", user) : "No") });
                        }
                        if (table.Headers.Count(h => h.Id == "delivered") > 0)
                        {
                            c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "delivered");
                            row.Columns.Add(new JTableColumn() { HeaderId = "delivered", Id = emailSend.UId.ToString() + "delivered", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.ToString(@"d \b\y e", user) : "No") });
                        }
                        if (table.Headers.Count(h => h.Id == "opened") > 0)
                        {
                            c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "opened");
                            row.Columns.Add(new JTableColumn() { HeaderId = "opened", Id = emailSend.UId.ToString() + "opened", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.ToString(@"d \b\y e", user) : "No") });
                        }
                        if (table.Headers.Count(h => h.Id == "clicked") > 0)
                        {
                            c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "clicked");
                            row.Columns.Add(new JTableColumn() { HeaderId = "clicked", Id = emailSend.UId.ToString() + "clicked", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.ToString(@"d \b\y e", user) : "No") });
                        }
                        if (table.Headers.Count(h => h.Id == "bounced") > 0)
                        {
                            c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "permanent bounce" || ev.Event.ToLower() == "temporary bounce");
                            row.Columns.Add(new JTableColumn() { HeaderId = "bounced", Id = emailSend.UId.ToString() + "bounced", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.ToString(@"d \b\y e", user) : "No") });
                        }
                        table.AddRow(row);
                    }
                }
                foreach (var emailSend in repository.Search<EmailSend>(es => es.EmailKey == null && es.FormKey.HasValue && es.FormKey == UId).OrderBy(es => es.DateSent))
                {
                    var t_eventObj = emailSend.EmailEvents.OrderByDescending(ev => ev.Event, Comparer<string>.Create((a, b) =>
                    {
                        var a_i = -1;
                        var b_i = -1;
                        if (EmailEvent.eventDepth.ContainsKey(a))
                        {
                            a_i = EmailEvent.eventDepth[a];
                        }
                        if (EmailEvent.eventDepth.ContainsKey(b))
                        {
                            b_i = EmailEvent.eventDepth[b];
                        }
                        return a_i.CompareTo(b_i);
                    })).FirstOrDefault();
                    var t_event_i = -1;
                    if (EmailEvent.eventDepth.ContainsKey(t_eventObj.Event))
                        t_event_i = EmailEvent.eventDepth[t_eventObj.Event];
                    var row = new JTableRow()
                    {
                        Id = emailSend.UId.ToString()
                    };
                    if (table.Headers.Count(h => h.Id == "recipient") > 0)
                        row.Columns.Add(new JTableColumn() { HeaderId = "recipient", Value = emailSend.Recipient, PrettyValue = (noHtml ? emailSend.Recipient : "<a href=\"#\" class=\"email-information\" data-id=\"" + emailSend.UId + "\">" + emailSend.Recipient + "</a>") });
                    if (table.Headers.Count(h => h.Id == "type") > 0)
                        row.Columns.Add(new JTableColumn() { HeaderId = "type", Value = ((int)EmailType.Unclassified).ToString(), PrettyValue = EmailType.Unclassified.GetStringValue() });
                    if (table.Headers.Count(h => h.Id == "email") > 0)
                        row.Columns.Add(new JTableColumn() { HeaderId = "email", Value = "Generic Email", PrettyValue = "Generic Form Email" });
                    if (table.Headers.Count(h => h.Id == "status") > 0)
                        row.Columns.Add(new JTableColumn() { HeaderId = "status", Value = t_event_i.ToString(), PrettyValue = t_eventObj.ToString(), Id = emailSend.UId.ToString() + "_status" });
                    var events = emailSend.EmailEvents.OrderByDescending(ev => ev.Date).ToList();
                    EmailEvent c_event = null;
                    if (table.Headers.Count(h => h.Id == "sentForProcessing") > 0)
                    {
                        c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "sent for processing");
                        row.Columns.Add(new JTableColumn() { HeaderId = "sentForProcessing", Id = emailSend.UId.ToString() + "sentForProcessing", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.ToString(@"d \b\y e", user) : "No") });
                    }
                    if (table.Headers.Count(h => h.Id == "delivered") > 0)
                    {
                        c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "delivered");
                        row.Columns.Add(new JTableColumn() { HeaderId = "delivered", Id = emailSend.UId.ToString() + "delivered", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.ToString(@"d \b\y e", user) : "No") });
                    }
                    if (table.Headers.Count(h => h.Id == "opened") > 0)
                    {
                        c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "opened");
                        row.Columns.Add(new JTableColumn() { HeaderId = "opened", Id = emailSend.UId.ToString() + "opened", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.ToString(@"d \b\y e", user) : "No") });
                    }
                    if (table.Headers.Count(h => h.Id == "clicked") > 0)
                    {
                        c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "clicked");
                        row.Columns.Add(new JTableColumn() { HeaderId = "clicked", Id = emailSend.UId.ToString() + "clicked", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.ToString(@"d \b\y e", user) : "No") });
                    }
                    if (table.Headers.Count(h => h.Id == "bounced") > 0)
                    {
                        c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "permanent bounce" || ev.Event.ToLower() == "temporary bounce");
                        row.Columns.Add(new JTableColumn() { HeaderId = "bounced", Id = emailSend.UId.ToString() + "bounced", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.ToString(@"d \b\y e", user) : "No") });
                    }
                    table.AddRow(row);
                }
            }
            return table;
        }

        protected JTable JTableInvitationReport(JTable table, bool admin = false, User user = null, bool noHtml = false)
        {
            if (String.IsNullOrEmpty(table.Name))
                table.Name = "Invitation Report";
            table.Id = UId.ToString();
            table.Parent = Name;
            table.Description = Description;
            table.Sortings.Add(new JTableSorting() { ActingOn = "email", Ascending = true, Order = 1 });
            // Now we add all the registration headers, but we mark them as hidden.
            var allComponents = GetComponents();
            if (table.Headers.Count == 0)
            {
                table.AddHeader(new JTableHeader() { Id = "email", Label = "Email" });
                table.AddHeader(new JTableHeader() { Id = "invitationsent", Label = "Invitation Sent", Type = "itemParent", PossibleValues = AllEmails.Where(e => e.EmailType == EmailType.Invitation).OrderBy(e => e.Name).Select(e => new JTableHeaderPossibleValue() { Id = e.UId.ToString(), Label = e.Name }).ToList() });
                table.AddHeader(new JTableHeader() { Id = "sendcount", Label = "Send Count", Type = "number" });
                var possibleStatus = new List<JTableHeaderPossibleValue>()
                {
                    new JTableHeaderPossibleValue() { Id = "0", Label = "Attempting to Send" },
                    new JTableHeaderPossibleValue() { Id = "1", Label = "Delivered" },
                    new JTableHeaderPossibleValue() { Id = "2", Label = "Opened" },
                    new JTableHeaderPossibleValue() { Id = "3", Label = "Clicked" },
                    new JTableHeaderPossibleValue() { Id = "4", Label = "Permanent Bounce" },
                    new JTableHeaderPossibleValue() { Id = "5", Label = "Temporary Bounce" }
                };
                table.AddHeader(new JTableHeader() { Id = "status", Label = "Delivery Status", Type = "itemParent", PossibleValues = possibleStatus });
                table.AddHeader(new JTableHeader() { Id = "firstname", Label = "First Name" });
                table.AddHeader(new JTableHeader() { Id = "lastname", Label = "Last Name" });
                var regStatusValues = new List<JTableHeaderPossibleValue>()
                {
                    new JTableHeaderPossibleValue() { Id = "-1", Label = "Not Registered" }
                };
                regStatusValues.AddRange(Enum.GetValues(typeof(RegistrationStatus)).Cast<RegistrationStatus>().Select(s => new JTableHeaderPossibleValue() { Id = ((int)s).ToString(), Label = s.GetStringValue() }));
                table.AddHeader(new JTableHeader() { Id = "registrationstatus", Label = "Registration Status", Type = "itemParent", SortByPretty = false, PossibleValues = regStatusValues });

                //Confirmation
                table.AddHeader(new JTableHeader() { Id = "regconfirmation", Label = "Confirmation", Removed = true});
                //Email
                table.AddHeader(new JTableHeader() { Id = "regemail", Label = "Registrant Email", Removed = true });
                //RSVP
                if (Pages.FirstOrDefault(p => p.Type == PageType.RSVP && p.Enabled) != null)
                    table.AddHeader(new JTableHeader() { Id = "rsvp", Label = "RSVP", Removed = true, Type = "itemParent", PossibleValues = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "True", Label = RSVPAccept }, new JTableHeaderPossibleValue() { Id = "False", Label = RSVPDecline } } });
                //Audience
                if (Pages.FirstOrDefault(p => p.Type == PageType.Audience && p.Enabled) != null && Audiences.Count > 0)
                    table.AddHeader(new JTableHeader() { Id = "audience", SortByPretty = true, Label = "Audience", Removed = true, Type = "itemParent", PossibleValues = Audiences.OrderBy(a => a.Order).Select(a => new JTableHeaderPossibleValue() { Id = a.UId.ToString(), Label = a.Label }).ToList() });
                //Date Registered
                table.AddHeader(new JTableHeader() { Id = "dateRegistered", SortByPretty = true, Label = "Date Registered", Removed = true, Type = "date" });
                //Last Edit
                table.AddHeader(new JTableHeader() { Id = "lastEdit", SortByPretty = true, Label = "Last Edit", Removed = true, Type = "date" });
                //Edited By
                var t_possibleUsers = new List<JTableHeaderPossibleValue>();
                t_possibleUsers.Add(new JTableHeaderPossibleValue() { Id = "registrant", Label = "Registrant" });
                foreach (var e_user in Company.Users.OrderBy(u => u.UserName))
                {
                    var name = "";
                    if (!String.IsNullOrWhiteSpace(e_user.FirstName) && String.IsNullOrWhiteSpace(e_user.LastName))
                        name = " (" + e_user.FirstName + ")";
                    else if (!String.IsNullOrWhiteSpace(e_user.LastName) && String.IsNullOrWhiteSpace(e_user.FirstName))
                        name = " (" + e_user.LastName + ")";
                    else if (!String.IsNullOrWhiteSpace(e_user.FirstName) && !String.IsNullOrWhiteSpace(e_user.LastName))
                        name = " (" + e_user.LastName + ", " + e_user.FirstName + ")";
                    t_possibleUsers.Add(new JTableHeaderPossibleValue() { Id = e_user.UId.ToString(), Label = e_user.UserName + name });
                }
                table.AddHeader(new JTableHeader() { Id = "editedBy", Label = "Edited By", SortByPretty = true, Removed = true, Type = "itemParent", PossibleValues = t_possibleUsers });
                //Balance
                if (PromotionalCodes.Count > 0)
                    table.AddHeader(new JTableHeader() { Id = "promotions", Label = "Discount Codes", Removed = true, Type = "multipleSelection", PossibleValues = PromotionalCodes.OrderBy(c => c.Code).Select(c => new JTableHeaderPossibleValue() { Id = c.UId.ToString(), Label = c.Code }).ToList() });
                if (Tax.HasValue)
                    table.AddHeader(new JTableHeader() { Id = "tax", Label = "Tax", Removed = true, Type = "number" });
                if (!DisableShoppingCart)
                {
                    table.AddHeader(new JTableHeader() { Id = "paymentMethod", Label = "Payment Method", Removed = true, Type = "itemParent", PossibleValues = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "nonerecorded", Label = "None Recorded" }, new JTableHeaderPossibleValue() { Id = "billme", Label = "Bill Me" }, new JTableHeaderPossibleValue() { Id = "credit", Label = "Credit Card" } } });
                    table.AddHeader(new JTableHeader() { Id = "balance", Label = "Balance", Removed = true, Type = "number" });
                }
                foreach (var component in allComponents)
                {
                    if (component is FreeText)
                        continue;
                    var label = component.Name;
                    if (component is IComponentItemParent)
                    {
                        var values = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "", Label = "No Selection" } };
                        values.AddRange((component as IComponentItemParent).Children.Select(i => new JTableHeaderPossibleValue() { Id = i.UId.ToString(), Label = i.LabelText }).ToList());
                        table.AddHeader(new JTableHeader() { Id = component.UId.ToString(), Label = label, Removed = true, Type = (component is IComponentMultipleSelection ? "multipleSelection" : "itemParent"), PossibleValues = values });
                    }
                    else
                    {
                        var type = "string";
                        if (component is Input)
                        {
                            if (((Input)component).Type == Domain.Entities.Components.InputType.Date || ((Input)component).Type == Domain.Entities.Components.InputType.DateTime || ((Input)component).Type == Domain.Entities.Components.InputType.Time)
                                type = "date";
                        }
                        table.AddHeader(new JTableHeader() { Id = component.UId.ToString(), Label = label, Removed = true, Type = type });
                    }
                }
                table.Sortings.Clear();
                table.Sortings.Add(new JTableSorting() { ActingOn = "email", Ascending = true, Order = 1 });
            }
            else
            {
                foreach (var header in table.Headers.Where(h => h.Type == "itemParent" || h.Type == "multipleSelection"))
                {
                    var component = allComponents.FirstOrDefault(c => c.UId.ToString() == header.Id);
                    if (component == null || !(component is IComponentItemParent))
                        continue;
                    var values = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "", Label = "No Selection" } };
                    values.AddRange((component as IComponentItemParent).Children.Select(i => new JTableHeaderPossibleValue() { Id = i.UId.ToString(), Label = i.LabelText }).ToList());
                    header.PossibleValues = values;
                }
            }
            using (var repository = new FormsRepository())
            {
                foreach (var email in AllEmails.Where(e => e.EmailType == EmailType.Invitation))
                {
                    System.Linq.Expressions.Expression<Func<EmailSend, bool>> search;
                    if (email.EmailType != EmailType.Invitation)
                    {
                        if (Status == FormStatus.Developement || Status == FormStatus.Ready)
                        {
                            search = e => e.EmailKey == email.UId && e.Registrant != null && e.Registrant.Type == RegistrationType.Test;
                        }
                        else
                        {
                            search = e => e.EmailKey == email.UId && e.Registrant != null && e.Registrant.Type == RegistrationType.Live;
                        }
                    }
                    else
                    {
                        search = e => e.EmailKey == email.UId;
                    }
                    foreach (var emailSend in repository.Search<EmailSend>(search).OrderByDescending(e => e.DateSent).GroupBy(e => e.Recipient.ToLower()).ToList())
                    {
                        // Now we need to find the deepest event
                        var t_eventObj = emailSend.SelectMany(e => e.EmailEvents).OrderByDescending(ev => ev.Event, Comparer<string>.Create((a, b) => {
                            var a_i = -1;
                            var b_i = -1;
                            if (EmailEvent.eventDepth.ContainsKey(a))
                            {
                                a_i = EmailEvent.eventDepth[a];
                            }
                            if (EmailEvent.eventDepth.ContainsKey(b))
                            {
                                b_i = EmailEvent.eventDepth[b];
                            }
                            return a_i.CompareTo(b_i);
                        })).FirstOrDefault();
                        var t_event = "Attempting to Send";
                        if (t_eventObj != null)
                            t_event = t_eventObj.Event;
                        var t_event_i = -1;
                        if (EmailEvent.eventDepth.ContainsKey(t_event))
                            t_event_i = EmailEvent.eventDepth[t_event];
                        var t_first = emailSend.First();
                        var row = new JTableRow()
                        {
                            Id = t_first.UId.ToString()
                        };
                        if (t_first.Contact == null)
                            continue;
                        // We know a contact is attached. Now we need to search through registrants.
                        var allEmails = new List<string>();
                        allEmails.Add(t_first.Contact.Email.ToLower());
                        t_first.Contact.Data.Where(d => d.Header.Descriminator == ContactDataType.Email).ToList().ForEach(e => allEmails.Add(e.Value.ToLower()));
                        var addressMismatch = false;
                        foreach (var t_email in allEmails)
                        {
                            if (t_first.Registrant != null)
                                break;
                            t_first.Registrant = Registrants.FirstOrDefault(r => r.Email.ToLower() == t_email.ToLower());
                        }
                        if (t_first.Registrant != null)
                        {
                            if (t_first.Contact.Email.ToLower() != t_first.Registrant.Email.ToLower())
                                addressMismatch = true;
                            emailSend.ToList().ForEach(e => e.Registrant = t_first.Registrant);
                        }
                        var t_firstname = t_first.Contact.Data.Where(e => e.Header.Name.ToLower() == "first name").FirstOrDefault();
                        var t_lastname = t_first.Contact.Data.Where(e => e.Header.Name.ToLower() == "last name").FirstOrDefault();
                        var t_firstnamevalue = "";
                        var t_lastnamevalue = "";
                        if (t_firstname == null)
                            t_firstname = t_first.Contact.Data.Where(e => e.Header.Name.ToLower() == "firstname").FirstOrDefault();
                        if (t_lastname == null)
                            t_lastname = t_first.Contact.Data.Where(e => e.Header.Name.ToLower() == "lastname").FirstOrDefault();
                        if (t_firstname != null)
                            t_firstnamevalue = t_firstname.Value;
                        if (t_lastname != null)
                            t_lastnamevalue = t_lastname.Value;
                        row.Columns.Add(new JTableColumn() { HeaderId = "email", Value = t_first.Recipient.ToLower(), Id = t_first.UId.ToString() + "_email", printPretty = false, PrettyValue = (noHtml ? t_first.Recipient : "<a href=\"#\" class=\"email-sendlist\" data-id=\"" + t_first.UId + "\">" + t_first.Recipient + (addressMismatch ? "<span class='glyphicon glyphicon-wargning glyphicon-small text-rsred'></span>" : "" ) + "</a>") } );
                        row.Columns.Add(new JTableColumn() { HeaderId = "invitationsent", Value = email.UId.ToString(), PrettyValue = email.Name, Id = t_first.UId.ToString() + "_invitationsent" });
                        row.Columns.Add(new JTableColumn() { HeaderId = "sendcount", Value = emailSend.Count().ToString(), PrettyValue = emailSend.Count().ToString(), Id = t_first.UId.ToString() + "_sendcount" });
                        row.Columns.Add(new JTableColumn() { HeaderId = "status", Value = t_event_i.ToString(), PrettyValue = t_eventObj.ToString(@"E: n \b\y e"), Id = t_first.UId.ToString() + "_status" });
                        row.Columns.Add(new JTableColumn() { HeaderId = "firstname", Value = t_firstnamevalue, PrettyValue = t_firstnamevalue, Id = t_first.UId.ToString() + "_firstname" });
                        row.Columns.Add(new JTableColumn() { HeaderId = "lastname", Value = t_lastnamevalue, PrettyValue = t_lastnamevalue, Id = t_first.UId.ToString() + "_lastname" });
                        var link = (noHtml ? "Not Registered" : "Not Registered <a href=\"#\" class=\"jTable_registration-link\" data-action=\"Start\" data-controller=\"AdminRegister\" data-options='{\"formKey\":\"" + UId.ToString() + "\",\"email\":\"" + t_first.Contact.Email + "\"}' target=\"_blank\">Admin Register</a>");
                        if (t_first.Registrant != null)
                        {
                            if (t_first.Registrant.Status == RegistrationStatus.Incomplete)
                            {
                                link = (noHtml ? "Incomplete" : link = "Incomplete <a href=\"#\" class=\"jTable_registration-link\" data-action=\"Start\" data-controller=\"AdminRegister\"  data-options='{\"formKey\":\"" + UId.ToString() + "\",\"email\":\"" + t_first.Contact.Email + "\"}' target=\"_blank\">Admin Continue</a>");
                            }
                            else
                            {
                                link = (noHtml ? t_first.Registrant.Status.GetStringValue() : link = t_first.Registrant.Status.GetStringValue() + " <a href=\"#\" class=\"jTable_registration-link\" data-action=\"Confirmation\" data-controller=\"AdminRegister\" data-options='{\"registrantKey\":\"" + t_first.Registrant.UId.ToString() + "\"}' target=\"_blank\">Admin Confirmation</a>");
                            }
                        }

                        row.Columns.Add(new JTableColumn() { HeaderId = "registrationstatus", Value = t_first.Registrant == null ? "-1" : ((int)t_first.Registrant.Status).ToString(), PrettyValue = link });
                        if (t_first.Registrant != null)
                        {
                            var registrant = t_first.Registrant;
                            if (table.Headers.Count(h => h.Id == "regconfirmation") > 0)
                                row.Columns.Add(new JTableColumn() { HeaderId = "regconfirmation", Id = "confirmation", printPretty = false, PrettyValue = (!noHtml ? "<a href='#' data-action='Registrant' data-controller='Cloud' data-options='{\"id\":\"" + registrant.UId.ToString() + "\"}'>" + registrant.Confirmation + "</a>" : registrant.Confirmation), Value = registrant.Confirmation, Editable = false });
                            if (table.Headers.Count(h => h.Id == "regemail") > 0)
                                row.Columns.Add(new JTableColumn() { HeaderId = "regemail", Id = "email", PrettyValue = registrant.Email, Value = registrant.Email, Editable = false });
                            if (table.Headers.Count(h => h.Id == "dateRegistered") > 0)
                                row.Columns.Add(new JTableColumn() { HeaderId = "dateRegistered", Id = "dateRegistered", PrettyValue = registrant.DateCreated.ToString("s"), Value = registrant.DateCreated.ToString(user), Editable = false });
                            if (table.Headers.Count(h => h.Id == "lastEdit") > 0)
                                row.Columns.Add(new JTableColumn() { HeaderId = "lastEdit", Id = "lastEdit", PrettyValue = registrant.DateModified.ToString("s"), Value = registrant.DateModified.ToString(user), Editable = false });
                            if (table.Headers.Count(h => h.Id == "status") > 0)
                                row.Columns.Add(new JTableColumn() { HeaderId = "status", Value = t_event_i.ToString(), PrettyValue = t_eventObj.Event + ": " + t_eventObj.Notes + (String.IsNullOrWhiteSpace(t_eventObj.Email) ? "" : " (" + t_eventObj.Email + ")"), Id = t_first.UId.ToString() + "_status" });
                            if (table.Headers.Count(h => h.Id == "editedBy") > 0)
                            {
                                var modifiedBy = Company.Users.FirstOrDefault(u => u.UId == registrant.ModifiedBy);
                                row.Columns.Add(new JTableColumn() { HeaderId = "editedBy", Id = "editedBy", PrettyValue = modifiedBy != null ? modifiedBy.UserName : "Registrant", Value = modifiedBy != null ? modifiedBy.UserName : "registrant", Editable = false });
                            }
                            if (table.Headers.Count(h => h.Id == "rsvp") > 0)
                                row.Columns.Add(new JTableColumn() { HeaderId = "rsvp", Id = "rsvp", PrettyValue = registrant.RSVP ? RSVPAccept : RSVPDecline, Value = registrant.RSVP.ToString(), Editable = false });
                            if (table.Headers.Count(h => h.Id == "audience") > 0)
                                row.Columns.Add(new JTableColumn() { HeaderId = "audience", Id = "audience", PrettyValue = registrant.Audience != null ? registrant.Audience.Label : "", Value = registrant.Audience != null ? registrant.Audience.UId.ToString() : "", Editable = false });
                            if (table.Headers.Count(h => h.Id == "inContactList") > 0)
                            {
                                var inList = EmailList.GetAllEmailAddresses().FirstOrDefault(e => e.ToLower() == registrant.Email.ToLower()) != null;
                                row.Columns.Add(new JTableColumn() { HeaderId = "inContactList", Id = "InContactList", PrettyValue = inList ? "Yes" : "No", Value = inList ? "True" : "False", Editable = false });
                            }
                            if (table.Headers.Count(h => h.Id == "promotions") > 0)
                            {
                                var p_codes = "";
                                var v_codes = new List<Guid>();
                                foreach (var code in registrant.PromotionalCodes)
                                {
                                    p_codes += code.Code.Code + ", ";
                                    v_codes.Add(code.CodeKey);
                                }
                                if (v_codes.Count > 0)
                                    p_codes = p_codes.Substring(0, p_codes.Length - 2);
                                row.Columns.Add(new JTableColumn() { HeaderId = "promotions", Id = "promotions", PrettyValue = p_codes, Editable = false, Value = JsonConvert.SerializeObject(v_codes) });
                            }
                            if (table.Headers.Count(h => h.Id == "tax") > 0)
                            {
                                if (registrant.Data.FirstOrDefault(d => d.Component.Variable != null && d.Component.Variable.Value == "__NoTax" && !String.IsNullOrEmpty(d.Value) && d.Value.ToLower() == "true") == null)
                                {
                                    var t_tax = Math.Round((registrant.Fees * Tax) ?? 0m);
                                    row.Columns.Add(new JTableColumn() { HeaderId = "tax", Editable = false, PrettyValue = t_tax.ToString("c", Culture), Value = t_tax.ToString() });
                                }
                                else
                                {
                                    row.Columns.Add(new JTableColumn() { HeaderId = "tax", Editable = false, PrettyValue = "Tax Exempt by Administrator", Value = "0" });
                                }
                            }
                            if (table.Headers.Count(h => h.Id == "balance") > 0)
                            {
                                var t_payment = "nonerecorded";
                                var t_p_payment = "<i>None Recorded</i>";
                                if ((registrant.Fees + registrant.Adjustings) > 0)
                                {
                                    if (registrant.TransactionRequests.Count == 0)
                                    {
                                        t_payment = "billme";
                                        t_p_payment = "Bill Me";
                                    }
                                    else
                                    {
                                        t_payment = "credit";
                                        t_p_payment = "Credit Card";
                                    }
                                }
                                row.Columns.Add(new JTableColumn() { HeaderId = "paymentMethod", Id = "paymentMethod", PrettyValue = t_p_payment, Value = t_payment, Editable = false });
                                row.Columns.Add(new JTableColumn() { HeaderId = "balance", Id = "balance", PrettyValue = (noHtml ? registrant.TotalOwed.ToString("c", Culture) : "<a href='#' class='balance'>" + registrant.TotalOwed.ToString("c", Culture) + "</a>"), Value = registrant.TotalOwed.ToString(), Editable = false });
                            }
                            foreach (var data in registrant.Data)
                            {
                                row.Columns.Add(new JTableColumn() { HeaderId = data.VariableUId.ToString(), Id = data.UId.ToString(), PrettyValue = data.GetFormattedValue(), Value = data.Value });
                            }
                        }
                        else if (t_first.Contact != null)
                        {
                            foreach (var mapped in GetComponents().Where(c => c.MappedTo != null))
                            {
                                var contactData = t_first.Contact.Data.FirstOrDefault(d => d.HeaderKey == mapped.MappedToKey);
                                if (contactData == null)
                                    continue;
                                row.Columns.Add(new JTableColumn() { HeaderId = mapped.UId.ToString(), Id = "contact_" + contactData.UId, PrettyValue = contactData.GetFormattedValue(), Value = contactData.Value, Editable = false });
                            }
                        }
                        table.AddRow(row);
                    }
                }
            }
            return table;
        }

        #region IJsonTableInformation
        /// <summary>
        /// Gets the <code>JsonTableInformation</code>.
        /// </summary>
        /// <param name="type">The type of JsonTableInformation to get.</param>
        /// <param name="showDeleted">If deleted records should be shown.</param>
        /// <returns>The information collected.</returns>
        public JsonTableInformation GetJsonTableInformation(IComplexDictionary complexDic = null, ReportType type = ReportType.Form, bool showDeleted = false)
        {
            switch (type)
            {
                case ReportType.Invitation:
                case ReportType.Email:
                case ReportType.Form:
                default:
                    return _GetFormJsonTableInformation(complexDic, showDeleted);
            }
        }
        /// <summary>
        /// Gets the <code>JsonTableInformation</code>.
        /// </summary>
        /// <param name="progress">The progress data.</param>
        /// <param name="type">The type of JsonTableInformation to get.</param>
        /// <param name="showDeleted">If deleted records should be shown.</param>
        /// <returns>The information collected.</returns>
        public TableInformation GetTableInformation(ITokenDictionary progress, ReportType type = ReportType.Form, bool showDeleted = false)
        {
            switch (type)
            {
                case ReportType.Invitation:
                case ReportType.Email:
                case ReportType.Form:
                default:
                    return _GetEmailTableInformation(progress, showDeleted);
            }
        }

        /// <summary>
        /// Gets the <code>JsonTableInformation</code> asynchronously.
        /// </summary>
        /// <param name="type">The type of JsonTableInformation to get.</param>
        /// <param name="showDeleted">If deleted records should be shown.</param>
        /// <returns>The information collected.</returns>
        public async Task<JsonTableInformation> GetJsonTableInformationAsync(IComplexDictionary complexDic = null, ReportType type = ReportType.Form, bool showDeleted = false)
        {
            switch (type)
            {
                case ReportType.Invitation:
                case ReportType.Email:
                case ReportType.Form:
                default:
                    return await _GetFormJsonTableInformationAsync(complexDic, showDeleted);
            }
        }

        #region Type: Form
        /// <summary>
        /// Gets a <code>JsonTableInformation</code> based on registrant data asyncronously.
        /// </summary>
        /// <param name="showDeleted">If deleted records should be shown.</param>
        /// <returns>The information collected.</returns>
        protected JsonTableInformation _GetFormJsonTableInformation(IComplexDictionary complexDic, bool showDeleted)
        {
            var reportData = new JsonTableInformation();
            reportData.Table = UId;
            var headerContext = new EFDbContext();
            var registrantContext = new EFDbContext();
            var regType = RegistrationType.Live;
            if (Status == FormStatus.Developement)
                regType = RegistrationType.Test;
            var stati = new List<RegistrationStatus>();
            if (!showDeleted)
                stati.Add(RegistrationStatus.Deleted);
            var registrantsFiltering = registrantContext.Registrants.Where(r => r.Type == regType && r.FormKey == UId && !stati.Contains(r.Status));
            var registrants = registrantsFiltering.ToList();
            if (complexDic != null)
                complexDic.UpdateProgress(UId, .1F);
            var components = headerContext.Components.Include(c => c.Variable).Where(c => !(c is FreeText || c is RadioItem || c is CheckboxItem || c is DropdownItem) && c.Panel.Page.FormKey == UId).OrderBy(c => c.Panel.Page.PageNumber).ThenBy(c => c.Panel.Order).ThenBy(c => c.Row).ThenBy(c => c.Order).ToList();
            if (complexDic != null)
                complexDic.UpdateProgress(UId, .2F);
            // We grab all the headers.
            var tableHeaders = new List<string>();
            tableHeaders.Add("Confirmation");
            tableHeaders.Add("Email");
            tableHeaders.Add("Status");
            if (Pages.FirstOrDefault(p => p.Type == PageType.RSVP && p.Enabled) != null)
                tableHeaders.Add("RSVP");
            if (Audiences.Count > 0 && Pages.FirstOrDefault(p => p.Type == PageType.Audience && p.Enabled) != null)
                tableHeaders.Add("Audience");
            tableHeaders.Add("Date Registered");
            tableHeaders.Add("Last Edit");
            tableHeaders.Add("Edited By");
            tableHeaders.Add("In Contact List");
            if (PromotionalCodes.Count > 0)
                tableHeaders.Add("Promotions");
            if (!DisableShoppingCart)
            {
                tableHeaders.Add("Balance");
                tableHeaders.Add("Tax");
                tableHeaders.Add("Fees");
                tableHeaders.Add("Adjustments");
                tableHeaders.Add("Transactions");
                tableHeaders.Add("Payment Method");
                if (MerchantAccount != null)
                    tableHeaders.Add("Credit Last Four");
            }
            tableHeaders.AddRange(components.Select(c => c.SortingId.ToString()));
            if (complexDic != null)
                complexDic.UpdateProgress(UId, .25F);
            long hid;
            var componentIds = tableHeaders.Where(h => long.TryParse(h, out hid)).Select(h => long.Parse(h)).ToList();
            var headers = headerContext.Components.Include(c => c.Variable).Where(c => componentIds.Contains(c.SortingId)).OrderBy(h => h.SortingId).ToList();
            var headersOrdered = new List<JsonTableHeader>();
            foreach (var hId in tableHeaders)
            {
                var type = "text";
                if (long.TryParse(hId, out hid))
                {
                    var header = headers.FirstOrDefault(h => h.SortingId == hid);
                    if (header != null)
                    {
                        if (header is Input)
                        {
                            #region input
                            switch ((header as Input).ValueType)
                            {
                                case Components.ValueType.DateTime:
                                    type = "datetime";
                                    break;
                                case Components.ValueType.Decimal:
                                case Components.ValueType.Number:
                                    type = "number";
                                    break;
                            }
                            switch ((header as Input).Type)
                            {
                                case Components.InputType.Date:
                                    type = "date";
                                    break;
                                case Components.InputType.DateTime:
                                    type = "datetime";
                                    break;
                                case Components.InputType.Time:
                                    type = "time";
                                    break;
                            }
                            #endregion
                        }
                        else if (header is IComponentMultipleSelection)
                        {
                            type = "multiple selection";
                        }
                        else if (header is IComponentItemParent)
                        {
                            type = "single selection";
                        }
                        headersOrdered.Add(new JsonTableHeader() { Editable = true, Id = header.SortingId.ToString(), Value = (header.Variable != null ? header.Variable.Value : header.LabelText), Token = header.UId, Type = type });
                    }
                }
                else
                {
                    var editable = false;
                    if (hId.ToLower().In("email", "audience", "rsvp"))
                        editable = true;
                    if (hId.ToLower().In("balance", "fees", "tax", "adjustments", "transactions"))
                        type = "number";
                    if (hId.ToLower().In("date registered", "last edit"))
                        type = "date";
                    if (hId.ToLower().In("rsvp", "in contact list", "audience", "status"))
                        type = "text-raw";
                    headersOrdered.Add(new JsonTableHeader() { Id = hId, Value = hId, Token = Guid.Empty, Editable = editable, Type = type });
                }
            }
            reportData.Headers = headersOrdered;
            if (complexDic != null)
                complexDic.UpdateProgress(UId, .3F);
            // Now we work with registrants

            // Now we order the registrants.
            var rows = new List<JsonTableRow>();
            var count = registrants.Count * reportData.Headers.Count;
            var curCount = 0;
            foreach (var registrant in registrants)
            {
                registrant.UpdateAccounts();
                var row = new JsonTableRow() { Id = registrant.SortingId, Token = registrant.UId, ModificationToken = registrant.ModificationToken, DateModified = registrant.DateModified };
                var values = new List<JsonTableValue>();
                foreach (var header in reportData.Headers)
                {
                    curCount++;
                    values.Add(registrant.GetJsonTableValue(header));
                    if (complexDic != null)
                        complexDic.UpdateProgress(UId, 0.3F + (curCount / (float)count * .7F));
                }
                row.Values = values;
                rows.Add(row);
            }
            reportData.Rows = rows;
            reportData.FilterHeaders = GetFormHeaders();
            complexDic.UpdateProgress(UId, 1F);
            return reportData;
        }

        /// <summary>
        /// Gets a <code>JsonTableInformation</code> based on registrant data asyncronously.
        /// </summary>
        /// <param name="showDeleted">If deleted records should be shown.</param>
        /// <returns>The information collected.</returns>
        protected async Task<JsonTableInformation> _GetFormJsonTableInformationAsync(IComplexDictionary complexDic, bool showDeleted)
        {
            var reportData = new JsonTableInformation();
            reportData.Table = UId;
            var headerContext = new EFDbContext();
            var registrantContext = new EFDbContext();
            var regType = RegistrationType.Live;
            if (Status == FormStatus.Developement)
                regType = RegistrationType.Test;
            var registrantsFiltering = registrantContext.Registrants.Where(r => r.Type == regType && r.FormKey == UId);
            if (!showDeleted)
                registrantsFiltering.Where(r => r.Status != RegistrationStatus.Deleted);
            var registrantsTask = registrantsFiltering.ToListAsync();
            var componentsTask = headerContext.Components.Include(c => c.Variable).Where(c => !(c is FreeText || c is RadioItem || c is CheckboxItem || c is DropdownItem) && c.Panel.Page.FormKey == UId).OrderBy(c => c.Panel.Page.PageNumber).ThenBy(c => c.Panel.Order).ThenBy(c => c.Row).ThenBy(c => c.Order).ToListAsync();

            // We grab all the headers.
            var tableHeaders = new List<string>();
            tableHeaders.Add("Confirmation");
            tableHeaders.Add("Email");
            tableHeaders.Add("Status");
            if (Pages.FirstOrDefault(p => p.Type == PageType.RSVP && p.Enabled) != null)
                tableHeaders.Add("RSVP");
            if (Audiences.Count > 0 && Pages.FirstOrDefault(p => p.Type == PageType.Audience && p.Enabled) != null)
                tableHeaders.Add("Audience");
            tableHeaders.Add("Date Registered");
            tableHeaders.Add("Last Edit");
            tableHeaders.Add("Edited By");
            tableHeaders.Add("In Contact List");
            if (PromotionalCodes.Count > 0)
                tableHeaders.Add("Promotions");
            if (!DisableShoppingCart)
            {
                tableHeaders.Add("Balance");
                tableHeaders.Add("Tax");
                tableHeaders.Add("Fees");
                tableHeaders.Add("Adjustments");
                tableHeaders.Add("Transactions");
                tableHeaders.Add("Payment Method");
                if (MerchantAccount != null)
                    tableHeaders.Add("Credit Last Four");
            }
            var components = await componentsTask;
            tableHeaders.AddRange(components.Select(c => c.SortingId.ToString()));

            long hid;
            var componentIds = tableHeaders.Where(h => long.TryParse(h, out hid)).Select(h => long.Parse(h)).ToList();
            var headers = headerContext.Components.Include(c => c.Variable).Where(c => componentIds.Contains(c.SortingId)).OrderBy(h => h.SortingId).ToList();
            var headersOrdered = new List<JsonTableHeader>();
            foreach (var hId in tableHeaders)
            {
                var type = "text";
                if (long.TryParse(hId, out hid))
                {
                    var header = headers.FirstOrDefault(h => h.SortingId == hid);
                    if (header != null)
                    {
                        if (header is Input)
                        {
                            #region input
                            switch ((header as Input).ValueType)
                            {
                                case Components.ValueType.DateTime:
                                    type = "date";
                                    break;
                                case Components.ValueType.Decimal:
                                case Components.ValueType.Number:
                                    type = "number";
                                    break;
                            }
                            switch ((header as Input).Type)
                            {
                                case Components.InputType.Date:
                                case Components.InputType.DateTime:
                                case Components.InputType.Time:
                                    type = "date";
                                    break;
                            }
                            #endregion
                        }
                        else if (header is IComponentMultipleSelection)
                        {
                            type = "multiple selection";
                        }
                        else if (header is IComponentItemParent)
                        {
                            type = "single selection";
                        }
                        headersOrdered.Add(new JsonTableHeader() { Editable = true, Id = header.SortingId.ToString(), Value = (header.Variable != null ? header.Variable.Value : header.LabelText), Token = header.UId, Type = type });
                    }
                }
                else
                {
                    var editable = false;
                    if (hId.ToLower().In("email", "audience", "rsvp"))
                        editable = true;
                    if (hId.ToLower().In("balance", "fees", "tax", "adjustments", "transactions"))
                        type = "number";
                    if (hId.ToLower().In("date registered", "last edit"))
                        type = "date";
                    if (hId.ToLower().In("rsvp", "in contact list", "audience", "status"))
                        type = "text-raw";
                    headersOrdered.Add(new JsonTableHeader() { Id = hId, Value = hId, Token = Guid.Empty, Editable = editable, Type = type });
                }
            }
            reportData.Headers = headersOrdered;

            // Now we work with registrants
            var registrants = await registrantsTask;

            // Now we order the registrants.
            var rows = new List<JsonTableRow>();
            var count = registrants.Count * reportData.Headers.Count;
            var curCount = 0;
            foreach (var registrant in registrants)
            {
                var row = new JsonTableRow() { Id = registrant.SortingId, Token = registrant.UId };
                var values = new List<JsonTableValue>();
                foreach (var header in reportData.Headers)
                {
                    values.Add(registrant.GetJsonTableValue(header));
                    if (complexDic != null)
                        complexDic.UpdateProgress(UId, (curCount / (float)count));
                }
                row.Values = values;
                rows.Add(row);
            }
            reportData.Rows = rows;
            reportData.FilterHeaders = GetFormHeaders();
            return reportData;
        }
        #endregion

        #region Type: Email
        /// <summary>
        /// Gets a <code>JsonTableInformation</code> based on invitation data.
        /// </summary>
        /// <param name="progress">The progress data.</param>
        /// <param name="showDeleted">If deleted records should be shown.</param>
        /// <returns>The information collected.</returns>
        protected TableInformation _GetEmailTableInformation(ITokenDictionary tokens, bool showDeleted)
        {
            var table = new TableInformation(UId);
            tokens.Add(table);
            var numberOfTicks = 0L;
            table.UpdateMessage("Pre Processing", "Retrieving headers for form and emails.");

            // We need to create the headers.
            var headers = new List<JsonTableHeader>();
            headers.AddRange(this._GetEmailHeaders());
            headers.AddRange(this._GetFormHeaders(hide: true));
            table.Headers = headers;

            // Now we get the registration type.
            var regType = RegistrationType.Live;
            if (Status == FormStatus.Developement)
                regType = RegistrationType.Test;

            // We need to create the context we will be using.
            using (var context = new EFDbContext())
            {
                // Grab the contact headers that are emails.
                table.UpdateMessage(details: "Grabbing contact headers with type 'Email'.");
                var emailHeaders = context.ContactHeaders.Where(c => c.Descriminator == ContactDataType.Email && c.CompanyKey == CompanyKey).Select(c => c.UId).ToList();

                // Count all the emails and headers and set the tick fraction.
                table.UpdateMessage(details: "Calculating process requirements.");
                var invitationEmails_count = 0L;
                var invitation_ids = new List<Guid>();
                invitation_ids.AddRange(context.RSEmails.Where(e => e.FormKey == UId).Select(e => e.UId).ToList());
                invitation_ids.AddRange(context.RSHtmlEmails.Where(e => e.FormKey == UId).Select(e => e.UId).ToList());
                invitationEmails_count += invitation_ids.Count;
                var emailSends_count = 0L;
                foreach (var id in invitation_ids)
                    emailSends_count += context.EmailSends.Where(e => e.EmailKey == id).GroupBy(e => e.Recipient).Count();
                numberOfTicks = emailSends_count * (long)headers.Count;
                table.SetTickFraction(numberOfTicks);

                // Grab the emails from the database.
                var emails = new List<IEmail>();
                emails.AddRange(context.RSEmails.Where(e => e.FormKey == UId).ToList());
                emails.AddRange(context.RSHtmlEmails.Where(e => e.FormKey == UId).ToList());
                foreach (var email in emails)
                {
                    // Now we run through each email send.
                    table.UpdateMessage("Retrieving Data", "Processing email " + email.Name + "");
                    
                    foreach (var sendGroup in context.EmailSends.Include(e => e.EmailEvents).Where(e => e.EmailKey == email.UId).OrderByDescending(e => e.DateSent).GroupBy(e => e.Recipient.ToLower()).ToList())
                    {
                        // Grab the most recent send.
                        var send_recent = sendGroup.First();
                        table.UpdateMessage("Processing Data", "Processing email data for " + send_recent.Recipient + " on " + email.Name + ".");
                        table.UpdateMessage(details: "Creating row.");
                        // Create the row to hold the data.
                        var row = new JsonTableRow()
                        {
                            Id = send_recent.SortingId,
                            Token = send_recent.UId,
                            DateModified = send_recent.DateSent
                        };

                        table.UpdateMessage(details: "Checking for associated contact.");
                        // Check to see if there is a contact associated with the send.
                        if (send_recent.Contact == null)
                        {
                            // No contact, we need to see if there is a contact with that email.
                            var contactEmail = context.ContactData.Where(c => emailHeaders.Contains(c.HeaderKey) && c.Value.ToLower() == send_recent.Recipient.ToLower()).Include(c => c.Contact).FirstOrDefault();
                            if (contactEmail != null)
                            {
                                // We found a matching contact.
                                send_recent.Contact = contactEmail.Contact;
                                foreach (var send in sendGroup)
                                    send.Contact = contactEmail.Contact;
                            }
                        }

                        table.UpdateMessage(details: "Checking for a registration record.");
                        // Check to see if there is a registrant associated with the send.
                        if (send_recent.Registrant == null)
                        {
                            // There is no registrant so we look for one.
                            var allEmails = new List<string>();
                            if (send_recent.Contact != null)
                            {
                                allEmails = context.ContactData.Where(c => c.UId == send_recent.ContactKey && emailHeaders.Contains(c.HeaderKey) && c.Value != null).Select(c => c.Value.ToLower()).ToList();
                                allEmails.Add(send_recent.Contact.Email.ToLower());
                            }
                            else
                            {
                                allEmails.Add(send_recent.Recipient.ToLower());
                            }
                            var registrant = context.Registrants.FirstOrDefault(r => r.Type == regType && allEmails.Contains(r.Email.ToLower()));
                            if (registrant != null)
                            {
                                // We found a registrant. We need to assign it.
                                foreach (var send in sendGroup)
                                    send.Registrant = registrant;
                                row.Id = registrant.SortingId;
                            }
                        }

                        table.UpdateMessage(details: "Looking for recipients name.");
                        // Now we get the first and last name.  The registration record is used if it can, or it falls back on the contact.
                        var t_firstnamevalue = "Not in Contact List";
                        var t_lastnamevalue = "Not in Contact List";
                        string fnNameRef = null;
                        string lnNameRef = null;
                        if (send_recent.Contact != null)
                        {
                            var t_firstname = send_recent.Contact.Data.Where(e => e.Header.Name == "FirstName").FirstOrDefault();
                            var t_lastname = send_recent.Contact.Data.Where(e => e.Header.Name == "LastName").FirstOrDefault();
                            if (t_firstname == null)
                            {
                                t_firstnamevalue = "";
                                fnNameRef = "contact";
                            }
                            else
                            {
                                fnNameRef = "contact";
                                t_firstnamevalue = t_firstname.PrettyValue;
                            }
                            if (t_lastname == null)
                            {
                                t_lastnamevalue = "";
                                lnNameRef = "contact";
                            }
                            else
                            {
                                lnNameRef = "contact";
                                t_lastnamevalue = t_lastname.PrettyValue;
                            }
                        }
                        if (send_recent.Registrant != null)
                        {
                            var t_firstname = send_recent.Registrant.Data.FirstOrDefault(d => d.Component.Variable != null && d.Component.Variable.Value == "FirstName");
                            var t_lastname = send_recent.Registrant.Data.FirstOrDefault(d => d.Component.Variable != null && d.Component.Variable.Value == "LastName");
                            if ((t_firstname == null || !String.IsNullOrWhiteSpace(t_firstname.Value)))
                            {
                                t_firstnamevalue = "";
                                fnNameRef = "registrant";
                            }
                            else
                            {
                                fnNameRef = "registrant";
                                t_firstnamevalue = t_firstname.GetFormattedValue();
                            }
                            if ((t_lastname == null || String.IsNullOrWhiteSpace(t_lastname.Value)))
                            {
                                t_lastnamevalue = "";
                                lnNameRef = "registrant";
                            }
                            else
                            {
                                lnNameRef = "registrant";
                                t_lastnamevalue = t_lastname.GetFormattedValue();
                            }
                        }
                        if (send_recent.Registrant != null && send_recent.Registrant.Type != regType)
                            continue;
                        table.Rows.Add(row);
                        table.UpdateMessage(message: "Populating Data", details: "Filling data in table.");
                        // Now we start filling in data.
                        foreach (var header in headers)
                        {
                            table.Tick();
                            var value = new JsonTableValue()
                            {
                                HeaderId = header.Id,
                                Header = header,
                                Id = send_recent.SortingId + "_" + header.Id
                            };
                            switch (header.Id)
                            {
                                case "Recipient":
                                    table.UpdateMessage(details: "Setting email.");
                                    value.Value = value.RawData = send_recent.Recipient;
                                    break;
                                case "EmailType":
                                    table.UpdateMessage(details: "Setting email type.");
                                    value.Value = email.EmailType.GetStringValue();
                                    value.RawData = ((int)email.EmailType).ToString();
                                    break;
                                case "SendCount":
                                    table.UpdateMessage(details: "Setting send count.");
                                    value.Value = value.RawData = sendGroup.Count().ToString();
                                    break;
                                case "EmailName":
                                    table.UpdateMessage(details: "Setting email name.");
                                    value.Value = value.RawData = email.Name;
                                    break;
                                case "DeliveryEvent":
                                    table.UpdateMessage(details: "Setting latest event.");
                                    value.Value = send_recent.TopEvent.Event;
                                    value.RawData = send_recent.TopEvent.Event;
                                    break;
                                case "FirstName":
                                    table.UpdateMessage(details: "Setting first name.");
                                    value.Value = value.RawData = t_firstnamevalue;
                                    value.SubScript = fnNameRef;
                                    break;
                                case "LastName":
                                    table.UpdateMessage(details: "Setting last name.");
                                    value.Value = value.RawData = t_lastnamevalue;
                                    value.SubScript = lnNameRef;
                                    break;
                                case "RegistrationStatus":
                                    table.UpdateMessage(details: "Setting registration status.");
                                    value.Value = "Not Registered";
                                    value.RawData = "-1";
                                    if (send_recent.Registrant != null)
                                    {
                                        value.Value = send_recent.Registrant.Status.GetStringValue();
                                        value.RawData = ((int)send_recent.Registrant.Status).ToString();
                                        var regEmail = send_recent.Recipient;
                                        if (send_recent.Registrant != null)
                                            regEmail = send_recent.Registrant.Email;
                                        value.Link = "~/AdminRegister/Start/" + SortingId + "/" + regEmail;
                                    }
                                    break;
                                case "DeliveryDetails":
                                    table.UpdateMessage(details: "Setting delivery details.");
                                    value.Value = value.RawData = send_recent.TopEvent.Details ?? "";
                                    break;
                                case "DeliveryNotes":
                                    table.UpdateMessage(details: "Setting delivery notes.");
                                    value.Value = value.RawData = send_recent.TopEvent.Notes ?? "";
                                    break;
                                case "DeliveryStatus":
                                    table.UpdateMessage(details: "Setting delivery status");
                                    value = send_recent.GetStatus(header);
                                    break;
                                default:
                                    table.UpdateMessage(details: "Setting form component " + header.Value + ".");
                                    if (send_recent.Registrant != null)
                                        value = send_recent.Registrant.GetJsonTableValue(header);
                                    else
                                    {
                                        // No registrant, we need to see if the fields are mapped to a contact.
                                        // DO NOTHING NOW.
                                    }
                                    break;
                            }
                            row.Values.Add(value);
                        }

                    }
                }
                table.LastActivity = DateTime.Now;
                table.Info.Complete = true;
                table.UpdateMessage(message: "Post Processing", details: "Commiting changes to database.");
                context.SaveChanges();
                return table;
            }
        }
        #endregion

        /// <summary>
        /// Gets the headers to use for filtering a <code>JsonTableInformation</code> objects rows.
        /// </summary>
        /// <returns>The headers.</returns>
        public List<JsonFilterHeader> GetFormHeaders()
        {
            var headers = new List<JsonFilterHeader>();
            headers.Add(new JsonFilterHeader() { Label = "Confirmation", Id = "Confirmation" });
            headers.Add(new JsonFilterHeader() { Label = "Email", Id = "Email" });
            headers.Add(new JsonFilterHeader() { Label = "Status", Id = "Status", Type = "itemParent", PossibleValues = Enum.GetValues(typeof(RegistrationStatus)).Cast<RegistrationStatus>().Select(e => new JsonFilterValue() { Id = ((int)e).ToString(), Value = e.GetStringValue() }).ToList() });
            if (Pages.FirstOrDefault(p => p.Type == PageType.RSVP) != null && Pages.FirstOrDefault(p => p.Type == PageType.RSVP).Enabled)
                headers.Add(new JsonFilterHeader() { Label = "RSVP", Id = "RSVP", Type = "boolean", PossibleValues = new List<JsonFilterValue>() { new JsonFilterValue() { Id = "true", Value = RSVPAccept }, new JsonFilterValue() { Id = "false", Value = RSVPDecline } } });
            if (Pages.FirstOrDefault(p => p.Type == PageType.Audience && p.Enabled) != null && Pages.FirstOrDefault(p => p.Type == PageType.Audience && p.Enabled).Enabled && Audiences.Count > 0)
                headers.Add(new JsonFilterHeader() { Label = "Audience", Id = "Audience", Type = "itemParent", PossibleValues = Audiences.OrderBy(a => a.Order).Select(a => new JsonFilterValue() { Id = a.SortingId.ToString(), Value = a.Label }).ToList() });
            headers.Add(new JsonFilterHeader() { Label = "Date Registered", Id = "Date Registered", Type = "date" });
            headers.Add(new JsonFilterHeader() { Label = "Last Edit", Id = "Last Edit", Type = "date" });
            if (EmailList != null)
                headers.Add(new JsonFilterHeader() { Label = "In Contact List", Id = "In Contact List", Type = "boolean", PossibleValues = new List<JsonFilterValue>() { new JsonFilterValue() { Id = "true", Value = "Yes" }, new JsonFilterValue() { Id = "false", Value = "No" } } });
            if (!DisableShoppingCart)
            {
                headers.Add(new JsonFilterHeader() { Label = "Promotions", Id = "Promotions", Type = "multipleSelection", PossibleValues = PromotionalCodes.OrderBy(c => c.Code).Select(c => new JsonFilterValue() { Id = c.SortingId.ToString(), Value = c.Code }).ToList() });
                if (Tax.HasValue)
                    headers.Add(new JsonFilterHeader() { Label = "Tax", Id = "Tax", Type = "number" });
                headers.Add(new JsonFilterHeader() { Label = "Payment Method", Id = "Payment Method", Type = "itemParent", PossibleValues = new List<JsonFilterValue>() { new JsonFilterValue() { Id = "None Recoreded", Value = "None Recorded" }, new JsonFilterValue() { Id = "Bill Me", Value = "Bill Me" }, new JsonFilterValue() { Id = "Credit Card", Value = "Credit Card" } } });
                if (MerchantAccount != null)
                    headers.Add(new JsonFilterHeader() { Label = "Credit Last Four", Id = "Credit Last Four" });
                headers.Add(new JsonFilterHeader() { Label = "Fees", Type = "number", Id = "Fees" });
                headers.Add(new JsonFilterHeader() { Label = "Adjustments", Type = "number", Id = "Adjustments" });
                headers.Add(new JsonFilterHeader() { Label = "Transactions", Type = "number", Id = "Transactions" });
                headers.Add(new JsonFilterHeader() { Label = "Balance", Type = "number", Id = "Balance" });
            }
            headers.Add(new JsonFilterHeader() { Id = "Modification Token", Label = "Modification Token", });
            IEnumerable<IComponent> allComponents;
            allComponents = GetComponents();
            foreach (var component in allComponents)
            {
                if (component is FreeText)
                    continue;
                var label = component.Name;
                if (component is IComponentItemParent)
                {
                    var values = new List<JsonFilterValue>() { new JsonFilterValue() { Id = "", Value = "No Selection" } };
                    values.AddRange((component as IComponentItemParent).Children.Select(i => new JsonFilterValue() { Id = i.SortingId.ToString(), Value = i.LabelText }).ToList());
                    headers.Add(new JsonFilterHeader() { Id = component.SortingId.ToString(), Label = label, Type = (component is IComponentMultipleSelection ? "multipleSelection" : "itemParent"), PossibleValues = values });
                }
                else
                {
                    var type = "string";
                    if (component is Input)
                    {
                        if (((Input)component).Type == Domain.Entities.Components.InputType.Date || ((Input)component).Type == Domain.Entities.Components.InputType.DateTime || ((Input)component).Type == Domain.Entities.Components.InputType.Time)
                            type = "date";
                    }
                    headers.Add(new JsonFilterHeader() { Id = component.SortingId.ToString(), Label = label, Type = type });
                }
            }
            return headers;
        }

        public List<JsonTableHeader> GetRegistrantHeaders()
        {
            using (var context = new EFDbContext())
            {
                var jHeaders = new List<JsonTableHeader>();
                var components = context.Components.Include(c => c.Variable).Where(c => !(c is FreeText || c is RadioItem || c is CheckboxItem || c is DropdownItem) && c.Panel.Page.FormKey == UId).OrderBy(c => c.Panel.Page.PageNumber).ThenBy(c => c.Panel.Order).ThenBy(c => c.Row).ThenBy(c => c.Order).ToList();
                var tableHeaders = new List<string>();
                tableHeaders.Add("Confirmation");
                tableHeaders.Add("Email");
                tableHeaders.Add("Status");
                if (Pages.FirstOrDefault(p => p.Type == PageType.RSVP && p.Enabled) != null)
                    tableHeaders.Add("RSVP");
                if (Audiences.Count > 0 && Pages.FirstOrDefault(p => p.Type == PageType.Audience && p.Enabled) != null)
                    tableHeaders.Add("Audience");
                tableHeaders.Add("Date Registered");
                tableHeaders.Add("Last Edit");
                tableHeaders.Add("Edited By");
                tableHeaders.Add("In Contact List");
                if (PromotionalCodes.Count > 0)
                    tableHeaders.Add("Promotions");
                if (!DisableShoppingCart)
                {
                    tableHeaders.Add("Balance");
                    tableHeaders.Add("Tax");
                    tableHeaders.Add("Fees");
                    tableHeaders.Add("Adjustments");
                    tableHeaders.Add("Transactions");
                    tableHeaders.Add("Payment Method");
                    if (MerchantAccount != null)
                        tableHeaders.Add("Credit Last Four");
                }
                tableHeaders.AddRange(components.Select(c => c.SortingId.ToString()));
                long hid;
                var componentIds = tableHeaders.Where(h => long.TryParse(h, out hid)).Select(h => long.Parse(h)).ToList();
                var headers = context.Components.Include(c => c.Variable).Where(c => componentIds.Contains(c.SortingId)).OrderBy(h => h.SortingId).ToList();
                foreach (var hId in tableHeaders)
                {
                    var type = "text";
                    if (long.TryParse(hId, out hid))
                    {
                        var header = headers.FirstOrDefault(h => h.SortingId == hid);
                        if (header != null)
                        {
                            if (header is Input)
                            {
                                #region input
                                switch ((header as Input).ValueType)
                                {
                                    case Components.ValueType.DateTime:
                                        type = "date";
                                        break;
                                    case Components.ValueType.Decimal:
                                    case Components.ValueType.Number:
                                        type = "number";
                                        break;
                                }
                                switch ((header as Input).Type)
                                {
                                    case Components.InputType.Date:
                                    case Components.InputType.DateTime:
                                    case Components.InputType.Time:
                                        type = "date";
                                        break;
                                }
                                #endregion
                            }
                            else if (header is IComponentMultipleSelection)
                            {
                                type = "multiple selection";
                            }
                            else if (header is IComponentItemParent)
                            {
                                type = "single selection";
                            }
                            jHeaders.Add(new JsonTableHeader() { Editable = true, Id = header.SortingId.ToString(), Value = (header.Variable != null ? header.Variable.Value : header.LabelText), Token = header.UId, Type = type });
                        }
                    }
                    else
                    {
                        var editable = false;
                        if (hId.ToLower().In("email", "audience", "rsvp"))
                            editable = true;
                        if (hId.ToLower().In("balance", "fees", "tax", "adjustments", "transactions"))
                            type = "number";
                        if (hId.ToLower().In("date registered", "last edit"))
                            type = "date";
                        if (hId.ToLower().In("rsvp", "in contact list", "audience", "status"))
                            type = "text-raw";
                        jHeaders.Add(new JsonTableHeader() { Id = hId, Value = hId, Token = Guid.Empty, Editable = editable, Type = type });
                    }
                }
                return jHeaders;
            }
        }

        /// <summary>
        /// Gets a list of invitation headers.
        /// </summary>
        /// <returns>The invitation headers.</returns>
        protected IEnumerable<JsonTableHeader> _GetEmailHeaders(bool hide = false)
        {
            var headers = new List<JsonTableHeader>();
            var possibleStatus = Enum.GetValues(typeof(RegistrationStatus)).Cast<RegistrationStatus>().Select(e => new JsonFilterValue() { Id = ((int)e).ToString(), Value = e.GetStringValue() }).ToList();
            possibleStatus.Add(new JsonFilterValue() { Id = "-1", Value = "Not Registered" });
            headers.Add(new JsonTableHeader() { Value = "Registration Status", Id = "RegistrationStatus", HideByDefault = hide, Type = "single selection", PossibleValues = possibleStatus });
            headers.Add(new JsonTableHeader() { Value = "Last Name", Id = "LastName", HideByDefault = hide });
            headers.Add(new JsonTableHeader() { Value = "First Name", Id = "FirstName", HideByDefault = hide });
            headers.Add(new JsonTableHeader() { Value = "Recipient", Id = "Recipient", HideByDefault = hide });
            headers.Add(new JsonTableHeader() { Value = "Email Type", Id = "EmailType", HideByDefault = hide });
            headers.Add(new JsonTableHeader() { Value = "Email Name", Id = "EmailName", HideByDefault = hide });
            headers.Add(new JsonTableHeader() { Value = "Delivery Status", Id = "DeliveryStatus", HideByDefault = hide, Type = "single selection", PossibleValues = new List<JsonFilterValue>() { new JsonFilterValue() { Id = "0", Value = "Not Delivered" }, new JsonFilterValue() { Id = "1", Value = "Sending" }, new JsonFilterValue() { Id = "2", Value = "Delivered" } } });
            headers.Add(new JsonTableHeader() { Value = "Delivery Details", Id = "DeliveryDetails", HideByDefault = hide });
            headers.Add(new JsonTableHeader() { Value = "Delivery Event", Id = "DeliveryEvent", HideByDefault = hide, Type = "single selection",
                PossibleValues = new List<JsonFilterValue>() {
                    new JsonFilterValue() { Id = "Attempting to Send", Value = "Attempting to Send" },
                    new JsonFilterValue() { Id = "Sending", Value = "Sending" },
                    new JsonFilterValue() { Id = "Delivered", Value = "Delivered" },
                    new JsonFilterValue() { Id = "Opened", Value = "Opened" },
                    new JsonFilterValue() { Id = "Temporary Bounce", Value = "Temporary Bounce" },
                    new JsonFilterValue() { Id = "Permanent Bounce", Value = "Permanent Bounce" },
                    new JsonFilterValue() { Id = "Spam Report", Value = "Spam Report" },
                    new JsonFilterValue() { Id = "Clicked", Value = "Clicked" },
                    new JsonFilterValue() { Id = "Transferring", Value = "Transferring" }
                }
            });
            headers.Add(new JsonTableHeader() { Value = "Delivery Notes", Id = "DeliveryNotes", HideByDefault = hide });
            headers.Add(new JsonTableHeader() { Value = "Send Count", Id = "SendCount", Type = "number", HideByDefault = hide });
            return headers;
        }

        /// <summary>
        /// Gets a list of form headers.
        /// </summary>
        /// <returns>The form headers to use.</returns>
        protected IEnumerable<JsonTableHeader> _GetFormHeaders(bool hide = false)
        {
            var headers = new List<JsonTableHeader>();
            using (var context = new EFDbContext())
            {
                // First we get the components for the form from the database.
                var components = context.Components.Include(c => c.Variable).Where(c => !(c is FreeText || c is RadioItem || c is CheckboxItem || c is DropdownItem) && c.Panel.Page.FormKey == UId).OrderBy(c => c.Panel.Page.PageNumber).ThenBy(c => c.Panel.Order).ThenBy(c => c.Row).ThenBy(c => c.Order).ToList();
                // Here is the container to hold the ids of the headers.
                var header_ids = new List<string>();
                // Now we populate the ids with the non component items.
                header_ids.Add("Confirmation");
                header_ids.Add("Email");
                header_ids.Add("Status");
                if (Pages.FirstOrDefault(p => p.Type == PageType.RSVP && p.Enabled) != null)
                    // Only if the form has rsvp.
                    header_ids.Add("RSVP");
                if (Audiences.Count > 0 && Pages.FirstOrDefault(p => p.Type == PageType.Audience && p.Enabled) != null)
                    // Only if the form has audiences.
                    header_ids.Add("Audience");
                header_ids.Add("DateRegistered");
                header_ids.Add("LastEdit");
                header_ids.Add("EditedBy");
                header_ids.Add("InContactList");
                if (PromotionalCodes.Count > 0)
                    // Only if the form has promotion codes.
                    header_ids.Add("Promotions");
                if (!DisableShoppingCart)
                {
                    // These headers are only if there is a shopping cart.
                    header_ids.Add("Balance");
                    header_ids.Add("Tax");
                    header_ids.Add("Fees");
                    header_ids.Add("Adjustments");
                    header_ids.Add("Transactions");
                    header_ids.Add("PaymentMethod");
                    if (MerchantAccount != null)
                        header_ids.Add("CreditLastFour");
                }
                // Now we add all the components to the ids.
                header_ids.AddRange(components.Select(c => c.SortingId.ToString()));
                foreach (var hId in header_ids)
                {
                    var jHeader = new JsonTableHeader()
                    {
                        HideByDefault = hide
                    };
                    // First we see if this is a component id.
                    long hid;
                    Guid uid;
                    IComponent header = null;
                    if (long.TryParse(hId, out hid))
                    {
                        header = components.FirstOrDefault(h => h.SortingId == hid);
                    }
                    else if (Guid.TryParse(hId, out uid))
                    {
                        header = components.FirstOrDefault(h => h.UId == uid);
                    }
                    
                    if (header != null)
                    {
                        jHeader.Id = header.SortingId.ToString();
                        jHeader.Value = header.Variable != null ? header.Variable.Value : header.LabelText;
                        jHeader.Token = header.UId;
                        jHeader.Editable = true;
                        // It was a component, so we build the header.
                        if (header is Input)
                        {
                            #region input
                            switch ((header as Input).ValueType)
                            {
                                case Components.ValueType.DateTime:
                                    jHeader.Type = "datetime";
                                    break;
                                case Components.ValueType.Decimal:
                                case Components.ValueType.Number:
                                    jHeader.Type = "number";
                                    break;
                            }
                            switch ((header as Input).Type)
                            {
                                case Components.InputType.Date:
                                    jHeader.Type = "date";
                                    break;
                                case Components.InputType.DateTime:
                                    jHeader.Type = "datetime";
                                    break;
                                case Components.InputType.Time:
                                    jHeader.Type = "time";
                                    break;
                            }
                            #endregion
                        }
                        else if (header is IComponentItemParent)
                        {
                            jHeader.Type = "single selection";
                            var values = new List<JsonFilterValue>() { new JsonFilterValue() { Id = "", Value = "No Selection" } };
                            values.AddRange((header as IComponentItemParent).Children.Select(i => new JsonFilterValue() { Id = i.SortingId.ToString(), Value = i.LabelText }).ToList());
                            jHeader.PossibleValues = values;
                            if (header is IComponentMultipleSelection)
                                jHeader.Type = "multiple selection";
                        }
                    }
                    else
                    {
                        jHeader.Id = jHeader.Value = hId;
                        switch (hId)
                        {
                            case "Email":
                                jHeader.Editable = true;
                                break;
                            case "Audience":
                                jHeader.Editable = true;
                                jHeader.PossibleValues = Audiences.OrderBy(a => a.Order).Select(a => new JsonFilterValue() { Id = a.SortingId.ToString(), Value = a.Label }).ToList();
                                jHeader.Type = "single selection";
                                break;
                            case "RSVP":
                                jHeader.Editable = true;
                                jHeader.PossibleValues = new List<JsonFilterValue>() { new JsonFilterValue() { Id = "true", Value = RSVPAccept }, new JsonFilterValue() { Id = "false", Value = RSVPDecline } };
                                jHeader.Type = "boolean";
                                break;
                            case "Balance":
                            case "Fees":
                            case "Adjustments":
                            case "Transaction":
                                jHeader.Type = "number";
                                break;
                            case "Status":
                                jHeader.PossibleValues = Enum.GetValues(typeof(RegistrationStatus)).Cast<RegistrationStatus>().Select(e => new JsonFilterValue() { Id = ((int)e).ToString(), Value = e.GetStringValue() }).ToList();
                                jHeader.Type = "single selection";
                                break;
                            case "InContactList":
                                jHeader.PossibleValues = new List<JsonFilterValue>() { new JsonFilterValue() { Id = "true", Value = "Yes" }, new JsonFilterValue() { Id = "false", Value = "No" } };
                                jHeader.Type = "boolean";
                                jHeader.Value = "In Contact List";
                                break;
                            case "DateRegistered":
                                jHeader.Type = "datetime";
                                jHeader.Value = "Date Registered";
                                break;
                            case "LastEdit":
                                jHeader.Type = "datetime";
                                jHeader.Value = "Last Edit";
                                break;
                        }
                    }
                    headers.Add(jHeader);
                }
            }
            return headers;
        }
        #endregion

        #region Non memory leak table data

        public async Task<TableData> GetRegistrationJson(Progress progress)
        {
            try
            {
                using (var registrantContext = new EFDbContext())
                using (var headerContext = new EFDbContext())
                {
                    var regType = RegistrationType.Live;
                    if (Status == FormStatus.Developement)
                        regType = RegistrationType.Test;
                    var stati = new List<RegistrationStatus>();
                    var regCount = registrantContext.Registrants.Where(r => r.Type == regType && r.FormKey == UId && !stati.Contains(r.Status)).Count();
                    var headerCount = headerContext.Components.Where(c => !(c is FreeText || c is RadioItem || c is CheckboxItem || c is DropdownItem) && c.Panel.Page.FormKey == UId).Count();
                    progress.UpdateMessage("Querying Database", "Retrieving registrants from the database.");
                    var registrantsFiltering = registrantContext.Registrants.Where(r => r.Type == regType && r.FormKey == UId && !stati.Contains(r.Status));
                    var registrants = registrantsFiltering.ToList();
                    progress.UpdateMessage("Querying Database", "Retrieving headers from the database.");
                    var components = headerContext.Components.Include(c => c.Variable).Where(c => !(c is FreeText || c is RadioItem || c is CheckboxItem || c is DropdownItem) && c.Panel.Page.FormKey == UId).OrderBy(c => c.Panel.Page.PageNumber).ThenBy(c => c.Panel.Order).ThenBy(c => c.Row).ThenBy(c => c.Order).ToList();

                    progress.UpdateMessage("Analyzing Data", "Preparing headers from the database.");

                    // We grab all the headers.
                    var tableHeaders = new List<string>();
                    tableHeaders.Add("Confirmation");
                    tableHeaders.Add("Email");
                    tableHeaders.Add("Status");
                    if (Pages.FirstOrDefault(p => p.Type == PageType.RSVP && p.Enabled) != null)
                        tableHeaders.Add("RSVP");
                    if (Audiences.Count > 0 && Pages.FirstOrDefault(p => p.Type == PageType.Audience && p.Enabled) != null)
                        tableHeaders.Add("Audience");
                    tableHeaders.Add("Date Registered");
                    tableHeaders.Add("Last Edit");
                    tableHeaders.Add("Edited By");
                    tableHeaders.Add("In Contact List");
                    if (PromotionalCodes.Count > 0)
                        tableHeaders.Add("Promotions");
                    if (!DisableShoppingCart)
                    {
                        tableHeaders.Add("Balance");
                        tableHeaders.Add("Tax");
                        tableHeaders.Add("Fees");
                        tableHeaders.Add("Adjustments");
                        tableHeaders.Add("Transactions");
                        tableHeaders.Add("Payment Method");
                        if (MerchantAccount != null)
                            tableHeaders.Add("Credit Last Four");
                    }
                    tableHeaders.AddRange(components.Select(c => c.SortingId.ToString()));
                    long hid;
                    var componentIds = tableHeaders.Where(h => long.TryParse(h, out hid)).Select(h => long.Parse(h)).ToList();
                    var headers = headerContext.Components.Include(c => c.Variable).Where(c => componentIds.Contains(c.SortingId)).OrderBy(h => h.SortingId).ToList();
                    var headersOrdered = new List<JsonTableHeader>();
                    progress.UpdateMessage(details: "Ordering headers for report");
                    progress.SetTickFraction(headerCount);
                    foreach (var hId in tableHeaders)
                    {
                        var possibleValues = new List<JsonFilterValue>();
                        var type = "text";
                        if (long.TryParse(hId, out hid))
                        {
                            var header = headers.FirstOrDefault(h => h.SortingId == hid);
                            if (header != null)
                            {
                                progress.Tick(details: header.LabelText + " being analyzed.");
                                if (header is Input)
                                {
                                    #region input
                                    switch ((header as Input).ValueType)
                                    {
                                        case Components.ValueType.DateTime:
                                            type = "datetime";
                                            break;
                                        case Components.ValueType.Decimal:
                                        case Components.ValueType.Number:
                                            type = "number";
                                            break;
                                    }
                                    switch ((header as Input).Type)
                                    {
                                        case Components.InputType.Date:
                                            type = "date";
                                            break;
                                        case Components.InputType.DateTime:
                                            type = "datetime";
                                            break;
                                        case Components.InputType.Time:
                                            type = "time";
                                            break;
                                    }
                                    #endregion
                                }
                                else if (header is IComponentItemParent)
                                {
                                    type = "single selection";
                                    possibleValues = new List<JsonFilterValue>() { new JsonFilterValue() { Id = "", Value = "No Selection" } };
                                    possibleValues.AddRange((header as IComponentItemParent).Children.Select(i => new JsonFilterValue() { Id = i.SortingId.ToString(), Value = i.LabelText }).ToList());
                                    if (header is IComponentMultipleSelection)
                                        type = "multiple selection";
                                }
                                headersOrdered.Add(new JsonTableHeader() { Editable = true, Id = header.SortingId.ToString(), Value = (header.Variable != null ? header.Variable.Value : header.LabelText), Token = header.UId, Type = type, PossibleValues = possibleValues });
                            }
                        }
                        else
                        {
                            progress.Tick(details: hId + " being analyzed.");
                            var editable = false;
                            if (hId.ToLower().In("email", "audience", "rsvp"))
                                editable = true;
                            if (hId.ToLower().In("balance", "fees", "tax", "adjustments", "transactions"))
                                type = "number";
                            if (hId.ToLower().In("date registered", "last edit"))
                                type = "date";
                            if (hId.ToLower().In("rsvp", "in contact list", "audience", "status"))
                                type = "single selection";
                            switch (hId.ToLower())
                            {
                                case "rsvp":
                                    possibleValues.Clear();
                                    possibleValues.Add(new JsonFilterValue() { Id = "true", Value = RSVPAccept });
                                    possibleValues.Add(new JsonFilterValue() { Id = "false", Value = RSVPDecline });
                                    break;
                                case "status":
                                    possibleValues.Clear();
                                    possibleValues.AddRange(Enum.GetValues(typeof(RegistrationStatus)).Cast<RegistrationStatus>().Select(e => new JsonFilterValue() { Id = ((int)e).ToString(), Value = e.GetStringValue() }).ToList());
                                    break;
                                case "audience":
                                    possibleValues.Clear();
                                    possibleValues.AddRange(Audiences.OrderBy(a => a.Order).Select(a => new JsonFilterValue() { Id = a.SortingId.ToString(), Value = a.Label }).ToList());
                                    break;
                                case "promotions":
                                    possibleValues.Clear();
                                    possibleValues.AddRange(PromotionalCodes.OrderBy(c => c.Code).Select(c => new JsonFilterValue() { Id = c.SortingId.ToString(), Value = c.Code }).ToList());
                                    break;
                                case "payment method":
                                    possibleValues.Clear();
                                    possibleValues.Add(new JsonFilterValue() { Id = "None Recoreded", Value = "None Recorded" });
                                    possibleValues.Add(new JsonFilterValue() { Id = "Bill Me", Value = "Bill Me" });
                                    possibleValues.Add(new JsonFilterValue() { Id = "Credit Card", Value = "Credit Card" });
                                    break;
                                default:
                                    possibleValues.Clear();
                                    break;
                            }
                            headersOrdered.Add(new JsonTableHeader() { Id = hId, Value = hId, Token = Guid.Empty, Editable = editable, Type = type, PossibleValues = possibleValues });
                        }
                    }
                    var count = registrants.Count * headersOrdered.Count;
                    progress.SetTickFraction(count);
                    progress.UpdateMessage(message: "Analyzing Registrants", details: "");
                    // Now we work with registrants

                    // Now we order the registrants.
                    var rows = new List<JsonTableRow>();
                    var curCount = 0;
                    foreach (var registrant in registrants)
                    {
                        registrant.UpdateAccounts();
                        var row = new JsonTableRow() { Id = registrant.SortingId, Token = registrant.UId, ModificationToken = registrant.ModificationToken, DateModified = registrant.DateModified };
                        var values = new List<JsonTableValue>();
                        foreach (var header in headersOrdered)
                        {
                            progress.Tick(details: "Set " + registrant.Confirmation + " value on " + header.Label + ".");
                            curCount++;
                            values.Add(registrant.GetJsonTableValue(header));
                        }
                        row.Values = values;
                        rows.Add(row);
                    }
                    progress.Update(1F, "Finishing Up", "Cleaning up and sending data.");
                    var tableData = new TableData()
                    {
                        Rows = rows,
                        Headers = headersOrdered
                    };
                    return tableData;
                }
            }
            catch (Exception e)
            {
                progress.Fail(e);
                using (var context = new EFDbContext())
                {
                    var logger = new Logging.Logger();
                    logger.Thread = "FormReportLoad";
                    logger.LoggingMethod = "Form.GetRegistrationJson(Progress progress)";
                    logger.Error(e);
                }
                return null;
            }
        }


        #endregion
    }

    public enum FormAccessType
    {
        Open = 0,
        InvitationOnly = 1,
        CustomLogIn =2
    }

    public enum BillingOptions
    {
        None = -1,
        CCandBM = 0,
        CC = 1,
        BM = 2
    }
}
