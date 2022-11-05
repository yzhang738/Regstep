using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Inputs.Register
{
    /// <summary>
    /// Holds information posted back from a registration page.
    /// </summary>
    public class PageInput
    {
        /// <summary>
        /// The page key.
        /// </summary>
        public Guid PageKey { get; set; }
        /// <summary>
        /// The page number.
        /// </summary>
        public int PageNumber { get; set; }
        /// <summary>
        /// The registrants key.
        /// </summary>
        public Guid RegistrantKey { get; set; }
        /// <summary>
        /// The form key.
        /// </summary>
        public Guid FormKey { get; set; }
        /// <summary>
        /// The components filled out.
        /// </summary>
        public Dictionary<Guid, string> Components { get; set; }
        /// <summary>
        /// The items chose for waitlisting.
        /// </summary>
        public Dictionary<Guid, bool> Waitlistings { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public PageInput()
        {
            Components = new Dictionary<Guid, string>();
            Waitlistings = new Dictionary<Guid, bool>();
        }
    }
}