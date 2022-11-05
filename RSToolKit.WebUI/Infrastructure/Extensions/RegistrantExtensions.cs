using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for registrants.
    /// </summary>
    public static class RegistrantExtensions
    {
        /// <summary>
        /// Removes the registrant from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="registrant">The registrant to remove.</param>
        /// <param name="context">The context being used.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Registrant registrant, EFDbContext context)
        {
            int count = 0;
            foreach (var oldRegistration in registrant.OldRegistrations.ToList())
            {
                count += oldRegistration.Data.Count + 1;
                context.OldRegistrantData.RemoveRange(oldRegistration.Data);
                context.OldRegistrants.Remove(oldRegistration);
            }
            count += registrant.PromotionalCodes.Count;
            context.PromotionCodeEntries.RemoveRange(registrant.PromotionalCodes);
            foreach (var emailSend in registrant.EmailSends.ToList())
            {
                emailSend.Registrant = null;
                emailSend.RegistrantKey = null;
            }
            count += registrant.Adjustments.Count;
            context.Adjustments.RemoveRange(registrant.Adjustments);
            count += registrant.Notes.Count;
            context.RegistrantNotes.RemoveRange(registrant.Notes);
            count += registrant.Seatings.Count;
            context.Seaters.RemoveRange(registrant.Seatings);
            registrant.ContactKey = null;
            registrant.Contact = null;
            count++;
            foreach (var transactionRequest in registrant.TransactionRequests.ToList())
            {
                transactionRequest.Registrant = null;
                transactionRequest.RegistrantKey = null;
            }
            context.Registrants.Remove(registrant);
            return count;
        }
    }
}