using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.Data
{
    public interface IJsonTableInformation
    {
        Guid UId { get; set; }
        JsonTableInformation GetJsonTableInformation(IComplexDictionary complexDic = null, ReportType type = ReportType.Form, bool showDeleted = false);
    }
}
