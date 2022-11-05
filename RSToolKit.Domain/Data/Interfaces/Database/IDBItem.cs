using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data.Interfaces.Database
{
    /// <summary>
    /// This represents an item that is located in a table in the database.
    /// </summary>
    /// <remarks>Each IDataItem implementation idealy needs to be in there own database table.</remarks>
    public interface IDBItem
    {
        /// <summary>
        /// The id of the data item.
        /// </summary>
        /// <remarks>In an ideal situation, this would be the primary key in the databse.</remarks>
        long Id { get; set; }
        /// <summary>
        /// The unique identifier of the item.
        /// </summary>
        /// <remarks>This is a global identifier in providing a unique way to point to any item in the database regardless of the table it is in.</remarks>
        Guid UId { get; set; }
        /// <summary>
        /// The name of the database item.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// The key of the company.
        /// </summary>
        Guid CompanyKey { get; set; }
        /// <summary>
        /// This is for concurrency checks.
        /// </summary>
        /// <remarks>
        /// This should be decorated with the EF code first attribute <code>[TimeStamp]</code>.
        /// </remarks>
        byte[] RowVersion { get; set; }
    }
}
