using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using USAePayAPI;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    public class USAePay : IMerchantAccount<TransactionRequest>
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


        public USAePay()
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
            var gateway = new USAePayAPI.USAePay();
            if (test)
            {
                gateway.SourceKey = "cJhegI1309peY6Ymv92qFL0XJ8H91FBL";
                gateway.Pin = "123456789";
                gateway.UseSandbox = true;
            }
            else
            {
                gateway.SourceKey = UserName;
                gateway.Pin = Key;
            }
            gateway.Amount = ammount;
            gateway.RefNum = detail.TransactionID;

            var code = GetCurrencyCode(detail.TransactionRequest.Currency);
            if (code != null)
                gateway.Currency = code;

            bool success = false;
            var td = new TransactionDetail()
            {
                Ammount = ammount,
                TransactionType = TransactionType.Credit
            };
            if (!test)
            {
                var authorized = false;
                try
                {
                    success = gateway.Credit();
                }
                catch (Exception)
                {
                    td.ResponseCode = "GATEWAY COMMUNICATION FAILURE";
                    td.Message = "COULD NOT CONNECT TO GATEWAY.";
                    td.Approved = false;
                }
                if (success)
                {
                    if (gateway.ResultCode == "A")
                    {
                        td.ResponseCode = gateway.ResultCode;
                        td.CVVResponse = gateway.Cvv2Result;
                        td.AuthorizationCode = gateway.AuthCode;
                        td.Message = gateway.ProfilerResponse;
                        td.TransactionID = gateway.ResultRefNum;
                        td.Approved = true;
                        authorized = true;
                    }
                    else if (gateway.ResultCode == "D")
                    {
                        td.ResponseCode = gateway.ResultCode;
                        td.TransactionID = gateway.ResultRefNum;
                        td.Message = "Transaction Declined. ";
                        td.Message += gateway.ProfilerResponse;
                        td.Approved = false;
                    }
                    else
                    {
                        td.ResponseCode = gateway.ResultCode;
                        td.TransactionID = gateway.ResultRefNum;
                        td.Message = "Transaction Error. ";
                        td.Message += gateway.ProfilerResponse;
                        td.Approved = false;
                    }
                }
                if (!authorized)
                {
                    try
                    {
                        success = gateway.Void();
                    }
                    catch (Exception)
                    {
                        td.ResponseCode = "GATEWAY COMMUNICATION FAILURE";
                        td.Message = "COULD NOT CONNECT TO GATEWAY.";
                        td.Approved = false;
                    }
                    if (success)
                    {
                        if (gateway.ResultCode == "A")
                        {
                            td.ResponseCode = gateway.ResultCode;
                            td.CVVResponse = gateway.Cvv2Result;
                            td.AuthorizationCode = gateway.AuthCode;
                            td.Message = gateway.ProfilerResponse;
                            td.TransactionID = gateway.ResultRefNum;
                            td.Approved = true;
                            td.voided = true;
                            authorized = true;
                        }
                        else if (gateway.ResultCode == "D")
                        {
                            td.ResponseCode = gateway.ResultCode;
                            td.TransactionID = gateway.ResultRefNum;
                            td.Message = "Transaction Declined. ";
                            td.Message += gateway.ProfilerResponse;
                            td.Approved = false;
                        }
                        else
                        {
                            td.ResponseCode = gateway.ResultCode;
                            td.TransactionID = gateway.ResultRefNum;
                            td.Message = "Transaction Error. ";
                            td.Message += gateway.ProfilerResponse;
                            td.Approved = false;
                        }
                    }
                }
            }
            else
            {
                td.Approved = true;
                td.AuthorizationCode = "TEST";
                td.TransactionID = detail.TransactionID;
            }
            td.CaptureKey = detail.UId;
            td.Capture = detail;
            td.TransactionRequest = detail.TransactionRequest;
            td.TransactionRequestKey = detail.TransactionRequestKey;
            return td;
        }


        public TransactionDetail Credit(decimal ammount, TransactionDetail detail, FormsRepository repository, bool test = false)
        {
            var gateway = new USAePayAPI.USAePay();
            if (test)
            {
                gateway.SourceKey = "cJhegI1309peY6Ymv92qFL0XJ8H91FBL";
                gateway.Pin = "123456789";
                gateway.UseSandbox = true;
            }
            else
            {
                gateway.SourceKey = UserName;
                gateway.Pin = Key;
            }
            gateway.Amount = ammount;
            gateway.RefNum = detail.TransactionID;

            var code = GetCurrencyCode(detail.TransactionRequest.Currency);
            if (code != null)
                gateway.Currency = code;

            bool success = false;
            var td = new TransactionDetail()
            {
                Ammount = ammount,
                TransactionType = TransactionType.Credit
            };
            if (!test)
            {
                var authorized = false;
                try
                {
                    success = gateway.Credit();
                }
                catch (Exception)
                {
                    td.ResponseCode = "GATEWAY COMMUNICATION FAILURE";
                    td.Message = "COULD NOT CONNECT TO GATEWAY.";
                    td.Approved = false;
                }
                if (success)
                {
                    if (gateway.ResultCode == "A")
                    {
                        td.ResponseCode = gateway.ResultCode;
                        td.CVVResponse = gateway.Cvv2Result;
                        td.AuthorizationCode = gateway.AuthCode;
                        td.Message = gateway.ProfilerResponse;
                        td.TransactionID = gateway.ResultRefNum;
                        td.Approved = true;
                        authorized = true;
                    }
                    else if (gateway.ResultCode == "D")
                    {
                        td.ResponseCode = gateway.ResultCode;
                        td.TransactionID = gateway.ResultRefNum;
                        td.Message = "Transaction Declined. ";
                        td.Message += gateway.ProfilerResponse;
                        td.Approved = false;
                    }
                    else
                    {
                        td.ResponseCode = gateway.ResultCode;
                        td.TransactionID = gateway.ResultRefNum;
                        td.Message = "Transaction Error. ";
                        td.Message += gateway.ProfilerResponse;
                        td.Approved = false;
                    }
                }
                if (!authorized)
                {
                    try
                    {
                        success = gateway.Void();
                    }
                    catch (Exception)
                    {
                        td.ResponseCode = "GATEWAY COMMUNICATION FAILURE";
                        td.Message = "COULD NOT CONNECT TO GATEWAY.";
                        td.Approved = false;
                    }
                    if (success)
                    {
                        if (gateway.ResultCode == "A")
                        {
                            td.ResponseCode = gateway.ResultCode;
                            td.CVVResponse = gateway.Cvv2Result;
                            td.AuthorizationCode = gateway.AuthCode;
                            td.Message = gateway.ProfilerResponse;
                            td.TransactionID = gateway.ResultRefNum;
                            td.Approved = true;
                            td.voided = true;
                            authorized = true;
                        }
                        else if (gateway.ResultCode == "D")
                        {
                            td.ResponseCode = gateway.ResultCode;
                            td.TransactionID = gateway.ResultRefNum;
                            td.Message = "Transaction Declined. ";
                            td.Message += gateway.ProfilerResponse;
                            td.Approved = false;
                        }
                        else
                        {
                            td.ResponseCode = gateway.ResultCode;
                            td.TransactionID = gateway.ResultRefNum;
                            td.Message = "Transaction Error. ";
                            td.Message += gateway.ProfilerResponse;
                            td.Approved = false;
                        }
                    }
                }
            }
            else
            {
                td.Approved = true;
                td.AuthorizationCode = "TEST";
                td.TransactionID = detail.TransactionID;
            }
            td.CaptureKey = detail.UId;
            td.Capture = detail;
            td.TransactionRequest = detail.TransactionRequest;
            td.TransactionRequestKey = detail.TransactionRequestKey;
            return td;
        }

        public TransactionDetail Authorize(TransactionRequest info)
        {
            if (info.TransactionType != TransactionType.Authorize)
                return null;
            var gateway = new USAePayAPI.USAePay();
            if (info.Mode == Data.ServiceMode.Test)
            {
                gateway.SourceKey = "cJhegI1309peY6Ymv92qFL0XJ8H91FBL";
                gateway.Pin = "123456789";
                gateway.UseSandbox = true;
            }
            else
            {
                gateway.SourceKey = UserName;
                gateway.Pin = Key;
            }
            gateway.Amount = info.Ammount;
            gateway.Description = info.Description;
            gateway.CardHolder = info.NameOnCard;
            gateway.CardNumber = info.CardNumber;
            gateway.CardExp = info.ExpMonthAndYear;
            if (info.Form != null)
            {
                switch (info.Form.Currency)
                {
                    case Currency.Euro:
                        gateway.Currency = "978";
                        break;
                }
            }
            if (Validate)
            {
                gateway.BillingFirstName = info.FirstName;
                gateway.BillingLastName = info.LastName;
                gateway.BillingStreet = info.Address;
                gateway.BillingCity = info.City;
                gateway.BillingState = info.State;
                gateway.BillingZip = info.Zip;
                gateway.BillingCountry = info.Country;
                gateway.Cvv2 = info.CVV;
            }
            bool success = false;
            var td = new TransactionDetail()
            {
                Ammount = info.Ammount,
                CardNumber = info.CardNumber,
                FormKey = info.FormKey,
                CreditCardKey = info.CreditCardKey,
                TransactionType = info.TransactionType
            };
            try
            {
                success = gateway.AuthOnly();
            }
            catch (Exception)
            {
                td.ResponseCode = "GATEWAY COMMUNICATION FAILURE";
                td.Message = "COULD NOT CONNECT TO GATEWAY.";
                td.Approved = false;
                info.Success = false;
            }
            if (success)
            {
                if (gateway.ResultCode == "A")
                {
                    info.Success = true;
                    td.ResponseCode = gateway.ResultCode;
                    td.CVVResponse = gateway.Cvv2Result;
                    td.AuthorizationCode = gateway.AuthCode;
                    td.TransactionID = gateway.ResultRefNum;
                    td.Message = gateway.ProfilerResponse;
                    td.Approved = true;
                }
                else if (gateway.ResultCode == "D")
                {
                    info.Success = false;
                    td.ResponseCode = gateway.ResultCode;
                    td.TransactionID = gateway.ResultRefNum;
                    td.Message = "Transaction Declined. ";
                    td.Message += gateway.ProfilerResponse;
                    td.Approved = false;
                }
                else
                {
                    info.Success = false;
                    td.ResponseCode = gateway.ResultCode;
                    td.TransactionID = gateway.ResultRefNum;
                    td.Message = "Transaction Error. ";
                    td.Message = gateway.ProfilerResponse;
                    td.Approved = false;
                }
            }
            return td;
        }

        public TransactionDetail AuthorizeCapture(TransactionRequest info)
        {
            if (info.TransactionType != TransactionType.AuthorizeCapture)
                return null;
            var gateway = new USAePayAPI.USAePay();
            if (info.Mode == Data.ServiceMode.Test)
            {
                gateway.SourceKey = "w2F0QQG5jKc90a1R2B7i6y5Q7NBkeBJX";
                gateway.Pin = "1234567890";
                if (info.Form.Currency == Currency.Euro)
                {
                    gateway.SourceKey = "i5J5n4lgR2FXwnsmcC14UQY28V9ZsxCn";
                    gateway.Pin = "1234567890";
                }
            }
            else
            {
                gateway.SourceKey = UserName;
                gateway.Pin = Key;
            }
            gateway.Amount = info.Ammount;
            gateway.Description = info.Description;
            gateway.CardHolder = info.NameOnCard;
            gateway.CardNumber = info.CardNumber;
            gateway.CardExp = info.ExpMonthAndYear;
            gateway.Cvv2 = info.CVV;
            if (info.Form != null)
            {
                var code = GetCurrencyCode(info.Form.Currency);
                if (code != null)
                    gateway.Currency = code;
            }
            gateway.BillingFirstName = info.FirstName;
            gateway.BillingLastName = info.LastName; 
            gateway.BillingStreet = info.Address;
            gateway.BillingCity = info.City;
            gateway.BillingState = info.State;
            gateway.BillingZip = info.Zip;
            gateway.BillingCountry = info.Country;
            gateway.Comments = info.FormKey.ToString();
            
            bool success = false;
            var td = new TransactionDetail()
            {
                Ammount = info.Ammount,
                CardNumber = info.CardNumber,
                FormKey = info.FormKey,
                CreditCardKey = info.CreditCardKey,
                TransactionType = info.TransactionType
            };
            try
            {
                success = gateway.Sale();
            }
            catch (Exception)
            {
                td.ResponseCode = "GATEWAY COMMUNICATION FAILURE";
                td.Message = "COULD NOT CONNECT TO GATEWAY.";
                td.Approved = false;
                info.Success = false;
            }
            if (success)
            {
                if (gateway.ResultCode == "A")
                {
                    info.Success = true;
                    td.ResponseCode = gateway.ResultCode;
                    td.CVVResponse = gateway.Cvv2Result;
                    td.AuthorizationCode = gateway.AuthCode;
                    td.TransactionID = gateway.ResultRefNum;
                    td.Message = gateway.ProfilerResponse;
                    td.Approved = true;
                    td.ConversionRate = decimal.Parse(gateway.ConversionRate);
                }
                else if (gateway.ResultCode == "D")
                {
                    info.Success = false;
                    td.ResponseCode = gateway.ResultCode;
                    td.TransactionID = gateway.ResultRefNum;
                    td.Message = "Transaction Declined.";
                    td.Message += gateway.ProfilerResponse;
                    td.Approved = false;
                }
                else
                {
                    info.Success = false;
                    td.ResponseCode = gateway.ResultCode;
                    td.TransactionID = gateway.ResultRefNum;
                    td.Message = "Transaction Error. ";
                    td.Message = gateway.ProfilerResponse;
                    td.Approved = false;
                }
            }
            return td;
        }

        public string GetCurrencyCode(Currency currency)
        {
            switch (currency)
            {
                case Currency.Euro:
                    return "978";
            }
            return null;
        }
    }
}
