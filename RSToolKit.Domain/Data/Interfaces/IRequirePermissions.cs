using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// Interface to represent an IRSData that requires permissions to access.
    /// </summary>
    public interface IRequirePermissions
        : IRSData
    {
        /// <summary>
        /// Gets the ICompanyHolder object that has the permissions settings.
        /// </summary>
        /// <returns>The ICompanyHolder object.</returns>
        IPermissionHolder GetPermissionHolder();
    }
}
