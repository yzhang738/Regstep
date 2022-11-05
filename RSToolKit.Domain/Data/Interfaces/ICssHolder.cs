using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.Data
{
    public interface ICssHolder
    {
        Guid UId { get; set; }
        Guid CssUId { get; set; }
        CSS Css { get; set; }

        void Initiate(EFDbContext context);
    }
}
