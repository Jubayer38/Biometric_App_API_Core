using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class PreToPostMigrationReqModel
    {
        public PreToPostMigrationData data { get; set; }
    }

    public class PreToPostMigrationData
    {
        public string type { get; set; }
        public int id { get; set; }
        public PretoPostMigrationAttributes attributes { get; set; }
    }

    public class PretoPostMigrationAttributes
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
        public string reg_date { get; set; }
        public bool is_b2b { get; set; }
        public string user_name { get; set; }
        public string dest_sim_category { get; set; }
    }
}
