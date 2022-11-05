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
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using System.Globalization;
using RSToolKit.Domain;

namespace RSToolKit.Domain.Entities
{
    [Table("RegistrantData")]
    public class RegistrantData
        : ISecureMap, IComparer<RegistrantData>, IComparable<RegistrantData>, IRegistrantItem, IRequirePermissions
    {
        protected string pr_Value;


        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [ClearRelationship("VariableUId")]
        [ForeignKey("VariableUId")]
        public virtual Component Component { get; set; }

        public Guid? VariableUId { get; set; }

        public Guid ReferenceKey
        {
            get
            {
                return VariableUId.HasValue ? VariableUId.Value : Guid.Empty;
            }
        }

        [MaxLength]
        public string Value
        {
            get
            {
                if (Component is IComponentMultipleSelection)
                {
                    if (String.IsNullOrEmpty(pr_Value))
                    {
                        pr_Value = "[]";
                    }
                    else
                    {
                        try
                        {
                            JsonConvert.DeserializeObject(pr_Value);
                        }
                        catch (Exception)
                        {
                            pr_Value = "[]";
                        }
                    }
                }
                return pr_Value;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    pr_Value = null;
                else
                    pr_Value = value;
                if (Component is IComponentMultipleSelection)
                {
                    if (String.IsNullOrEmpty(pr_Value))
                    {
                        pr_Value = "[]";
                    }
                    else
                    {
                        try
                        {
                            JsonConvert.DeserializeObject(pr_Value);
                        }
                        catch (Exception)
                        {
                            pr_Value = "[]";
                        }
                    }
                }
                pr_Value = pr_Value != null ? pr_Value.Trim() : null;
            }
        }

        [ClearRelationship("RegistrantKey")]
        [ForeignKey("RegistrantKey")]
        public virtual Registrant Registrant { get; set; }
        public Guid RegistrantKey { get; set; }

        [NotMapped]
        public IPerson Parent
        {
            get
            {
                return Registrant;
            }
            set
            {
                if (value is Registrant)
                    Registrant = value as Registrant;
            }
        }
        [NotMapped]
        public Guid ParentKey
        {
            get
            {
                return RegistrantKey;
            }
        }

        [CascadeDelete]
        public virtual RegistrantFile File { get; set; }

        public RegistrantData()
        {
            UId = Guid.NewGuid();
            Value = null;
            VariableUId = Guid.Empty;
            pr_Value = null;
        }

        public static RegistrantData New(FormsRepository repository, Registrant registrant, Component component, string value = "")
        {
            var data = new RegistrantData();
            data.Value = value;
            data.Component = component;
            registrant.Data.Add(data);
            return data;
        }

        public Form GetForm()
        {
            return Registrant.Form;
        }

        public INode GetNode()
        {
            return Registrant.Form as INode;
        }

        public bool Empty()
        {
            if (String.IsNullOrWhiteSpace(pr_Value))
                return true;
            if (Component is Input)
            {
                var input = (Input)Component;
                if (input.Type == InputType.File)
                    return File == null;
            }
            if (String.IsNullOrEmpty(pr_Value))
                return true;
            if (Component is IComponentMultipleSelection)
            {
                try
                {
                    var t_val = JsonConvert.DeserializeObject<List<Guid>>(pr_Value);
                    return t_val.Count == 0;
                }
                catch (Exception)
                {
                    return true;
                }
            }
            if (Component is IComponentItemParent)
            {
                Guid id;
                return !Guid.TryParse(pr_Value.Trim(), out id);
            }
            return false;
        }

        public string GetRaw()
        {
            if (Component is IComponentMultipleSelection)
            {
                var selections = new List<long>();
                var c_selections = JsonConvert.DeserializeObject<List<Guid>>(Value);
                foreach (var t_selection in c_selections)
                {
                    var t_item = (Component as IComponentMultipleSelection).Children.FirstOrDefault(i => i.UId == t_selection);
                    if (t_item != null)
                        selections.Add(t_item.SortingId);
                }
                return JsonConvert.SerializeObject(selections);
            }
            else if (Component is IComponentItemParent)
            {
                Guid selection;
                if (Guid.TryParse(Value, out selection))
                {
                    var t_item = (Component as IComponentItemParent).Children.FirstOrDefault(i => i.UId == selection);
                    if (t_item != null)
                        return t_item.SortingId.ToString();
                }
                return Value;
            }
            else if (Component is Input && (Component as Input).Type == InputType.File)
            {
                if (File == null)
                    return "";
                return File.SortingId.ToString();
            }
            else
            {
                return Value;
            }
        }

        public string GetFormattedValue()
        {
            if (String.IsNullOrEmpty(Value))
                return "";
            if (Component is Input)
            {
                #region Input
                var t_comp = Component as Input;
                if (t_comp.Type == InputType.Default)
                {
                    if (t_comp.ValueType == Components.ValueType.Decimal)
                    {
                        decimal t_moneyValue;
                        if (decimal.TryParse(Value, out t_moneyValue))
                            return t_moneyValue.ToString(Registrant.Form.Culture);
                    }
                    else if (t_comp.ValueType == Components.ValueType.USPhone)
                    {
                        if (String.IsNullOrEmpty(Value))
                            return Value;
                        var t_numbers = Regex.Replace(Value, @"\D+", "");
                        if (t_comp.Formatting == Domain.Entities.Components.Formatting.GenericPhone)
                            t_numbers = Regex.Replace(t_numbers, @"^(\d{3})(\d{3})(\d{4})(\d*)", "($1) $2-$3 x $4");
                        else if (t_comp.Formatting == Domain.Entities.Components.Formatting.DotPhone)
                            t_numbers = Regex.Replace(t_numbers, @"^(\d{3})(\d{3})(\d{4})(\d*)", "$1.$2.$3 x $4");
                        else if (t_comp.Formatting == Domain.Entities.Components.Formatting.SpacePhone)
                            t_numbers = Regex.Replace(t_numbers, @"^(\d{3})(\d{3})(\d{4}(\d*))", "$1 $2 $3 x $4");
                        t_numbers = t_numbers.Trim();
                        if (t_numbers.EndsWith("x"))
                            t_numbers = t_numbers.Substring(0, t_numbers.Length - 2);
                        return t_numbers;
                    }
                    if (t_comp.Formatting == Components.Formatting.Caps)
                        return Value.ToUpper();
                    else if (t_comp.Formatting == Components.Formatting.Lowercase)
                        return Value.ToLower();
                    else if (t_comp.Formatting == Components.Formatting.SentenceCase)
                    {
                        var lowerCase = Value.ToLower();
                        // matches the first sentence of a string, as well as subsequent sentences
                        var r = new Regex(@"(^[a-z])|\.\s+(.)", RegexOptions.ExplicitCapture);
                        // MatchEvaluator delegate defines replacement of setence starts to uppercase
                        return r.Replace(lowerCase, s => s.Value.ToUpper());
                    }
                    else if (t_comp.Formatting == Components.Formatting.TitleCase)
                        return Value.ToTitleCase();
                    return Value;
                }
                else if (t_comp.Type == InputType.DateTime)
                {
                    DateTimeOffset t_datetime;
                    if (!DateTimeOffset.TryParse(Value, out t_datetime))
                        return null;
                    return t_datetime.ToString("yyyy-M-d h:mm tt");
                }
                else if (t_comp.Type == InputType.Time)
                {
                    TimeSpan t_time;
                    if (!TimeSpan.TryParse(Value, out t_time))
                        return null;
                    return t_time.ToString("h:mm tt");
                }
                else if (t_comp.Type == InputType.Date)
                {
                    DateTime date;
                    if (!DateTime.TryParse(Value, out date))
                        return null;
                    return date.ToString("yyyy-M-d");
                }
                else if (t_comp.Type == InputType.SSN)
                {
                    var ssn_ret = Value.Replace("-", "").Trim();
                    if (ssn_ret.Length != 9)
                        return "Format Error";
                    return ssn_ret.Insert(5, "-").Insert(3, "-");
                }
                else if (t_comp.Type == InputType.UniversalCreditCard)
                {
                    var cc_id = Guid.Empty;
                    if (Guid.TryParse(Value, out cc_id))
                    {
                        using (var repository = new FormsRepository())
                        {
                            var cc_obj = repository.SecurePeek<ISecure>(c => c.UId == cc_id).FirstOrDefault();
                            if (cc_obj == null)
                                return "No Data";
                            return cc_obj;
                        }
                    }
                    return "No Data";
                }
                else if (t_comp.Type == InputType.File)
                {
                    if (File != null)
                        return "UPLOADED";
                }
                #endregion
            }
            else if (Component is IComponentMultipleSelection)
            {
                #region Multiple Item Selection
                var ret = "";
                List<Guid> t_cbSelectedItems = null;
                try
                {
                    t_cbSelectedItems = JsonConvert.DeserializeObject<List<Guid>>(Value ?? "[]");
                }
                catch (Exception)
                {
                    t_cbSelectedItems = new List<Guid>();
                }
                var t_cbItems = (Component as CheckboxGroup).Items;
                foreach (var t_cbUId in t_cbSelectedItems)
                {
                    var t_cbItem = t_cbItems.Where(i => i.UId == t_cbUId).FirstOrDefault();
                    if (t_cbItem == null)
                        continue;
                    ret += t_cbItem.LabelText + ", ";
                }
                if (ret.Length > 2)
                    ret = ret.Substring(0, ret.Length - 2);
                // Now we need to check for waitlistings.
                ret += this._AddWaitlistings();
                return ret;
                #endregion
            }
            else if (Component is IComponentItemParent)
            {
                #region Single Item Selection
                if (Empty())
                    return Value;
                Guid t_rgSelectedItem;
                if (!Guid.TryParse(Value, out t_rgSelectedItem))
                    return null;
                var t_rgItem = (Component as IComponentItemParent).Children.FirstOrDefault(i => i.UId == t_rgSelectedItem);
                if (t_rgItem == null)
                    return null;
                return t_rgItem.LabelText + this._AddWaitlistings();
                #endregion
            }
            return Value;
        }
        
        public T GetTypedValue<T>()
        {
            if (typeof(T) == typeof(int))
            {
                int ret = 0;
                if (!int.TryParse(Value, out ret))
                    return default(T);
                return (T)(object)ret;
            }
            else if (typeof(T) == typeof(long))
            {
                long ret = 0;
                if (!long.TryParse(Value, out ret))
                    return default(T);
                return (T)(object)ret;
            }
            else if (typeof(T) == typeof(float))
            {
                float ret = 0;
                if (!float.TryParse(Value, out ret))
                    return default(T);
                return (T)(object)ret;
            }
            else if (typeof(T) == typeof(double))
            {
                double ret = 0;
                if (!double.TryParse(Value, out ret))
                    return default(T);
                return (T)(object)ret;
            }
            else if (typeof(T) == typeof(decimal))
            {
                decimal ret = 0;
                if (!decimal.TryParse(Value, out ret))
                    return default(T);
                return (T)(object)ret;
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                DateTimeOffset ret = DateTimeOffset.MinValue;
                if (!DateTimeOffset.TryParse(Value, out ret))
                    return default(T);
                return (T)(object)ret;
            }
            else if (typeof(T) == typeof(DateTime))
            {
                DateTime ret = DateTime.MinValue;
                if (!DateTime.TryParse(Value, out ret))
                    return default(T);
                return (T)(object)ret;
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)Value;
            }
            else
            {
                return default(T);
            }
        }

