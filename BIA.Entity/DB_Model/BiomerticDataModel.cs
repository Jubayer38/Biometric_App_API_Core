using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.DB_Model
{
    public class BiomerticDataModel
    {
        public string bi_token_number { get; set; }
        public string bss_request_id { get; set; } //DBSS Bio Request Id.
        public int purpose_number { get; set; }
        public string msisdn { get; set; }
        public int sim_category { get; set; }
        public string sim_number { get; set; }
        public string dest_doc_type_no { get; set; }
        public string dest_doc_id { get; set; }
        public string dest_dob { get; set; }
        public string src_doc_type_no { get; set; }
        public string src_doc_id { get; set; }
        public string src_dob { get; set; }
        public byte[] dest_left_thumb { get; set; }
        public byte[] dest_left_index { get; set; }
        public byte[] dest_right_thumb { get; set; }
        public byte[] dest_right_index { get; set; }
        public byte[] src_left_thumb { get; set; }
        public byte[] src_left_index { get; set; }
        public byte[] src_right_thumb { get; set; }
        public byte[] src_right_index { get; set; }
        public string user_id { get; set; }
        public string poc_number { get; set; }
        public int status { get; set; }
        public long error_id { get; set; }
        public string error_description { get; set; }
        public string create_date { get; set; }
        public string dest_imsi { get; set; }
        public string dest_id_type_exp_time { get; set; }
        public string src_id_type_exp_time { get; set; }
        public int dest_ec_verification_required { get; set; }
        public int src_ec_verification_required { get; set; }
        public int? is_paired { get; set; }
        public int dest_foreign_flag { get; set; }
        public int sim_replacement_type { get; set; }
        public int src_sim_category { get; set; }
        public string otp_no { get; set; }
        public int dbss_subscription_id { get; set; }
        public string user_name { get; set; }

    }
}
