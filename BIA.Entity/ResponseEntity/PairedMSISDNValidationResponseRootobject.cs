using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class PairedMSISDNValidationResponseRootobject
    {
        public PairedMSISDNValidationResponseData data { get; set; }
    }

    public class PairedMSISDNValidationResponseData
    {
        public string type { get; set; }
        public PairedMSISDNValidationResponseAttributes attributes { get; set; }
    }

    public class PairedMSISDNValidationResponseAttributes
    {
        public string icc { get; set; }
        public object price { get; set; }
        public string msisdn { get; set; }
        public string status { get; set; }
        public string numbercategory { get; set; }
        [JsonProperty("subscription-type")]
        public string subscriptionType { get; set; }
        public object currency { get; set; }
        public string imsi { get; set; }
        public string salesman_id { get; set; }
    }

}
