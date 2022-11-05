using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Data
{
    public interface ILogicHolder
        : INamedNode
    {
        [Obsolete]
        List<Logic> Logics { get; set; }
        List<JLogic> JLogics { get; set; }
        string RawJLogics { get; set; }
    }

    [Obsolete]
    public interface IFormItem
        : INodeItem
    {
        Form GetForm();
    }
}
