using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Data.OldJsonData;

namespace RSToolKit.Domain.Data
{
    public interface IFormReport
        : IReport
    {
        Form Form { get; set; }
        Guid FormKey { get; set; }
    }

    public interface IReport
        : IPointerTarget, ICompanyHolder, INamedNode
    {
        bool Favorite { get; set; }
        Guid ParentKey { get; set; }
    }

    public interface IReportData
    {
        bool Favorite { get; set; }
        IJsonTable Table { get; set; }
    }
}
