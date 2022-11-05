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
using RSToolKit.Domain.Entities.Clients;
using Newtonsoft.Json;
using RSToolKit.Domain.JItems;

// Complete
namespace RSToolKit.Domain.Entities
{
    /// <summary>
    /// Holds information for a contact.
    /// </summary>
    public class Contact
        : ISecureHolder, IComparable<Contact>
    {
        /// <summary>
        /// The email.
        /// </summary>
        protected string _email;

        /// <summary>
        /// The long id of the contact.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        /// <summary>
        /// The unique identifier for the contact.
        /// </summary>
        [Key]
        public Guid UId { get; set; }
        /// <summary>
        /// The company the contact belongs to.
        /// </summary>
        [JsonIgnore]
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        /// <summary>
        /// The key of the owning company.
        /// </summary>
        public Guid CompanyKey { get; set; }
        /// <summary>
        /// The registrants the contact is assigned to.
        /// </summary>
        public virtual List<Registrant> Registrants { get; set; }
        /// <summary>
        /// The Email of the contact.
        /// </summary>
        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name
        {
            get
            {
                return this._email;
            }
            set
            {
                this._email = value.ToLower();
            }
        }
        /// <summary>
        /// The description of the contact.
        /// </summary>
        [MaxLength(5000)]
        public string Description { get; set; }
        /// <summary>
        /// The date of creation.
        /// </summary>
        public DateTimeOffset DateCreated { get; set; }
        /// <summary>
        /// The date of last modification.
        /// </summary>
        public DateTimeOffset DateModified { get; set; }
        /// <summary>
        /// The token of last modification.
        /// </summary>
        public Guid ModificationToken { get; set; }
        /// <summary>
        /// The last modifier.
        /// </summary>
        public Guid ModifiedBy { get; set; }
        /// <summary>
        /// The data of the contact.
        /// </summary>
        public virtual List<ContactData> Data { get; set; }
        /// <summary>
        /// The list this contact is saved in.
        /// </summary>
        [JsonIgnore]
        public virtual List<SavedList> SavedLists { get; set; }
        /// <summary>
        /// The email sends this contact has accumulated.
        /// </summary>
        [JsonIgnore]
        public virtual List<EmailSend> EmailSends { get; set; }
        /// <summary>
        /// The email of the contact. Note, this item is not mapped and return Name so it cannot be used in linq to sql.
        /// <code>
        /// get
        /// {
        ///     return Name;
        /// }
        /// set
        /// {
        ///     Name = value;
        /// }
        /// </code>
        /// </summary>
        [NotMapped]
        public string Email
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        #region interface properties for IPerson
        IPersonHolder IPerson.Holder
        {
            get
            {
                return Company;
            }
            set
            {
                if (value is Company)
                    Company = value as Company;
            }
        }
        Guid IPerson.HolderKey
        {
            get
            {
                return CompanyKey;
            }
        }
        IEnumerable<IPersonData> IPerson.IData
        {
            get
            {
                return Data.AsEnumerable<IPersonData>();
            }
        }
        #endregion

        /// <summary>
        /// Creates a default contact.
        /// </summary>
        public Contact()
        {
            Data = new List<ContactData>();
            EmailSends = new List<EmailSend>();
            SavedLists = new List<SavedList>();
            this._email = "";
            Description = "";
            Registrants = new List<Registrant>();
            DateCreated = DateModified = DateTimeOffset.Now;
        }

        /// <summary>
        /// Creates a new contact and adds it to the repository.
        /// </summary>
        /// <param name="repository">The repository being used.</param>
        /// <param name="company">The company the contact is assigned to.</param>
        /// <param name="user">The user creating the contact.</param>
        /// <param name="owner">The owner UId.</param>
        /// <param name="group">The group UId.</param>
        /// <param name="permission">The permission to use.</param>
        /// <param name="email">The email to assign to the contact.</param>
        /// <param name="description">The description of the contact.</param>
        /// <returns>The contact that is added to the database.</returns>
        [Obsolete]
        public static Contact New(FormsRepository repository, Company company, User user, Guid? owner = null, Guid? group = null, string permission = "770", string email = "", string description = "")
        {
            var contact = new Contact()
            {
                UId = Guid.NewGuid(),
                Company = company,
                CompanyKey = company.UId,
                DateCreated = DateTimeOffset.UtcNow,
                DateModified = DateTimeOffset.UtcNow,
                Description = description,
                Email = email
            };
            repository.Add(contact);
            repository.Commit();
            return contact;
        }

        /// <summary>
        /// Gets all the emails assigned to this contact in lowercase format.
        /// </summary>
        /// <returns>An enumerable collection of emails in lowercase format.</returns>
        public IEnumerable<string> GetEmails()
        {
            var t_list = new List<string>() { Email.ToLower() };
            t_list.AddRange(Data.Where(d => d.Header.Descriminator == ContactDataType.Email && d.Value != null).Select(d => d.Value.ToLower()));
            return t_list.AsEnumerable();
        }

        /// <summary>
        /// Retrieves all secured items.  This method creates an access log entry for each secured item.
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

        #region interface mapping for IPerson
        /// <summary>
        /// Finds the data based on the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The data.</returns>
        IPersonData IPerson.FindData(string key)
        {
            Guid id;
            if (Guid.TryParse(key, out id))
                return Data.FirstOrDefault(d => d.HeaderKey == id);
            else
                return Data.FirstOrDefault(d => d.Header.Name == key);
        }
        #endregion

        /// <summary>
        /// Sets the data for the contact header.
        /// </summary>
        /// <param name="key">The key of the data setting.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="ignoreValidation">True to ignore validation, false is default.</param>
        /// <param name="ignoreCapacity">NOT USED</param>
        /// <param name="ignoreRequired">True to ignore required, false is default.</param>
        /// <param name="resetValueOnError">False to keep value even on error. true is default.</param>
        /// <returns>A set data result of the success.</returns>
        public SetDataResult SetData(string key, string value, bool ignoreValidation = false, bool ignoreCapacity = false, bool ignoreRequired = false, bool resetValueOnError = true)
        {
            var result = new SetDataResult();
            switch (key.ToLower())
            {
                case "email":
                    Email = value;
                    return result;
            }
            Guid t_key;
            if (Guid.TryParse(key, out t_key))
            {
                var dp = Data.FirstOrDefault(d => d.Header != null && d.Header.UId == t_key);
                if (dp == null)
                {
                    var header = Company.ContactHeaders.FirstOrDefault(c => c.UId == t_key);
                    if (header == null)
                    {
                        result.Errors.Add(new SetDataError("", "The header does not exist."));
                        return result;
                    }
                    Data.Add(new ContactData() { Header = header, Value = value });
                    return result;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the JsonTableValue based on the supplied JsonTableHeader.
        /// </summary>
        /// <param name="header">The header the value is associated with.</param>
        /// <returns>The value.</returns>
        public JsonTableValue GetJsonValue(JsonTableHeader header)
        {
            if (header.Id == "Email")
            {
                return new JsonTableValue()
                {
                    Header = header,
                    HeaderId = header.Id,
                    Editable = true,
                    Id = "Email-" + SortingId,
                    Value = Email,
                    RawData = Email,
                    Type = header.Type
                };
            }
            var data = Data.FirstOrDefault(d => d.HeaderKey == header.Token);
            var value = new JsonTableValue();
            value.Header = header;
            value.HeaderId = header.Id;
            value.Editable = true;
            value.Id = "";
            value.Value = "";
            value.RawData = "";
            value.Type = header.Type;
            if (data != null)
            {
                value.Value = data.PrettyValue;
                value.RawData = data.Value;
                value.Id = data.SortingId.ToString();
            }
            return value;
        }

        #region Compare
        public bool CompareData(string key, string value, string test, ref List<Guid> matchedContacts, bool caseSensitive = false)
        {
            switch (key.ToLower())
            {
                case "email":
                    return Compare(Email, value, test);
                case "datecreated":
                    return Compare(DateCreated, value, test);
                case "description":
                    return Compare(Description, value, test);
            }
            Guid h_key;
            if (!Guid.TryParse(key, out h_key))
                return false;
            var data = Data.Where(d => d.HeaderKey == h_key).FirstOrDefault();
            if (data == null)
                return false;
            var desc = data.Header.Descriminator;
            switch (desc)
            {
                case ContactDataType.Integer:
                    return CompareInt(data.GetTypedValue<int>(), (object)value, test);
                case ContactDataType.Number:
                    return Compare(data.GetTypedValue<float>(), (object)value, test);
                case ContactDataType.Money:
                    return Compare(data.GetTypedValue<decimal>(), (object)value, test);
                case ContactDataType.DateTime:
                    return CompareDTO(data.GetTypedValue<DateTimeOffset>(), (object)value, test);
                case ContactDataType.Time:
                    return Compare(data.GetTypedValue<TimeSpan>(), (object)value, test);
                default:
                    return Compare(data.GetTypedValue<String>(), value, test);
            }

        }

        public bool Compare(string variable, string value, string test = "==")
        {
            int compare = variable.CompareTo(value);
            var numb = 0;
            var isNumb = Int32.TryParse(value, out numb);
            switch (test)
            {
                case "==":
                    return compare == 0;
                case ">":
                    if (!isNumb)
                        return false;
                    return variable.Length > numb;
                case ">=":
                    if (!isNumb)
                        return false;
                    return variable.Length >= numb;
                case "<":
                    if (!isNumb)
                        return false;
                    return variable.Length < numb;
                case "<=":
                    if (!isNumb)
                        return false;
                    return variable.Length <= numb;
                case "!=":
                    return variable.Equals(value);
                case "^=":
                    return variable.StartsWith(value);
                case "!^=":
                    return !variable.StartsWith(value);
                case "$=":
                    return variable.EndsWith(value);
                case "!$=":
                    return variable.EndsWith(value);
                case "*=":
                case "in":
                    return variable.Contains(value);
                case "!*=":
                case "not in":
                    return !variable.Contains(value);
                default:
                    return false;
            }
        }

        public bool Compare(DateTimeOffset variable, object value, string test = "==")
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

        public bool CompareInt(int variable, object val, string test = "==")
        {
            int value = 0;
            if (val is int)
                value = (int)val;
            else if (!int.TryParse(val.ToString(), out value))
                return false;
            return CompareNumber((double)variable, (double)value, test);
        }

        public bool CompareLong(long variable, object value, string test = "==")
        {
            long val = 0;
            if (value is long)
                val = (long)value;
            else if (!long.TryParse(value.ToString(), out val))
                return false;
            int compare = variable.CompareTo(val);
            switch (test)
            {
                case "==":
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

        public bool Compare(double variable, object value, string test = "==")
        {
            double val = 0;
            if (value is double)
                val = (double)value;
            else if (!double.TryParse(value.ToString(), out val))
                return false;
            return CompareNumber(variable, val, test);
        }

        public bool Compare(float variable, object value, string test = "==")
        {
            float val = 0;
            if (value is float)
                val = (float)value;
            else if (!float.TryParse(value.ToString(), out val))
                return false;
            return CompareNumber((double)variable, (double)val, test);
        }

        public bool Compare(decimal variable, object value, string test = "==")
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

        public bool Compare(DateTime variable, object value, string test = "==")
        {
            DateTime val = DateTime.MinValue;
            if (value is DateTime)
                val = (DateTime)value;
            else if (!DateTime.TryParse(value.ToString(), out val))
                return false;
            return CompareDTO((DateTimeOffset)DateTime.SpecifyKind(variable, DateTimeKind.Utc), (object)(DateTimeOffset)DateTime.SpecifyKind(val, DateTimeKind.Utc), test);
        }

        public bool Compare(TimeSpan variable, object value, string test = "==")
        {
            var val = TimeSpan.MinValue;
            DateTime time;
            if (value is TimeSpan)
                val = (TimeSpan)value;
            else
            {
                if (DateTime.TryParseExact(value.ToString(), "h:mm tt", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out time))
                {
                    val = time.TimeOfDay;
                }
                else
                {
                    return false;
                }
            }
            int compare = variable.CompareTo(val);
            switch (test)
            {
                case "==":
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

        public bool CompareDTO(DateTimeOffset variable, object value, string test = "==")
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

        protected bool CompareNumber(double variable, double value, string test = "==")
        {
            int compare = variable.CompareTo(value);
            switch (test)
            {
                case "==":
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

        public int CompareTo(Contact other)
        {
            return UId.CompareTo(other.UId);
        }

        public override int GetHashCode()
        {
            return UId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Contact)
                return UId.Equals(((Contact)obj).UId);
            return false;
        }
        #endregion

    }
}
