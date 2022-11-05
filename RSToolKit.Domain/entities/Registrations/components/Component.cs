using System;
using System.Collections.Generic;
using System.Linq;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Entities.Components
{
    public class Component
        : IComponent, IComparable<Component>
    {

        protected string pr_name;

        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [JsonIgnore]
        [CascadeDelete]
        public virtual List<RegistrantData> RegistrantDatas { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name
        {
            get
            {
                if (Variable != null)
                {
                    Guid t_id;
                    if (!Guid.TryParse(Variable.Value, out t_id))
                    {
                        return Variable.Value;
                    }
                    else
                    {
                        return LabelText;
                    }
                }
                else if (!String.IsNullOrWhiteSpace(LabelText))
                {
                    return LabelText;
                }
                return pr_name;
            }
            set
            {
                pr_name = value;
            }
        }

        [JsonIgnore]
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        [JsonIgnore]
        [ForeignKey("SeatingKey")]
        public virtual Seating Seating { get; set; }
        public Guid? SeatingKey { get; set; }

        [JsonIgnore]
        [CascadeDelete]
        public virtual Variable Variable { get; set; }

        [JsonIgnore]
        [ForeignKey("PanelKey")]
        public virtual Panel Panel { get; set; }
        public Guid? PanelKey { get; set; }

        [JsonIgnore]
        [CascadeDelete]
        public virtual PriceGroup PriceGroup { get; set; }

        [JsonIgnore]
        [CascadeDelete]
        public virtual List<ComponentStyle> Styles { get; set; }
        [JsonIgnore]
        [CascadeDelete]
        public virtual List<Logic> Logics { get; set; }
        [JsonIgnore]
        public virtual List<Audience> Audiences { get; set; }
        [JsonIgnore]
        [CascadeDelete]
        public virtual List<Seater> Seaters { get; set; }

        [MaxLength(500)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }
        public DateTimeOffset AgendaStart { get; set; }
        public DateTimeOffset AgendaEnd { get; set; }
        public bool AgendaItem { get; set; }
        public DateTimeOffset Open { get; set; }
        public DateTimeOffset Close { get; set; }
        public RSVPType RSVP { get; set; }
        public bool AdminOnly { get; set; }
        public bool Display { get; set; }
        public bool Enabled { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AltText { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LabelText { get; set; }

        public int Row { get; set; }
        public int Order { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public bool DisplayAgendaDate { get; set; }
        public bool Required { get; set; }
        public bool Locked { get; set; }
        public string DisplayOrder
        {
            get
            {
                return JsonConvert.SerializeObject(DisplayComponentOrder);
            }
            set
            {
                DisplayComponentOrder = JsonConvert.DeserializeObject<ComponentOrder>(value);
            }
        }

        [ForeignKey("MappedToKey")]
        [JsonIgnore]
        public virtual ContactHeader MappedTo { get; set; }
        public Guid? MappedToKey { get; set; }

        [NotMapped]
        public ComponentOrder DisplayComponentOrder { get; set; }

        [NotMapped]
        public string SpecialDescriminator
        {
            get
            {
                if (this is Input)
                {
                    if (((Input)this).Type == InputType.File)
                    {
                        return "file";
                    }
                }
                return GetDescriminator(this);
            }
        }

        [NotMapped]
        public List<JLogic> JLogics { get; set; }
        public string RawJLogics
        {
            get
            {
                return JsonConvert.SerializeObject(JLogics);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    JLogics = new List<JLogic>();
                }
                else
                {
                    try
                    {
                        JLogics = JsonConvert.DeserializeObject<List<JLogic>>(value);
                    }
                    catch (Exception)
                    {
                        JLogics = new List<JLogic>();
                    }
                }
            }
        }

        public Component()
        {
            SortingId = 0;
            pr_name = "New Component";
            DateCreated = DateTimeOffset.UtcNow;
            DateModified = DateTimeOffset.UtcNow;
            ModifiedBy = Guid.Empty;
            ModificationToken = Guid.Empty;
            Description = "";
            AgendaStart = DateTimeOffset.UtcNow;
            AgendaEnd = AgendaStart.AddMinutes(30);
            Open = DateTimeOffset.UtcNow;
            Close = DateTimeOffset.UtcNow.AddHours(24);
            RSVP = RSVPType.None;
            Styles = new List<ComponentStyle>();
            AdminOnly = false;
            Display = true;
            Enabled = true;
            AltText = "";
            LabelText = "";
            Row = -1;
            Order = -1;
            DisplayComponentOrder = new ComponentOrder()
            {
                Items = new List<ComponentItem>()
                {
                    new ComponentItem()
                    {
                        Item = "Label",
                        Order = 1,
                        Class = "form-component-label"
                    },
                    new ComponentItem()
                    {
                        Item = "Description",
                        Order = 2,
                        Class = "form-component-description"
                    }
                }
            };
            DisplayAgendaDate = false;
            Audiences = new List<Audience>();
            Styles = new List<ComponentStyle>();
            Logics = new List<Logic>();
            Locked = false;
            JLogics = new List<JLogic>();
            AgendaItem = false;
        }

        public Form GetForm()
        {
            if (this is IComponentItem)
                return (this as IComponentItem).Parent.Panel.Page.Form;
            return Panel.Page.Form;
        }

        public INode GetNode()
        {
            return GetForm() as INode;
        }

        #region Logic Manipulation

        public void AddItem(Logic item)
        {
            item.UId = UId;
            if (Logics.Count > 0)
                item.Order = Logics.Last().Order + 1;
            Logics.Add(item);
            CollapseLogicOrder();
        }

        public void AddRange(IEnumerable<Logic> items)
        {
            if (Logics.Count > 0)
            {
                var lOrder = items.Last().Order;
                foreach (var i in items)
                {
                    i.UId = UId;
                    i.Order = ++lOrder;
                }
            }
            Logics.AddRange(items);
            CollapseLogicOrder();
        }

        public void RemoveRange(IEnumerable<Logic> items)
        {
            foreach (var i in items)
            {
                var index = -1;
                var item = Logics.FirstOrDefault(itm => itm.UId == i.UId);
                index = item == null ? -1 : Logics.IndexOf(item);
                if (index != -1)
                    Logics.RemoveAt(index);
            }
            CollapseLogicOrder();
        }

        public void Remove(Logic item)
        {
            var index = -1;
            var itm = Logics.FirstOrDefault(i => i.UId == i.UId);
            index = itm == null ? -1 : Logics.IndexOf(itm);
            if (index != -1)
                Logics.RemoveAt(index);
            CollapseLogicOrder();
        }

        public void CollapseLogicOrder()
        {
            Logics.Sort((i1, i2) => i1.Order - i2.Order);
            var prevOrder = 0;
            foreach (var i in Logics)
            {
                var cascadeValue = 0;
                if (i.Order <= prevOrder || i.Order > (prevOrder + 1))
                    cascadeValue = prevOrder - i.Order + 1;
                i.Order += cascadeValue;
                prevOrder = i.Order;
            }
        }

        #endregion

        public static string GetDescriminator(Component component)
        {
            if (component is Input)
            {
                var t_inp = component as Input;
                switch (t_inp.Type)
                {
                    case InputType.Date:
                        return "date";
                    case InputType.DateTime:
                        return "datetime";
                    case InputType.SSN:
                        return "ssn";
                    case InputType.Time:
                        return "time";
                    case InputType.UniversalCreditCard:
                        return "creditcard";
                    case InputType.Default:
                    case InputType.Multiline:
                        switch (t_inp.ValueType)
                        {
                            case ValueType.Number:
                                return "number";
                            case ValueType.Decimal:
                                return "decimal";
                            default:
                                return "text";
                        }
                }
            }
            if (component is CheckboxGroup)
                return "checkboxgroup";
            if (component is RadioGroup)
                return "radiogroup";
            if (component is DropdownGroup)
                return "dropdowngroup";
            return "unknown";
        }

        public int CompareTo(Component other)
        {
            int rowDif = Row - other.Row;
            if (rowDif != 0)
                return rowDif;
            return Order - other.Order;
        }

        public virtual IComponent DeepCopy(Panel panel, IEnumerable<Audience> audiences = null, IEnumerable<Seating> seatings = null)
        {
            return new Component();
        }

        public virtual IEnumerable<Logic> DeepCopyLogics(Form form, Form oldForm)
        {
            var logics = new List<Logic>();
            foreach (var logic in Logics)
                logics.Add(logic.DeepCopy(this, form, oldForm));
            return logics;
        }

        protected void DeepCopyStuff(IComponent comp, Panel panel, IEnumerable<Audience> audiences, IEnumerable<Seating> seatings)
        {
            audiences = audiences ?? new List<Audience>();
            seatings = seatings ?? new List<Seating>();
            comp.UId = Guid.NewGuid();
            comp.AdminOnly = AdminOnly;
            comp.AgendaEnd = AgendaEnd;
            comp.AgendaStart = AgendaStart;
            comp.AltText = AltText;
            comp.Close = Close;
            comp.Company = Company;
            comp.CompanyKey = CompanyKey;
            comp.DateCreated = DateTimeOffset.UtcNow;
            comp.DateModified = DateCreated;
            comp.Description = Description;
            comp.Display = Display;
            comp.DisplayAgendaDate = DisplayAgendaDate;
            comp.DisplayOrder = DisplayOrder;
            comp.LabelText = LabelText;
            comp.Locked = Locked;
            comp.MappedTo = MappedTo;
            comp.MappedToKey = MappedToKey;
            comp.Name = Name;
            comp.Open = Open;
            comp.Order = Order;
            comp.Required = Required;
            comp.Row = Row;
            comp.RSVP = RSVP;
            panel.Components.Add(comp as Component);
            comp.Panel = panel;
            if (comp is IVariableHolder)
            {
                comp.Variable = new Variable();
                comp.Variable.Component = comp as Component;
                comp.Variable.Value = Variable.Value;
                comp.Variable.Form = panel.Page.Form;
                panel.Page.Form.Variables.Add(comp.Variable);
            }
            // Pricegroup
            if (PriceGroup != null)
            {
                var pg = new PriceGroup();
                comp.PriceGroup = pg;
            }
            var t_seat = seatings.FirstOrDefault(s => s.Name == Seating.Name);
            if (t_seat != null)
            {
                comp.Seating = t_seat;
                comp.SeatingKey = t_seat.UId;
            }
            foreach (var audience in Audiences)
            {
                var n_aud = audiences.FirstOrDefault(a => a.Label == audience.Label);
                if (n_aud != null)
                    comp.Audiences.Add(n_aud);
            }
        }

        public string GetVariable()
        {
            if (Variable != null)
                return Variable.Value;
            return LabelText;
        }
    }

    public class ComponentOrder
    {
        public List<ComponentItem> Items { get; set; }
        
        public ComponentOrder()
        {
            Items = new List<ComponentItem>();
        }
    }

    public class ComponentItem : IComparable<ComponentItem>
    {
        public string Item { get; set; }
        public int Order { get; set; }
        public string Class { get; set; }

        public int CompareTo(ComponentItem other)
        {
            return Order - other.Order;
        }
    }
}
