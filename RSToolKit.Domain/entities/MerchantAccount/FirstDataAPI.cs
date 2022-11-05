using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using RSToolKit.Domain.Data;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using RSToolKit.Domain;
using RSToolKit.Domain.FirstData;
using Newtonsoft.Json;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    public class FirstDataAPI : IMerchantAccount<TransactionRequest>
    {
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
        public string StoreNumber { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public FirstDataAPI()
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
            StoreNumber = "";
        }

        public TransactionDetail Credit(decimal ammount, TransactionDetail detail, bool test = false)
        {
            if (!(detail.TransactionType == TransactionType.AuthorizeCapture || detail.TransactionType == TransactionType.Capture))
                throw new InvalidOperationException("The transaction request must be of type AuthorizeCapture or Capture.");
            if (detail.Ammount < ammount)
                throw new InvalidOperationException("The amount cannot be more than the amount of the previous authorize and capture.");
            var td = new TransactionDetail()
            {
                Ammount = ammount,
                TransactionType = TransactionType.Credit
            };
            ServicePointManager.Expect100Continue = false;
            var order = new FDGGWSApiOrderService();
            order.Url = @"https://ws.firstdataglobalgateway.com/fdggwsapi/services/order.wsdl";
            order.ClientCertificates.Add(X509Certificate.CreateFromCertFile("C:\\FDGGWSClient\\" + UserName + ".pem"));
            order.Credentials = new NetworkCredential(UserName, Key);
            var orderReq = new FDGGWSApiOrderRequest();
            var trans = new Transaction();
            var txType = new CreditCardTxType();
            txType.Type = CreditCardTxTypeType.@return;
            var tDet = new FirstData.TransactionDetails();
            tDet.OrderId = detail.TransactionID;
            trans.TransactionDetails = tDet;
            trans.Items = new object[] { txType };
            orderReq.Item = trans;
            trans.Payment = new Payment() { ChargeTotal = ammount };
            FDGGWSApiOrderResponse orderResponse = null;
            try
            {
                orderResponse = order.FDGGWSApiOrder(orderReq);
                td.Approved = orderResponse.ProcessorResponseMessage == "APPROVED" || orderResponse.TransactionResult == "APPROVED";
                if (!td.Approved)
                {
                    td.Message = (orderResponse.ErrorMessage ?? "") + orderResponse.ProcessorResponseMessage;
                }
                else
                {
                    td.Message = "Approved";
                    td.ResponseCode = orderResponse.TDate;
                    td.Response = JsonConvert.SerializeObject(orderResponse);
                    td.TransactionID = orderResponse.OrderId;
                    detail.Credits.Add(td);
                    detail.TransactionRequest.Details.Add(td);
                }
                //*/
            }
            catch (Exception ex)
            {
                var iLog = new Logging.Logger();
                var err = iLog.Error(ex);
                td.Message = ("Failed: " + ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : "")).Trim();
            }
            return td;
        }

        public TransactionDetail Credit(decimal ammount, TransactionDetail detail, FormsRepository repository, bool test = false)
        {
            if (!(detail.TransactionType == TransactionType.AuthorizeCapture || detail.TransactionType == TransactionType.Capture))
                throw new InvalidOperationException("The transaction request must be of type AuthorizeCapture or Capture.");
            if (detail.Ammount < ammount)
                throw new InvalidOperationException("The amount cannot be more than the amount of the previous authorize and capture.");
            var td = new TransactionDetail()
            {
                Ammount = ammount,
                TransactionType = TransactionType.Credit
            };
            ServicePointManager.Expect100Continue = false;
            var order = new FDGGWSApiOrderService();
            order.Url = @"https://ws.firstdataglobalgateway.com/fdggwsapi/services/order.wsdl";
            order.ClientCertificates.Add(X509Certificate.CreateFromCertFile("C:\\FDGGWSClient\\" + UserName + ".pem"));
            order.Credentials = new NetworkCredential(UserName, Key);
            var orderReq = new FDGGWSApiOrderRequest();
            var trans = new Transaction();
            var txType = new CreditCardTxType();
            txType.Type = CreditCardTxTypeType.@return;
            var tDet = new FirstData.TransactionDetails();
            tDet.OrderId = detail.TransactionID;
            trans.TransactionDetails = tDet;
            trans.Items = new object[] { txType };
            orderReq.Item = trans;
            trans.Payment = new Payment() { ChargeTotal = ammount };
            FDGGWSApiOrderResponse orderResponse = null;
            try
            {
                orderResponse = order.FDGGWSApiOrder(orderReq);
                td.Approved = orderResponse.ProcessorResponseMessage == "APPROVED" || orderResponse.TransactionResult == "APPROVED";
                if (!td.Approved)
                {
                    td.Message = (orderResponse.ErrorMessage ?? "") + orderResponse.ProcessorResponseMessage;
                }
                else
                {
                    td.Message = "Approved";
                    td.ResponseCode = orderResponse.TDate;
                    td.Response = JsonConvert.SerializeObject(orderResponse);
                    td.TransactionID = orderResponse.OrderId;
                    detail.Credits.Add(td);
                    detail.TransactionRequest.Details.Add(td);
                }
                //*/
            }
            catch (Exception ex)
            {
                var iLog = new Logging.Logger();
                var err = iLog.Error(ex);
                td.Message = ("Failed: " + ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : "")).Trim();
            }
            return td;
        }

        public TransactionDetail Authorize(TransactionRequest info)
        {
            throw new NotImplementedException();
        }

        public TransactionDetail AuthorizeCapture(TransactionRequest info)
        {
            if (info.TransactionType != TransactionType.AuthorizeCapture)
                throw new InvalidOperationException("The transaction request must be of type AuthorizeCapture.");
            var td = new TransactionDetail()
            {
                Ammount = info.Ammount,
                TransactionType = TransactionType.AuthorizeCapture
            };
            info.Details.Add(td);
            ServicePointManager.Expect100Continue = false;
            var order = new FDGGWSApiOrderService();
            order.Url = @"https://ws.firstdataglobalgateway.com/fdggwsapi/services/order.wsdl";
            order.ClientCertificates.Add(X509Certificate.CreateFromCertFile("C:\\FDGGWSClient\\" + UserName + ".pem"));
            order.Credentials = new NetworkCredential(UserName, Key);
            var orderReq = new FDGGWSApiOrderRequest();
            var trans = new Transaction();
            var txType = new CreditCardTxType();
            txType.Type = CreditCardTxTypeType.sale;
            var cData = new CreditCardData();
            cData.ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.CardNumber, ItemsChoiceType.ExpMonth, ItemsChoiceType.ExpYear, ItemsChoiceType.CardCodeValue };
            cData.Items = new string[] { info.CardNumber, info.ExpMonthAndYear.Substring(0,2), info.ExpMonthAndYear.Substring(2,2), info.CVV };
            var billing = new Billing();
            billing.Zip = info.Zip;
            billing.Name = info.NameOnCard;
            trans.Items = new object[] { txType, cData };
            trans.Billing = billing;
            var payment = new Payment();
            payment.ChargeTotal = info.Ammount;
            trans.Payment = payment;
            orderReq.Item = trans;
            FDGGWSApiOrderResponse orderResponse = null;
            try
            {
                orderResponse = order.FDGGWSApiOrder(orderReq);
                td.Approved = orderResponse.ProcessorResponseMessage == "APPROVED";
                if (!td.Approved)
                {
                    td.Message = orderResponse.ErrorMessage ?? "";
                }
                else
                {
                    td.Message = "Approved";
                    td.ResponseCode = orderResponse.TDate;
                    td.Response = JsonConvert.SerializeObject(orderResponse);
                    td.TransactionID = orderResponse.OrderId;
                }
                //*/
            }
            catch (Exception ex)
            {
                var iLog = new Logging.Logger();
                var err = iLog.Error(ex);
                td.Message = "Failed; " + ex.Message;
            }
            return td;
        }
    }
}