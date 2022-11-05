using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Mail;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.RegularExpressions;
using RSToolKit.WebUI.Infrastructure;
using System.Web.Http.Cors;

namespace RSToolKit.WebUI.Controllers.API
{
    [EnableCors("http://www.regstep.com,http://regstep.com", "*", "POST,OPTIONS")]
    public class SubmitController : ApiController
    {
        [AllowAnonymous]
        public IHttpActionResult Post(SubmitApiRequest request)
        {
            if (request == null)
                return BadRequest("No data supplied.");
            using (var repository = new FormsRepository())
            {
                SmtpServer smtpServer = null;
                if (smtpServer == null)
                    smtpServer = repository.Search<SmtpServer>(s => s.Name == "Primary").FirstOrDefault();
                if (smtpServer == null)
                    return BadRequest("No smtp client.");

                var html = "";

                if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Emails/" + request.emailType + ".html")))
                    using (var fileReader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Emails/" + request.emailType + ".html")))
                        html = fileReader.ReadToEnd();
                else
                    return BadRequest("No email file found.");

                foreach (var kvp in request.values)
                    html = Regex.Replace(html, @"\[" + kvp.Key + @"\]", kvp.Value);

                html = Regex.Replace(html, @"\[ServerTimeRequest\]", DateTimeOffset.Now.ToString("s"));

                MailAddress mailAddy;
                try { mailAddy = new MailAddress(request.email); }
                catch (Exception) { return BadRequest("Bad email address"); }

                var email = request.email;
                if (email == null)
                    return BadRequest("No email supplied.");
                var message = new MailMessage();
                message.IsBodyHtml = true;
                message.Body = html;
                message.To.Add(mailAddy);
                message.From = new MailAddress("info@regstep.com");
                if (request.emailType.EndsWith("rs"))
                    message.Subject = request.emailType;
                else
                    message.Subject = "Thank You For Contacting Us";

                var server = smtpServer.CreateServer();
                server.Send(message);
                return Ok();
            }
        }

        public IHttpActionResult Options()
        {
            return Ok();
        }
    }

    public class SubmitApiRequest
    {
        public string email { get; set; }
        public Dictionary<string, string> values { get; set; }
        public string emailType { get; set; }
    }
}
