using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class IndiSimTransferCustomerCreateReqModel
    {
        public IndiSimTransferCustomerCreateData data { get; set; }
    }

    public class IndiSimTransferCustomerCreateData
    {
        public string type { get; set; }

        public IndiSimTransferCustomerCreateAttributes attributes { get; set; }
    }

    public class IndiSimTransferCustomerCreateAttributes
    {
        [JsonProperty(PropertyName = "id-document-type")]
        public string id_document_type { get; set; }

        [JsonProperty(PropertyName = "id-document-number")]
        public string id_document_number { get; set; }

        public string birthday { get; set; }

        public string nationality { get; set; }
    }
}
