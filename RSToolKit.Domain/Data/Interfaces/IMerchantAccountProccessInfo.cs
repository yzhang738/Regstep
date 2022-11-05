using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public interface IMerchantAccountProccessInfo
    {
        decimal Ammount { get; set; }
        int Status { get; set; }
        bool Success { get; set; }
        ServiceMode Mode { get; set; }
        Dictionary<string, string> Parameters { get; set; }
    }

    public enum ServiceMode
    {
        Test,
        Live
    }
}
