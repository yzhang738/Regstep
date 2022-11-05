using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Email;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for email events.
    /// </summary>
    public static class EmailEventExtensions
    {
        /// <summary>
        /// Removes the email event from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="emailEvent">The email event to remove.</param>
        /// <param name="context">The context the email event is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this EmailEvent emailEvent, EFDbContext context)
        {
            emailEvent.EmailSend.EmailEvents.Remove(emailEvent);
            context.EmailEvents.Remove(emailEvent);
            return 1;
        }
    }
}