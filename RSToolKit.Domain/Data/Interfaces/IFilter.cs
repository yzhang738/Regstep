using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    [Obsolete]
    public interface IFilter
    {
        FilterLink Link { get; set; }
        RSToolKit.Domain.Entities.LogicTest Test { get; set; }
        string ActingOn { get; set; }
        string Value { get; set; }
        int Priority { get; set; }
        bool GroupNext { get; set; }

    }
}
