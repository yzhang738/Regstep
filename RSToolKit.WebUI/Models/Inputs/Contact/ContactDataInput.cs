using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Inputs.Contact
{
    /// <summary>
    /// Holds information about PUT data for contact data.
    /// </summary>
    public class ContactDataInput
    {
        /// <summary>
        /// The id of the contact data object or null if new.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The value.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// The id of the header that this information applies to or -1 for a contact column.
        /// </summary>
        public long HeaderId { get; set; }
        /// <summary>
        /// The id of the contact.
        /// </summary>
        public long ContactId { get; set; }
    }
}