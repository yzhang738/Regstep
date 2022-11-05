using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public interface IUpdatable
        : IKeepAlive
    {
        DateTime LastUpdate { get; set; }
        Guid ModificationToken { get; set; }
    }
}
