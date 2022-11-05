using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Security
{
    /// <summary>
    /// An interface that represents a class associated with permission settings.
    /// </summary>
    public interface IProtected
    {
        /// <summary>
        /// The global unique identifier of the item.
        /// </summary>
        Guid UId { get; set; }
        /// <summary>
        /// The key of the company that owns the item.
        /// </summary>
        Guid CompanyKey { get; set; }
        /// <summary>
        /// The name of the item.
        /// </summary>
        string Name { get; set; }
    }

    /// <summary>
    /// An interface that represents a class that can only be accessed by global administrators.
    /// </summary>
    public interface IGloballyProtected
        : IProtected
    {
    }

    /// <summary>
    /// An interface that represents a class that uses another items permissions.
    /// </summary>
    public interface IProtectionInherited
    {
        /// <summary>
        /// The key of the item that has the permissions sets in the database.
        /// </summary>
        Guid ProtectionTarget { get; }
        /// <summary>
        /// The item that is protected.
        /// </summary>
        IProtected ProtectedItem { get; }
    }
}
