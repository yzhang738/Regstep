using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public interface IEmailList
        : ICompanyHolder, IjTableProcessor, INamedNode
    {
        IEnumerable<IPerson> GetAllEmails(FormsRepository repository);
        IEnumerable<string> GetAllEmailAddresses();
        Guid UId { get; set; }
    }
}
