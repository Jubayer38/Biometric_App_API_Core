using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class CorpSimReplceReqModel
    {
        public CorpSimReplceData data { get; set; }
    }

    public class CorpSimReplceData
    {
        public string type { get; set; }
        public string id { get; set; }
        public CorpSimReplceAttributes attributes { get; set; }
    }

    public class CorpSimReplceAttributes
    {
        [JsonProperty(PropertyName = "biometric-request-id")]
        public string biometric_request_id { get; set; }
        [JsonProperty(PropertyName = "new-icc")]
        public string new_icc { get; set; }
        public string reason { get; set; }
        [JsonProperty(PropertyName = "payment-mode")]
        public string payment_mode { get; set; }
        public CorpSimReplceMeta meta { get; set; }
    }

    public class CorpSimReplceMeta
    {
        public string channel { get; set; }
        public string reseller { get; set; }
        public string salesman { get; set; }
    }
}
