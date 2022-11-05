using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    public class AuthorizeDotNetSilentPost
    {
        public string x_response_code { get; set; }
        public string x_response_subcode { get; set; }
        public string x_response_reason_code { get; set; }
        public string x_response_reason_text { get; set; }
        public string x_auth_code { get; set; }
        public string x_avs_code { get; set; }
        public string x_trans_id { get; set; }
        public string x_invoice_num { get; set; }
        public string x_description { get; set; }
        public string x_amount { get; set; }
        public string x_method { get; set; }
        public string x_type { get; set; }
        public string x_cust_id { get; set; }
        public string x_first_name { get; set; }
        public string x_last_name { get; set; }
        public string x_company { get; set; }
        public string x_address { get; set; }
        public string x_city { get; set; }
        public string x_state { get; set; }
        public string x_zip { get; set; }
        public string x_country { get; set; }
        public string x_phone { get; set; }
        public string x_fax { get; set; }
        public string x_email { get; set; }
        public string x_ship_to_first_name { get; set; }
        public string x_ship_to_last_name { get; set; }
        public string x_ship_to_company { get; set; }
        public string x_ship_to_address { get; set; }
        public string x_ship_to_city { get; set; }
        public string x_ship_to_state { get; set; }
        public string x_ship_to_zip { get; set; }
        public string x_ship_to_country { get; set; }
        public string x_tax { get; set; }
        public string x_duty { get; set; }
        public string x_freight { get; set; }
        public string x_tax_exempt { get; set; }
        public string x_po_num { get; set; }
        public string x_MD5_Hash { get; set; }
        public string x_cavv_response { get; set; }
        public string x_test_request { get; set; }
        public string x_subscription_id { get; set; }
        public string x_subscription_paynum { get; set; }
    }
}
