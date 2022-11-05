using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Entities.Finances
{
    public interface ICharge
        : IFinanceAction
    {
        /// <summary>
        /// The type of charge.
        /// </summary>
        new ChargeType Type { get; }
        /// <summary>
        /// The id of the form the charge applies to.
        /// </summary>
        long FormKey { get; set; }
    }
}
