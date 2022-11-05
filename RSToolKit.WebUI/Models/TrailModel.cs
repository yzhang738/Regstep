using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities.Navigation;
using RSToolKit.Domain.Data;

namespace RSToolKit.WebUI.Models
{
    public class TrailModel
        : IEnumerable<Pheromone>
    {
        /// <summary>
        /// Gets the amount of items to render.
        /// </summary>
        public int TrailCount { get; protected set; }
        /// <summary>
        /// Gets the trail.
        /// </summary>
        public Trail<Pheromone> Trail { get; protected set; }
        
        /// <summary>
        /// Sets up the class with the specified parameters.
        /// </summary>
        /// <param name="trail">The trail to use.</param>
        /// <param name="count"The number of items to use.</param>
        public TrailModel(Trail<Pheromone> trail, int count = 5)
        {
            Trail = trail;
            TrailCount = count;
        }

        /// <summary>
        /// Gets the number of items specified or less from most recent to oldest.
        /// </summary>
        /// <returns>The enumerator holding the items.</returns>
        public IEnumerator<Pheromone> GetEnumerator()
        {
            var array = Trail.GetArray();
            var start = array.Length - TrailCount;
            if (start < 0)
                start = 0;
            for (var i = start; i < array.Length; i++)
                yield return array[i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}