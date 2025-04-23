using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class FPValidationReqModel
    {
        public string user_name { get; set; }
        public string finger_print { get; set; }
        public string session_token { get; set; }
        public int version_code { get; set; } = 0; 
        public string version_name { get; set; }
        public string? os_version { get; set; } = "";
        public string? kernel_version { get; set; } = "";
        public string? fermware_version { get; set; }
        public decimal? latitude { get; set; } = 0;
        public decimal? longitude { get; set; } = 0;
        public int? lac { get; set; } = 0;
        public int? cid { get; set; } = 0;
        public string? device_model { get; set; } = ""; 
        public object? deviceId { get; set; }
        public string? BPMSISDN { get; set; } = "";
        public string? lan { get; set; } = "";
    }
}
