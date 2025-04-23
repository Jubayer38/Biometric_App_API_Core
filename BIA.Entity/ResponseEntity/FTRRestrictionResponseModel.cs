using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class FTRRestrictionResponseModel
    {
        public FTRRespData[] data { get; set; }
    }
    public class FTRRespData
    {
        public string type { get; set; }
        public string id { get; set; }
        public FTRRespAttributes attributes { get; set; }
    }

    public class FTRRespAttributes
    {
        [JsonProperty("request-id")]
        public string request_id { get; set; }
        public string href { get; set; }
        public string status { get; set; }
        [JsonProperty("scheduled-at")]
        public DateTime scheduled_at { get; set; }
    }
}
