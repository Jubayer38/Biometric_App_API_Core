using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class CorpSimReplacebyBulkReqModel
    {
        public CorpSimReplacebyBulkData data { get; set; }
    }

    public class CorpSimReplacebyBulkData
    {
        public string type { get; set; }
        public int id { get; set; }
        public CorpSimReplacebyBulkAttributes attributes { get; set; }
    }

    public class CorpSimReplacebyBulkAttributes
    {
        public int purpose_no { get; set; }
        public string dest_doc_type_no { get; set; }
        public string dest_doc_id { get; set; }
        public string msisdn { get; set; }
        public string dest_dob { get; set; }
        public string reg_date { get; set; }
        //public string dest_imsi { get; set; }
        public string corp_sim_replace_type { get; set; }
        public bool is_b2b { get; set; }
        public string user_name { get; set; }
        public int dest_ec_verification_required { get; set; }
    }
}
