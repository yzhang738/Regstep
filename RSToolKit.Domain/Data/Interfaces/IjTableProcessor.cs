using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Clients;
using System.Globalization;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Data
{
    [Obsolete]
    public interface IjTableProcessor
        : ICompanyHolder
    {
        JTable FillJTable(JTable table = null, bool admin = false, Dictionary<string, string> options = null, User user = null, bool noHtml = false);
        CultureInfo Culture { get; set; }
    }
}
