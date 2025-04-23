using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.DB_Model
{
    public class OrderDataModel
    {
        public string bi_token_number { get; set; }
        public string bss_request_id { get; set; } //DBSS Bio Request Id.
        public int purpose_number { get; set; }
        public string msisdn { get; set; }
        public int sim_category { get; set; }
        public string sim_number { get; set; }
        public string subscription_code { get; set; }// subscription  type code
        public string package_code { get; set; }
        public string dest_doc_type_no { get; set; }
        public string dest_doc_id { get; set; }
        public string dest_dob { get; set; }
        public string customer_name { get; set; }
        public string gender { get; set; }
        public string flat_number { get; set; }
        public string house_number { get; set; }
        public string road_number { get; set; }
        public string village { get; set; }
        public string division_Name { get; set; }
        public string district_Name { get; set; }
        public string thana_Name { get; set; }
        public string postal_code { get; set; }
        public string user_id { get; set; }
        public string port_in_date { get; set; }
        public string alt_msisdn { get; set; }
        public int status { get; set; }
        public long error_id { get; set; }
        public string error_description { get; set; }
        public string create_date { get; set; }
        public string dest_id_type_exp_time { get; set; }
        public string confirmation_code { get; set; }//DBSS Order Confarmation Code.
        public string email { get; set; }
        public string salesman_code { get; set; }
        public string channel_name { get; set; }
        public string center_or_distributor_code { get; set; }
        public string sim_replace_reason { get; set; }
        public int? is_paired { get; set; }
        public int dbss_subscription_id { get; set; }
        public string old_sim_number { get; set; }
        public int sim_replacement_type { get; set; }
        public int src_sim_category { get; set; }
        public string port_in_confirmation_code { get; set; }
        public string payment_type { get; set; }
        public string poc_number { get; set; }
    }
}
