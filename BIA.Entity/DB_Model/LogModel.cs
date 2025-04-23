using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.DB_Model
{
    public class LogModel
    {
        public decimal bss_log_id { get; set; }
        public string bi_token_number { get; set; }
        public string msisdn { get; set; }
        public string bss_request_id { get; set; }
        public string purpose_number { get; set; }
        public string user_id { get; set; }

        public byte[] req_blob { get; set; }
        public byte[] res_blob { get; set; }

        public DateTime req_time { get; set; }
        public DateTime res_time { get; set; }
        public decimal is_success { get; set; }
        public string message { get; set; }
        public string error_code { get; set; }
        public string error_source { get; set; }
        public string method_name { get; set; }
        public decimal integration_point_from { get; set; }
        public decimal integration_point_to { get; set; }
        public string login_attempt_id { get; set; }
        public string remarks { get; set; }

        public int status { get; set; }
        public string req_string { get; set; }
        public string res_string { get; set; }
    }
}
