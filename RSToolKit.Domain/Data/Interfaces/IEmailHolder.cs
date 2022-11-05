using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Email;

namespace RSToolKit.Domain.Data
{
    public interface IEmailHolder
        : ICompanyHolder, INamedNode
    {
        List<RSEmail> Emails { get; set; }
        List<RSHtmlEmail> HtmlEmails { get; set; }
        IEnumerable<IEmail> AllEmails { get; }
    }
}
