using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class CorpSimReplacebyAuthPerReqModel
    {
        public CorpSimReplacebyAuthPerData data { get; set; }
    }

    public class CorpSimReplacebyAuthPerData
    {
        public string type { get; set; }
        public int id { get; set; }
        public CorpSimReplacebyAuthPerAttributes attributes { get; set; }
    }
    public class CorpSimReplacebyAuthPerAttributes
    {
        public int purpose_no { get; set; }
        public int dest_doc_type_no { get; set; }
        public string dest_doc_id { get; set; }
        public string msisdn { get; set; }
        public int dest_ec_verification_required { get; set; }
        public string dest_dob { get; set; }
        public string dest_left_thumb { get; set; }
        public string dest_left_index { get; set; }
        public string dest_right_thumb { get; set; }
        public string dest_right_index { get; set; }
        public string reg_date { get; set; }
        public string dest_imsi { get; set; }
        public string corp_sim_replace_type { get; set; }
        public bool is_b2b { get; set; }
        public string user_name { get; set; }
        public int poc_doc_type_no { get; set; }
        public string poc_doc_id { get; set; }
        public string poc_dob { get; set; }
    }
}
