using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Entities
{
    public enum RSVPType
    {
        [StringValue("All")]
        None = -1,
        [StringValue("Accept")]
        Accept = 1,
        [StringValue("Decline")]
        Decline = 0
    }
}
