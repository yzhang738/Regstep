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

namespace RSToolKit.Domain.Entities
{
    [Table("OldRegistrantData")]
    public class OldRegistrantData : IFormItem, IPersonData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength]
        public string Value { get; set; }

        public Guid? VariableUId { get; set; }

        [ForeignKey("RegistrantKey")]
        public virtual OldRegistrant Registrant { get; set; }
        public Guid RegistrantKey { get; set; }

        Guid IPersonData.ReferenceKey
        {
            get
            {
                return Guid.Empty;
            }
        }

        Guid IPersonData.ParentKey
        {
            get
            {
                return RegistrantKey;
            }
        }

        IPerson IPersonData.Parent
        {
            get
            {
                return null;
            }

            set
            {
                return;
            }
        }

        public OldRegistrantData()
        {
            UId = Guid.Empty;
            Value = "";
        }

        public Form GetForm()
        {
            return Registrant.GetForm();
        }

        public INode GetNode()
        {
            return Registrant.GetNode();
        }

        public string GetPretty(IComponent component, bool html = true)
        {
            return GetFormattedValue(component);
        }

        public bool Empty(IComponent component)
        {
            if (String.IsNullOrWhiteSpace(Value))
                return true;
            if (component is Input)
            {
                var input = (Input)component;
                if (input.Type == InputType.File)
                    return true;
            }
            if (String.IsNullOrEmpty(Value))
                return true;
            if (component is IComponentMultipleSelection)
            {
                try
                {
                    var t_val = JsonConvert.DeserializeObject<List<Guid>>(Value);
                    return t_val.Count == 0;
                }
                catch (Exception)
                {
                    return true;
                }
            }
            if (component is IComponentItemParent)
            {
                Guid id;
                return !Guid.TryParse(Value.Trim(), out id);
            }
            return false;
        }


        public string GetFormattedValue(IComponent component)
        {
            if (String.IsNullOrEmpty(Value))
                return "";
            if (VariableUId == null)
                return "";
            if (component == null)
                return "";
            if (component is Input)
            {
                #region Input
                var t_comp = component as Input;
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
                    return t_datetime.ToString(Registrant.Form.Culture);
                }
                else if (t_comp.Type == InputType.Time)
                {
                    TimeSpan t_time;
                    if (!TimeSpan.TryParse(Value, out t_time))
                        return null;
                    return t_time.ToString();
                }
                else if (t_comp.Type == InputType.Date)
                {
                    DateTime date;
                    if (!DateTime.TryParse(Value, out date))
                        return null;
                    return date.ToShortDateString();
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
                #endregion
            }
            else if (component is IComponentMultipleSelection)
            {
                #region Multiple Item Selection
                var token = JToken.Parse(Value ?? "[]");
                if (!(token is JArray))
                    return "";
                var ret = "";
                var t_cbSelectedItems = JsonConvert.DeserializeObject<List<Guid>>(Value ?? "[]");
                var t_cbItems = (component as CheckboxGroup).Items;
                foreach (var t_cbUId in t_cbSelectedItems)
                {
                    var t_cbItem = t_cbItems.Where(i => i.UId == t_cbUId).FirstOrDefault();
                    if (t_cbItem == null)
                        continue;
                    ret += t_cbItem.LabelText + ", ";
                }
                if (ret.Length > 2)
                    ret = ret.Substring(0, ret.Length - 2);
                return ret;
                #endregion
            }
            else if (component is IComponentItemParent)
            {
                #region Single Item Selection
                Guid t_rgSelectedItem;
                if (!Guid.TryParse(Value, out t_rgSelectedItem))
                    return null;
                var t_rgItem = (component as IComponentItemParent).Children.FirstOrDefault(i => i.UId == t_rgSelectedItem);
                if (t_rgItem == null)
                    return null;
                return t_rgItem.LabelText;
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

        INode INodeItem.GetNode()
        {
            throw new NotImplementedException();
        }
    }
}