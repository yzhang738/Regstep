using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Entities
{
    public enum FormStatus
    {
        [StringValue("Developement")]
        Developement = 0,
        [StringValue("Ready")]
        Ready = 1,
        [StringValue("Open")]
        Open = 2,
        [StringValue("Closed")]
        Closed = 3,
        [StringValue("Complete")]
        Complete = 4,
        [StringValue("Payment Complete")]
        PaymentComplete = 5,
        [StringValue("Archive")]
        Archive = 6,
        [StringValue("maintenance")]
        Maintenance = -1
    }
}
