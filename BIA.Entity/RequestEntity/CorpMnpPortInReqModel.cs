using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class CorpMnpPortInReqModel
    {
        public CorpMnpPortInData data { get; set; }
    }
    public class CorpMnpPortInData
    {
        public string type { get; set; }
        public int id { get; set; }
        public CorpMnpPortInAttributes attributes { get; set; }
    }

    public class CorpMnpPortInAttributes
    {
        public int purpose_no { get; set; }
        public string dest_doc_type_no { get; set; }
        public string dest_doc_id { get; set; }
        public string msisdn { get; set; }
        public int dest_ec_verification_required { get; set; }
        public string dest_dob { get; set; }
        public string dest_left_thumb { get; set; }
        public string dest_left_index { get; set; }
        public string dest_right_thumb { get; set; }
        public string dest_right_index { get; set; }
        public bool is_b2b { get; set; }
        public string reg_date { get; set; }
    }
}
