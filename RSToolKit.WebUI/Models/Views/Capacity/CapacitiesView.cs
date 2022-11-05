using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Views.Capacity
{
    /// <summary>
    /// Holds information about capacities.
    /// </summary>
    public class CapacitiesView
        : ViewBase
    {
        /// <summary>
        /// The form id.
        /// </summary>
        public long FormId { get; set; }
        /// <summary>
        /// The form key.
        /// </summary>
        public Guid FormKey { get; set; }
        /// <summary>
        /// The name of the form.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The list of capacities.
        /// </summary>
        public List<CapacityInfo> Capacities { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public CapacitiesView()
            : base()
        {
            Title = "Capacities";
            Capacities = new List<CapacityInfo>();
            FormId = -1;
            FormKey = Guid.Empty;
            Name = "No Name";
        }

        /// <summary>
        /// Initializes the class with the specified form.
        /// </summary>
        /// <param name="form">The form to populate the view with.</param>
        public CapacitiesView(Domain.Entities.Form form)
            : this()
        {
            FormKey = form.UId;
            FormId = form.SortingId;
            Name = form.Name;
            foreach (var capacity in form.Seatings)
            {
                Capacities.Add(new CapacityInfo(capacity));
            }
        }

    }

    /// <summary>
    /// Holds information about a capacity limit.
    /// </summary>
    public class CapacityInfo
    {
        /// <summary>
        /// The id.
        /// </summary>
        public long Id { get; protected set; }
        /// <summary>
        /// The global unique identifier.
        /// </summary>
        public Guid Key { get; protected set; }
        /// <summary>
        /// The name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The number of waitlisters.
        /// </summary>
        public long Waiters { get; protected set; }
        /// <summary>
        /// The number of seaters.
        /// </summary>
        public long Seaters { get; protected set; }
        /// <summary>
        /// Flag for has people waitlisting.
        /// </summary>
        public bool HasWaiters { get { return Waiters > 0; } }
        /// <summary>
        /// Flag for has people occupying seats.
        /// </summary>
        public bool HasSeaters { get { return Seaters > 0; } }
        /// <summary>
        /// The amount of seats.
        /// </summary>
        public long Seats { get; protected set; }
        /// <summary>
        /// The amount of available seats.
        /// </summary>
        public long AvailableSeats { get { return (Seats == -1 ? -1 : Seats - Seaters); } }
        /// <summary>
        /// Flag for seats available to be occupied.
        /// </summary>
        public bool SeatsAvailable { get { return AvailableSeats > 0; } }
        /// <summary>
        /// Flag for the user being able to make changes.
        /// </summary>
        public bool CanWrite { get; set; }
        /// <summary>
        /// Flag for waitlisting available.
        /// </summary>
        public bool Waitable { get; protected set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public CapacityInfo()
        {
            Id = -1L;
            Key = Guid.Empty;
            Waiters = 0;
            Seaters = 0;
            Seats = -1;
            Name = "Capacity Limit";
            CanWrite = false;
            Waitable = false;
        }

        /// <summary>
        /// Initializes the class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity limit.</param>
        public CapacityInfo(Domain.Entities.Seating capacity)
            : this()
        {
            Id = capacity.SortingId;
            Key = capacity.UId;
            Waiters = capacity.Waiters;
            Seaters = capacity.SeatsTaken;
            Seats = capacity.MaxSeats;
            Name = capacity.Name;
            Waitable = capacity.Waitlistable;
        }
    }
}