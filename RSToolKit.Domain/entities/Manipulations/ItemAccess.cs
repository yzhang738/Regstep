using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSToolKit.Domain.Entities.Manipulations
{
    /// <summary>
    /// Holds information about an item that was accessed in the database.
    /// </summary>
    public class ItemAccess
    {
        /// <summary>
        /// The id of the access.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        /// <summary>
        /// The unique identifier of the item being accessed.
        /// </summary>
        public Guid Target { get; set; }
        /// <summary>
        /// The type of access.
        /// </summary>
        public ItemAccessTypes AccessType { get; set; }
        /// <summary>
        /// The user that is doing the accessing.
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// The date this took place.
        /// </summary>
        public DateTimeOffset Date { get; set; }
    }

    /// <summary>
    /// Holds the types of access that can be done to a database item.
    /// </summary>
    public enum ItemAccessTypes
    {
        /// <summary>
        /// No access.
        /// </summary>
        None = 0,
        /// <summary>
        /// Reading an item.
        /// </summary>
        Read = 1,
        /// <summary>
        /// Writing an item, as in modification.
        /// </summary>
        Write = 2,
        /// <summary>
        /// Creating an item.
        /// </summary>
        Create = 3,
        /// <summary>
        /// Deleting an item.
        /// </summary>
        Delete = 4,
        /// <summary>
        /// Updating security for an item.
        /// </summary>
        Security = 5
    }
}
