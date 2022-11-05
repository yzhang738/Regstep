using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class ColorPicker : Attribute
    {
        public ColorPicker()
        {

        }
    }
}
