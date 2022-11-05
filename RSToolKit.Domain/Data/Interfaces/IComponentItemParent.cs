using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public interface IComponentItemParent
    {
        IEnumerable<IComponentItem> Children { get; }
    }
}
