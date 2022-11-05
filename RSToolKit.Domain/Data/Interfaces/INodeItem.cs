using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    [Obsolete]
    public interface INodeItem
    {
        INode GetNode();
    }
}
