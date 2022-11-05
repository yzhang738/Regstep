using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.MerchantAccount;

namespace RSToolKit.Domain.Data
{
    public interface IMerchantAccount<TReq>
        where TReq : IMerchantAccountProccessInfo
    {
        Guid UserUId { get; set; }
        string Confirmation { get; set; }
        Guid FormUId { get; set; }
        Guid CompanyUId { get; set; }
        string Description { get; set; }
        DateTimeOffset DateCreated { get; set; }
        DateTimeOffset DateModified { get; set; }
        bool Validate { get; set; }
        string UserName { get; set; }
        string Key { get; set; }

        TransactionDetail Credit(decimal ammount, TransactionDetail detail, FormsRepository repository, bool test = false);
        TransactionDetail Authorize(TReq info);
        TransactionDetail AuthorizeCapture(TReq info);
        TransactionDetail Credit(decimal ammount, TransactionDetail detail, bool test = false);
    }
}
