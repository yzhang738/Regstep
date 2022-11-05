using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Data;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Errors;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    public static class CCHelper
    {
        /// <summary>
        /// Validates a credit card by modulus 10.
        /// </summary>
        /// <param name="number">The number to validate.</param>
        /// <returns>True on valid, false otherwise.</returns>
        public static bool ValidateCard(string number)
        {
            if (String.IsNullOrEmpty(number))
                return false;
            if (!Regex.IsMatch(number, @"^[0-9]*$"))
                return false;
            int sumOfDigit = number.Where(e => e >= '0' && e <= '9').Reverse().Select((e, i) => ((int)e - 48) * (i % 2 == 0 ? 1 : 2)).Sum(e => e / 10 + e % 10);
            return sumOfDigit % 10 == 0;
        }

        /// <summary>
        /// Gets the type of the card.
        /// </summary>
        /// <param name="number">The number</param>
        /// <returns>A credit card type.</returns>
        public static CreditCardType GetCardType(string number)
        {
            CreditCardType type = CreditCardType.Unknown;
            number = number.Trim();
            number = Regex.Replace(Regex.Replace(number, @" ", ""), @"-", "");
            switch (number.Substring(0, 2))
            {
                case "34":
                case "37":
                    type = CreditCardType.AmericanExpress;
                    break;
                case "38":
                    type = CreditCardType.CarteBlanche;
                    break;
                case "51":
                case "52":
                case "53":
                case "54":
                case "55":
                    type = CreditCardType.MasterCard;
                    break;
                default:
                    switch (number.Substring(0, 4))
                    {
                        case "2014":
                        case "2149":
                            type = CreditCardType.EnRoute;
                            break;
                        case "6011":
                            type = CreditCardType.Discover;
                            break;
                        default:
                            switch (number.Substring(0, 3))
                            {
                                case "300":
                                case "301":
                                case "302":
                                case "303":
                                case "304":
                                case "305":
                                    type = CreditCardType.DinersClub;
                                    break;
                                default:
                                    switch (number.Substring(0, 1))
                                    {
                                        case "4":
                                            type = CreditCardType.Visa;
                                            break;
                                        default:
                                            type = CreditCardType.Unknown;
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
            }
            return type;
        }

        /// <summary>
        /// Encrypts the credit card number
        /// </summary>
        /// <param name="number">The number to encrypt</param>
        /// <returns>A dynamic object of bytes = [encrypted bytes], and salt</returns>
        public static CreditCardEncryptionResult EncryptCreditCard(string number)
        {
            if (String.IsNullOrWhiteSpace(number))
                return new CreditCardEncryptionResult() { Hash = null, Salt = null };
            var result = new CreditCardEncryptionResult();
            result.Salt = GetSalt(32);
            byte[] numberBytes = Encoding.ASCII.GetBytes(number);
            byte[] ivBytes = Encoding.ASCII.GetBytes("OFRna73m*aze01xY");
            PasswordDeriveBytes PDB = new PasswordDeriveBytes("RegStep likes to secure all credit card numbers.", Convert.FromBase64String(result.Salt), "SHA1", 2);
            byte[] keyBytes = PDB.GetBytes(256 / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            byte[] cipherTextBytes = null;
            using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, ivBytes))
            using (MemoryStream memoryStream = new MemoryStream())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(numberBytes, 0, numberBytes.Length);
                cryptoStream.FlushFinalBlock();
                cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                symmetricKey.Clear();
            }
            result.Hash = Convert.ToBase64String(cipherTextBytes);
            return result;
        }

        public static string GetSalt(int size)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[size];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }

        /// <summary>
        /// Decrypts the credit card from the database.
        /// </summary>
        /// <param name="cipherText">The cipher.</param>
        /// <param name="salt">The salt</param>
        /// <returns>the number as a string</returns>
        public static string DecryptCreditCard(string cipher, string salt)
        {
            if (String.IsNullOrWhiteSpace(cipher) || String.IsNullOrWhiteSpace(salt))
                return null;
            var cipherText = Convert.FromBase64String(cipher);
            byte[] ivBytes = Encoding.ASCII.GetBytes("OFRna73m*aze01xY");
            PasswordDeriveBytes PDB = new PasswordDeriveBytes("RegStep likes to secure all credit card numbers.", Convert.FromBase64String(salt), "SHA1", 2);
            byte[] keyBytes = PDB.GetBytes(256 / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            byte[] plainTextBytes = new byte[cipherText.Length];
            int byteCount = 0;
            using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, ivBytes))
            using (MemoryStream memoryStream = new MemoryStream(cipherText))
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                symmetricKey.Clear();
            }
            return Encoding.ASCII.GetString(plainTextBytes, 0, byteCount).TrimEnd("\0".ToCharArray());
        }
    }

    public class CreditCard
        : ISecure, IRequirePermissions
    {
        protected string _number;
        protected string _hash;

        [ClearKeyOnDelete("CreditCardKey")]
        public virtual List<TransactionRequest> Requests { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [Key]
        public Guid UId { get; set; }

        public Guid ModifiedBy { get; set; }
        public Guid ModificationToken { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }
        public string Name { get; set; }

        public Guid? RegistrantKey { get; set; }

        [NotMapped]
        public string Value
        {
            get
            {
                return Number;
            }
        }
        [NotMapped]
        public string Number
        {
            get { return CCHelper.DecryptCreditCard(_hash, Salt); }
            set
            {
                if (!CCHelper.ValidateCard(value))
                    throw new CreditCardException("Not valid credit card.");
                Type = CCHelper.GetCardType(value);
                var result = CCHelper.EncryptCreditCard(value);
                Salt = result.Salt;
                _hash = result.Hash;
            }
        }
        public string HashNumber
        {
            get
            {
                return _hash;
            }
            set
            {
                _hash = value;
            }
        }
        public CreditCardType Type { get; set; }
        public string NameOnCard { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string SecurityCode { get; set; }
        public Guid FormKey { get; set; }
        public string Confirmation { get; set; }
        public string Salt { get; set; }
        public string Exp { get; set; }
        public string Code { get; set; }

        public bool Delete { get; set; }
        public string Peek()
        {
            var result = "";
            var xCount = Number.Length - 4;
            for (var i = 0; i < xCount; i++)
                result += "X";
            return result + Number.GetLast(4);
        }

        public CreditCard()
        {
            _number = null;
            SortingId  = 0;
            UId = Guid.NewGuid();
            Type = CreditCardType.Unknown;
            NameOnCard = Address = City = State = Zip = Country = Phone = SecurityCode = Exp = Code = "";
            Confirmation = "";
            Salt = null;
            Requests = new List<TransactionRequest>();
            Delete = false;
            Name = "New CreditCard";
            ModifiedBy = Guid.Empty;
        }

        public IPermissionHolder GetPermissionHolder()
        {
            Form form = null;
            if (RegistrantKey.HasValue)
            {
                using (var context = new EFDbContext())
                {
                    form = (from r in context.Registrants where r.UId == RegistrantKey.Value select r.Form).FirstOrDefault();
                }
            }
            return form;
        }
    }

    public class CreditCardEncryptionResult
    {
        public string Hash { get; set; }
        public string Salt { get; set; }
    }

    public enum CreditCardType
    {
        [StringValue("Unknown")]
        Unknown = 0,
        [StringValue("Visa")]
        [PayPalValue("visa")]
        Visa = 1,
        [StringValue("American Express")]
        [PayPalValue("amex")]
        AmericanExpress = 2,
        [StringValue("Diner's Club")]
        DinersClub = 3,
        [StringValue("Carte Blanche")]
        CarteBlanche = 4,
        [StringValue("Discover")]
        [PayPalValue("discover")]
        Discover = 5,
        [StringValue("EnRoute")]
        EnRoute = 6,
        [StringValue("Master Card")]
        [PayPalValue("mastercard")]
        MasterCard = 7,
        [StringValue("Diners")]
        [PayPalValue("diners")]
        Diners = 8
    }

}
