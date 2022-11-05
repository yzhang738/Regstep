using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities.Navigation
{
    /// <summary>
    /// Holds the trail information in the database.
    /// </summary>
    public class UserTrail
    {
        /// <summary>
        /// The rawtrail in the database.
        /// </summary>
        protected string _rawTrail;

        /// <summary>
        /// The key of the item in the database.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The id of the user in the database.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The Json object of the trail items as an encoded array.
        /// </summary>
        public string RawTrail { get; set; }

        /// <summary>
        /// Gets the trail in the form of a <code>Trail</code>
        /// </summary>
        /// <typeparam name="T">The type of trail items.</typeparam>
        /// <returns>The trail retrieved.</returns>
        public Trail<T> GetTrail<T>()
            where T : class, ITrailItem
        {
            var items = JsonConvert.DeserializeObject<IEnumerable<T>>(RawTrail ?? "[]");
            return new Trail<T>(items);
        }

        /// <summary>
        /// Sets the trail to save to the database.
        /// </summary>
        /// <typeparam name="T">The type of <code>ITrailItem</code>.</typeparam>
        /// <param name="trail">The trail being used.</param>
        public void SetTrail<T>(Trail<T> trail)
            where T : class, ITrailItem
        {
            RawTrail = JsonConvert.SerializeObject(trail.GetArray());
        }
    }
}
