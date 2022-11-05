using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public interface IComponentMultipleSelection : IComponentItemParent
    {
        bool TimeExclusion { get; set; }
        string DialogText { get; set; }
    }
}
