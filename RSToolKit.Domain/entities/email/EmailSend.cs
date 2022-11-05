using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities.Email
{
    public class EmailSend
        : IRSData
    {
        protected EmailEvent _topEvent;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [Key]
        public Guid UId { get; set; }

        public virtual List<EmailEvent> EmailEvents { get; set; }

        [JsonIgnore]
        [ForeignKey("RegistrantKey")]
        public virtual Registrant Registrant { get; set; }
        public Guid? RegistrantKey { get; set; }

        [JsonIgnore]
        [ForeignKey("ContactKey")]
        public virtual Contact Contact { get; set; }
        public Guid? ContactKey { get; set; }

        [JsonIgnore]
        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid? FormKey { get; set; }

        public SendType Type { get; set; }

        public string EmailDescription { get; set; }

        public string Recipient { get; set; }

        public DateTimeOffset DateSent { get; set; }

        public Guid? EmailKey { get; set; }

        [NotMapped]
        public List<string> IgnoreEventsFrom { get; set; }

        /// <summary>
        /// The top level event.
        /// </summary>
        [NotMapped]
        public EmailEvent TopEvent
        {
            get
            {
                if (this._topEvent == null)
                    return this._GetTopEvent();
                return this._topEvent;
            }
        }

        public string RawIgnoreEventsFrom
        {
            get
            {
                if (IgnoreEventsFrom != null)
                    return JsonConvert.SerializeObject(IgnoreEventsFrom);
                return "[]";
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    IgnoreEventsFrom = new List<string>();
                else
                    IgnoreEventsFrom = JsonConvert.DeserializeObject<List<string>>(value);
            }
        }


        public EmailSend()
        {
            DateSent = DateTimeOffset.UtcNow;
            Recipient = null;
            EmailEvents = new List<EmailEvent>();
            Type = SendType.FormEmail;
            EmailDescription = "none";
            UId = Guid.Empty;
            IgnoreEventsFrom = new List<string>();
            _topEvent = null;
        }

        public JItems.JsonTableValue GetStatus(JItems.JsonTableHeader header)
        {
            var value = new JItems.JsonTableValue()
            {
                Id = UId.ToString() + "_DeliveryStatus",
                Header = header,
                HeaderId = header.Id,
                Value = "Not Delivered",
                RawData = "0",
                Editable = false,
                Type = "text"
            };
            if (TopEvent.Event.In("Clicked", "Opened", "Delivered"))
            {
                value.RawData = "2";
                value.Value = "Delivered";
            } else if (TopEvent.Event.In("Sending", "sending", "attempting to send"))
            {
                value.RawData = "1";
                value.Value = "Sending";
            }
            return value;
        }

        /// <summary>
        /// Gets the top level event.
        /// </summary>
        /// <returns>The top level event.</returns>
        protected EmailEvent _GetTopEvent()
        {
            EmailEvent top = null;
            var orderedEvents = EmailEvents.OrderByDescending(e => e.Date);
            top = orderedEvents.FirstOrDefault(e => e.Event == "Clicked");
            if (top == null)
                top = orderedEvents.FirstOrDefault(e => e.Event == "Opened");
            if (top == null)
                top = orderedEvents.FirstOrDefault(e => e.Event == "Permanent Bounce");
            if (top == null)
                top = orderedEvents.FirstOrDefault(e => e.Event == "Dropped");
            if (top == null)
                top = orderedEvents.FirstOrDefault(e => e.Event == "Spam Report");
            if (top == null)
                top = orderedEvents.FirstOrDefault(e => e.Event == "Delivered");
            if (top == null)
                top = orderedEvents.FirstOrDefault(e => e.Event == "Sending");
            if (top == null)
                top = orderedEvents.FirstOrDefault(e => e.Event == "Temporary Bounce");
            if (top == null)
                top = orderedEvents.FirstOrDefault(e => e.Event == "Transferring");
            if (this._topEvent == null)
                top = orderedEvents.First();
            this._topEvent = top;
            return top;
        }
    }

    public class EmailSendComparer_Recipient : IEqualityComparer<EmailSend>
    {
        public bool Equals(EmailSend x, EmailSend y)
        {
            if (x.Recipient.ToLower() == y.Recipient.ToLower())
                return true;
            return false;
        }

        public int GetHashCode(EmailSend x)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(x, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashProductName = x.Recipient == null ? 0 : x.Recipient.GetHashCode();

            //Calculate the hash code for the product.
            return hashProductName;
        }
    }

    public class EmailSendComparer_Contact : IEqualityComparer<EmailSend>
    {
        public bool Equals(EmailSend x, EmailSend y)
        {
            if (x.ContactKey == y.ContactKey)
                return true;
            return false;
        }

        public int GetHashCode(EmailSend x)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(x, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashProductName = x.ContactKey == null ? 0 : x.ContactKey.GetHashCode();

            //Calculate the hash code for the product.
            return hashProductName;
        }
    }


    public enum SendType
    {
        FormEmail = 0,
        InvitationEmail = 1
    }
}
