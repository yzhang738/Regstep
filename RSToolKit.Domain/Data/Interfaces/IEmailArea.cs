using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public interface IEmailArea
    {
        Guid UId { get; set; }
        string Type { get; set; }
        string Html { get; set; }
        long SortingId { get; set; }
        string Render(string body);
    }
}
