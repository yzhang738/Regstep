using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Email;
using System.IO;
using System.Drawing;

namespace RSToolKit.WebUI.Controllers
{
    [AllowAnonymous]
    public class EmailInformationController : Controller
    {

        public FormsRepository Repository { get; set; }

        public EmailInformationController()
        {
            Repository = new FormsRepository();
        }

        public FileContentResult Open(Guid id)
        {
            // We need to set up the binary object to hold the image.
            var binaryData = new byte[0];

            // Now we load the image from a file.
            // We will use a memory stream to hold the data while we read.
            var img = Image.FromFile(Server.MapPath("~/Images/Email/openevent.gif"));
            using (MemoryStream m_stream = new MemoryStream())
            {
                img.Save(m_stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                binaryData = m_stream.ToArray();
            }
            // Now that we have the binary, we will look for the emailSend object in the database.
            var emailSend = Repository.Search<EmailSend>(r => r.UId == id).FirstOrDefault();
            if (emailSend == null)
                // There was no object, so we return the image and do nothing else.
                return new FileContentResult(binaryData, "image/gif");

            // The object was found so we create an open event then commit it to the database.
            emailSend.EmailEvents.Add(new EmailEvent()
            {
                Date = DateTimeOffset.Now,
                Event = "Opened"
            });
            Repository.Commit();

            // No we return the image.
            return new FileContentResult(binaryData, "image/gif");
        }
    }
}