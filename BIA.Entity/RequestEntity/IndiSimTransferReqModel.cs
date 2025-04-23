using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class IndiSimTransferReqModel
    {
        public IndiSimTransferReqData data { get; set; }
    }

    public class IndiSimTransferReqData
    {
        public string type { get; set; }

        public string id { get; set; }

        public IndiSimTransferReqAttributes attributes { get; set; }
    }

    public class IndiSimTransferReqAttributes
    {
        [JsonProperty(PropertyName = "owner-customer")]
        public IndiSimTransferReqOwnerCustomer owner_customer { get; set; }

        [JsonProperty(PropertyName = "biometric-request-id")]
        public string biometric_request_id { get; set; }

        public IndiSimTransferReqMeta _meta { get; set; }
    }

    public class IndiSimTransferReqOwnerCustomer
    {
        public string id { get; set; }

        public string type { get; set; }
    }

    public class IndiSimTransferReqMeta
    {
        public string channel { get; set; }
    }
}
