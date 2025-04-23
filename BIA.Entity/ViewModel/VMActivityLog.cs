using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class VMActivityLog : RACommonResponse
    {
        public string token_id { get; set; }
        public string time { get; set; }
        public string mobile_number { get; set; }
        public string nid { get; set; }
        public string dob { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public int is_re_submittable { get; set; }
        public int re_submit_expire_time { get; set; }
        public string re_submit_error_message { get; set; }
        public int right_id { get; set; }
        public string is_bp_user { get; set; }
        public string bp_msisdn { get; set; }
        public string action_point { get; set; }
        public string designation { get; set; }

    }
    public class VMActivityLogRevamp
    {
        public string token_id { get; set; }
        public string time { get; set; }
        public string mobile_number { get; set; }
        public string nid { get; set; }
        public string dob { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public int is_re_submittable { get; set; }
        public int re_submit_expire_time { get; set; }
        public string re_submit_error_message { get; set; }
        public int right_id { get; set; }
        public string is_bp_user { get; set; }
        public string bp_msisdn { get; set; }
        public string action_point { get; set; }
        public string designation { get; set; }
        public string recharge_status { get; set; }
        public int is_recharge_done { get; set; }

    }
}
