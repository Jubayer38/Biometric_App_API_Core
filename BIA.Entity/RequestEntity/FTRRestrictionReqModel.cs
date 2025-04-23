using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class FTRRestrictionReqModel
    {
        public FTRData data { get; set; }
    }
    public class FTRMeta
    {
        public Dictionary<string, object> services { get; set; }
        public string channel { get; set; }
    }

    public class FTRData
    {
        [JsonProperty("type")]
        public string type { get; set; }
        public string id { get; set; }
        public FTRMeta meta { get; set; }
    }
}
