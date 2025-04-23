using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class VMBIAToDBSSLog
    {
        public string bi_token_number { get; set; }
        public string dbss_request_id { get; set; }
        public string msisdn { get; set; }
        public int purpose_number { get; set; }
        public string username { get; set; }

        public byte[] req_blob { get; set; }
        public byte[] res_blob { get; set; }
        public decimal complain_id { get; set; } = 0;
        public DateTime req_time { get; set; }
        public DateTime res_time { get; set; }
         
        public decimal is_success { get; set; }
        public string message { get; set; }
        public string error_code { get; set; }
        public string error_source { get; set; }
        public string method_name { get; set; }
        public decimal integration_point_from { get; set; }
        public decimal integration_point_to { get; set; }
        public string remarks { get; set; }
        public string server_name { get; set; }
    }
}
