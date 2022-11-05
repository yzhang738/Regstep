using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;
using System.Web.Mvc;
using RSToolKit.Domain.Security;
using Newtonsoft.Json;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    [Table("MerchantAccountInfo")]
    public class MerchantAccountInfo
        : INamedNode, IPermissionHolder
    {
        protected string _key;
        protected string _hash;

        protected string _username;
        protected string _uHash;

        public virtual List<TransactionRequest> Requests { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        [MaxLength(3)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Permission { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid Owner { get; set; }
        public Guid Group { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        [NotMapped]
        public Dictionary<string, string> Parameters { get; set; }
        public string RawParameters
        {
            get
            {
                return JsonConvert.SerializeObject(Parameters ?? new Dictionary<string, string>());
            }
            set
            {
                try
                {
                    Parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(value ?? "[]");
                }
                catch (Exception)
                {
                    Parameters = new Dictionary<string, string>();
                }
            }
        }

        public virtual List<Form> Forms { get; set; }

        [NotMapped]
        public string Key
        {
            get
            {
                return CCHelper.DecryptCreditCard(_hash, KeySalt);
            }
            set
            {
                var result = CCHelper.EncryptCreditCard(value);
                KeySalt = result.Salt;
                _hash = result.Hash;
            }
        }

        [NotMapped]
        public string UserName
        {
            get
            {
                return CCHelper.DecryptCreditCard(_uHash, UserSalt);
            }
            set
            {
                var result = CCHelper.EncryptCreditCard(value);
                UserSalt = result.Salt;
                _uHash = result.Hash;
            }
        }

        public string UserKey
        {
            get
            {
                return _uHash;
            }
            set
            {
                _uHash = value;
            }
        }
        public string HashKey
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

        [Required]
        public string Descriminator { get; set; }
        public string KeySalt { get; set; }
        public string UserSalt { get; set; }

        public MerchantAccountInfo()
        {
            _username = null;
            _uHash = null;
            _key = null;
            _hash = null;
            Descriminator = "UNKNOWN";
            Forms = new List<Form>();
            Requests = new List<TransactionRequest>();
        }

        public static MerchantAccountInfo New(FormsRepository repository, Company company, User user, string username, string key, string descriminator, Guid? owner = null, Guid? group = null, string permission = "770")
        {
            var node = new MerchantAccountInfo()
            {
                UId = Guid.NewGuid(),
                UserName = username,
                Key = key,
                Descriminator = descriminator,
                Owner = owner.HasValue ? owner.Value : user.UId,
                Group = group.HasValue ? group.Value : company.UId,
                Company = company,
                CompanyKey = company.UId,
                Permission = permission
            };
            PermissionSet.CreateDefaultPermissions(repository, node, company.UId);
            repository.Add(node);
            repository.Commit();
            return node;
        }

        public IMerchantAccount<TransactionRequest> GetGateway()
        {
            switch (Descriminator)
            {
                case "authorize.net":
                    return new AuthorizeDotNet()
                    {
                        UserName = UserName,
                        Key = Key
                    };
                case "usaepay":
                    return new USAePay()
                    {
                        UserName = UserName,
                        Key = Key
                    };
                case "paypal":
                    return new PayPalWrapper()
                    {
                        UserName = UserName,
                        Key = Key
                    };
                case "ipay":
                    return new iPayWrapper()
                    {
                        UserName = UserName,
                        Key = Key,
                        Parameters = Parameters
                    };
                case "firstdataglobalgateway":
                    return new FirstDataAPI()
                    {
                        UserName = UserName,
                        Key = Key,
                        Parameters = Parameters
                    };
                case "payeezy":
                    return new PayeezyWrapper()
                    {
                        Key = Key,
                        UserName = UserName,
                        Token = Parameters["token"],
                        Secret = Parameters["apisecret"],
                        CurrencyCode = Parameters["currencycode"]
                    };
                default:
                    return null;
            }
        }

        public List<SelectListItem> Types()
        {
            var list = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "UNKNOWN",
                    Text = "Pick Type",
                    Selected = Descriminator == "UNKNOWN"
                },
                new SelectListItem()
                {
                    Value = "authorize.net",
                    Text = "Authorize.Net",
                    Selected = Descriminator == "authorize.net"
                },
                new SelectListItem()
                {
                    Value = "usaepay",
                    Text = "USAePay",
                    Selected = Descriminator == "usaepay"
                },
                new SelectListItem()
                {
                    Value = "paypal",
                    Text = "PayPal",
                    Selected = Descriminator == "paypal"
                },
                new SelectListItem()
                {
                    Value = "ipay",
                    Text = "iPay",
                    Selected = Descriminator == "ipay"
                },
                new SelectListItem()
                {
                    Value = "firstdataglobalgateway",
                    Text = "First Data Global Gateway",
                    Selected = Descriminator == "firstdataglobalgateway"
                },
                new SelectListItem()
                {
                    Value = "payeezy",
                    Text = "Payeezy",
                    Selected = Descriminator == "payeezy"
                }
            };

            return list;
        }

        public string GetParameter(string key)
        {
            if (!Parameters.ContainsKey(key))
                return "";
            else
                return Parameters[key];
        }
    }
}
