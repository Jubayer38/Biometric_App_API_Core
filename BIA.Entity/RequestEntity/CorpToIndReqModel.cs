using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class CorpToIndReqModel
    {
        public CorpToIndData data { get; set; }
    }
    public class CorpToIndData
    {
        public string type { get; set; }
        public int id { get; set; }
        public CorpToIndAttributes attributes { get; set; }
    }

    public class CorpToIndAttributes
    {
        public int purpose_no { get; set; }
        public string dest_imsi { get; set; }
        public string dest_doc_type_no { get; set; }
        public string dest_doc_id { get; set; }
        public string user_name { get; set; }
        public string msisdn { get; set; }
        public string reg_date { get; set; }
        public int dest_ec_verification_required { get; set; }
        public int src_ec_verification_required { get; set; }
        public string src_sim_category { get; set; }
        public string dest_sim_category { get; set; }
        public string dest_dob { get; set; }
        public string src_doc_type_no { get; set; }
        public string src_doc_id { get; set; }
        public string src_dob { get; set; }
        public string dest_left_thumb { get; set; }
        public string dest_left_index { get; set; }
        public string dest_right_thumb { get; set; }
        public string dest_right_index { get; set; }
        public string src_left_thumb { get; set; }
        public string src_left_index { get; set; }
        public string src_right_thumb { get; set; }
        public string src_right_index { get; set; }
        public bool is_b2b { get; set; }
    }
}
