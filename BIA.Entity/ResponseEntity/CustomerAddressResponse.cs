using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{

    public class CustomerAddressResponse
    {
    }


    public class CustomerAddressResponseRootobject
    {
        public List<CustomerAddressResponseDatum> data { get; set; }
    }

    public class CustomerAddressResponseDatum
    {
        [JsonProperty(PropertyName = "type")]
        public string type { get; set; }
        public CustomerAddressResponseAttributes attributes { get; set; }
        public string id { get; set; }
        public CustomerAddressResponseLinks links { get; set; }
    }

    public class CustomerAddressResponseAttributes
    {
        public string city { get; set; }
        public string area { get; set; }
        [JsonProperty(PropertyName = "postal-code")]
        public string postalcode { get; set; }
        public string co { get; set; }
        public string addresstype { get; set; }
        public string apartment { get; set; }
        public string validated { get; set; }
        public string country { get; set; }
        public string building { get; set; }
        public string county { get; set; }
        public string lastmodified { get; set; }
        public string floor { get; set; }
        public string province { get; set; }
        public CustomerAddressResponseCountryName countryname { get; set; }
        public string postalbox { get; set; }
        public string street { get; set; }
        public string road { get; set; }
        public string room { get; set; }
        [JsonProperty(PropertyName = "postal-district")]
        public string postaldistrict { get; set; }
    }

    public class CustomerAddressResponseCountryName
    {
        public string fr { get; set; }
        public string en { get; set; }
        public string de { get; set; }
        public string it { get; set; }
    }

    public class CustomerAddressResponseLinks
    {
        public string self { get; set; }
    }
}
