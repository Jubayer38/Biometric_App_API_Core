using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class CheckUserResponseModel
    {
        public bool isError { get; set; }
        public string message { get; set; }
        public RespData data { get; set; }
    }

    public class RespData
    {
        public bool is_fp_validation_need { get; set; }
        public bool is_registered { get; set; }
        public string msisdn { get; set; }
        public string SessionToken { get; set; }
        public string MinimumScore { get; set; }
        public string MaximumRetry { get; set; }
    }

    public class DBResponseModel
    {
        public int is_fp_validation_need { get; set; }
        public int is_registered { get; set; }
        public string msisdn { get; set; }
        public int is_user_valid { get; set; }
        public string message { get; set; }
    }
}
 