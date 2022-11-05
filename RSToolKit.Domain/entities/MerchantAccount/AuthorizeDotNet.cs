using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using AuthorizeNet;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    public class AuthorizeDotNet : IMerchantAccount<TransactionRequest>
    {
        #region Properties

        public Guid UserUId { get; set; }
        public string Confirmation { get; set; }
        public Guid FormUId { get; set; }
        public Guid CompanyUId { get; set; }
        public string Description { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }
        public bool Validate { get; set; }
        public string UserName { get; set; }
        public string Key { get; set; }

        public AuthorizeDotNet()
        {
            Confirmation = "";
            FormUId = Guid.Empty;
            CompanyUId = Guid.Empty;
            Description = "";
            DateCreated = DateTimeOffset.UtcNow;
            DateModified = DateTimeOffset.UtcNow;
            Validate = true;
            UserName = "";
            Key = "";
        }

        #endregion
      
        public TransactionDetail AuthorizeCapture(TransactionRequest info)
        {
            if (info.TransactionType != TransactionType.AuthorizeCapture)
                return null;
            if (info.Mode == Data.ServiceMode.Test)
            {
                UserName = "94uJ6vRv";
                Key = "6Aag7U54M747u7w6";
            }
            var request = new AuthorizationRequest(info.CardNumber, info.ExpMonthAndYear, info.Ammount, info.Description);
            if (Validate)
                request.AddCustomer(info.Registrant.UId.ToString(), info.Registrant.Email, info.FirstName, info.LastName, info.Address, info.City, info.State, info.Zip);
            var gate = new Gateway(UserName, Key, info.Mode == Data.ServiceMode.Test);
            request.AddInvoice(info.SortingId.ToString());
            var response = gate.Send(request);
            var td = new TransactionDetail() { TransactionType = info.TransactionType, Ammount = response.Amount, Approved = response.Approved, AuthorizationCode = response.AuthorizationCode, CardNumber = response.CardNumber, FormKey = info.FormKey, InvoiceNumber = request.InvoiceNum, Message = response.Message, TransactionRequestKey = info.UId, ResponseCode = response.ResponseCode, TransactionID = response.TransactionID };
            return td;
        }

        public TransactionDetail Authorize(TransactionRequest info)
        {
            if (info.TransactionType != TransactionType.Authorize)
                return null;
            var request = new AuthorizationRequest(info.CardNumber, info.ExpMonthAndYear, info.Ammount, info.Description, false);
            if (info.Mode == Data.ServiceMode.Test)
            {
                UserName = "94uJ6vRv";
                Key = "6Aag7U54M747u7w6";
            }
            if (Validate)
                request.AddCustomer(info.Registrant.UId.ToString(), info.Registrant.Email, info.FirstName, info.LastName, info.Address, info.City, info.State, info.Zip);
            var gate = new Gateway(UserName, Key, info.Mode == Data.ServiceMode.Test);
            request.AddInvoice(info.SortingId.ToString());
            var response = gate.Send(request);
            var td = new TransactionDetail() { TransactionType = info.TransactionType, Ammount = response.Amount, Approved = response.Approved, AuthorizationCode = response.AuthorizationCode, CardNumber = response.CardNumber, FormKey = info.FormKey, InvoiceNumber = request.InvoiceNum, Message = response.Message, TransactionRequestKey = info.UId, ResponseCode = response.ResponseCode, TransactionID = response.TransactionID };
            return td;
        }

        public TransactionDetail Credit(decimal refundAmmount, TransactionDetail detail, FormsRepository repository, bool test = false)
        {
            var info = detail;
            if (info == null)
                return null;
            if (info.Approved && (info.TransactionType == TransactionType.Credit || info.TransactionType == TransactionType.Authorize))
                return null;
            var request = new CreditRequest(info.TransactionID, refundAmmount, info.TransactionRequest.LastFour);
            var gate = new Gateway(UserName, Key, test);
            request.AddInvoice(info.SortingId.ToString());
            TransactionDetail td;
            if (!test)
            {
                var response = gate.Send(request);
                td = new TransactionDetail() { TransactionType = TransactionType.Credit, Ammount = response.Amount, Approved = response.Approved, AuthorizationCode = response.AuthorizationCode, CardNumber = response.CardNumber, FormKey = info.FormKey, InvoiceNumber = request.InvoiceNum, Message = response.Message, TransactionRequestKey = info.UId, ResponseCode = response.ResponseCode, TransactionID = response.TransactionID };
            }
            else
            {
                td = new TransactionDetail() { TransactionType = TransactionType.Credit, Ammount = refundAmmount, Approved = true, AuthorizationCode = "TEST", CardNumber = "", FormKey = info.FormKey, InvoiceNumber = request.InvoiceNum, Message = "TEST", TransactionRequestKey = info.UId, ResponseCode = "TEST", TransactionID = "TEST" };
            }
            td.CaptureKey = detail.UId;
            td.Capture = detail;
            td.TransactionRequest = detail.TransactionRequest;
            td.TransactionRequestKey = detail.TransactionRequestKey;
            return td;
        }

        public TransactionDetail Credit(decimal refundAmmount, TransactionDetail detail, bool test = false)
        {
            var info = detail;
            if (info == null)
                return null;
            if (info.Approved && (info.TransactionType == TransactionType.Credit || info.TransactionType == TransactionType.Authorize))
                return null;
            var request = new CreditRequest(info.TransactionID, refundAmmount, info.TransactionRequest.LastFour);
            var gate = new Gateway(UserName, Key, test);
            request.AddInvoice(info.SortingId.ToString());
            TransactionDetail td;
            if (!test)
            {
                var response = gate.Send(request);
                td = new TransactionDetail() { TransactionType = TransactionType.Credit, Ammount = response.Amount, Approved = response.Approved, AuthorizationCode = response.AuthorizationCode, CardNumber = response.CardNumber, FormKey = info.FormKey, InvoiceNumber = request.InvoiceNum, Message = response.Message, TransactionRequestKey = info.UId, ResponseCode = response.ResponseCode, TransactionID = response.TransactionID };
            }
            else
            {
                td = new TransactionDetail() { TransactionType = TransactionType.Credit, Ammount = refundAmmount, Approved = true, AuthorizationCode = "TEST", CardNumber = "", FormKey = info.FormKey, InvoiceNumber = request.InvoiceNum, Message = "TEST", TransactionRequestKey = info.UId, ResponseCode = "TEST", TransactionID = "TEST" };
            }
            td.CaptureKey = detail.UId;
            td.Capture = detail;
            td.TransactionRequest = detail.TransactionRequest;
            td.TransactionRequestKey = detail.TransactionRequestKey;
            return td;
        }
    }
}
