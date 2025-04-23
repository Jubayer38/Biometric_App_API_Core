using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class CustomerInfoUpdaterRequestRootobject
    {
        public CustomerInfoUpdaterRequestData data { get; set; }
    }

    public class CustomerInfoUpdaterRequestData
    {
        public string type { get; set; }
        public string id { get; set; }
        public CustomerInfoUpdaterRequestAttributes attributes { get; set; }
    }

    public class CustomerInfoUpdaterRequestAttributes
    {
        public string email { get; set; }
        public string alt_contact_phone { get; set; }
        [JsonProperty(PropertyName = "first-name")]
        public string firstname { get; set; }

        public string gender { get; set; }

        [JsonProperty(PropertyName = "legal-address")]
        public CustomerInfoUpdaterRequestLegalAddress legaladdress { get; set; }
    }

    public class CustomerInfoUpdaterRequestLegalAddress
    {
        public string type { get; set; }
        public string area { get; set; }
        [JsonProperty(PropertyName = "flat-number")]
        public string flatnumber { get; set; }
        public string thana { get; set; }
        public string country { get; set; }
        public string division { get; set; }
        public string road { get; set; }
        [JsonProperty(PropertyName = "house-number")]
        public string housenumber { get; set; }
        public string district { get; set; }
        [JsonProperty(PropertyName = "post-code")]
        public string postcode { get; set; }
    }
}
