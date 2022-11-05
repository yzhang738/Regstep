using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities.MerchantAccount;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Entities
{
    public class Registrant
        : ISecureHolder, IFilterable, IApiLogin, INode, IRequirePermissions
    {
        private ShoppingCart pr_shoppingCart = null;
        private string _invoiceNumber;
        private string pr_firstName;
        private string pr_lastName;
        private string pr_title;

        [ClearKeyOnDelete("RegistrantKey")]
        public virtual List<TransactionRequest> TransactionRequests { get; set; }
        [ClearKeyOnDelete("RegistrantKey")]
        [CascadeDelete]
        public virtual List<RegistrantData> Data { get; set; }
        [CascadeDelete]
        public virtual List<OldRegistrant> OldRegistrations { get; set; }
        [CascadeDelete]
        public virtual List<PromotionCodeEntry> PromotionalCodes { get; set; }
        [ClearKeyOnDelete("RegistrantKey")]
        public virtual List<EmailSend> EmailSends { get; set; }
        [CascadeDelete]
        public virtual List<Adjustment> Adjustments { get; set; }
        [CascadeDelete]
        public virtual List<RegistrantNote> Notes { get; set; }
        [CascadeDelete]
        public virtual List<Seater> Seatings { get; set; }
        [ForeignKey("ContactKey")]
        public virtual Contact Contact { get; set; }
        public Guid? ContactKey { get; set; }
        public bool Attended { get; set; }

        [NotMapped]
        public List<Attendance> Attendance { get; set; }

        public string RawAttendace
        {
            get
            {
                return JsonConvert.SerializeObject(Attendance);
            }
            set
            {
                Attendance = JsonConvert.DeserializeObject<List<Attendance>>(value ?? "[]");
            }
        }

        [NotMapped]
        public IEnumerable<IPersonData> IData
        {
            get
            {
                return Data.AsEnumerable<IPersonData>();
            }
        }

        [NotMapped]
        public IPersonHolder Holder
        {
            get
            {
                return Form;
            }
            set
            {
                if (value is Form)
                    Form = value as Form;
            }
        }
        [NotMapped]
        public Guid HolderKey
        {
            get
            {
                return FormKey;
            }
        }

        public decimal Fees { get; set; }
        public decimal Transactions { get; set; }
        public decimal Adjustings { get; set; }
        public decimal Taxes { get; set; }

        [NotMapped]
        public decimal TotalOwed
        {
            get
            {
                return Math.Round(Fees + Taxes + Adjustings - Transactions, 2);
            }
        }

        [NotMapped]
        public bool PaysTaxes
        {
            get
            {
                if (Form.Tax.HasValue && Data.Where(d => d.Component.Variable.Value == "__NoTax" && d.Value != null && d.Value.ToLower() == "true").Count() == 0)
                    return true;
                return false;
            }
        }

        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        [Index("IX_FormKey_Email_Type", IsUnique = true, Order = 1)]
        public Guid FormKey { get; set; }

        [ForeignKey("AudienceKey")]
        public virtual Audience Audience { get; set; }
        public Guid? AudienceKey { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(250)]
        [Index("IX_FormKey_Email_Type", IsUnique = true, Order = 2)]
        public string Email { get; set; }

        [NotMapped]
        public string Confirmation
        {
            get
            {
                var playValue = 0L;
                if (SortingId != null)
                    playValue = SortingId;
                return playValue.ToString("X");
            }
        }

        public string PayingAgentNumber { get; set; }
        public string PayingAgentName { get; set; }
        public string InvoiceNumber
        {
            get
            {
                if (String.IsNullOrEmpty(_invoiceNumber))
                    return Confirmation;
                return _invoiceNumber;
            }
            set
            {
                _invoiceNumber = value;
            }
        }

        public bool RSVP { get; set; }

        public RegistrationStatus Status { get; set; }
        public DateTimeOffset StatusDate { get; set; }

        [Index("IX_FormKey_Email_Type", IsUnique = true, Order = 3)]
        public RegistrationType Type { get; set; }

        public string this[Guid variable]
        {
            get
            {
                return this[variable.ToString()];
            }
        }

        public string this[string variable]
        {
            get
            {
                Guid uid;
                if (Guid.TryParse(variable, out uid))
                {
                    var t_dp = Data.FirstOrDefault(d => d.Component.UId == uid);
                    if (t_dp == null)
                        return String.Empty;
                    if (t_dp.Component is CheckboxGroup && t_dp.Empty())
                        return "[]";
                    if (t_dp.Component is Input && (t_dp.Component as Input).Type.In(InputType.Date, InputType.DateTime))
                    {
                        DateTime datetime;
                        if (DateTime.TryParse(t_dp.Value, out datetime))
                        {
                            return datetime.ToString("M/d/yyyy h:mm tt");
                        }
                        return String.Empty;
                    }
                    return t_dp.Value;
                }
                else
                {
                    if (variable.StartsWith("[") && variable.EndsWith("]"))
                        variable = variable.Substring(1, variable.Length - 2);
                    var data = Data.FirstOrDefault(d => d.Component.Variable.Value == variable);
                    if (data == null)
                        return String.Empty;
                    return data.Value;
                }
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

        public string ApiToken { get; set; }
        public DateTimeOffset? ApiTokenExpiration { get; set; }

        #region Not Mapped
        [NotMapped]
        public string FullName
        {
            get
            {
                var ret_string = FirstName + " " + LastName;
                if (String.IsNullOrWhiteSpace(ret_string))
                    return Email;
                return ret_string;
            }
        }
        [NotMapped]
        public string LastNameFirst
        {
            get
            {
                var ret_string = LastName + ", " + FirstName;
                if (String.IsNullOrWhiteSpace(ret_string.Replace(',', ' ')))
                    return Email;
                return ret_string;
            }
        }
        [NotMapped]
        public string FirstName
        {
            get
            {
                if (pr_firstName != null)
                    return pr_firstName;
                string t_name = null;
                if (pr_firstName == null)
                    pr_firstName = SearchPrettyValue("FirstName");
                if (pr_firstName == null)
                    t_name = SearchPrettyValue("Name");
                if (pr_firstName == null && t_name == null)
                    pr_firstName = "";
                if (t_name != null && String.IsNullOrWhiteSpace(pr_firstName))
                {
                    var t_names = t_name.Split(' ');
                    if (t_names.Length > 0)
                        pr_firstName = t_names[0].Trim();
                }
                return pr_firstName;
            }
        }
        [NotMapped]
        public string LastName
        {
            get
            {
                if (pr_lastName != null)
                    return pr_lastName;
                string t_name = null;
                if (pr_lastName == null)
                    pr_lastName = SearchPrettyValue("LastName");
                if (pr_lastName == null)
                    t_name = SearchPrettyValue("Name");
                if (pr_lastName == null && t_name == null)
                    pr_lastName = "";
                if (t_name != null && String.IsNullOrWhiteSpace(pr_firstName))
                {
                    var t_names = t_name.Split(' ');
                    if (t_names.Length > 0)
                        pr_lastName = t_names[t_names.Length - 1].Trim();
                }
                return pr_lastName;
            }
        }
        #endregion

        public Registrant()
        {
            RSVP = true;
            Email = "";
            Data = new List<RegistrantData>();
            TransactionRequests = new List<TransactionRequest>();
            OldRegistrations = new List<OldRegistrant>();
            PromotionalCodes = new List<PromotionCodeEntry>();
            EmailSends = new List<EmailSend>();
            Adjustments = new List<Adjustment>();
            Notes = new List<RegistrantNote>();
            PayingAgentNumber = PayingAgentName = "";
            StatusDate = DateTimeOffset.UtcNow;
            ApiTokenExpiration = null;
            ApiToken = null;
            Attended = false;
            DateCreated = DateModified = DateTimeOffset.Now;
            Attendance = new List<Attendance>();
            _invoiceNumber = null;
        }

        /// <summary>
        /// Creates a new Registrant.
        /// </summary>
        /// <param name="repository">The repository that is storing the data.</param>
        /// <param name="form">The form that the registrant is attached to.</param>
        /// <param name="email">The email that the registrant is using.</param>
        /// <param name="owner">The person who owns the registrant. default: null</param>
        /// <param name="group">The group the registrant belongs to. defualt: null</param>
        /// <param name="permission">The permission for the registrant. default: 750</param>
        /// <param name="rsvp">The rsvp value for the registrant. default: true</param>
        /// <param name="status">The status for the registrant. default: Incomplete</param>
        /// <param name="type">The type of registrant. default: live</param>
        /// <returns>The newly created registrant.</returns>
        public static Registrant New(FormsRepository repository, Form form, string email, Guid? owner = null, Guid? group = null, string permission = "750", bool rsvp = false, RegistrationStatus status = RegistrationStatus.Incomplete, RegistrationType type = RegistrationType.Live)
        {
            var registrant = new Registrant()
            {
                Email = email,
                Form = form,
                FormKey = form.UId,
                Status = status,
                Type = type,
                UId = Guid.NewGuid()
            };
            form.Registrants.Add(registrant);
            registrant.DateCreated = registrant.DateModified = DateTimeOffset.Now;
            return registrant;
        }

        public Form GetForm()
        {
            return Form;
        }

        public INode GetNode()
        {
            return Form as INode;
        }

        /// <summary>
        /// Gets the data as an IPersonData.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>An IPersonData with the data. Null if no data was found.</returns>
        public SetDataResult SetData(string key, string value, bool ignoreValidation = false, bool ignoreRequired = false, bool ignoreCapacity = false, bool resetValueOnError = true)
        {
            using (var repository = new FormsRepository())
            {
                var iLog = new Logging.Logger();
                iLog.LoggingMethod = this.GetType().ToString();
                var result = new SetDataResult() { Id = key, Value = value, PrettyValue = value };
                switch (key.ToLower())
                {
                    case "email":
                        var regType = RegistrationType.Live;
                        if (Form.Status == FormStatus.Developement)
                            regType = RegistrationType.Test;
                        value = value.ToLower();
                        var sameEmail = Form.Registrants.Where(r => r.Type == regType && r.Email.ToLower() == value && r.SortingId != SortingId).ToList();
                        if (sameEmail.Count > 0)
                            return new SetDataResult() { Errors = new List<SetDataError>() { new SetDataError("email", "Email already in use.") } };
                        Email = value;
                        return result;
                    case "audience":
                        Guid t_audKey;
                        long t_audId;
                        if (Guid.TryParse(value, out t_audKey))
                            Audience = Form.Audiences.FirstOrDefault(a => a.UId == t_audKey);
                        else if (long.TryParse(value, out t_audId))
                            Audience = Form.Audiences.FirstOrDefault(a => a.SortingId == t_audId);
                        if (Audience == null)
                            result.Errors.Add(new SetDataError("audience", "Invalid audience."));
                        else
                            result.PrettyValue = Audience.Label;
                        return result;
                    case "rsvp":
                        bool t_rsvp;
                        if (Boolean.TryParse(value, out t_rsvp))
                        {
                            RSVP = t_rsvp;
                            result.PrettyValue = RSVP ? Form.RSVPAccept : Form.RSVPDecline;
                            return result;
                        }
                        result.Errors.Add(new SetDataError("rsvp", "Invalid boolean value."));
                        return result;
                }
                Guid t_key;
                long t_id;
                IComponent component = null;
                RegistrantData dp = null;
                var components = Form.GetUnorderedComponents();
                if (Guid.TryParse(key, out t_key))
                {
                    dp = Data.FirstOrDefault(d => d.Component != null && d.Component.UId == t_key);
                    if (dp == null)
                        component = components.FirstOrDefault(c => c.UId == t_key);
                    else
                        component = dp.Component;
                }
                else if (long.TryParse(key, out t_id))
                {
                    dp = Data.FirstOrDefault(d => d.Component != null && d.Component.SortingId == t_id);
                    if (dp == null)
                        component = components.FirstOrDefault(c => c.SortingId == t_id);
                    else
                        component = dp.Component;
                }
                else
                {
                    component = components.FirstOrDefault(c => c.Variable != null && c.Variable.Value.ToLower() == key.ToLower());
                    if (component != null)
                        dp = Data.FirstOrDefault(d => d.Component != null && d.Component.SortingId == component.SortingId);
                }
                if (component != null)
                {
                    if (dp == null)
                    {
                        dp = new RegistrantData() { Component = component as Component, VariableUId = component.UId, Registrant = this, RegistrantKey = this.UId };
                        Data.Add(dp);
                    }
                    result.Data = dp;
                    // Let's hold the old value in case we need to roll back.
                    var o_value = dp.Value;
                    dp.Value = value;
                    if (String.IsNullOrEmpty(value) || value == "__skipped")
                        dp.Value = null;
                    if (value == "__skipped")
                        return result;
                    if (!ignoreRequired)
                    {
                        if (dp.Component.Required && dp.Empty())
                            result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "This is a required field."));
                    }
                    if (!ignoreValidation)
                    {
                        // Lets do some validation.
                        if (dp.Component is Input)
                        {
                            if (dp.Empty())
                                return result;
                            #region Input
                            var input = dp.Component as Input;
                            if (input.Type == Domain.Entities.Components.InputType.Default || input.Type == Domain.Entities.Components.InputType.Multiline)
                            {
                                if (input.ValueType == Domain.Entities.Components.ValueType.USPhone)
                                {
                                    var t_phoneValue = Regex.Replace(dp.Value, @"\D+", "");
                                    if (t_phoneValue.Length < 10)
                                        result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "Your phone number must be 10 digits or more."));
                                    if (t_phoneValue.Length > 10 && !(dp.Value.ToLower().Contains("x") || dp.Value.ToLower().Contains("ext")))
                                        result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "Youre phone number looks like it has an extension but you did not designate it as such. Use x or ext before the extension."));
                                    dp.Value = t_phoneValue;
                                    if (input.Formatting == Domain.Entities.Components.Formatting.GenericPhone)
                                        dp.Value = Regex.Replace(dp.Value, @"^(\d{3})(\d{3})(\d{4})(\d*)", "($1) $2-$3 x $4");
                                    else if (input.Formatting == Domain.Entities.Components.Formatting.DotPhone)
                                        dp.Value = Regex.Replace(dp.Value, @"^(\d{3})(\d{3})(\d{4})(\d*)", "$1.$2.$3 x $4");
                                    else if (input.Formatting == Domain.Entities.Components.Formatting.SpacePhone)
                                        dp.Value = Regex.Replace(dp.Value, @"^(\d{3})(\d{3})(\d{4}(\d*))", "$1 $2 $3 x $4");
                                    else if (input.Formatting == Components.Formatting.DashPhone)
                                        dp.Value = Regex.Replace(dp.Value, @"^(\d{3})(\d{3})(\d{4}(\d*))", "$1-$2-$3 x $4");
                                    dp.Value = dp.Value.Trim();
                                    if (dp.Value.EndsWith("x"))
                                        dp.Value = dp.Value.Substring(0, dp.Value.Length - 2);
                                }
                                switch (input.Formatting)
                                {
                                    case Domain.Entities.Components.Formatting.Caps:
                                        dp.Value = dp.Value.ToUpper();
                                        break;
                                    case Domain.Entities.Components.Formatting.Lowercase:
                                        dp.Value = dp.Value.ToLower();
                                        break;
                                    case Domain.Entities.Components.Formatting.SentenceCase:
                                        if (dp.Value.Length == 1)
                                            dp.Value = dp.Value.ToUpper();
                                        else if (dp.Value.Length > 1)
                                            dp.Value = char.ToUpper(dp.Value[0]) + dp.Value.Substring(1);
                                        break;
                                    case Domain.Entities.Components.Formatting.TitleCase:
                                        dp.Value = dp.Value.ToTitleCase();
                                        break;
                                    case Domain.Entities.Components.Formatting.None:
                                    default:
                                        break;
                                }
                            }
                            #region DateTime, Date, and Time
                            if (input.Type == Domain.Entities.Components.InputType.Date)
                            {
                                // We are manipulating dates without times now.
                                var date = DateTime.MinValue;
                                bool dateSupplied = DateTime.TryParse(dp.Value, out date);
                                if (dateSupplied)
                                {
                                    // The date was valid.  Now we check for contraints in case javascript is off.
                                    date = date.Date;
                                    if (input.MinDate.HasValue)
                                    {
                                        // There is a minimun date contraint.  We need to check it against the provided value and register an error if needed.
                                        if (date < input.MinDate.Value.Date)
                                            result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "Your date is below the minimum of " + input.MinDate.Value.Date.ToShortDateString()));
                                    }
                                    if (input.MaxDate.HasValue)
                                    {
                                        // There is a maximum date contraint.  We need to check it against the provided value and register an error if needed.
                                        if (date > input.MaxDate.Value.Date)
                                            result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "Your date is above the maximum of " + input.MaxDate.Value.Date.ToShortDateString()));
                                    }
                                    dp.Value = date.ToShortDateString();
                                }
                                else
                                {
                                    // A valid date was not entered.
                                    result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "You must enter a valid date."));
                                }
                            }
                            else if (input.Type == Domain.Entities.Components.InputType.Time)
                            {
                                // We are minpulating a time.
                                var time = TimeSpan.MinValue;
                                var timeSupplied = TimeSpan.TryParse(dp.Value, out time);
                                if (timeSupplied)
                                {
                                    // The time supplied is valid.  Now we check for contraints in case javascript is off.
                                    if (input.MinDate.HasValue)
                                    {
                                        // There is a minimun time contraint.  We need to check it against the provided value and register an error if needed.
                                        if (time < input.MinDate.Value.TimeOfDay)
                                            result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "Your time is below the minimum of " + input.MinDate.Value.Date.ToShortTimeString()));
                                    }
                                    if (input.MaxDate.HasValue)
                                    {
                                        // There is a maximum time contraint.  We need to check it against the provided value and register an error if needed.
                                        if (time > input.MaxDate.Value.TimeOfDay)
                                            result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "Your time is above the maximum of " + input.MaxDate.Value.Date.ToShortTimeString()));
                                    }
                                    dp.Value = time.ToString();
                                }
                                else
                                {
                                    // A valid time was not entered.  We need to register an error.
                                    result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "You must enter a valid time."));
                                }
                            }
                            else if (input.Type == Domain.Entities.Components.InputType.DateTime)
                            {
                                // We are manipulating date and times.
                                var date = DateTime.MinValue;
                                bool dateSupplied = DateTime.TryParse(dp.Value, out date);
                                if (dateSupplied)
                                {
                                    // The datetime group is valid.  Now we check for contraints in case javascript is off.
                                    if (input.MinDate.HasValue)
                                    {
                                        // There is a minimun datetime group contraint.  We need to check it against the provided value and register an error if needed.
                                        if (date < input.MinDate.Value)
                                            result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "Your date is below the minimum of " + input.MinDate.Value.ToString(Form.Culture)));
                                    }
                                    if (input.MaxDate.HasValue)
                                    {
                                        // There is a maximun datetime group contraint.  We need to check it against the provided value and register an error if needed.
                                        if (date > input.MaxDate.Value)
                                            result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "Your date is above the maximum of " + input.MaxDate.Value.ToString(Form.Culture)));
                                    }
                                    dp.Value = date.ToString();
                                }
                                else
                                {
                                    // The datetime group is invalid and we need to register an error.
                                    result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "You must enter a valid date."));
                                }
                            }
                            #endregion
                            #region Credit Card
                            else if (input.Type == Domain.Entities.Components.InputType.UniversalCreditCard)
                            {
                                // We are manipulating credit cards now.
                                // We need to roll back the dp value and use it as the credit card key.
                                dp.Value = o_value;

                                // We need to skip credit card if the value is already saved.
                                Guid cardId;
                                CreditCard card;
                                if (Guid.TryParse(dp.Value, out cardId))
                                {
                                    // The credit card is already in the database.
                                    if (value.Trim() != (repository.SecurePeek<CreditCard>(c => c.UId == cardId).FirstOrDefault() ?? ""))
                                    {
                                        // The field was changed
                                        if (CCHelper.ValidateCard(dp.Value))
                                            repository.UpdateSecure<CreditCard>(c => c.UId == cardId, "Number", value);
                                        else
                                            result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "Invalid credit card number."));
                                    }
                                }
                                else
                                {
                                    card = new CreditCard()
                                    {
                                        FormKey = FormKey,
                                        RegistrantKey = UId
                                    };
                                    if (CCHelper.ValidateCard(value))
                                    {
                                        card.Number = value;
                                        repository.Add(card);
                                        dp.Value = card.UId.ToString();
                                    }
                                    else
                                        result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "Invalid credit card number."));
                                }
                            }
                            #endregion
                            if (input.Length.HasValue && input.Length < dp.Value.Length)
                                result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "The maximum ammount of characters is " + input.Length.Value + "."));
                            #endregion
                        }
                        else if (dp.Component is IComponentMultipleSelection)
                        {
                            #region IComponentMultipleSelection
                            var seating = dp.Component.Seating;
                            // Now we are manipulating checkbox groups
                            // First lets see if any of the items are required.  If they are we will register an error;
                            var cbg = dp.Component as IComponentMultipleSelection;
                            List<IComponentItem> i_selections = new List<IComponentItem>();
                            var t_errors = ParseItemList(cbg.Children, dp.Value, out i_selections);
                            result.Errors.AddRange(t_errors);
                            var selections = i_selections.Select(i => i.UId);
                            var items = cbg.Children.ToList();
                            foreach (var item in items)
                            {
                                if (item.Required && !ignoreRequired && !selections.Contains(item.UId))
                                    result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "This is a required item."));
                            }
                            if (cbg.TimeExclusion)
                            {
                                // If time exclusion is set, we need to check it here in case javascript was turned off.
                                bool collision = false;
                                var t_items = new List<IComponentItem>();
                                items.ForEach(i => { if (selections.Contains(i.UId)) t_items.Add(i); });
                                foreach (var selection in selections)
                                {
                                    // Here we check each item against the other.
                                    var item = items.Where(i => i.UId == selection).First();
                                    collision = collision || t_items.Where(i => i.UId != item.UId && i.AgendaEnd >= item.AgendaStart && i.AgendaEnd <= item.AgendaEnd).Count() > 0;
                                }
                                if (collision)
                                    result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), cbg.DialogText));  // There was a time collision.  We register an error now.
                            }
                            #endregion
                        }
                        else if (dp.Component is IComponentItemParent)
                        {
                            #region IComponentItemParent
                            var radiogroup = dp.Component as IComponentItemParent;
                            var items = radiogroup.Children.ToList();
                            var i_selections = new List<IComponentItem>();
                            var t_errors = ParseItemList(items, dp.Value, out i_selections);
                            result.Errors.AddRange(t_errors);
                            IComponentItem item = i_selections.FirstOrDefault();
                            if (item != null)
                                dp.Value = item.UId.ToString();
                            else
                                dp.Value = null;
                            #endregion
                        }
                        else if (dp.Component is RatingSelect)
                        {
                            #region Rating Select
                            var rs = dp.Component as RatingSelect;
                            var t_value = 0;
                            if (!int.TryParse(dp.Value, out t_value))
                                result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "You must supply a number."));
                            if (rs.MaxRating < t_value)
                                result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "You must supply a number less than " + rs.MaxRating + "."));
                            if (rs.MinRating > t_value)
                                result.Errors.Add(new SetDataError(dp.Component.UId.ToString(), "You must supply a number more than " + rs.MaxRating + "."));
                            #endregion
                        }
                        if (!result.Success)
                        {
                            if (resetValueOnError)
                                dp.Value = o_value;
                            return result;
                        }
                    }
                    // Now we check capacities for the selections.
                    if (dp.Component is IComponentItemParent)
                    {
                        #region Capacity Checking
                        var currentSelections = new List<Guid>();
                        var oldSelections = new List<Guid>();
                        if (dp.Component is IComponentMultipleSelection)
                        {
                            try
                            {
                                currentSelections = JsonConvert.DeserializeObject<List<Guid>>(dp.Value);
                            }
                            catch (Exception)
                            {

                                currentSelections = new List<Guid>();
                            }
                            try
                            {
                                oldSelections = JsonConvert.DeserializeObject<List<Guid>>(o_value);
                            }
                            catch (Exception)
                            {
                                oldSelections = new List<Guid>();
                            }
                        }
                        else
                        {
                            Guid temp_id;
                            if (Guid.TryParse(dp.Value, out temp_id))
                                currentSelections.Add(temp_id);
                            Guid o_id;
                            if (Guid.TryParse(o_value, out o_id))
                                oldSelections.Add(o_id);
                        }

                        currentSelections.ForEach(i => oldSelections.Remove(i));

                        foreach (var temp_id in currentSelections)
                        {
                            var item = (dp.Component as IComponentItemParent).Children.FirstOrDefault(i => i.UId == temp_id);
                            if (item != null)
                            {
                                if (item.Seating != null)
                                {
                                    // There are capacity limits for the item selection.
                                    var t_seating = item.Seating.Seaters.FirstOrDefault(s => s.RegistrantKey == UId && s.ComponentKey == item.UId);
                                    if (t_seating != null)
                                    {
                                        if (t_seating.Seated)
                                            // The user was already seated. We just skip this item.
                                            continue;
                                        // The registrant was not currently seated.
                                        if (!ignoreCapacity)
                                        {
                                            // We are not ignoring capacities
                                            if (item.Seating.AvailableSeats == 0)
                                            {
                                                result.Errors.Add(new SetDataError(item.UId.ToString(), "There are no available spots for this selection."));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!ignoreCapacity && item.Seating.AvailableSeats == 0)
                                            result.Errors.Add(new SetDataError(item.UId.ToString(), "There are no available spots for this selection."));
                                    }
                                    if (!result.Success)
                                    {
                                        dp.Value = o_value;
                                        return result;
                                    }
                                    if (t_seating == null)
                                        item.Seating.Seaters.Add(new Seater() { UId = Guid.NewGuid(), Date = DateTimeOffset.UtcNow, DateSeated = DateTimeOffset.UtcNow, Seated = true, Registrant = this, Component = item as Component });
                                    else
                                    {
                                        t_seating.Seated = true;
                                        t_seating.DateSeated = DateTimeOffset.UtcNow;
                                    }
                                }
                            }
                        }
                        // Now we remove old selections seats.
                        foreach (var temp_id in oldSelections)
                        {
                            var item = (dp.Component as IComponentItemParent).Children.FirstOrDefault(i => i.UId == temp_id);
                            if (item != null)
                            {
                                if (item.Seating != null)
                                {
                                    var t_seating = item.Seating.Seaters.FirstOrDefault(s => s.RegistrantKey == UId && s.ComponentKey == item.UId);
                                    if (t_seating != null)
                                    {
                                        var s_remove = repository.Context.Seaters.Find(t_seating.UId);
                                        repository.Context.Seaters.Remove(s_remove);
                                        repository.Context.SaveChanges();
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    return result;
                }
                if (key.ToLower().StartsWith("seating"))
                {
                    #region Capacity Setting
                    // We are modifieing a capacity limit.
                    if (Guid.TryParse(key.Substring(8), out t_key))
                    {
                        component = Form.GetComponents().FirstOrDefault(c => c.UId == t_key);
                        if (component == null)
                            component = Form.GetComponentItems().FirstOrDefault(c => c.UId == t_key);
                        if (component == null)
                            result.Errors.Add(new SetDataError("", "Invalid component."));
                        if (component.Seating == null)
                            result.Errors.Add(new SetDataError("", "No capacity limit."));
                        // We found the capacity limit. Now we need to see if there is a current seating.
                        var seating = component.Seating.Seaters.FirstOrDefault(s => s.RegistrantKey == UId && s.ComponentKey == component.UId);
                        if (seating == null)
                        {
                            // There was not seating, so we need to add it.
                            seating = new Seater() { Registrant = this, Component = component as Component, Date = DateTimeOffset.UtcNow, Seating = component.Seating };
                            Seatings.Add(seating);
                        }
                        // Now we need to change the value of the seating.
                        var t_comp = component;
                        var t_value = new List<Guid>();
                        // We have grabbed the parent component.
                        if (t_comp is IComponentItem)
                            t_comp = (component as IComponentItem).Parent;
                        // We grab the datapoint.
                        dp = Data.FirstOrDefault(d => d.VariableUId == t_comp.UId);
                        if (dp == null)
                            // There was no datapoint, we create it.
                            dp = new RegistrantData() { Registrant = this, Component = t_comp as Component };
                        if (t_comp is IComponentMultipleSelection && !dp.Empty())
                            // It can have multiple selections, we deserialize the current selections.
                            t_value = JsonConvert.DeserializeObject<List<Guid>>(dp.Value);
                        if (value == "seated")
                        {
                            // We are changing him to seated for t_key
                            if (t_comp is IComponentMultipleSelection)
                            {
                                // It is a multiple selection. If the value wasn't selected. We select it.
                                if (!t_value.Contains(t_key))
                                    t_value.Add(t_key);
                                // We set the datapoint value.
                                dp.Value = JsonConvert.SerializeObject(t_value);
                            }
                            else if (t_comp is IComponentItemParent)
                            {
                                // It can have only one selection.
                                if (!dp.Empty())
                                {
                                    // The datapoint is not empty so we need to remove the old value and add the new one.
                                    Guid o_key;
                                    if (Guid.TryParse(dp.Value, out o_key))
                                    {
                                        var t_item = Form.GetComponentItems().FirstOrDefault(i => i.UId == o_key);
                                        if (t_item != null)
                                        {
                                            // The old item is now t_item.
                                            if (t_item.Seating != null)
                                            {
                                                // t_item has a seating. We no longer have this selection so we need to see if he was actually seated.
                                                var t_seat = t_item.Seating.Seaters.FirstOrDefault(s => s.RegistrantKey == UId && s.ComponentKey == t_item.UId);
                                                if (t_seat != null)
                                                {
                                                    // There was a seat so we remove it.
                                                    repository.Remove(t_seat);
                                                }
                                            }
                                        }
                                    }
                                }
                                // We set the new value that he is being seated to.
                                dp.Value = t_key.ToString();
                            }
                            // We set him as seated.
                            seating.Seated = true;
                            seating.DateSeated = DateTimeOffset.UtcNow;
                        }
                        else
                        {
                            // The registrant is being put on the waitlist.
                            if (t_comp is IComponentMultipleSelection)
                            {
                                // It is a multiple selection so we need to remove the value.
                                if (t_value.Contains(t_key))
                                    t_value.Remove(t_key);
                                dp.Value = JsonConvert.SerializeObject(t_value);
                            }
                            else if (t_comp is IComponentItemParent)
                            {
                                // If they previously had this checked, we remove it.
                                if (dp.Value.ToLower() == t_key.ToString().ToLower())
                                    dp.Value = null;
                            }
                            // We add them to the waitlist.
                            seating.Seated = false;
                        }
                    }
                    #endregion
                    return result;
                }
                if (Contact != null)
                    Contact.SetData(key, value);
                repository.Commit();
                return result;
            }
        }

        /// <summary>
        /// Removes the item from selection and adjusts seating accordingly.
        /// </summary>
        /// <param name="item">The item to unselect.</param>
        /// <returns>The result.</returns>
        public SetDataResult RemoveItem(IComponentItem item)
        {
            var component = item.Parent;

            if (component is IComponentMultipleSelection)
            {
                var dp = Data.FirstOrDefault(d => d.VariableUId == component.UId);
                if (dp == null)
                    return new SetDataResult();
                var selections = JsonConvert.DeserializeObject<List<Guid>>(dp.Value);
                selections.Remove(item.UId);
                return SetData(component.SortingId.ToString(), JsonConvert.SerializeObject(selections));
            }
            else if (component is IComponentItemParent)
            {
                return SetData(component.SortingId.ToString(), null, ignoreValidation: true);
            }

            return new SetDataResult() { Errors = new List<SetDataError>() { new SetDataError("", "Invalid component.") } };
        }

        /// <summary>
        /// Gets the date that the item was selected.
        /// </summary>
        /// <param name="item">The item in question.</param>
        /// <returns>The <code>DateTimeOffset</code> of the first time this item was selected and not changed working from recent to earliest.</returns>
        public DateTimeOffset GetModifiedDate(IComponentItem item)
        {
            var data = Data.FirstOrDefault(d => d.VariableUId == item.ParentKey);
            if (data == null)
                return DateTimeOffset.Now;
            var lastDate = IsCurrentSelection(item, data) ? DateModified : DateTimeOffset.Now;
            foreach (var oldRegistration in OldRegistrations.OrderByDescending(r => r.DateCreated))
            {
                var t_data = oldRegistration.Data.Where(d => d.VariableUId.HasValue && d.VariableUId == item.ParentKey).FirstOrDefault();
                if (t_data == null)
                    return lastDate;
                if (IsCurrentSelection(item, t_data))
                    lastDate = oldRegistration.DateCreated;
                else
                    return lastDate;
            }
            return lastDate;
        }

        /// <summary>
        /// Checks to see if the item is currently selected for the specified data set.
        /// </summary>
        /// <param name="item">The compontent item.</param>
        /// <param name="data">The data set.</param>
        /// <returns>True if currently selected, false otherwise.</returns>
        public bool IsCurrentSelection(IComponentItem item, IPersonData data)
        {
            if (data == null)
                return false;
            if (item.Parent is IComponentMultipleSelection)
            {
                var selections = JsonConvert.DeserializeObject<List<Guid>>(data.Value) ?? new List<Guid>();
                if (selections.Contains(item.UId))
                    return true;
                return false;
            }
            Guid id;
            if (Guid.TryParse(data.Value ?? "", out id))
            {
                if (id == item.UId)
                    return true;
                return false;
            }
            return false;
        }

        #region Search Methods

        /// <summary>
        /// Searches for a datapoint.
        /// </summary>
        /// <param name="search">The field name or Guid of the datapoint.</param>
        /// <returns>The RegistrantData object representing the datapoint.</returns>
        public RegistrantData SearchData(string search)
        {
            RegistrantData dp = null;
            Guid t_id;
            if (Guid.TryParse(search, out t_id))
                dp = Data.FirstOrDefault(d => d.Component.UId == t_id);
            if (dp == null)
                dp = Data.FirstOrDefault(d => d.Component.Variable.Value == search);
            return dp;
        }

        /// <summary>
        /// Creates a <code>JsonTableValue</code> for the specified header.
        /// </summary>
        /// <param name="header">The header to make the value off of.</param>
        /// <returns>The <code>JsonTableValue</code> created.</returns>
        public JsonTableValue GetJsonTableValue(JsonTableHeader header)
        {
            var value = "";
            var rawValue = "";
            long lid;
            var id = header.Id;
            if (long.TryParse(header.Id, out lid))
            {
                // We need to get a datapoint.
                var data = Data.FirstOrDefault(h => h.Component.SortingId == lid);
                if (data != null)
                {
                    value = data.GetPretty();
                    rawValue = data.Value;
                    if (data.Component is IComponentMultipleSelection && !data.Empty())
                    {
                        var l_raw = new List<long>();
                        JsonConvert.DeserializeObject<List<Guid>>(data.Value).ForEach(itemId =>
                        {
                            var item = ((IComponentItemParent)data.Component).Children.FirstOrDefault(c => c.UId == itemId);
                            if (item != null)
                                l_raw.Add(item.SortingId);
                        });
                        rawValue = JsonConvert.SerializeObject(l_raw);
                    }
                    else if (data.Component is IComponentItemParent && !data.Empty())
                    {
                        Guid s_id;
                        if (Guid.TryParse(data.Value, out s_id))
                        {
                            var item = ((IComponentItemParent)data.Component).Children.FirstOrDefault(c => c.UId == s_id);
                            if (item != null)
                                rawValue = item.SortingId.ToString();
                        }
                    }
                    id = data.SortingId.ToString();
                }
            }
            else
            {
                rawValue = null;
                switch (header.Value.ToLower())
                {
                    case "confirmation":
                        value = Confirmation;
                        break;
                    case "email":
                        value = Email;
                        break;
                    case "status":
                        value = Status.GetStringValue();
                        rawValue = ((int)Status).ToString();
                        break;
                    case "rsvp":
                        value = RSVP ? Form.RSVPAccept : Form.RSVPDecline;
                        rawValue = RSVP ? "true" : "false";
                        break;
                    case "audience":
                        value = Audience != null ? Audience.Label : "";
                        rawValue = Audience != null ? Audience.SortingId.ToString() : null;
                        break;
                    case "date registered":
                        value = DateCreated.ToString("yyyy-MM-dd h:mm tt");
                        rawValue = DateCreated.ToString();
                        break;
                    case "last edit":
                        value = DateModified.ToString("yyyy-MM-dd h:mm tt");
                        rawValue = DateModified.ToString();
                        break;
                    case "edited by":
                        using (var context = new EFDbContext())
                        {
                            var sid = ModifiedBy.ToString();
                            if (ModifiedBy == Guid.Empty)
                            {
                                value = "Registrant";
                                rawValue = Guid.Empty.ToString();
                                break;
                            }
                            var t_user = context.Users.FirstOrDefault(f => f.Id == sid);
                            if (t_user == null)
                            {
                                value = "System";
                                rawValue = null;
                                break;
                            }
                            value = t_user.UserName;
                            rawValue = t_user.Id;
                        }
                        break;
                    case "in contact list":
                        var allEmails = Form.Company.Contacts.SelectMany(c => c.GetEmails());
                        value = allEmails.Contains(Email) ? "Yes" : "No";
                        rawValue = value == "Yes" ? "true" : "false";
                        break;
                    case "promotions":
                        var promotionlist = "";
                        for (var p_i = 0; p_i < PromotionalCodes.Count; p_i++)
                        {
                            var code = PromotionalCodes[p_i].Code;
                            promotionlist += code.Code;
                            if ((p_i + 1) < PromotionalCodes.Count)
                                promotionlist += "<br />";
                        }
                        value = promotionlist;
                        break;
                    case "balance":
                        value = TotalOwed.ToString("c", System.Globalization.CultureInfo.GetCultureInfo(Form.Currency.GetCurrencyFormat()));
                        rawValue = TotalOwed.ToString();
                        break;
                    case "tax":
                        value = Taxes.ToString("c", System.Globalization.CultureInfo.GetCultureInfo(Form.Currency.GetCurrencyFormat()));
                        rawValue = Taxes.ToString();
                        break;
                    case "fees":
                        value = Fees.ToString("c", System.Globalization.CultureInfo.GetCultureInfo(Form.Currency.GetCurrencyFormat()));
                        rawValue = Fees.ToString();
                        break;
                    case "adjustments":
                        value = Adjustings.ToString("c", System.Globalization.CultureInfo.GetCultureInfo(Form.Currency.GetCurrencyFormat()));
                        rawValue = Adjustings.ToString();
                        break;
                    case "transactions":
                        value = Transactions.ToString("c", System.Globalization.CultureInfo.GetCultureInfo(Form.Currency.GetCurrencyFormat()));
                        rawValue = Transactions.ToString();
                        break;
                    case "payment method":
                        if (TransactionRequests.Count > 0)
                        {
                            value = "Credit Card";
                            break;
                        }
                        value = "Bill Me";
                        break;
                    case "credit last four":
                        if (TransactionRequests.Count > 0)
                        {
                            value = TransactionRequests.First().LastFour;
                            break;
                        }
                        value = "";
                        break;
                }
                if (rawValue == null)
                    rawValue = value;
            }
            return new JsonTableValue() { Header = header, HeaderId = header.Id, Value = value, Id = id, Type = header.Type, Editable = header.Editable, RawData = rawValue };
            
        }

        public string GetPretty(JsonTableHeader header, bool html = true)
        {
            long lid;
            if (long.TryParse(header.Id, out lid))
            {
                // We need to get a datapoint.
                var data = Data.FirstOrDefault(h => h.SortingId == lid);
                if (data == null)
                    return "";
                else
                    return data.GetPretty();
            }
            else
            {
                switch (header.Value.ToLower())
                {
                    case "confirmation":
                        return Confirmation;
                    case "email":
                        return Email;
                    case "status":
                        return Status.GetStringValue();
                    case "rsvp":
                        return RSVP ? Form.RSVPAccept : Form.RSVPDecline;
                    case "audience":
                        return Audience != null ? Audience.Label : "";
                    case "date registered":
                        return DateCreated.LocalDateTime.ToString();
                    case "edited by":
                        using (var context = new EFDbContext())
                        {
                            var sid = ModifiedBy.ToString();
                            if (ModifiedBy == Guid.Empty)
                                return "Registrant";
                            var t_user = context.Users.FirstOrDefault(f => f.Id == sid);
                            if (t_user == null)
                                return "System";
                            return t_user.UserName;
                        }
                    case "in contact list":
                        return Form.EmailList != null && Form.EmailList.GetAllEmailAddresses().Contains(Email) ? "Yes" : "No";
                    case "promotions":
                        var promotionlist = "";
                        for (var p_i = 0; p_i < PromotionalCodes.Count; p_i++)
                        {
                            var code = PromotionalCodes[p_i].Code;
                            promotionlist += code.Code;
                            if ((p_i + 1) < PromotionalCodes.Count)
                                promotionlist += "<br />";
                        }
                        return promotionlist;
                    case "balance":
                        return TotalOwed.ToString("c", System.Globalization.CultureInfo.GetCultureInfo(Form.Currency.GetCurrencyFormat()));
                    case "tax":
                        return Taxes.ToString("c", System.Globalization.CultureInfo.GetCultureInfo(Form.Currency.GetCurrencyFormat()));
                    case "fees":
                        return Fees.ToString("c", System.Globalization.CultureInfo.GetCultureInfo(Form.Currency.GetCurrencyFormat()));
                    case "adjustemnts":
                        return Adjustings.ToString("c", System.Globalization.CultureInfo.GetCultureInfo(Form.Currency.GetCurrencyFormat()));
                    case "transactions":
                        return Transactions.ToString("c", System.Globalization.CultureInfo.GetCultureInfo(Form.Currency.GetCurrencyFormat()));
                    case "payment method":
                        if (TransactionRequests.Count > 0)
                            return "Credit Card";
                        return "Bill Me";
                    case "credit last four":
                        if (TransactionRequests.Count > 0)
                            return TransactionRequests.First().LastFour;
                        return "";
                }
            }
            return "";
        }

        public object SortValue(string search)
        {
            var t_search = search.ToLower();
            switch (t_search)
            {
                case "confirmation":
                    return SortingId;
                case "date registered":
                    return DateCreated;
                case "date modified":
                case "last edit":
                    return DateModified;
                case "audience":
                    if (Audience != null)
                        return Audience.Label;
                    else
                        return null;
                case "email":
                    return Email;
                case "balance":
                    return TotalOwed;
                case "fees":
                    return Fees;
                case "adjustments":
                    return Adjustings;
                case "transactino":
                    return Transactions;
                case "taxes":
                    return Taxes;
                case "status":
                    return (int)Status;
            }
            Guid gid;
            long lid;
            RegistrantData dp = null;
            if (long.TryParse(search, out lid))
                dp = Data.FirstOrDefault(d => d.Component.SortingId == lid);
            else if (Guid.TryParse(search, out gid))
                dp = Data.FirstOrDefault(d => d.VariableUId == gid);
            if (dp == null)
                return null;
            return dp.GetFormattedValue();
        }

        /// <summary>
        /// Gets the pretty value of a datapoint.
        /// </summary>
        /// <param name="search">The field name to search for.</param>
        /// <returns>The string representing pretty value.</returns>
        public string SearchPrettyValue(string search)
        {
            switch (search.ToLower())
            {
                case "confirmation":
                    return Confirmation;
                case "dateregisted":
                case "date registered":
                    return DateCreated.ToString("s");
                case "datemodified":
                case "date modified":
                case "last edit":
                    return DateModified.ToString("s");
                case "email":
                    return Email;
                case "audience":
                    return Audience.Label;
            }
            RegistrantData dp = null;
            Guid t_id;
            if (Guid.TryParse(search, out t_id))
                dp = Data.FirstOrDefault(d => d.VariableUId == t_id);
            if (dp == null)
                dp = Data.FirstOrDefault(d => d.Component.Variable.Value == search);
            if (dp != null)
                return dp.GetFormattedValue();
            else
                return null;
        }

        /// <summary>
        /// Retrieves all secured items.  This method creates an accesslog entry for each secured item.
        /// </summary>
        /// <param name="user">The user trying to access the system.</param>
        /// <returns>An enumerable list of ISecure items.</returns>
        public IEnumerable<ISecure> GetSecuredItems(User user)
        {
            var list = new List<ISecure>();
            using (var rep = new FormsRepository())
            {
                foreach (var datapoint in Data.Where(dp => dp.Value.IsGuid()))
                {
                    var id = Guid.Parse(datapoint.Value);
                    var sec = rep.Search<ISecure>(s => s.UId == id).FirstOrDefault();
                    if (sec != null)
                        list.Add(sec);
                }
            }
            return list;
        }

        public IPersonData FindData(string search)
        {
            return SearchData(search);
        }

        /// <summary>
        /// Gets the raw value of the registrant data in question.
        /// </summary>
        /// <param name="search">The data to look for.</param>
        /// <returns>A string of the raw value.</returns>
        public string GetRawValue(string search)
        {
            var data = SearchData(search);
            if (data == null)
                return "";
            return data.Value ?? "";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses a raw list of item identifiers and compares them to the items for the component items being passed.
        /// </summary>
        /// <typeparam name="T">The type of component item.</typeparam>
        /// <param name="items">The list of available items.</param>
        /// <param name="rawSelection">The json object of an array of component identifiers.</param>
        /// <returns>Returns a list of items that are selected.</returns>
        public IEnumerable<SetDataError> ParseItemList(IEnumerable<IComponentItem> items, string rawSelection, out List<IComponentItem> selections)
        {
            var t_selections = new List<IComponentItem>();
            selections = t_selections;
            var errors = new List<SetDataError>();
            List<string> obj_selections = new List<string>();
            if (!String.IsNullOrWhiteSpace(rawSelection))
            {
                try
                {
                    obj_selections = JsonConvert.DeserializeObject<List<string>>(rawSelection);
                }
                catch (Exception)
                {
                    obj_selections = new List<string>();
                    obj_selections.Add(rawSelection);
                }
            }
            // Now we check throuh each item and determine it's match.
            foreach (var t_unkId in obj_selections)
            {
                var st_unkId = t_unkId.Trim().ToLower();
                Guid uidId;
                long lId;
                IComponentItem item;
                if (Guid.TryParse(t_unkId, out uidId))
                    item = items.FirstOrDefault(i => i.UId == uidId);
                else if (long.TryParse(t_unkId, out lId))
                    item = items.FirstOrDefault(i => i.SortingId == lId);
                else
                    item = items.FirstOrDefault(i => i.LabelText.ToLower() == st_unkId);
                if (item != null)
                    t_selections.Add(item);
                else
                    errors.Add(new SetDataError(items.First().ParentKey.ToString(), "Invalid selection: " + t_unkId));
            }
            selections = t_selections;
            return errors;
        }

        /// <summary>
        /// Updates the Fees, Transactions, and Adjustings.
        /// </summary>
        public void UpdateAccounts()
        {
            var fees = GetShoppingCartItems().Total();
            // Now we add in promotion codes
            foreach (var promotion in PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Subtract))
                fees -= promotion.Code.Amount;
            foreach (var promotion in PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Add))
                fees += promotion.Code.Amount;
            foreach (var promotion in PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Multiply))
                fees *= promotion.Code.Amount;
            Taxes = 0m;
            if (Data.Where(d => d.Component.Variable != null && d.Component.Variable.Value == "__NoTax" && d.Value == "true").Count() == 0)
                Taxes = fees * (Form.Tax.HasValue ? (Form.Tax.Value) : 0);

            Fees = Math.Round(fees, 2);
            Taxes = Math.Round(Taxes, 2);
            Adjustings = Math.Round(TotalAdjustments(), 2);
            Transactions = Math.Round(Settlement(), 2);
        }

        /// <summary>
        /// Builds a shopping cart based on the registration data.
        /// </summary>
        /// <returns>The shopping cart to be used.</returns>
        public ShoppingCart GetShoppingCartItems()
        {
            if (pr_shoppingCart != null)
                return pr_shoppingCart;

            pr_shoppingCart = new ShoppingCart(Form, this);
            if (Form.Price.HasValue && RSVP)
            pr_shoppingCart.Items.Add(new ShoppingCartItem("Base Registration Fee", Form.Price.Value, 1));
            foreach (var data in Data)
            {
                var component = data.Component;
                if (component is FreeText || component is Input)
                    continue;
                if (data.Empty())
                    continue;
                if (data.Component == null)
                    continue;
                if (data.Component is IComponentMultipleSelection)
                {
                    var comp = (IComponentItemParent)data.Component;
                    var selections = JsonConvert.DeserializeObject<List<Guid>>(data.Value);
                    foreach (var id in selections)
                    {
                        var item = comp.Children.FirstOrDefault(i => i.UId == id);
                        if (item == null)
                            continue;
                        var itemPrice = PriceGroup.DisplayPrice((Component)item, this);
                        if (itemPrice.HasValue)
                            pr_shoppingCart.Items.Add(new ShoppingCartItem((Component)item) { Quanity = 1, Ammount = itemPrice.Value });
                    }
                }
                else
                {
                    decimal? price = PriceGroup.GetPrice(data.Component, this);
                    var quanity = 1;
                    if (!price.HasValue)
                        continue;
                    var t_component = data.Component;
                    if (t_component is IComponentItemParent)
                    {
                        t_component = (data.Component as IComponentItemParent).Children.FirstOrDefault(c => c.UId.ToString() == data.Value) as Component;
                        t_component = t_component ?? data.Component;
                    }
                    if (t_component is NumberSelector)
                    {
                        if (!int.TryParse(data.Value ?? "0", out quanity))
                            quanity = 0;
                    }
                    if (quanity > 0)
                        pr_shoppingCart.Items.Add(new ShoppingCartItem(t_component) { Quanity = quanity, Ammount = price.Value });
                }
            }
            return pr_shoppingCart;
        }

        protected decimal TotalAdjustments()
        {
            var total = 0.00m;
            foreach (var adj in Adjustments.Where(a => !a.Voided))
            {
                total += adj.Amount;
            }
            return total;
        }

        public List<TransactionDetail> Details()
        {
            var list = new List<TransactionDetail>();
            foreach (var req in TransactionRequests.OrderBy(r => r.Date))
            {
                foreach (var detail in req.Details.Where(d => (d.TransactionType == TransactionType.AuthorizeCapture || d.TransactionType == TransactionType.Capture || d.TransactionType == TransactionType.Credit) && d.Approved).OrderBy(d => d.DateCreated))
                {
                    list.Add(detail);
                }
            }
            return list;
        }

        protected decimal Settlement()
        {
            var settlements = 0.00m;
            foreach (var settlement in this.TransactionRequests.OrderBy(r => r.Date))
            {
                foreach (var capture in settlement.Details.Where(d => d != null && (d.TransactionType == TransactionType.AuthorizeCapture || d.TransactionType == TransactionType.Capture) && d.Approved))
                {
                    settlements += settlement.Ammount;
                    foreach (var credit in capture.Credits)
                    {
                        if (!credit.Approved)
                            continue;
                        settlements -= credit.Ammount;
                    }
                }
            }
            return settlements;
        }

        /// <summary>
        /// Gets a list of finances.
        /// </summary>
        /// <param name="Repository">Used to get the user who created the adjustment.</param>
        /// <returns>Returns an IEnumerable of type IFinanceAmmount.</returns>
        public IEnumerable<IFinanceAmmount> Finances(FormsRepository Repository)
        {
            var list = new List<IFinanceAmmount>();
            foreach (var req in TransactionRequests)
            {
                foreach (var det in req.Details.Where(d => d.Approved).ToList())
                {
                    list.Add(det);
                }
            }
            foreach (var adj in Adjustments)
            {
                list.Add(adj);
                var name = Form.Company.Name;
                var user = ((EFDbContext)Repository.Context).Users.FirstOrDefault(u => u.Id == adj.ModifiedBy.ToString());
                    name = user.UserName;
                adj.Creator = name;
            }
            list.Sort((a, b) => a.DateCreated.CompareTo(a.DateCreated));
            return list;
        }

        public IPermissionHolder GetPermissionHolder()
        {
            return Form;
        }
        
        #endregion

        #region ComparableData

        public int CompareValue(string field, string testValue, string test, bool caseSensitive = false)
        {
            var testingValue = testValue;

            if (!caseSensitive && testingValue != null)
                testingValue = testingValue.ToLower();


            #region Registrant Data
            long lid;
            if (long.TryParse(field, out lid))
            {
                var data = Data.FirstOrDefault(d => d.Component.SortingId == lid);
                if (testingValue == null && (data == null || data.Empty()))
                    return 0;
                if (data == null)
                    return 1;
                if (data.Component is IComponentMultipleSelection)
                {
                    #region IComponentMultipleSelection
                    long iLid;
                    if (!long.TryParse(testValue, out iLid))
                        return 1;
                    var selections = new List<Guid>();
                    try
                    {
                        selections = JsonConvert.DeserializeObject<List<Guid>>(data.Value);
                    }
                    catch (Exception)
                    {
                        return 1;
                    }
                    var testItem = (data.Component as IComponentItemParent).Children.FirstOrDefault(c => c.SortingId == iLid);
                    if (testItem == null)
                        return 1;
                    var iP_contains = selections.Contains(testItem.UId);
                    var count = selections.Count;
                    switch (test)
                    {
                        case "==":
                            if (iP_contains)
                            {
                                if (count > 0)
                                    return 1;
                                if (count < 0)
                                    return -1;
                                else
                                    return 0;
                            }
                            if (count > 0)
                                return 1;
                            else
                                return -1;
                        case "!=":
                            if (iP_contains)
                                return 1;
                            return 0;
                        case "*=":
                        case "in":
                            if (iP_contains)
                                return 0;
                            return 1;
                        case "!*=":
                        case "not in":
                            if (iP_contains)
                                return 1;
                            return 0;
                    }
                    #endregion
                }
                else if (data.Component is IComponentItemParent)
                {
                    #region IComponentItemParent
                    long iLid;
                    if (!long.TryParse(testValue, out iLid))
                        return 1;
                    Guid selection;
                    if (!Guid.TryParse(data.Value ?? "", out selection))
                        return 1;
                    var testItem = (data.Component as IComponentItemParent).Children.FirstOrDefault(c => c.SortingId == iLid);
                    if (testItem == null)
                        return 1;
                    switch (test)
                    {
                        case "==":
                            if (testItem.UId == selection)
                                return 0;
                            return 1;
                        case "!=":
                            if (testItem.UId != selection)
                                return 0;
                            return 1;
                    }
                    #endregion
                }
                else
                {
                    #region Value Component
                    var value = data.Value ?? "";
                    if (!caseSensitive)
                        value = value.ToLower();
                    switch (test)
                    {
                        case "==":
                            return value == testingValue ? 0 : 1;
                        case "!=":
                            return value != testingValue ? 0 : 1;
                        case "^=":
                            return value.StartsWith(testingValue) ? 0 : 1;
                        case "!^=":
                            return !value.StartsWith(testingValue) ? 0 : 1;
                        case "$=":
                            return value.EndsWith(testingValue) ? 0 : 1;
                        case "!$=":
                            return !value.EndsWith(testingValue) ? 0 : 1;
                        case "*=":
                            return value.Contains(testingValue) ? 0 : 1;
                        case "!*=":
                            return !value.Contains(testingValue) ? 0 : 1;
                        case "=rgx=":
                        case "!=rgx=":
                            var rgx = new Regex(testValue);
                            if (test == "=rgx=")
                                return rgx.IsMatch(value) ? 0 : 1;
                            return !rgx.IsMatch(value) ? 0 : 1;
                    }
                    #endregion
                }

            }
            #endregion
            #region Properties
            switch (field.ToLower())
            {
                case "email":
                    return Compare(Email, testingValue ?? "", test, caseSensitive) ? 0 : 1;
                case "confirmation":
                    return Compare(Confirmation, testingValue ?? "", test, caseSensitive) ? 0 : 1;
                case "datecreated":
                case "dateregistered":
                case "date registered":
                    return Compare(DateCreated, testingValue ?? DateTimeOffset.MinValue.ToString(), test) ? 0 : 1;
                case "status":
                    return CompareStatus(testingValue ?? "0", test) ? 0 : 1;
                case "type":
                    return CompareType(testingValue ?? "0", test) ? 0 : 1;
                case "audience":
                    return CompareAudience(AudienceKey.HasValue ? AudienceKey.Value : Guid.Empty, testingValue ?? Guid.Empty.ToString()) ? 0 : 1;
                case "rsvp":
                    return CompareRsvp(testingValue ?? "False", test) ? 0 : 1;
                case "lastedit":
                case "last edit":
                    return Compare(DateModified, testingValue ?? DateTimeOffset.MinValue.ToString(), test) ? 0 : 1;
                case "incontactList":
                case "in contact list":
                    var t_email = Form.Company.Contacts.Where(c => c.Email.ToLower() == Email.ToLower()).Count() > 0;
                    if (!t_email)
                    {
                        t_email = Form.Company.Contacts.Where(c => c.Data.Where(cd => cd.Value.ToLower() == Email.ToLower() && cd.Header.Descriminator == ContactDataType.Email).Count() > 0).Count() > 0;
                    }
                    if (testingValue == "true")
                        return t_email ? 0 : 1;
                    else
                        return !t_email ? 0 : 1;
                case "balance":
                    return Compare(TotalOwed, testingValue, test) ? 0 : 1;
                case "fees":
                    return Compare(Fees, testingValue, test) ? 0 : 1;
                case "adjustments":
                    return Compare(Adjustings, testingValue, test) ? 0 : 1;
                case "transactions":
                    return Compare(Transactions, testingValue, test) ? 0 : 1;
            }
            return 1;

            #endregion
        }

        public bool ValueEquals(string field, string testValue, string test, bool caseSensitive = false)
        {
            return CompareValue(field, testValue, test, caseSensitive) == 0;
        }

        #endregion

        #region Comparing

        // Used for filtering.
        /// <summary>
        /// Tests a value against a LogicTest for filtering.
        /// </summary>
        /// <param name="field">The field to check.</param>
        /// <param name="testValue">The value to test against.</param>
        /// <param name="test">The logic test.</param>
        /// <param name="company">The company the class belongs to.</param>
        /// <param name="repository">The <code>FormsRepository</code> used for the database.</param>
        /// <param name="caseSensitive">Whether the search is case sensitive or not.</param>
        /// <returns>A boolean value of the test result.</returns>
        public bool TestValue(string field, string testValue, LogicTest test, Company company, FormsRepository repository, bool caseSensitive = false)
        {
            var items = new List<Guid>();
            return CompareData(field, testValue, test.GetTestValue(), ref items, caseSensitive);
        }

        public bool TestValue(string field, string testValue, string test, bool caseSensitive = false)
        {
            var items = new List<Guid>();
            return CompareData(field, testValue, test, ref items, caseSensitive);
        }

        /// <summary>
        /// Compares data sets with a value.
        /// </summary>
        /// <param name="key">The key of the datapoint to find.</param>
        /// <param name="value">The value to compare it to.</param>
        /// <param name="test">The LogicTest to use for comparing.</param>
        /// <param name="caseSensitive">Whether the comparing is case sensitive. default: false</param>
        /// <returns>A boolean value of the comparing.</returns>
        public bool CompareData(string key, string value, string test, ref List<Guid> matchedContacts, bool caseSensitive = false)
        {
            if (value == "_empty_")
                value = "";
            value = value ?? "";         
            value = Engines.RegParser.Parse(this, value);
            value = (caseSensitive ? value : value.ToLower());
            switch (key.ToLower())
            {
                case "email":
                    return Compare(Email, value ?? "", test, caseSensitive);
                case "confirmation":
                    return Compare(Confirmation, value ?? "", test, caseSensitive);
                case "datecreated":
                case "dateregistered":
                case "date registereed":
                    return Compare(DateCreated, value ?? DateTimeOffset.MinValue.ToString(), test);
                case "status":
                    return CompareStatus(value ?? "0", test);
                case "type":
                    return CompareType(value ?? "0", test);
                case "audience":
                    return CompareAudience(AudienceKey.HasValue ? AudienceKey.Value : Guid.Empty, value ?? Guid.Empty.ToString());
                case "rsvp":
                    return CompareRsvp(value ?? "False", test);
                case "lastedit":
                case "last edit":
                    return Compare(DateModified, value ?? DateTimeOffset.MinValue.ToString(), test);
                case "incontactList":
                case "in contact list":
                    var t_email = Form.Company.Contacts.Where(c => c.Email.ToLower() == Email.ToLower()).Count() > 0;
                    if (!t_email)
                    {
                        t_email = Form.Company.Contacts.Where(c => c.Data.Where(cd => cd.Value.ToLower() == Email.ToLower() && cd.Header.Descriminator == ContactDataType.Email).Count() > 0).Count() > 0;
                    }
                    if (value == "true")
                        return t_email;
                    else
                        return !t_email;
                case "balance":
                    return Compare(TotalOwed, value, test);
                case "fees":
                    return Compare(Fees, value, test);
                case "adjustments":
                    return Compare(Adjustings, value, test);
                case "transactions":
                    return Compare(Transactions, value, test);
            }
            var dataPoint = Data.FirstOrDefault(d => d.Component != null && d.Component.Variable != null && d.Component.Variable.Value == key);
            if (dataPoint == null)
            {
                // Lets check if this is a seating comparing.
                if (key.ToLower().StartsWith("seating"))
                {
                    Guid t_key;
                    // We are modifying a capacity limit.
                    if (Guid.TryParse(key.Substring(8), out t_key))
                    {
                        var component = Form.GetComponents().FirstOrDefault(c => c.UId == t_key);
                        if (component == null)
                            component = Form.GetComponentItems().FirstOrDefault(c => c.UId == t_key);
                        if (component == null)
                            return false;
                        if (component.Seating == null)
                            return false;
                        // We found the capacity limit. Now we need to see if there is a current seating.
                        var seating = component.Seating.Seaters.FirstOrDefault(s => s.RegistrantKey == UId);
                        if (seating == null)
                            return false;
                        else
                            return seating.Seated == (value.ToLower() == "seated" ? true : false);
                    }
                }
                // It's not a seating.
                Guid uid = Guid.Empty;
                if (!Guid.TryParse(key, out uid))
                {
                    // The user does not have the specified data. Now we need to decide what to return.
                    if (String.IsNullOrEmpty(value))
                    {
                        // We are checkin if the registrant value is null or empty. It is.
                        if (test == "!=")
                            return false;
                        return true;
                    }
                }
                dataPoint = Data.FirstOrDefault(d => d.Component.UId == uid);
                if (dataPoint == null)
                {
                    var cH = Form.Company.ContactHeaders.FirstOrDefault(c => c.UId == uid);
                    if (cH != null)
                    {
                        // It is a contact header. We need to see if this value exists in the contact header.
                        using (var context = new EFDbContext())
                        {
                            var contacts = context.ContactData.Where(d => d.HeaderKey == cH.UId && d.Value == value).Select(c => c.UId).ToList();
                            if (matchedContacts == null)
                                matchedContacts = contacts;
                            else
                                matchedContacts = matchedContacts.Intersect(contacts).ToList();
                            switch (test)
                            {
                                case "==":
                                case "in":
                                    if (matchedContacts.Count > 0)
                                        return true;
                                    return false;
                                default:
                                    if (matchedContacts.Count == 0)
                                        return true;
                                    return false;
                            }
                        }
                        /*
                        if (Contact == null)
                            return false;
                        return Contact.CompareData(key, value, test, caseSensitive);
                        //*/
                    }
                    else
                    {
                        if ((test == "==" || test == "=") && String.IsNullOrEmpty(value))
                            return true;
                        else if (test == "==" && !String.IsNullOrEmpty(value))
                            return false;
                        else if (test == "!=" && String.IsNullOrEmpty(value))
                            return false;
                        else if (test == "!=" && !String.IsNullOrEmpty(value))
                            return true;
                        else if ((test == "not in" || test == "!^=" || test == "!*=" || test == "!$=" || test == "!=rgx=") && !String.IsNullOrEmpty(value))
                            return true;
                        else
                            return false;
                    }
                }
            }
            return CompareDataSet(dataPoint, value, test, caseSensitive);
        }

        /// <summary>
        /// Compares data sets with a value.
        /// </summary>
        /// <param name="datapoint">The datapoint for comparing.</param>
        /// <param name="value">The value to compare it to.</param>
        /// <param name="test">The LogicTest to use for comparing.</param>
        /// <param name="caseSensitive">Whether the comparing is case sensitive. default: false</param>
        /// <returns>A boolean value of the comparing.</returns>
        public bool CompareDataSet(RegistrantData datapoint, string value, string test, bool caseSensitive = false)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                if (test == "==" || test == "=")
                    return datapoint.Empty();
                else
                    return !datapoint.Empty();
            }
            var r_value = datapoint.Value ?? "";
            r_value = (caseSensitive ? r_value : r_value.ToLower());
            value = (caseSensitive ? value : value.ToLower());
            if (datapoint.Component is IComponentMultipleSelection)
            {
                #region Multiple Selection
                var items = JsonConvert.DeserializeObject<List<Guid>>(datapoint.Value);
                Guid item;
                if (!Guid.TryParse(value, out item))
                {
                    var t_item = ((IComponentItemParent)datapoint.Component).Children.Where(i => i.LabelText.ToLower() == value.ToLower()).FirstOrDefault();
                    if (t_item != null)
                        item = t_item.UId;
                }
                switch (test)
                {
                    case "in":
                    case "*=":
                        return items.Contains(item);
                    case "notin":
                    case "!*=":
                    case "!=":
                        return !items.Contains(item);
                    case "==":
                    case "=":
                        return items.Contains(item) && items.Count == 1;
                    default:
                        return false;
                }
                #endregion
            }
            else if (datapoint.Component is IComponentItemParent)
            {
                #region Single Selection
                Guid item;
                var selected = Guid.Empty;
                Guid.TryParse(r_value, out selected);
                if (!Guid.TryParse(value, out item))
                {
                    var t_item = ((IComponentItemParent)datapoint.Component).Children.Where(i => i.LabelText.ToLower() == value.ToLower()).FirstOrDefault();
                    if (t_item != null)
                        item = t_item.UId;
                }
                switch (test)
                {
                    case "==":
                    case "=":
                    case "*=":
                    case "in":
                        return selected == item;
                    case "!=":
                    case "not in":
                    case "!*=":
                        return selected != item;
                    case ">":
                        var gt_value = 0m;
                        var gtp_value = 0m;
                        if (decimal.TryParse(value, out gt_value) || decimal.TryParse(r_value, out gtp_value))
                            return false;
                        return gtp_value > gt_value;
                    case ">=":
                        var gte_value = 0m;
                        var gtep_value = 0m;
                        if (decimal.TryParse(value, out gte_value) || decimal.TryParse(r_value, out gtep_value))
                            return false;
                        return gtep_value >= gte_value;
                    case "<":
                        var lt_value = 0m;
                        var ltp_value = 0m;
                        if (decimal.TryParse(value, out lt_value) || decimal.TryParse(r_value, out ltp_value))
                            return false;
                        return ltp_value < lt_value;
                    case "<=":
                        var lte_value = 0m;
                        var ltep_value = 0m;
                        if (decimal.TryParse(value, out lte_value) || decimal.TryParse(r_value, out ltep_value))
                            return false;
                        return ltep_value <= lte_value;
                    default:
                        return false;
                }
                #endregion
            }
            else if (datapoint.Component is Input)
            {
                #region Input
                value = value.ToLower();
                switch (((Input)datapoint.Component).Type)
                {
                    case InputType.Date:
                    case InputType.DateTime:
                    case InputType.Time:
                        var datetime = DateTimeOffset.MinValue;
                        var testVal = DateTimeOffset.MinValue;
                        DateTimeOffset.TryParse(r_value, out datetime);
                        DateTimeOffset.TryParse(value, out testVal);
                        switch (test)
                        {
                            case "==":
                            case "=":
                                return datetime.Equals(testVal);
                            case "!=":
                                return !datetime.Equals(testVal);
                            case ">":
                                return datetime > testVal;
                            case ">=":
                                return datetime >= testVal;
                            case "<":
                                return datetime < testVal;
                            case "<=":
                                return datetime <= testVal;
                        }
                        break;
                    case InputType.SSN:
                        var ssn_ret = datapoint.Value.Replace("-", "").Trim();
                        var t_ssn = value.Replace("-", "").Trim();
                        if (ssn_ret.Length != 9)
                            return false;
                        return Compare(ssn_ret, t_ssn, test);
                    case InputType.UniversalCreditCard:
                        var cc_id = Guid.Empty;
                        if (Guid.TryParse(datapoint.Value, out cc_id))
                        {
                            using (var repository = new FormsRepository())
                            {
                                var t_cc = repository.Search<CreditCard>(c => c.UId == cc_id, silent: true).FirstOrDefault();
                                if (t_cc == null)
                                    return false;
                                return Compare(t_cc.Number, value, test);
                            }
                        }
                        return false;
                    case InputType.Multiline:
                    case InputType.Default:
                        switch (((Input)datapoint.Component).ValueType)
                        {
                            case Components.ValueType.Number:
                            case Components.ValueType.Decimal:
                                return Compare(decimal.Parse(r_value), value, test);
                            case Components.ValueType.USPhone:
                                var t_numbers = "";
                                foreach (var character in datapoint.Value)
                                {
                                    if (Char.IsDigit(character))
                                        t_numbers += character;
                                }
                                if (t_numbers.Length == 10)
                                    return Compare(t_numbers, value);
                                else
                                    return false;
                            default:
                                switch (test)
                                {
                                    case "==":
                                    case "=":
                                        return value.Equals(r_value);
                                    case "!=":
                                        return !r_value.Equals(value);
                                    case "^=":
                                        return r_value.StartsWith(value);
                                    case "$=":
                                        return r_value.EndsWith(value);
                                    case "!^=":
                                        return !r_value.StartsWith(value);
                                    case "!$=":
                                        return !r_value.EndsWith(value);
                                    case "*=":
                                    case "in":
                                        return r_value.Contains(value);
                                    case "not in":
                                    case "!*=":
                                        return !r_value.Contains(value);
                                    case ">":
                                        var gt_i_val = 0;
                                        if (int.TryParse(value, out gt_i_val))
                                            return false;
                                        return r_value.Length > gt_i_val;
                                    case ">=":
                                        var gte_i_val = 0;
                                        if (int.TryParse(value, out gte_i_val))
                                            return false;
                                        return r_value.Length >= gte_i_val;
                                    case "<":
                                        var lt_i_val = 0;
                                        if (int.TryParse(value, out lt_i_val))
                                            return false;
                                        return r_value.Length < lt_i_val;
                                    case "<=":
                                        var lte_i_val = 0;
                                        if (int.TryParse(value, out lte_i_val))
                                            return false;
                                        return r_value.Length <= lte_i_val;
                                    case "=rgx=":
                                        return Regex.IsMatch(r_value, value);
                                    case "!=rgx=":
                                        return !Regex.IsMatch(r_value, value);
                                }
                                break;
                        }
                        break;
                }
                #endregion
            }
            return false;
        }

        #region protected comparers

        protected bool CompareAudience(Guid value, string test)
        {
            if (Audience == null)
            {
                if (String.IsNullOrEmpty(test))
                    return true;
                else
                    return false;
            }
            Guid gid;
            long lid;
            if (Guid.TryParse(test, out gid))
                return Audience.UId == gid;
            if (long.TryParse(test, out lid))
                return Audience.SortingId == lid;
            return Audience.Label.ToLower() == test;            
        }

        protected bool CompareStatus(object value, string test)
        {
            RegistrationStatus status = RegistrationStatus.Incomplete;
            if (value is RegistrationStatus)
                status = (RegistrationStatus)value;
            else
            {
                var int_status = 0;
                if (!int.TryParse(value.ToString(), out int_status))
                {
                    switch (value.ToString().ToLower())
                    {
                        case "submitted":
                            status = RegistrationStatus.Submitted;
                            break;
                        case "incomplete":
                            status = RegistrationStatus.Incomplete;
                            break;
                        case "cancelled":
                            status = RegistrationStatus.Canceled;
                            break;
                        case "cancelled by company":
                            status = RegistrationStatus.CanceledByCompany;
                            break;
                        case "cancelled by administrator":
                            status = RegistrationStatus.CanceledByAdministrator;
                            break;
                        case "deleted":
                            status = RegistrationStatus.Deleted;
                            break;
                    }
                }
                else
                {
                    status = (RegistrationStatus)int_status;
                }
            }
            switch (test)
            {
                case "==":
                case "=":
                    return status == Status;
                case "!=":
                    return status != Status;
                default:
                    return false;
            }
        }

        protected bool CompareType(object value, string test)
        {
            var status = RegistrationType.Test;
            if (value is RegistrationType)
                status = (RegistrationType)value;
            else
            {
                var int_status = 0;
                if (!int.TryParse(value.ToString(), out int_status))
                    return false;
                status = (RegistrationType)int_status;
            }
            switch (test)
            {
                case "==":
                case "=":
                    return status == Type;
                case "!=":
                    return status != Type;
                default:
                    return false;
            }
        }

        protected bool CompareRsvp(object value, string test)
        {
            bool rsvp;
            if (value is bool)
                rsvp = (bool)value;
            else
            {
                switch (value.ToString().ToLower())
                {
                    case "1":
                    case "true":
                        rsvp = true;
                        break;
                    case "0":
                    case "false":
                    default:
                        rsvp = false;
                        break;
                }
            }
            switch (test)
            {
                case "==":
                case "=":
                    return RSVP == rsvp;
                case "!=":
                    return RSVP != rsvp;
                default:
                    return false;
            }
        }

        protected bool Compare(string variable, string value, string test = "==", bool caseSensitive = false)
        {
            if (variable == null)
                    return false;
            if (!caseSensitive)
            {
                variable = variable.ToLower();
                value = value.ToLower();
            }
            int compare = variable.CompareTo(value);
            switch (test)
            {
                case "==":
                case "=":
                    return variable.Equals(value);
                case "!=":
                    return !variable.Equals(value);
                case "^=":
                    return variable.StartsWith(value);
                case "$=":
                    return variable.EndsWith(value);
                case "!^=":
                    return !variable.StartsWith(value);
                case "!$=":
                    return !variable.EndsWith(value);
                case "*=":
                case "in":
                    return variable.Contains(value);
                case "!*=":
                case "not in":
                    return !variable.Contains(value);
                case ">":
                    var gt_i_val = 0m;
                    if (decimal.TryParse(value, out gt_i_val))
                        return false;
                    return variable.Length > gt_i_val;
                case ">=":
                    var gte_i_val = 0m;
                    if (decimal.TryParse(value, out gte_i_val))
                        return false;
                    return variable.Length >= gte_i_val;
                case "<":
                    var lt_i_val = 0m;
                    if (decimal.TryParse(value, out lt_i_val))
                        return false;
                    return variable.Length < lt_i_val;
                case "<=":
                    var lte_i_val = 0m;
                    if (decimal.TryParse(value, out lte_i_val))
                    return false;
                    return variable.Length <= lte_i_val;
                case "=rgx=":
                    return Regex.IsMatch(variable, value);
                case "!=rgx=":
                    return !Regex.IsMatch(variable, value);
            }
            return false;
        }

        protected bool Compare(DateTimeOffset variable, object value, string test = "==")
        {
            var val = new DateTimeOffset();
            if (value is DateTimeOffset)
                val = (DateTimeOffset)value;
            else if (!DateTimeOffset.TryParse(value.ToString(), out val))
                return false;
            var compare = variable.CompareTo(val);
            switch (test)
            {
                case "==":
                case "=":
                    return compare == 0;
                case ">":
                    return compare > 0;
                case ">=":
                    return compare >= 0;
                case "<":
                    return compare < 0;
                case "<=":
                    return compare <= 0;
                case "1=":
                    return compare != 0;
                default:
                    return false;
            }
        }

        protected bool Compare(decimal variable, object value, string test = "==")
        {
            decimal val = 0;
            if (value is decimal)
                val = (decimal)value;
            else if (!decimal.TryParse(value.ToString(), out val))
                return false;
            int compare = variable.CompareTo(val);
            switch (test)
            {
                case "==":
                case "=":
                    return compare == 0;
                case ">":
                    return compare > 0;
                case ">=":
                    return compare >= 0;
                case "<":
                    return compare < 0;
                case "<=":
                    return compare <= 0;
                case "!=":
                    return compare != 0;
                default:
                    return false;
            }
        }

        protected bool Compare(DateTime variable, object value, string test = "==")
        {
            DateTime val = DateTime.MinValue;
            if (value is DateTime)
                val = (DateTime)value;
            else if (!DateTime.TryParse(value.ToString(), out val))
                return false;
            return CompareDTO((DateTimeOffset)DateTime.SpecifyKind(variable, DateTimeKind.Utc), (object)(DateTimeOffset)DateTime.SpecifyKind(val, DateTimeKind.Utc), test);
        }

        protected bool Compare(TimeSpan variable, object value, string test = "==")
        {
            TimeSpan val = TimeSpan.MinValue;
            if (value is TimeSpan)
                val = (TimeSpan)value;
            else if (!TimeSpan.TryParse(value.ToString(), out val))
                return false;
            var compare = variable.CompareTo(val);
            switch (test)
            {
                case "==":
                case "=":
                    return compare == 0;
                case ">":
                    return compare > 0;
                case ">=":
                    return compare >= 0;
                case "<":
                    return compare < 0;
                case "<=":
                    return compare <= 0;
                case "!=":
                    return compare != 0;
                default:
                    return false;
            }
        }

        protected bool CompareDTO(DateTimeOffset variable, object value, string test = "==")
        {
            DateTimeOffset val = DateTimeOffset.MinValue;
            if (value is DateTimeOffset)
                val = (DateTimeOffset)value;
            else if (!DateTimeOffset.TryParse(value.ToString(), out val))
                return false;
            int compare = variable.CompareTo(val);
            switch (test)
            {
                case "==":
                case "=":
                    return compare == 0;
                case ">":
                    return compare > 0;
                case ">=":
                    return compare >= 0;
                case "<":
                    return compare < 0;
                case "<=":
                    return compare <= 0;
                case "!=":
                    return compare != 0;
                default:
                    return false;
            }
        }
        #endregion

        #endregion

        #region Equal Overrides

        public override bool Equals(object obj)
        {
            if (obj is Registrant)
            {
                return UId.Equals(((Registrant)obj).UId);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return UId.GetHashCode();
        }

        #endregion

        internal JsonTableValue GetJsonTableRow()
        {
            throw new NotImplementedException();
        }
    }

    public enum RegistrationType
    {
        [StringValue("Test")]
        Test = 0,
        [StringValue("Live")]
        Live = 1
    }

    public enum RegistrationStatus
    {
        [StringValue("Incomplete")]
        Incomplete = 0,
        [StringValue("Submitted")]
        Submitted = 1,
        [StringValue("Cancelled")]
        Canceled = 2,
        [StringValue("Cancelled By Company")]
        CanceledByCompany = 3,
        [StringValue("Cancelled By Administrator")]
        CanceledByAdministrator = 4,
        [StringValue("Marked For Deletion")]
        Deleted = 5
    }

    public class SetDataResult
    {
        public List<SetDataError> Errors { get; set; }
        protected string _id;
        public string _prettyValue { get; set; }
        public string PrettyValue
        {
            get
            {
                if (Data != null)
                    return Data.GetPretty();
                return _prettyValue;
            }
            set
            {
                _prettyValue = value;
            }
        }
        public string Value { get; set; }
        public string Id
        {
            get
            {
                if (Data != null)
                    return Data.SortingId.ToString();
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        public RegistrantData Data { get; set; }

        public bool Success
        {
            get
            {
                return Errors.Count == 0;
            }
        }

        public SetDataResult()
        {
            Errors = new List<SetDataError>();
            Data = null;
            _id = "";
            _prettyValue = "";
        }

    }

    public class SetDataError
    {
        public string Id { get; set; }
        public string Message { get; set; }

        public SetDataError(string id, string message)
        {
            Id = id;
            Message = message;
        }
    }

    public class Attendance
    {
        public AttendType Status { get; set; }
        public int Time { get; set; }
        public long AgendaEvent { get; set; }

        public Attendance()
        {
            Status = AttendType.Unknown;
            Time = 0;
            AgendaEvent = -1;
        }
    }

    public enum AttendType
    {
        [StringValue("Unknown")]
        Unknown = 0,
        [StringValue("Present")]
        Present = 1,
        [StringValue("Tardy")]
        Tardy = 2,
        [StringValue("No Show")]
        NoShow = 3,
        [StringValue("Cancelled")]
        Cancelled = 4
    }
}
