using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class NewRegBioReqModel
    {
        public NewRegData data { get; set; }
    }

    public class NewRegData
    {
        public string type { get; set; }
        public int id { get; set; }
        public NewRegAttributes attributes { get; set; }
    }
    public class NewRegAttributes
    {
        public int purpose_no { get; set; }
        public string dest_doc_type_no { get; set; }
        public string dest_doc_id { get; set; }
        public string user_name { get; set; }
        public string msisdn { get; set; }
        public string reg_date { get; set; }
        public int dest_ec_verification_required { get; set; }
        public string dest_imsi { get; set; }
        public int dest_foreign_flag { get; set; }
        //public string dest_id_type_exp_time { get; set; }
        public string dest_sim_category { get; set; }
        public string dest_dob { get; set; }
        public string dest_left_thumb { get; set; }
        public string dest_left_index { get; set; }
        public string dest_right_thumb { get; set; }
        public string dest_right_index { get; set; }
        public bool is_b2b { get; set; }
    }
}
