using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public interface IFinanceAmmount
        : IRSData
    {
        DateTimeOffset DateCreated { get; set; }
        string Name { get; set; }
        decimal? TotalAmount();
        Guid UId { get; set; }
        string Creator { get; set; }
    }
}
