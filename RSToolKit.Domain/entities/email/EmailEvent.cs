using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities.Email
{
    public class EmailEvent
        : IRSData
    {
        [NotMapped]
        public static Dictionary<string, int> eventDepth = new Dictionary<string, int>()
        {
            { "Attempting to Send", 0 },
            { "Delivered", 1 },
            { "Opened", 2 },
            { "Clicked", 3 },
            { "Permanent Bounce", 4 },
            { "Temporary Bounce", 5 }
        };

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [Key]
        public Guid UId { get; set; }

        [ForeignKey("EmailSendKey")]
        [JsonIgnore]
        public virtual EmailSend EmailSend { get; set; }
        public Guid EmailSendKey { get; set; }
        [MaxLength(250)]
        public string Event { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Notes { get; set; }
        public string Email { get; set; }
        public string Details { get; set; }
        public string Response { get; set; }
        [NotMapped]
        public string PrettyValue
        {
            get
            {
                return ToString();
            }
        }

        public EmailEvent()
        {
            UId = Guid.NewGuid();
            EmailSendKey = UId;
            Event = "SentForProcessing";
            Date = DateTimeOffset.Now;
            Notes = "";
            Email = "";
            Details = "";
            Response = "";
        }

        public override string ToString()
        {
            return ToString(@"E: n \b\y e");
        }

        public string ToString(string format, Clients.User user = null)
        {
            var r_string = "";
            var skipNext = false;
            foreach (var character in format.ToCharArray())
            {
                if (skipNext)
                {
                    r_string += character;
                    skipNext = false;
                    continue;
                }
                switch (character)
                {
                    case 'E':
                        r_string += Event;
                        break;
                    case 'e':
                        if (String.IsNullOrWhiteSpace(Email))
                            r_string += EmailSend.Recipient.ToLower();
                        else
                            r_string += Email;
                        break;
                    case 'n':
                        if (Event == "Clicked")
                        {
                            Dictionary<string, string> t_clickInfo;
                            try
                            {
                                t_clickInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(Notes);
                                if (!t_clickInfo.ContainsKey("url"))
                                    t_clickInfo["url"] = "no url";
                            }
                            catch (Exception)
                            {
                                t_clickInfo = new Dictionary<string, string>();
                                t_clickInfo["url"] = "no url";
                            }
                            r_string += t_clickInfo["url"].GetElipse(50);
                        }
                        else
                        {
                            r_string += Notes;
                        }
                        break;
                    case 'd':
                        r_string += Date.ToString(user);
                        break;
                    case 'D':
                        r_string += Details;
                        break;
                    case '\\':
                        skipNext = true;
                        break;
                    default:
                        r_string += character;
                        break;
                }
            }
            return r_string;
        }
    }
}
