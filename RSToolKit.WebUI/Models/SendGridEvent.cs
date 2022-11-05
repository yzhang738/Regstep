using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RSToolKit.WebUI.Models
{
    public class SendGridEvent
    {
        public string email { get; set; }
        public string sg_event_id { get; set; }
        public string sg_message_id { get; set; }
        public int timestamp { get; set; }
        [JsonProperty("smtp-id")]
        public string smtp_id { get; set; }
        [JsonProperty("event")]
        public string sg_event { get; set; }
        public string response { get; set; }
        public List<string> category { get; set; }
        public string id { get; set; }
        public string purchase { get; set; }
        public string uid { get; set; }
        public string reason { get; set; }
        public string attempt { get; set; }
        public string type { get; set; }

        [JsonProperty("email-id")]
        public string email_id { get; set; }
        [JsonProperty("rs-email")]
        public string rs_email { get; set; }
        public string user { get; set; }

        public SendGridEvent()
        {
            email = sg_event_id = sg_message_id = smtp_id = sg_event = id = purchase = uid = reason = email_id = rs_email = response = user = attempt = type = "";
            timestamp = 1;
            category = new List<string>();
        }
    }
}