using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using PayPal;
using PayPal.Api;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    public class PayPalWrapper : IMerchantAccount<TransactionRequest>
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

        public PayPalWrapper()
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

        #region Old
        //*
        public TransactionDetail AuthorizeCapture(TransactionRequest info)
        {
           if (info.TransactionType != TransactionType.AuthorizeCapture)
                return null;
            if (info.Mode == ServiceMode.Test)
            {
                return new TransactionDetail()
                {
                    TransactionType = info.TransactionType,
                    Ammount = info.Ammount,
                    Approved = false,
                    AuthorizationCode = "",
                    CardNumber = info.CardNumber.GetLast(4),
                    FormKey = info.FormKey,
                    InvoiceNumber = info.UId.ToString(),
                    Message = "TEST TRANSACTION",
                    ResponseCode = "",
                    TransactionID = "TEST"
                };
            }
            var firstAndLast = Regex.Replace(info.NameOnCard, @"\s\w\s", " ");
            var spaceIndex = firstAndLast.IndexOf(' ');
            if (spaceIndex == -1)
            {
                info.FirstName = "";
                info.LastName = info.NameOnCard;
            }
            else
            {
                info.FirstName = firstAndLast.Substring(0, spaceIndex).Trim();
                info.LastName = firstAndLast.Substring(spaceIndex).Trim();
            }

            var config = PayPalConfiguration.GetAPIContext(UserName, Key);
            var itemList = new ItemList();
            itemList.items = new List<Item>() { new Item() { name = "Registration", currency = info.Form.Currency.GetPayPalValue(), price = info.Ammount.ToString(), quantity = "1", sku = info.Registrant.UId.ToString() } };
            var bAddy = new Address()
            {
                postal_code = info.Zip,
                city = info.City,
                country_code = info.Country,
                line1 = info.Address,
                line2 = info.Address2,
                phone = info.Phone,
                state = info.State
            };
            var ccard = new PayPal.Api.CreditCard()
            {
                billing_address = bAddy,
                cvv2 = info.CVV,
                expire_month = int.Parse(info.ExpMonthAndYear.Substring(0, 2)),
                expire_year = 2000 + int.Parse(info.ExpMonthAndYear.Substring(2, 2)),
                first_name = info.FirstName,
                last_name = info.LastName,
                type = info.CardType,
                number = info.CardNumber
            };
            var amnt = new Amount()
            {
                currency = info.Currency.GetPayPalValue(),
                total = info.Ammount.ToString()
            };
            var tran = new Transaction()
            {
                amount = amnt,
                description = "Registration fee for company " + info.Form.Company.Name + " on form " + info.Form.Name + ".",
                item_list = itemList,
                invoice_number = info.UId.ToString()
            };
            var trans = new List<Transaction>() { tran };
            var fundInsts = new List<FundingInstrument>() { new FundingInstrument() { credit_card = ccard } };
            var payr = new Payer()
            {
                funding_instruments = fundInsts,
                payment_method = "credit_card"
            };
            var payment = new Payment()
            {
                intent = "sale",
                payer = payr,
                transactions = trans
            };
            TransactionDetail td;
            try
            {
                td = payment.Create(config, info);
            }
            catch (Exception e)
            {
                var pex = e as PayPalException;
                using (var logger = new Logging.Logger())
                {
                    logger.LoggingMethod = "PayPalWrapper";
                    logger.Error(e);
                }
                var message = e.Message;
                if (pex != null)
                {
                    message = pex.Message + ": " + pex.HResult + " : " + pex.HelpLink;
                }
                // Here we create an error log.
                var id = Guid.NewGuid();
                try
                {
                    var path = @"C:\paypal log\" + id.ToString() + ".txt";
                    using (var write = new StreamWriter(path))
                    {
                        write.WriteLine(JsonConvert.SerializeObject(e));
                    }
                }
                catch (Exception)
                { }

                td = new TransactionDetail()
                {
                    TransactionType = info.TransactionType,
                    Ammount = info.Ammount,
                    Approved = false,
                    AuthorizationCode = "",
                    CardNumber = info.CardNumber.GetLast(4),
                    FormKey = info.FormKey,
                    InvoiceNumber = info.UId.ToString(),
                    Message = "Failed do to an exception: " + message,
                    ResponseCode = id.ToString(),
                    TransactionID = "FAILURE"
                };
                info.Details.Add(td);
            }
            return td;
        }

        public TransactionDetail Authorize(TransactionRequest info)
        {
            if (info.TransactionType != TransactionType.Authorize)
                return null;
            if (info.Mode == ServiceMode.Test)
            {
                return new TransactionDetail()
                {
                    TransactionType = info.TransactionType,
                    Ammount = info.Ammount,
                    Approved = false,
                    AuthorizationCode = "",
                    CardNumber = info.CardNumber.GetLast(4),
                    FormKey = info.FormKey,
                    InvoiceNumber = info.UId.ToString(),
                    Message = "TEST TRANSACTION",
                    ResponseCode = "",
                    TransactionID = "TEST"
                };
            }
            var config = PayPalConfiguration.GetAPIContext(UserName, Key);
            var itemList = new ItemList();
            itemList.items = new List<Item>() { new Item() { name = "Registration", currency = info.Form.Currency.GetPayPalValue(), price = info.Ammount.ToString(), quantity = "1", sku = "" } };
            var bAddy = new Address()
            {
                postal_code = info.Zip
            };
            var ccard = new PayPal.Api.CreditCard()
            {
                billing_address = bAddy,
                cvv2 = info.CVV,
                expire_month = int.Parse(info.ExpMonthAndYear.Substring(0, 2)),
                expire_year = int.Parse(info.ExpMonthAndYear.Substring(2, 2)),
                first_name = info.FirstName,
                last_name = info.LastName,
                type = CCHelper.GetCardType(info.CardNumber).GetStringValue()
            };
            var amnt = new Amount()
            {
                currency = info.Currency.GetPayPalValue(),
                total = info.Ammount.ToString()
            };
            var tran = new Transaction()
            {
                amount = amnt,
                description = "Registration fee for company " + info.Form.Company.Name + " on form " + info.Form.Name + ".",
                item_list = itemList,
                invoice_number = info.UId.ToString()
            };
            var trans = new List<Transaction>() { tran };
            var fundInsts = new List<FundingInstrument>() { new FundingInstrument() { credit_card = ccard } };
            var payr = new Payer()
            {
                funding_instruments = fundInsts,
                payment_method = "credit_card"
            };
            var payment = new Payment()
            {
                intent = "authorize",
                payer = payr,
                transactions = trans
            };
            TransactionDetail td;
            try
            {
                td = payment.Create(config, info);
            }
            catch (Exception e)
            {
                td = new TransactionDetail()
                {
                    TransactionType = info.TransactionType,
                    Ammount = info.Ammount,
                    Approved = false,
                    AuthorizationCode = "",
                    CardNumber = info.CardNumber.GetLast(4),
                    FormKey = info.FormKey,
                    InvoiceNumber = info.UId.ToString(),
                    Message = "Failed do to an exception: " + e.Message,
                    ResponseCode = "",
                    TransactionID = "FAILURE"
                };
                info.Details.Add(td);
            }
            return td;
        }

        public TransactionDetail Credit(decimal refundAmmount, TransactionDetail detail, FormsRepository repository, bool test = false)
        {
            var info = detail;
            if (info == null)
                return null;
            if (!info.Approved && !(info.TransactionType == TransactionType.AuthorizeCapture || info.TransactionType == TransactionType.Capture))
                return null;
            if (info.TransactionRequest.Mode == ServiceMode.Test)
            {
                return new TransactionDetail()
                {
                    TransactionType = TransactionType.Credit,
                    Ammount = info.Ammount,
                    Approved = false,
                    AuthorizationCode = "",
                    CardNumber = info.CardNumber.GetLast(4),
                    FormKey = info.FormKey,
                    InvoiceNumber = info.UId.ToString(),
                    Message = "TEST TRANSACTION",
                    ResponseCode = "",
                    TransactionID = "TEST"
                };
            }
            var config = PayPalConfiguration.GetAPIContext(UserName, Key);
            var amount = new Amount()
            {
                currency = info.TransactionRequest.Currency.GetPayPalValue(),
                total = refundAmmount.ToString()
            };
            var refund = new Refund()
            {
                amount = amount,
                capture_id = info.TransactionID,
                sale_id = info.TransactionID
            };
            var sale = new Sale()
            {
                id = info.TransactionID
            };
            TransactionDetail td;
            try
            {
                td = sale.Refund(config, refund, info);
            }
            catch (Exception e)
            {
                td = new TransactionDetail()
                {
                    TransactionType = TransactionType.Credit,
                    Ammount = info.Ammount,
                    Approved = false,
                    AuthorizationCode = "",
                    CardNumber = info.CardNumber.GetLast(4),
                    FormKey = info.FormKey,
                    InvoiceNumber = info.UId.ToString(),
                    Message = "Failed do to an exception: " + e.Message,
                    ResponseCode = "",
                    TransactionID = "FAILURE"
                };
                info.Credits.Add(td);
                info.TransactionRequest.Details.Add(td);
            }
            return td;
        }


        public TransactionDetail Credit(decimal refundAmmount, TransactionDetail detail, bool test = false)
        {
            var info = detail;
            if (info == null)
                return null;
            if (!info.Approved && !(info.TransactionType == TransactionType.AuthorizeCapture || info.TransactionType == TransactionType.Capture))
                return null;
            if (info.TransactionRequest.Mode == ServiceMode.Test)
            {
                return new TransactionDetail()
                {
                    TransactionType = TransactionType.Credit,
                    Ammount = info.Ammount,
                    Approved = false,
                    AuthorizationCode = "",
                    CardNumber = info.CardNumber.GetLast(4),
                    FormKey = info.FormKey,
                    InvoiceNumber = info.UId.ToString(),
                    Message = "TEST TRANSACTION",
                    ResponseCode = "",
                    TransactionID = "TEST"
                };
            }
            var config = PayPalConfiguration.GetAPIContext(UserName, Key);
            var amount = new Amount()
            {
                currency = info.TransactionRequest.Currency.GetPayPalValue(),
                total = refundAmmount.ToString()
            };
            var refund = new Refund()
            {
                amount = amount,
                capture_id = info.TransactionID,
                sale_id = info.TransactionID
            };
            var sale = new Sale()
            {
                id = info.TransactionID
            };
            TransactionDetail td;
            try
            {
                td = sale.Refund(config, refund, info);
            }
            catch (Exception e)
            {
                td = new TransactionDetail()
                {
                    TransactionType = TransactionType.Credit,
                    Ammount = info.Ammount,
                    Approved = false,
                    AuthorizationCode = "",
                    CardNumber = info.CardNumber.GetLast(4),
                    FormKey = info.FormKey,
                    InvoiceNumber = info.UId.ToString(),
                    Message = "Failed do to an exception: " + e.Message,
                    ResponseCode = "",
                    TransactionID = "FAILURE"
                };
                info.Credits.Add(td);
                info.TransactionRequest.Details.Add(td);
            }
            return td;
        }
        //*/
        #endregion
    }

    public static class PayPalConfiguration
    {
        // Create the configuration map that contains mode and other optional configuration details.
        public static Dictionary<string, string> GetConfig(string clientId, string clientSecret, bool live)
        {
            return new Dictionary<string, string>()
            {
                { "connectionTimeout", "36000" },
                { "requestRetries", "1" },
                { "mode", (live && clientId != "ARnIOxC_OSO6WVaz_sAeai_EMQ8bCuEg39TsU3YXr0_3hQzR-zYvepGbmMTS") ? "live" : "sandbox" },
                { "clientId", clientId },
                { "clientSecret", clientSecret}
            };
        }

        // Create accessToken
        private static string GetAccessToken(string ClientId, string ClientSecret, bool live)
        {
            // ###AccessToken
            // Retrieve the access token from
            // OAuthTokenCredential by passing in
            // ClientID and ClientSecret
            // It is not mandatory to generate Access Token on a per call basis.
            // Typically the access token can be generated once and
            // reused within the expiry window                
            try
            {
                var dic = GetConfig(ClientId, ClientSecret, live);
                var accessTokenHolder = new OAuthTokenCredential(ClientId, ClientSecret, dic);
                var accessToken = accessTokenHolder.GetAccessToken();
                return accessToken;
            }
            catch (Exception e)
            {
                Console.Write(e.StackTrace);
            }
            return null;
        }

        // Returns APIContext object
        public static APIContext GetAPIContext(string ClientId, string ClientSecret, bool live = true)
        {
            // ### Api Context
            // Pass in a `APIContext` object to authenticate 
            // the call and to send a unique request id 
            // (that ensures idempotency). The SDK generates
            // a request id if you do not pass one explicitly. 
            APIContext apiContext = new APIContext(GetAccessToken(ClientId, ClientSecret, live));
            apiContext.Config = GetConfig(ClientId, ClientSecret, live);

            // Use this variant if you want to pass in a request id  
            // that is meaningful in your application, ideally 
            // a order id.
            // String requestId = Long.toString(System.nanoTime();
            // APIContext apiContext = new APIContext(GetAccessToken(), requestId ));

            return apiContext;
        }

        public static TransactionDetail Create(this Payment payment, APIContext config, TransactionRequest request)
        {
            var response = payment.Create(config);
            var approved = response.state == "approved";
            var amount = 0.00m;
            if (response.transactions.Count > 0)
               decimal.TryParse(response.transactions.First().amount.total, out amount);
            var saleId = "none";
            var saleReason = response.state;
            if (response.transactions.Count > 0 && response.transactions.First().related_resources.Count > 0 && response.transactions.First().related_resources[0].sale != null)
            {
                saleReason = response.transactions.First().related_resources[0].sale.reason_code;
                saleId = response.transactions.First().related_resources[0].sale.id;
            }
            var td = new TransactionDetail()
            {
                TransactionType = request.TransactionType,
                Ammount = amount,
                Approved = approved,
                AuthorizationCode = "",
                CardNumber = response.payer.funding_instruments.First().credit_card.number,
                FormKey = request.FormKey,
                InvoiceNumber = request.UId.ToString(),
                Message = saleReason,
                ResponseCode = "",
                TransactionID = saleId
            };
            request.Details.Add(td);
            return td;
        }

        public static TransactionDetail Refund(this Sale sale, APIContext config, Refund refund, TransactionDetail request)
        {
            var response = sale.Refund(config, refund);
            var approved = response.state == "completed";
            var amount = 0.00m;
            if (response.amount != null)
                amount = decimal.Parse(response.amount.total ?? "0.00");
            var td = new TransactionDetail()
            {
                TransactionType = TransactionType.Credit,
                Ammount = amount,
                Approved = approved,
                AuthorizationCode = "",
                FormKey = request.FormKey,
                InvoiceNumber = request.UId.ToString(),
                Message = response.state,
                ResponseCode = "",
                TransactionID = response.id
            };
            request.Credits.Add(td);
            request.TransactionRequest.Details.Add(td);
            return td;
        }

    }
}
