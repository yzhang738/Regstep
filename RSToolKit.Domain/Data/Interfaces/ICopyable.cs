using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Data
{
    public interface ICopyable
        : ICompanyHolder
    {
        ICopyable DeepCopy(string name, Company company, User owner, FormsRepository repository);
    }
}