        protected string _AddWaitlistings()
        {
            using (var context = new EFDbContext())
            {
                var seatings = new List<Seater>();
                seatings.AddRange(context.Seaters.Where(s => s.ComponentKey == VariableUId && s.RegistrantKey == RegistrantKey && !s.Seated));
                if (Component is IComponentItemParent)
                {
                    foreach (var item in (Component as IComponentItemParent).Children)
                    {
                        seatings.AddRange(context.Seaters.Where(s => s.ComponentKey == item.UId && s.RegistrantKey == RegistrantKey && !s.Seated));
                    }
                }
                seatings = seatings.Distinct().ToList();
                if (seatings.Count == 0)
                    return "";
                var ret = "<br />Waitlistings:";
                foreach (var seat in seatings)
                {
                    ret += "<br /> " + seat.Component.LabelText;
                }
                return ret;
            }
        }

        public string GetPretty(bool html = true)
        {
            return GetFormattedValue();
        }

        public int Compare(RegistrantData x, RegistrantData y)
        {
            if (x.VariableUId != y.VariableUId)
                return 1;
            if (x.Component is CheckboxGroup)
            {
                var x_json = JsonConvert.DeserializeObject<List<Guid>>(x.Value);
                var y_json = JsonConvert.DeserializeObject<List<Guid>>(x.Value);
                return x_json.Count.CompareTo(y_json.Count);
            }
            else if (x.Component is Input)
            {
                var x_inp = (Input)x.Component;
                var y_inp = (Input)y.Component;
                if (x_inp.Type == InputType.UniversalCreditCard)
                {
                    Guid x_id;
                    Guid y_id;
                    if (Guid.TryParse(x.Value, out x_id) && Guid.TryParse(y.Value, out y_id))
                    {
                        using (var repository = new FormsRepository())
                        {
                            var x_cc = repository.Search<RSToolKit.Domain.Entities.MerchantAccount.CreditCard>(c => c.UId == x_id, silent: true).FirstOrDefault();
                            var y_cc = repository.Search<RSToolKit.Domain.Entities.MerchantAccount.CreditCard>(c => c.UId == y_id, silent: true).FirstOrDefault();
                            return x_cc.Number.CompareTo(y_cc.Number);
                        }
                    }
                }
                else if (y_inp.Type == InputType.Date || y_inp.Type == InputType.DateTime || y_inp.Type == InputType.Time)
                {
                    DateTimeOffset x_Date;
                    DateTimeOffset y_Date;
                    if (DateTimeOffset.TryParse(x.Value, out x_Date) && DateTimeOffset.TryParse(y.Value, out y_Date))
                    {
                        return x_Date.CompareTo(y_Date);
                    }
                }
            }
            return x.Value.CompareTo(y.Value);
        }

