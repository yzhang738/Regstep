using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public interface IApiLogin
    {
        string ApiToken { get; set; }
        DateTimeOffset? ApiTokenExpiration { get; set; }
    }
}
