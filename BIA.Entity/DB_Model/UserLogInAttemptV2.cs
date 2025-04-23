using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.DB_Model
{
    public class UserLogInAttemptV2
    {
        public int login_attempt_id { get; set; }
        public string userid { get; set; }
        public DateTime attempt_time { get; set; }
        public int is_success { get; set; }
        public string ip_address { get; set; }
        public string machine_name { get; set; }
        public string loginprovider { get; set; }
        public object? deviceid { get; set; }
        public string lan { get; set; }
        public int versioncode { get; set; }
        public string versionname { get; set; }
        public string? osversion { get; set; }
        public string? kernelversion { get; set; }
        public string? fermwarevirsion { get; set; }
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public int? lac { get; set; }
        public int? cid { get; set; }
        public decimal is_bp { get; set; }
        public string bp_msisdn { get; set; }
        public string? device_model { get; set; }
    }
}
