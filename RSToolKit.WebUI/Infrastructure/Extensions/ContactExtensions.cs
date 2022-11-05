using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.WebUI.Models.Inputs.Contact;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds static methods for contacts.
    /// </summary>
    public static class ContactExtensions
    {
        /// <summary>
        /// Sets data based on the <code>ContactInputData</code>.
        /// </summary>
        /// <param name="contact">The contact being extended.</param>
        /// <param name="input">The input data to use.</param>
        /// <returns></returns>
        public static SetDataResult SetData(this Contact contact, ContactDataInput input)
        {
            var result = contact.SetData(input.Id, input.Value);
            return result;
        }
    }
}