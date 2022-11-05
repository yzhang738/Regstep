using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Notification
{
    /// <summary>
    /// A notification for a user.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// The id of the notification in the database.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        /// <summary>
        /// The id of the <code>User</code> this is attached to.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// The notification description.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Whether the notification is hidden or not.
        /// </summary>
        public bool Hidden { get; set; }
        /// <summary>
        /// The url of the notification.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The <code>User</code> the notification is attached to.
        /// </summary>
        [ForeignKey("UserId")]
        public User User { get; set; }

        /// <summary>
        /// Initializes the class with defaults.
        /// </summary>
        public Notification()
        {
            UserId = null;
            Name = "New Notification";
            Hidden = false;
            Url = null;
        }

        /// <summary>
        /// Initializes the class with the specified data.
        /// </summary>
        /// <param name="user">The user to attach the notification to.</param>
        /// <param name="name">The description of the notification.</param>
        /// <param name="url">The url for the notification if there is one.</param>
        public Notification(User user, string name, string url = null)
        {
            User = user;
            UserId = user.Id;
            Name = name;
            Hidden = false;
            Url = url;
        }
    }
}
