using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;
using System.Text.RegularExpressions;
using RSToolKit.Domain.JItems;
using Newtonsoft.Json;

namespace RSToolKit.Domain.Entities
{
    public class OldRegistrant : IFormItem
    {
        [CascadeDelete]
        public virtual List<OldRegistrantData> Data { get; set; }

        [ForeignKey("AudienceKey")]
        public virtual Audience Audience { get; set; }
        public Guid? AudienceKey { get; set; }

        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid FormKey { get; set; }

        [ForeignKey("CurrentRegistrantKey")]
        public virtual Registrant CurrentRegistration { get; set; }
        public Guid? CurrentRegistrantKey { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(250)]
        [Required]
        public string Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string Confirmation { get; set; }

        public bool RSVP { get; set; }

        public RegistrationStatus Status { get; set; }
        public RegistrationType Type { get; set; }

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

        public OldRegistrant()
        {
            RSVP = false;
            Email = "";
            Data = new List<OldRegistrantData>();
        }

        public string GetPretty(JsonTableHeader header, IComponent component, bool html = true)
        {
            long lid;
            if (long.TryParse(header.Id, out lid))
            {
                // We need to get a datapoint.
                var data = Data.FirstOrDefault(h => h.SortingId == lid);
                if (data == null)
                    return "";
                else
                    return data.GetPretty(component);
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
                    case "balance":
                    case "tax":
                    case "fees":
                    case "adjustemnts":
                    case "transactions":
                    case "payment method":
                    case "credit last four":
                        return "Not Tracked";
                }
            }
            return "";
        }


        public JsonTableValue GetJsonTableValue(JsonTableHeader header, EFDbContext context)
        {
            var value = "";
            var rawValue = "";
            long lid;
            var id = header.Id;
            if (long.TryParse(header.Id, out lid))
            {
                // We need to get a datapoint.
                var component = context.Components.FirstOrDefault(c => c.SortingId == lid);
                if (component == null)
                    return new JsonTableValue() { Header = header, HeaderId = header.Id, Value = "" };
                var data = Data.FirstOrDefault(h => h.VariableUId == component.UId);
                if (data != null)
                {
                    value = data.GetPretty(component);
                    rawValue = data.Value;
                    if (component is IComponentMultipleSelection && !data.Empty(component))
                    {
                        var l_raw = new List<long>();
                        JsonConvert.DeserializeObject<List<Guid>>(data.Value).ForEach(itemId =>
                        {
                            var item = ((IComponentItemParent)component).Children.FirstOrDefault(c => c.UId == itemId);
                            if (item != null)
                                l_raw.Add(item.SortingId);
                        });
                        rawValue = JsonConvert.SerializeObject(l_raw);
                    }
                    else if (component is IComponentItemParent && !data.Empty(component))
                    {
                        Guid s_id;
                        if (Guid.TryParse(data.Value, out s_id))
                        {
                            var item = ((IComponentItemParent)component).Children.FirstOrDefault(c => c.UId == s_id);
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
                        rawValue = Audience != null ? Audience.UId.ToString() : null;
                        break;
                    case "date registered":
                        value = DateCreated.LocalDateTime.ToString();
                        break;
                    case "last edit":
                        value = DateModified.LocalDateTime.ToString();
                        break;
                    case "edited by":
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
                        break;
                    case "in contact list":
                        var allEmails = Form.Company.Contacts.SelectMany(c => c.GetEmails());
                        value = allEmails.Contains(Email) ? "Yes" : "No";
                        rawValue = value == "Yes" ? "true" : "false";
                        break;
                    case "promotions":
                    case "balance":
                    case "tax":
                    case "fees":
                    case "adjustments":
                    case "transactions":
                    case "payment method":
                    case "credit last four":
                        value = "Not Tracked";
                        break;
                }
                if (rawValue == null)
                    rawValue = value;
            }
            return new JsonTableValue() { Header = header, HeaderId = header.Id, Value = value, Id = id, Type = header.Type, Editable = header.Editable, RawData = rawValue };

        }


        public Form GetForm()
        {
            return Form;
        }

        public INode GetNode()
        {
            return Form as INode;
        }

    }
}
