using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Entities.Email;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

//<img src="https://u369049.ct.sendgrid.net/wf/open?upn=8Mk0bck2Mvv9gw7tB5byjjBqIb6SOqKJuSKWvOjWU3FsWbgp-2BadDE1RYBfzxHSxQ4rIF4rszHav2Ipbpe60dAOvG3dfjm7TcSIkDMVLKOTRAOsbPkyZpvLHDlxuGRQbmVsbBVFwgm36zLqoeSQuGYOLVx7DdiLDkKcGYNe0W9Tt1A7Qc0w0UE8Or85EXY433qMvDR2gJP6wmFceSL6GWC72ZzBv6UG9y8Y-2FI1R-2F6R8dJwbesP1JQ8chuqmOAuRXbOzhtri2xQfNSn9Y-2FGQM6CbRrZaaT3u0411MeN6MUnx7-2BntmooiyQ9jDmHfxdh5MMnHUKlewNbmgMrG6RcxZ-2FblH9UQ8oOC-2FB2LrYIE3MJwjeAFbUQzbv4nwIBiWP24A4hkReopJXT4APpGGFz-2BM5PpQ1eEgH-2Bu2e8qkQ4CG42Grm8V-2Fvh3jYnIxKrcM2-2BkeqtGS3PoLWjHgHWkO2pnjolw-3D-3D" alt="" width="1" height="1" border="0" style="height:1px !important;width:1px !important;border-width:0 !important;padding-top:0 !important;padding-bottom:0 !important;padding-right:0 !important;padding-left:0 !important;">

namespace RSToolKit.WebUI.Controllers
{
    public class EmailEventController : RSController
    {
        public RedirectResult Click(long id, string aId, string url)
        {
            var send = Repository.Search<EmailSend>(s => s.SortingId == id).FirstOrDefault();
            if (send != null)
            {
                var clickEvent = new EmailEvent();
                clickEvent.Event = "Clicked";
                clickEvent.Details = "Recipient clicked a link.";
                clickEvent.Notes = url;
                clickEvent.Response = @"{""url"":""" + url + @""",""aId"":""" + aId + @"""}";
                send.EmailEvents.Add(clickEvent);
                Repository.Commit();
            }
            return Redirect(url);
        }

        public FileResult Open(long id)
        {
            var send = Repository.Search<EmailSend>(s => s.SortingId == id).FirstOrDefault();
            if (send != null)
            {
                var note = @"In house event.";
                var clickEvent = new EmailEvent() { Date = DateTimeOffset.Now, EmailSend = send, EmailSendKey = send.UId, Event = "Opened", Notes = note };
                send.EmailEvents.Add(clickEvent);
                //Repository.Commit();
            }
            var binaryData = new byte[0];
            using (FileStream f_stream = new FileStream(Server.MapPath("~/Images/Email/openevent.gif"), FileMode.Open, FileAccess.Read))
            {
                var b_reader = new BinaryReader(f_stream);
                var length = f_stream.Length;
                binaryData = b_reader.ReadBytes((int)length);
            }
            return new FileContentResult(binaryData, "image/gif");
        }
    }
}