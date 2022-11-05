using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Views
{
    /// <summary>
    /// Holds information needed for rendering the most basic view.
    /// </summary>
    public class ViewBase
    {
        /// <summary>
        /// The title of the page.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The list of scripts to use.
        /// </summary>
        public List<string> ScriptFiles { get; set; }
        /// <summary>
        /// The list of styles to use.
        /// </summary>
        public List<string> StyleSheets { get; set; }

        /// <summary>
        /// Initializes the class and creates empty lists.
        /// </summary>
        public ViewBase()
        {
            ScriptFiles = new List<string>();
            StyleSheets = new List<string>();
            Title = "RegStep Technologies";
        }
    }
}