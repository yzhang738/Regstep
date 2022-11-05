using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Data
{
    public interface ICompanyHolder
        : INode
    {
        Company Company { get; set; }
        Guid CompanyKey { get; set; }
    }
}
