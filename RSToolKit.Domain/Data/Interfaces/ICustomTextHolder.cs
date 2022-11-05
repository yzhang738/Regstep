using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.Data
{
    public interface ICustomTextHolder
    {
        Guid UId { get; set; }
        List<CustomText> CustomTexts { get; set; }
    }
}
