using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.WebUI.Models.Views;
using RSToolKit.WebUI.Models.Inputs.Contact;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Models.Views.Contact
{
    /// <summary>
    /// Holds information about the uploaded list.
    /// </summary>
    public class ContactUploadListView
         : ViewBase
    {
        /// <summary>
        /// The list input that holds all the uploaded data.
        /// </summary>
        public ContactUploadListInput ListInput { get; set; }
        /// <summary>
        /// Holds the contact headers available.
        /// </summary>
        public List<ContactHeader> Headers { get; set; }
        /// <summary>
        /// The number of contacts that already exist in the database.
        /// </summary>
        public int ContactsInDatabase
        {
            get
            {
                return ListInput.Contacts.Count(c => c.ContactKey.HasValue);
            }
        }
        /// <summary>
        /// The number of headers that are not in the database.
        /// </summary>
        public int NewHeaders
        {
            get
            {
                return ListInput.Headers.Count(h => h.HeaderKey == null);
            }
        }

        /// <summary>
        /// Initizlizes the class.
        /// </summary>
        public ContactUploadListView()
            : base()
        {
            Title = "Rectify Contact Upload";
            Headers = new List<ContactHeader>();
        }
    }
}