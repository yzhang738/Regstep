using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RSToolKit.WebUI.Models.Inputs.Register
{
    /// <summary>
    /// Holds information about the registration input.
    /// </summary>
    public class StartInput
    {
        /// <summary>
        /// The form key.
        /// </summary>
        public Guid FormKey { get; set; }
        /// <summary>
        /// The supplied email.
        /// </summary>
        [Required]
        public string Email { get; set; }
    }
}