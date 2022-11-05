using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using Newtonsoft.Json;

namespace RSToolKit.Domain.Entities.Navigation
{
    /// <summary>
    /// Holds the information of the <code>Trail</code>.
    /// </summary>
    public class Pheromone
        : ITrailItem
    {
        /// <summary>
        /// The label of the <code>Pheromone</code> to use.
        /// </summary>
        protected string _label;

        /// <summary>
        /// The id of the crumb as a <code>Guid</code>.
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }
        /// <summary>
        /// The date the action happened.
        /// </summary>
        [JsonProperty("actionDate")]
        public DateTimeOffset ActionDate { get; set; }
        /// <summary>
        /// The action that happened.
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }
        /// <summary>
        /// The controller that handled the action.
        /// </summary>
        [JsonProperty("controller")]
        public string Controller { get; set; }
        /// <summary>
        /// The label of the action.
        /// </summary>
        [JsonProperty("label")]
        public string Label
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_label))
                    return Action;
                return _label;
            }
            set
            {
                _label = value;
            }
        }
        /// <summary>
        /// The parameters that went along with the action.
        /// </summary>
        [JsonProperty("parameters")]
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Initializes the class with default values.
        /// </summary>
        public Pheromone()
        {
            ActionDate = DateTimeOffset.UtcNow;
            Action = "Action";
            _label = "";
            Controller = "Controler";
            Parameters = new Dictionary<string, string>();
            Id = Guid.Empty;
        }

        /// <summary>
        /// Creates a route value dictionary to be used.
        /// </summary>
        /// <returns>The <code>RouteValueDictionary</code> made.</returns>
        public System.Web.Routing.RouteValueDictionary RVD()
        {
            var rvd = new System.Web.Routing.RouteValueDictionary();
            foreach (var kvp in Parameters)
            {
                rvd.Add(kvp.Key, kvp.Value);
            }
            rvd.Add("trailid", Id);
            rvd.Add("controller", Controller);
            rvd.Add("action", Action);
            return rvd;
        }
    }
}
