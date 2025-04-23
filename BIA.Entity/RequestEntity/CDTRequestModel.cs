using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class CDTRequestModel
    {
        public CDTData data { get; set; }
    }

    public class CDTData
    {
        public string type { get; set; }
        public CDTAttributes attributes { get; set; }
    }

    public class CDTAttributes
    {
        [JsonProperty(PropertyName = "order-channel")]
        public string orderchannel { get; set; } = "";
        public CDTOrderer orderer { get; set; }
        public CDTOrder order { get; set; }
    }

    public class CDTOrderer
    {
        [JsonProperty(PropertyName = "first-name")]
        public string firstname { get; set; }
        [JsonProperty(PropertyName = "last-name")]
        public string lastname { get; set; }
        public string nationality { get; set; }
        [JsonProperty(PropertyName = "employment-type")]
        public string employmenttype { get; set; }
        [JsonProperty(PropertyName = "date-of-birth")]
        public string dateofbirth { get; set; }
        //public string email { get; set; }
        [JsonProperty(PropertyName = "id-document-type")]
        public string iddocumenttype { get; set; }
        [JsonProperty(PropertyName = "id-document-number")]
        public string iddocumentnumber { get; set; }
        [JsonProperty(PropertyName = "home-phone-number")]
        public string homephonenumber { get; set; }
    }

    public class CDTOrder
    {
        public string id { get; set; }
        [JsonProperty(PropertyName = "created-at")]
        public string createdat { get; set; }
    }
}
