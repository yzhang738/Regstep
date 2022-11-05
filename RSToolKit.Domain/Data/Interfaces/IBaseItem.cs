using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Data
{
    public interface IBaseItem
    {
        string[] roles { get; }
        Guid CompanyKey { get; set; }
        Company Company { get; set; }
    }

    public interface IAdminOnly
    { }
}