        public int CompareTo(RegistrantData other)
        {
            if (other == null)
                return -1;
            if (Value == null && other.Value == null)
                return 0;
            else if (Value == null && other.Value != null)
                return 1;
            else if (Value != null && other.Value == null)
                return -1;
            var x = this;
            var y = other;
            if (x.VariableUId != y.VariableUId)
                return 1;
            if (x.Component is CheckboxGroup)
            {
                var x_json = JsonConvert.DeserializeObject<List<Guid>>(x.Value);
                var y_json = JsonConvert.DeserializeObject<List<Guid>>(x.Value);
                return x_json.Count.CompareTo(y_json.Count);
            }
            else if (x.Component is Input)
            {
                var x_inp = (Input)x.Component;
                var y_inp = (Input)y.Component;
                if (x_inp.Type == InputType.UniversalCreditCard)
                {
                    Guid x_id;
                    Guid y_id;
                    if (Guid.TryParse(x.Value, out x_id) && Guid.TryParse(y.Value, out y_id))
                    {
                        using (var repository = new FormsRepository())
                        {
                            var x_cc = repository.Search<RSToolKit.Domain.Entities.MerchantAccount.CreditCard>(c => c.UId == x_id, silent: true).FirstOrDefault();
                            var y_cc = repository.Search<RSToolKit.Domain.Entities.MerchantAccount.CreditCard>(c => c.UId == y_id, silent: true).FirstOrDefault();
                            return x_cc.Number.CompareTo(y_cc.Number);
                        }
                    }
                }
                else if (y_inp.Type == InputType.Date || y_inp.Type == InputType.DateTime || y_inp.Type == InputType.Time)
                {
                    DateTimeOffset x_Date;
                    DateTimeOffset y_Date;
                    if (DateTimeOffset.TryParse(x.Value, out x_Date) && DateTimeOffset.TryParse(y.Value, out y_Date))
                    {
                        return x_Date.CompareTo(y_Date);
                    }
                }
            }
            return x.Value.CompareTo(y.Value);
        }

        public IPermissionHolder GetPermissionHolder()
        {
            return Registrant.Form;
        }
    }
}