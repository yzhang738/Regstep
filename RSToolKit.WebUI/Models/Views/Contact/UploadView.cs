using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Views.Contact
{
    /// <summary>
    /// Holds information about the view.
    /// </summary>
    public class UploadView
        : ViewBase
    {
        /// <summary>
        /// The list of saved lists.
        /// </summary>
        public List<SavedListView> SavedLists { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public UploadView()
            : base()
        {
            Title = "Upload Contact List";
        }
    }

    /// <summary>
    /// Holds information about a saved list.
    /// </summary>
    public class SavedListView
    {
        /// <summary>
        /// The key of the saved list.
        /// </summary>
        public Guid Key { get; set; }
        /// <summary>
        /// The label of the saved list.
        /// </summary>
        public string Label { get; set; }
    }
}