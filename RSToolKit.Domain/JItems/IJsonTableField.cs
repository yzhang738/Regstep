using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.JItems
{
    public interface IJsonTableField
        : IJsonFilterHeader
    {
        /// <summary>
        /// Flag for if the field is currently being used.
        /// </summary>
        bool Using { get; set; }
    }
}
