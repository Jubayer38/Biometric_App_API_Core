using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class MnpEmergReturnReqModel
    {
        public MnpEmergReturnData data { get; set; }
        public MnpEmergReturnMeta meta { get; set; }
    }

    public class MnpEmergReturnData
    {
        public MnpEmergReturnAttributes attributes { get; set; }
        public string type { get; set; }
    }

    public class MnpEmergReturnAttributes
    {
        public int brand { get; set; }
        public string correction_for { get; set; }
        public string delivery_type { get; set; }
        public string offer { get; set; }
        public string biometric_request_id { get; set; }
    }

    public class MnpEmergReturnMeta
    {
        public MnpEmergReturnCustomer customer { get; set; }
        public System.Collections.ArrayList products { get; set; }
        public MnpEmergReturnSales_Info sales_info { get; set; }
    }

    public class MnpEmergReturnCustomer
    {
        public string alt_contact_phone { get; set; }
        public string area { get; set; }
        public string birthday { get; set; }
        public string city { get; set; }
        public string co_address { get; set; }
        public string contact_phone { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string house_number { get; set; }
        public string id_expiry { get; set; }
        public string id_number { get; set; }
        public string id_type { get; set; }
        public string invoice_delivery_method { get; set; }
        public bool is_company { get; set; }
        public string language { get; set; }
        public string last_name { get; set; }
        public bool marketing_own { get; set; }
        public string nationality { get; set; }
        public string occupation { get; set; }
        public string post_code { get; set; }
        public string postal_code { get; set; }
        public string province { get; set; }
        public string road { get; set; }
        public string street { get; set; }
        public string title { get; set; }
    }

    public class MnpEmergReturnSales_Info
    {
        public string chain { get; set; }
        public string channel { get; set; }
        //public string msidn { get; set; }
        public string msisdn { get; set; }
        public string reseller { get; set; }
        public string sales_type { get; set; }
        public string salesman { get; set; }
    }

    public class MnpEmergReturnProduct
    {
        public object[] barrings { get; set; }
        public int initial_period { get; set; }
        public MnpEmergReturn mnp { get; set; }
        public string msisdn { get; set; }
        public dynamic packages { get; set; }
        //public string payer { get; set; }
        public bool paying_monthly { get; set; }
        public string product_type { get; set; }
        public MnpEmergReturnPayer payer { get; set; }
        public int recurring_period { get; set; }
        public float retention_penalty_fee { get; set; }
        public float termination_penalty_fee { get; set; }
        public string type { get; set; }
        //public string user { get; set; }
        public MnpEmergReturnUser user { get; set; }
        public string user_privacy { get; set; }
    }
    public class MnpEmergReturnProduct1
    {
        public string type { get; set; }
        public string product_type { get; set; }
        public string article_id { get; set; }
        public MnpEmergReturnData_Dict data_dict { get; set; }
        public int price { get; set; }
    }
    public class MnpEmergReturn
    {
        public string document_id { get; set; }
        public string msisdn { get; set; }
        public string portation_time { get; set; }
        public string recipient_operator { get; set; }
        public bool is_emergency_return { get; set; }
    }
    public class MnpEmergReturnData_Dict
    {
        public string msisdn { get; set; }
    }

    public class MnpEmergReturnPayer
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

    public class MnpEmergReturnUser
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
}
