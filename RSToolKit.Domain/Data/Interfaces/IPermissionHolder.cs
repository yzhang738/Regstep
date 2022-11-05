using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// An interface that tells us the ICompanyHolder item requires explicit permissiosn to access.
    /// </summary>
    public interface IPermissionHolder
        : ICompanyHolder
    {
    }
}
