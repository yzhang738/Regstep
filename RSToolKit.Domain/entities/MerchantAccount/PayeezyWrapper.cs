using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.Net.Http;
using System.Security.Cryptography;
using System.Globalization;
using Newtonsoft.Json;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    public class PayeezyWrapper
        : IMerchantAccount<TransactionRequest>
    {
        public static Dictionary<CreditCardType, string> CardType = new Dictionary<CreditCardType, string>()
        {
            { CreditCardType.Visa, "Visa" },
            { CreditCardType.MasterCard, "Mastercard" },
            { CreditCardType.Discover, "Discover" },
            { CreditCardType.AmericanExpress, "American Express" }
        };

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
        public string Secret { get; set; }
        public string Key { get; set; }
        public string Token { get; set; }
        public string CurrencyCode { get; set; }

        public PayeezyWrapper()
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

        protected string _GenerateHmac(int nonce, string currentTimestamp, string payload)
        {
            string message = Key + nonce.ToString() + currentTimestamp + Token + payload;
            HMAC hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
            byte[] hmacData = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(message));
            string hex = BitConverter.ToString(hmacData);
            hex = hex.Replace("-", "").ToLower();
            byte[] hexArray = Encoding.UTF8.GetBytes(hex);
            return Convert.ToBase64String(hexArray);
        }

        public TransactionDetail AuthorizeCapture(TransactionRequest request)
        {
            string currentTimestamp = this._GetEpochTimestampInMilliseconds();
            int nonce = this._GetNonce();

            var cardType = CCHelper.GetCardType(request.CardNumber);

            var td = new TransactionDetail()
            {
                TransactionRequest = request,
                TransactionRequestKey = request.UId,
                DateCreated = DateTimeOffset.Now,
                TransactionType = TransactionType.AuthorizeCapture,
                UId = Guid.NewGuid(),
                Ammount = request.Ammount,
                CardNumber = request.CardNumber.GetLast(4),
                FormKey = request.FormKey
            };

            if (!cardType.In(CreditCardType.AmericanExpress, CreditCardType.Discover, CreditCardType.MasterCard, CreditCardType.Visa))
            {
                td.Approved = false;
                td.Message = "The credit card must be American Express, Discover, MasterCard, or Visa.";
                td.TransactionID = "FAILED";
                return td;
            }

            var payload = new
            {
                merchant_ref = UserName,
                transaction_type = "purchase",
                method = "credit_card",
                amount = this._GetUsDollarAmountAsCents(request.Ammount.ToString()),
                currency_code = CurrencyCode,
                credit_card = new
                {
                    type = CardType[cardType],
                    cardholder_name = request.NameOnCard,
                    card_number = request.CardNumber,
                    exp_date = request.ExpMonthAndYear,
                    cvv = request.CVV
                }
            };

            var payloadString = JsonConvert.SerializeObject(payload);
            var url = "https://api.payeezy.com/v1/transactions";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Add("apikey", Key);
            httpRequest.Headers.Add("token", Token);
            httpRequest.Headers.Add("nonce", nonce.ToString());
            httpRequest.Headers.Add("timestamp", currentTimestamp);
            httpRequest.Headers.Add("Authorization", this._GenerateHmac(nonce, currentTimestamp, payloadString));
            httpRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            httpRequest.Content.Headers.ContentLength = payloadString.Length;

            using (var client = new HttpClient())
            {
                var response = client.SendAsync(httpRequest).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    td.Approved = false;
                    td.Message = "Invalid credentials.";
                }
                else if (response.StatusCode.In(System.Net.HttpStatusCode.InternalServerError, System.Net.HttpStatusCode.BadGateway, System.Net.HttpStatusCode.ServiceUnavailable))
                {
                    td.Approved = false;
                    td.Message = "Error at payeezy gateway.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    td.Approved = false;
                    td.Message = "The requested resource does not exist.";
                }

                var jsonReponse = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                var status = ((string)jsonReponse["transaction_status"]).ToLower();
                td.Approved = (status == "approved");
                var validated = (((string)jsonReponse["validation_status"]).ToLower() == "success");


                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || !td.Approved || !validated)
                {
                    td.Message = String.Join(", ", (string[])jsonReponse["Error"]["messages"]);
                    return td;
                }

                td.InvoiceNumber = (string)jsonReponse["correlation_id"];
                td.TransactionID = (string)jsonReponse["transaction_id"];
                td.ResponseCode = (string)jsonReponse["transaction_tag"];
                td.Message = (string)jsonReponse["bank_message"];

                return td;
            }
        }

        public TransactionDetail Authorize(TransactionRequest info)
        {
            return null;
        }

        public TransactionDetail Credit(decimal refundAmmount, TransactionDetail detail, FormsRepository repository, bool test = false)
        {
            return Credit(refundAmmount, detail, test);
        }

        public TransactionDetail Credit(decimal refundAmmount, TransactionDetail detail, bool test = false)
        {
            string currentTimestamp = this._GetEpochTimestampInMilliseconds();
            int nonce = this._GetNonce();

            var td = new TransactionDetail()
            {
                TransactionRequest = detail.TransactionRequest,
                TransactionRequestKey = detail.TransactionRequestKey,
                DateCreated = DateTimeOffset.Now,
                TransactionType = TransactionType.AuthorizeCapture,
                UId = Guid.NewGuid(),
                Ammount = refundAmmount,
                CardNumber = detail.CardNumber,
                FormKey = detail.FormKey
            };

            var payload = new
            {
                merchant_ref = UserName,
                transaction_type = "refund",
                transaction_tag = detail.ResponseCode,
                method = "credit_card",
                amount = this._GetUsDollarAmountAsCents(refundAmmount.ToString()),
                currency_code = CurrencyCode,
            };

            var payloadString = JsonConvert.SerializeObject(payload);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.payeezy.com/v1/transactions/" + detail.TransactionID);
            httpRequest.Headers.Add("apikey", Key);
            httpRequest.Headers.Add("token", Token);
            httpRequest.Headers.Add("nonce", nonce.ToString());
            httpRequest.Headers.Add("timestamp", currentTimestamp);
            httpRequest.Headers.Add("Authorization", this._GenerateHmac(nonce, currentTimestamp, payloadString));
            httpRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            httpRequest.Content.Headers.ContentLength = payloadString.Length;

            using (var client = new HttpClient())
            {
                var response = client.SendAsync(httpRequest).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    td.Approved = false;
                    td.Message = "Invalid credentials.";
                }
                else if (response.StatusCode.In(System.Net.HttpStatusCode.InternalServerError, System.Net.HttpStatusCode.BadGateway, System.Net.HttpStatusCode.ServiceUnavailable))
                {
                    td.Approved = false;
                    td.Message = "Error at payeezy gateway.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    td.Approved = false;
                    td.Message = "The requested resource does not exist.";
                }

                var jsonReponse = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                var status = ((string)jsonReponse["transaction_status"]).ToLower();
                td.Approved = (status == "Approved");
                var validated = (((string)jsonReponse["validation_status"]).ToLower() == "success");


                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest || !td.Approved || !validated)
                {
                    String.Join(", ", (string[])jsonReponse["Error"]["messages"]);
                    return td;
                }

                td.InvoiceNumber = (string)jsonReponse["correlation_id"];
                td.TransactionID = (string)jsonReponse["transaction_id"];
                td.ResponseCode = (string)jsonReponse["transaction_tag"];
                td.Message = (string)jsonReponse["bank_message"];

                return td;
            }
        }

        protected string _GetEpochTimestampInMilliseconds()
        {
            long millisecondsSinceEpoch = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            return millisecondsSinceEpoch.ToString();
        }

        /// <summary>
        /// Parse a US dollar amount (string) and return the total number of cents.
        /// </summary>
        protected string _GetUsDollarAmountAsCents(string dollarAmount)
        {
            if (string.IsNullOrWhiteSpace(dollarAmount))
            {
                throw new Exception("Dollar amount is null / empty");
            }

            string[] dollarParts = dollarAmount.Trim().Split('.');
            if (dollarParts.Length > 2)
            {
                throw new Exception("US dollar amounts can only have one decimal point");
            }

            int parsedCentsAmount = 0;
            if (dollarParts.Length == 2)
            {
                if (dollarParts[1].Length != 2)
                {
                    throw new Exception("US dollar amounts must have two digit cent amounts");
                }

                if (!int.TryParse(dollarParts[1], out parsedCentsAmount))
                {
                    throw new Exception("Cent amount must be numeric");
                }
            }

            int parsedDollarAmount = 0;

            try
            {
                parsedDollarAmount = int.Parse(dollarParts[0], NumberStyles.Currency);
            }
            catch (Exception ex)
            {
                throw new Exception("Dollar amount must be numeric", ex);
            }

            parsedDollarAmount *= 100;

            return (parsedDollarAmount + parsedCentsAmount).ToString();
        }

        /// <summary>
        /// Generates a cryptographically strong random number.
        /// </summary>
        /// <see cref="https://en.wikipedia.org/wiki/Cryptographic_nonce"/>
        protected int _GetNonce()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }
                return BitConverter.ToInt32(bytes, 0);
            }
        }

    }
}
