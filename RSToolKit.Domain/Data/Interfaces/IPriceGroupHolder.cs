using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.Data
{
    public interface IPriceGroupHolder
    {
        PriceGroup Price { get; set; }
        Guid UId { get; set; }
    }
}
