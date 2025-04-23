using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class UnpairedMSISDNListReqModel : RACommonRequest
    {
        public string msisdn { get; set; }
        public string retailer_id { get; set; }
        public string channel_name { get; set; }
    }

    public class UnpairedMSISDNListReqModelV2 : RACommonRequest
    {
        public string msisdn { get; set; }
        public string retailer_id { get; set; }
        public string channel_name { get; set; }
        public string Selected_category { get; set; }
    }

    public class PairedMSISDNReqModel : RACommonRequest
    {
        public string sim_serial { get; set; }
        public string retailer_id { get; set; } 
    }

}
