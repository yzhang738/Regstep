using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.WebUI.Models.Views;

namespace RSToolKit.WebUI.Models.Views.Company
{
    /// <summary>
    /// Holds the information for the companies view.
    /// </summary>
    public class CompaniesView
        : ViewBase
    {
        /// <summary>
        /// The companies to display.
        /// </summary>
        public List<CompaniesView_CompanyView> Companies { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public CompaniesView()
            : base()
        {
            Title = "Companies";
            Companies = new List<CompaniesView_CompanyView>();
        }
    }

    /// <summary>
    /// Holds a minimal amount of information for the companies view.
    /// </summary>
    public class CompaniesView_CompanyView
    {
        /// <summary>
        /// The id of the company.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// The name of the company.
        /// </summary>
        public string Name { get; set; }
    }
}