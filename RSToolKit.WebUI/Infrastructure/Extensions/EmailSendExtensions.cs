using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Data;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for email sends.
    /// </summary>
    public static class EmailSendExtensions
    {
        /// <summary>
        /// Removese the email send from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="emailSend">The email send to remove.</param>
        /// <param name="context">The context the email send is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this EmailSend emailSend, EFDbContext context)
        {
            var count = 0;
            foreach (var emailEvent in emailSend.EmailEvents.ToList())
                count += emailEvent.Delete(context);
            if (emailSend.Contact != null)
                emailSend.Contact.EmailSends.Remove(emailSend);
            if (emailSend.Registrant != null)
                emailSend.Registrant.EmailSends.Remove(emailSend);
            context.EmailSends.Remove(emailSend);
            return ++count;
        }
    }
}