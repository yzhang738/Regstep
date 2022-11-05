using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.XTrans;
using RSToolKit.Domain.Data;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    public class iPayWrapper : IMerchantAccount<TransactionRequest>
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
        public Dictionary<string, string> Parameters { get; set; }

        public iPayWrapper()
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

        public TransactionDetail Credit(decimal ammount, TransactionDetail detail, bool test = false)
        {
            if (!(detail.TransactionType == TransactionType.AuthorizeCapture || detail.TransactionType == TransactionType.Capture))
                throw new InvalidOperationException("The transaction detail to be refunded must be of type AuthorizeCapture or Capture.");
            if (!detail.Approved)
                throw new InvalidOperationException("The transaction detail must have been approved.");
            Processor gateway;
            var encryption = EncryptionModeType.NONE;
            if (Parameters.ContainsKey("encryption"))
            {
                switch (Parameters["encryption"])
                {
                    case "1":
                        encryption = EncryptionModeType.TripleDES;
                        break;
                    case "2":
                        encryption = EncryptionModeType.Rijndael;
                        break;
                    case "3":
                        encryption = EncryptionModeType.RC2;
                        break;
                    case "4":
                        encryption = EncryptionModeType.DES;
                        break;
                    default:
                        break;
                }
            }
            var secure = false;
            if (Parameters.ContainsKey("secure") && Parameters["secure"] == "true")
                secure = true;
            if (secure)
                gateway = new Processor("prd.txngw.com", 443, UserName, 3000);
            else
                gateway = new Processor("prd.txngw.com", 86, UserName, 3000);
            gateway.EncryptionMode = encryption;
            switch (gateway.EncryptionMode)
            {
                case EncryptionModeType.DES:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    break;
                case EncryptionModeType.RC2:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    break;
                case EncryptionModeType.Rijndael:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    break;
                case EncryptionModeType.TripleDES:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    gateway.AddEncryptionKey(Parameters["key2"]);
                    gateway.AddEncryptionKey(Parameters["key3"]);
                    break;
            }
            gateway.SetValue("TRANSACTION_INDICATOR", "7");
            gateway.SetValue("SERVICE", "CC");
            gateway.SetValue("SERVICE_TYPE", "CREDIT");
            gateway.SetValue("SERVICE_SUBTYPE", "REFUND");
            gateway.SetValue("SERVICE_FORMAT", "1010");
            gateway.SetValue("AMOUNT", ammount.ToString());
            gateway.SetValue("PIN", Key);
            gateway.SetValue("TRANSACTION_ID", detail.TransactionID);
            gateway.SetValue("TERMINAL_ID", Parameters["terminalid"]);
            gateway.SetValue("OPERATOR", "RegStep Technologies, LLC");
            gateway.SetValue("CURRENCY_CODE", detail.TransactionRequest.Form.Currency.GetiPayValue());
            if (Parameters.ContainsKey("currencyindicator") && !String.IsNullOrWhiteSpace(Parameters["currencyindicator"]))
                gateway.SetValue("CURRENCY_INDICATOR", Parameters["currencyindicator"]);
            gateway.Build();
            gateway.ProcessRequest();
            var td = new TransactionDetail();
            td.Ammount = detail.Ammount;
            var xml = new XmlDocument();
            xml.LoadXml(gateway.ResponseXml);
            var mrc = xml.SelectSingleNode("descendant::FIELD[@KEY='MRC']").InnerText;
            var arc = xml.SelectSingleNode("descendant::FIELD[@KEY='ARC']").InnerText;
            var response = xml.SelectSingleNode("descendant::FIELD[@KEY='RESPONSE_TEXT']").InnerText;
            var exchangeRate = "1";
            var approvalCode = "Not Approved";
            var transactionId = "Not Approved";
            var t_transactionId = xml.SelectSingleNode("descendant::FIELD[@KEY='TRANSACTION_ID']");
            if (t_transactionId != null)
                transactionId = t_transactionId.InnerText;
            td.ResponseCode = response;
            if (mrc == "00" && arc == "00")
            {
                td.Approved = true;
                var t_exchangeRate = xml.SelectSingleNode("descendant::FIELD[@KEY='EXCHANGE_RATE']");
                var t_approvalCode = xml.SelectSingleNode("descendant::FIELD[@KEY='APPROVAL_CODE']");
                if (t_exchangeRate != null)
                    exchangeRate = t_exchangeRate.InnerText;
                if (t_approvalCode != null)
                    approvalCode = t_approvalCode.InnerText;
            }
            else
            {
                td.Approved = false;
                td.ResponseCode += "ARC: " + arc + " and MRC: " + mrc;
            }
            td.AuthorizationCode = approvalCode;
            td.TransactionID = transactionId;
            td.TransactionType = TransactionType.Credit;
            td.ConversionRate = decimal.Parse(exchangeRate);
            td.FormKey = detail.FormKey;
            td.InvoiceNumber = "";
            td.UId = Guid.NewGuid();
            detail.Credits.Add(td);
            detail.TransactionRequest.Details.Add(td);
            return td;
        }


        public TransactionDetail Credit(decimal ammount, TransactionDetail detail, FormsRepository repository, bool test = false)
        {
            if (!(detail.TransactionType == TransactionType.AuthorizeCapture || detail.TransactionType == TransactionType.Capture))
                throw new InvalidOperationException("The transaction detail to be refunded must be of type AuthorizeCapture or Capture.");
            if (!detail.Approved)
                throw new InvalidOperationException("The transaction detail must have been approved.");
            Processor gateway;
            var encryption = EncryptionModeType.NONE;
            if (Parameters.ContainsKey("encryption"))
            {
                switch (Parameters["encryption"])
                {
                    case "1":
                        encryption = EncryptionModeType.TripleDES;
                        break;
                    case "2":
                        encryption = EncryptionModeType.Rijndael;
                        break;
                    case "3":
                        encryption = EncryptionModeType.RC2;
                        break;
                    case "4":
                        encryption = EncryptionModeType.DES;
                        break;
                    default:
                        break;
                }
            }
            var secure = false;
            if (Parameters.ContainsKey("secure") && Parameters["secure"] == "true")
                secure = true;
            if (secure)
                gateway = new Processor("prd.txngw.com", 443, UserName, 3000);
            else
                gateway = new Processor("prd.txngw.com", 86, UserName, 3000);
            gateway.EncryptionMode = encryption;
            switch (gateway.EncryptionMode)
            {
                case EncryptionModeType.DES:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    break;
                case EncryptionModeType.RC2:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    break;
                case EncryptionModeType.Rijndael:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    break;
                case EncryptionModeType.TripleDES:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    gateway.AddEncryptionKey(Parameters["key2"]);
                    gateway.AddEncryptionKey(Parameters["key3"]);
                    break;
            }
            gateway.SetValue("TRANSACTION_INDICATOR", "7");
            gateway.SetValue("SERVICE", "CC");
            gateway.SetValue("SERVICE_TYPE", "CREDIT");
            gateway.SetValue("SERVICE_SUBTYPE", "REFUND");
            gateway.SetValue("SERVICE_FORMAT", "1010");
            gateway.SetValue("AMOUNT", ammount.ToString());
            gateway.SetValue("PIN", Key);
            gateway.SetValue("TRANSACTION_ID", detail.TransactionID);
            gateway.SetValue("TERMINAL_ID", Parameters["terminalid"]);
            gateway.SetValue("OPERATOR", "RegStep Technologies, LLC");
            gateway.SetValue("CURRENCY_CODE", detail.TransactionRequest.Form.Currency.GetiPayValue());
            if (Parameters.ContainsKey("currencyindicator") && !String.IsNullOrWhiteSpace(Parameters["currencyindicator"]))
                gateway.SetValue("CURRENCY_INDICATOR", Parameters["currencyindicator"]);
            gateway.Build();
            gateway.ProcessRequest();
            var td = new TransactionDetail();
            td.Ammount = detail.Ammount;
            var xml = new XmlDocument();
            xml.LoadXml(gateway.ResponseXml);
            var mrc = xml.SelectSingleNode("descendant::FIELD[@KEY='MRC']").InnerText;
            var arc = xml.SelectSingleNode("descendant::FIELD[@KEY='ARC']").InnerText;
            var response = xml.SelectSingleNode("descendant::FIELD[@KEY='RESPONSE_TEXT']").InnerText;
            var exchangeRate = "1";
            var approvalCode = "Not Approved";
            var transactionId = "Not Approved";
            var t_transactionId = xml.SelectSingleNode("descendant::FIELD[@KEY='TRANSACTION_ID']");
            if (t_transactionId != null)
                transactionId = t_transactionId.InnerText;
            td.ResponseCode = response;
            if (mrc == "00" && arc == "00")
            {
                td.Approved = true;
                var t_exchangeRate = xml.SelectSingleNode("descendant::FIELD[@KEY='EXCHANGE_RATE']");
                var t_approvalCode = xml.SelectSingleNode("descendant::FIELD[@KEY='APPROVAL_CODE']");
                if (t_exchangeRate != null)
                    exchangeRate = t_exchangeRate.InnerText;
                if (t_approvalCode != null)
                    approvalCode = t_approvalCode.InnerText;
            }
            else
            {
                td.Approved = false;
                td.ResponseCode += "ARC: " + arc + " and MRC: " + mrc;
            }
            td.AuthorizationCode = approvalCode;
            td.TransactionID = transactionId;
            td.TransactionType = TransactionType.Credit;
            td.ConversionRate = decimal.Parse(exchangeRate);
            td.FormKey = detail.FormKey;
            td.InvoiceNumber = "";
            td.UId = Guid.NewGuid();
            detail.Credits.Add(td);
            detail.TransactionRequest.Details.Add(td);
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
            Processor gateway;
            var encryption = EncryptionModeType.NONE;
            if (Parameters.ContainsKey("encryption"))
            {
                switch (Parameters["encryption"])
                {
                    case "1":
                        encryption = EncryptionModeType.TripleDES;
                        break;
                    case "2":
                        encryption = EncryptionModeType.Rijndael;
                        break;
                    case "3":
                        encryption = EncryptionModeType.RC2;
                        break;
                    case "4":
                        encryption = EncryptionModeType.DES;
                        break;
                    default:
                        break;
                }
            }
            var secure = false;
            var cardType = CCHelper.GetCardType(info.CardNumber);
            var amexCompany = Parameters.ContainsKey("amexcompanynumber") ? Parameters["amexcompanynumber"] : null;
            if (cardType == CreditCardType.AmericanExpress && amexCompany == null)
                return new TransactionDetail()
                {
                    TransactionRequest = info,
                    Approved = false,
                    ResponseCode = "Amex is not accepted."
                };
            if (Parameters.ContainsKey("secure") && Parameters["secure"] == "True")
                secure = true;
            gateway = new Processor("prd.txngw.com", (secure ? 443 : 86), (cardType != CreditCardType.AmericanExpress ? UserName : amexCompany), 3000);
            gateway.EncryptionMode = encryption;
            switch (gateway.EncryptionMode)
            {
                case EncryptionModeType.DES:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    break;
                case EncryptionModeType.RC2:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    break;
                case EncryptionModeType.Rijndael:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    break;
                case EncryptionModeType.TripleDES:
                    gateway.AddEncryptionKey(Parameters["key1"]);
                    gateway.AddEncryptionKey(Parameters["key2"]);
                    gateway.AddEncryptionKey(Parameters["key3"]);
                    break;
            }
            gateway.SetValue("TRANSACTION_INDICATOR", "7");
            gateway.SetValue("SERVICE", "CC");
            gateway.SetValue("SERVICE_TYPE", "DEBIT");
            gateway.SetValue("SERVICE_SUBTYPE", "SALE");
            gateway.SetValue("SERVICE_FORMAT", "1010");
            gateway.SetValue("ACCOUNT_NUMBER", info.CardNumber);
            gateway.SetValue("FIRST_NAME", info.FirstName);
            gateway.SetValue("LAST_NAME", info.LastName);
            if (!String.IsNullOrWhiteSpace(Key))
                gateway.SetValue("PIN", Key);
            gateway.SetValue("CURRENCY_CODE", info.Form.Currency.GetiPayValue());
            if (Parameters.ContainsKey("currencyindicator") && !String.IsNullOrWhiteSpace(Parameters["currencyindicator"]))
                gateway.SetValue("CURRENCY_INDICATOR", Parameters["currencyindicator"]);
            gateway.SetValue("CVV", info.CVV);
            gateway.SetValue("AMOUNT", info.Ammount.ToString());
            gateway.SetValue("EXPIRATION", info.ExpMonthAndYear);
            gateway.SetValue("TERMINAL_ID", Parameters["terminalid"]);
            gateway.SetValue("OPERATOR", "RegStep Technologies, LLC");
            gateway.Build();
            gateway.ProcessRequest();
            var td = new TransactionDetail();
            td.TransactionRequest = info;
            td.Ammount = info.Ammount;
            var xml = new XmlDocument();
            xml.LoadXml(gateway.ResponseXml);
            var mrc = xml.DocumentElement.SelectSingleNode("//FIELD[@KEY='MRC']").InnerText;
            var arc = xml.SelectSingleNode("descendant::FIELD[@KEY='ARC']").InnerText;
            var response = xml.SelectSingleNode("descendant::FIELD[@KEY='RESPONSE_TEXT']").InnerText;
            var exchangeRate = "1";
            var approvalCode = "Not Approved";
            var transactionId = "Not Approved";
            var t_transactionId = xml.SelectSingleNode("descendant::FIELD[@KEY='TRANSACTION_ID']");
            if (t_transactionId != null)
                transactionId = t_transactionId.InnerText;
            td.ResponseCode = response;
            if (mrc == "00" && arc == "00")
            {
                td.Approved = true;
                var t_exchangeRate = xml.SelectSingleNode("descendant::FIELD[@KEY='EXCHANGE_RATE']");
                var t_approvalCode = xml.SelectSingleNode("descendant::FIELD[@KEY='APPROVAL_CODE']");
                var t_cvvResponse = xml.SelectSingleNode("descendant::FIELD[@KEY='CVV_RESPONSE']");
                if (t_exchangeRate != null)
                    exchangeRate = t_exchangeRate.InnerText;
                if (t_approvalCode != null)
                    approvalCode = t_approvalCode.InnerText;
                if (t_cvvResponse != null)
                    td.CVVResponse = t_cvvResponse.InnerText;
            }
            else
            {
                td.ResponseCode += "ARC: " + arc + " and MRC: " + mrc;
                td.Approved = false;
            }
            td.AuthorizationCode = approvalCode;
            td.TransactionID = transactionId;
            td.TransactionType = info.TransactionType;
            td.ConversionRate = decimal.Parse(exchangeRate);
            td.FormKey = info.FormKey;
            td.InvoiceNumber = "";
            td.UId = Guid.NewGuid();
            info.Details.Add(td);
            return td;
        }

    }
}
