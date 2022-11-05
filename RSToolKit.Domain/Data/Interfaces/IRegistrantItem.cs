using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.Data
{
    public interface IRegistrantItem
        : IFormItem
    {
        Registrant Registrant { get; set; }
    }
}
