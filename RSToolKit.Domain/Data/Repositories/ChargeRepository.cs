using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Finances;

namespace RSToolKit.Domain.Data.Repositories
{
    public class ChargeRepository
        : RSRepository<Charge>
    {
        public ChargeRepository(Security.SecureUser user)
        { }

        public ChargeRepository(EFDbContext context, Security.SecureUser user)
        { }
    }
}
