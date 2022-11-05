using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Inputs.Register
{
    /// <summary>
    /// Holds the input information.
    /// </summary>
    public class AudienceInput
    {
        /// <summary>
        /// The value of RSVP.
        /// </summary>
        public Guid Audience { get; set; }
        /// <summary>
        /// The registrants key.
        /// </summary>
        public Guid RegistrantKey { get; set; }
    }
}