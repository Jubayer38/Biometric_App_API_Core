using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class NewRegPairReqModel
    {
        public NewRegPairData data { get; set; }
        public NewRegPairMeta meta { get; set; }
    }
    public class NewRegPairData
    {
        public string type { get; set; }
        public NewRegPairAttributes attributes { get; set; }
    }
    public class NewRegPairMeta
    {
        public NewRegPairCustomer customer { get; set; }
        public NewRegPairSales_Info sales_info { get; set; }
        public ArrayList products { get; set; }
    }
    public class NewRegPairAttributes
    {
        public string offer { get; set; }
        public int brand { get; set; }
        public string delivery_type { get; set; }
        public string correction_for { get; set; }
        public string ordered_at { get; set; }
        public string biometric_request_id { get; set; }
    }
    public class NewRegPairCustomer
    {
        public string province { get; set; }
        public string post_code { get; set; }
        public string area { get; set; }
        //public string id_expiry { get; set; }
        public string alt_contact_phone { get; set; }
        public string road { get; set; }
        public string city { get; set; }
        public string house_number { get; set; }
        public string co_address { get; set; }
        public string street { get; set; }
        public string last_name { get; set; }
        public string language { get; set; }
        public string title { get; set; }
        public bool is_company { get; set; }
        public string country { get; set; }
        public bool marketing_own { get; set; }
        public string id_type { get; set; }
        public string id_number { get; set; }
        public string birthday { get; set; }
        public string contact_phone { get; set; }
        public string nationality { get; set; }
        public string postal_code { get; set; }
        public string invoice_delivery_method { get; set; }
        public string first_name { get; set; }
        public string email { get; set; }
        public string occupation { get; set; }
    }
    public class NewRegPairSales_Info
    {
        public string reseller { get; set; }
        public string salesman { get; set; }
        public string channel { get; set; }
        public string chain { get; set; }
        public string sales_type { get; set; }
        public string msisdn { get; set; }
    }
    public class NewRegPairProduct
    {
        public object[] barrings { get; set; }
        public int termination_penalty_fee { get; set; }
        //public string payer { get; set; }
        public int initial_period { get; set; }
        public string user_privacy { get; set; }
        public string msisdn { get; set; }
        public bool paying_monthly { get; set; }
        public int recurring_period { get; set; }
        public int retention_penalty_fee { get; set; }
        public string type { get; set; }
        public string product_type { get; set; }
        public NewRegPairPayer payer { get; set; }
        //public string user { get; set; }
        public NewRegPairUser user { get; set; }
        public string connection_type { get; set; }
        public object packages { get; set; }

    }
    public class NewRegPairPayer
    {
        public string province { get; set; }
        public string post_code { get; set; }
        public string area { get; set; }
        //public string id_expiry { get; set; }
        public string alt_contact_phone { get; set; }
        public string road { get; set; }
        public string city { get; set; }
        public string house_number { get; set; }
        public string co_address { get; set; }
        public string street { get; set; }
        public string last_name { get; set; }
        public string language { get; set; }
        public string title { get; set; }
        //public bool is_company { get; set; }
        public string country { get; set; }
        //public bool marketing_own { get; set; }
        //public string id_type { get; set; }
        //public string id_number { get; set; }
        //public string birthday { get; set; }
        public string contact_phone { get; set; }
        public string nationality { get; set; }
        public string postal_code { get; set; }
        public string invoice_delivery_method { get; set; }
        public string first_name { get; set; }
        public string email { get; set; }
        public string occupation { get; set; }
    }
    public class NewRegPairUser
    {
        public string province { get; set; }
        public string post_code { get; set; }
        public string area { get; set; }
        //public string id_expiry { get; set; }
        public string alt_contact_phone { get; set; }
        public string road { get; set; }
        public string city { get; set; }
        public string house_number { get; set; }
        public string co_address { get; set; }
        public string street { get; set; }
        public string last_name { get; set; }
        public string language { get; set; }
        public string title { get; set; }
        //public bool is_company { get; set; }
        public string country { get; set; }
        //public bool marketing_own { get; set; }
        //public string id_type { get; set; }
        //public string id_number { get; set; }
        //public string birthday { get; set; }
        public string contact_phone { get; set; }
        public string nationality { get; set; }
        public string postal_code { get; set; }
        public string invoice_delivery_method { get; set; }
        public string first_name { get; set; }
        public string email { get; set; }
        public string occupation { get; set; }
    }
    public class NewRegPairProduct1
    {
        public string type { get; set; }
        public string product_type { get; set; }
        public string article_id { get; set; }
        public NewRegPairData_Dict data_dict { get; set; }
        public int price { get; set; }
    }
    public class NewRegPairData_Dict
    {
        public string msisdn { get; set; }
    }
}
