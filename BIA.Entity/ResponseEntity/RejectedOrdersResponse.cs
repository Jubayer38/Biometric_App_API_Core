using BIA.Entity.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    /// <summary>
    /// Rejected order's response type.
    /// </summary>
    public class RejectedOrdersResponse
    {
        /// <summary>
        /// Rejected orders data.
        /// </summary>
        public List<VMRejectedOrder> data { get; set; }
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool result { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
    }

    public class RejectedOrdersResponseRev
    {
        /// <summary>
        /// Rejected orders data. 
        /// </summary>
        public List<VMRejectedOrder> data { get; set; }
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
    } 

    public class RejectedOrdersRootobject
    {
        public List<RejectedOrdersDatum> data { get; set; }
    }

    public class RejectedOrdersDatum
    {
        public RejectedOrdersAttributes attributes { get; set; }
        public RejectedOrdersRelationships relationships { get; set; }
        public RejectedOrdersLinks3 links { get; set; }
        public string id { get; set; }
        public string type { get; set; }
    }

    public class RejectedOrdersAttributes
    {
        public string qcresponsible { get; set; }
        public string channel { get; set; }
        public string qcuser { get; set; }
        public string msisdn { get; set; }
        public string reason { get; set; }
        [JsonProperty(PropertyName = "last-modified")]
        public DateTime lastmodified { get; set; }
        [JsonProperty(PropertyName = "activation-time")]
        public DateTime activationtime { get; set; }
        public string status { get; set; }
        public string reseller { get; set; }
        public string confirmationcode { get; set; }
    }

    public class RejectedOrdersRelationships
    {
        [JsonProperty(PropertyName = "owner-customer")]
        public RejectedOrdersOwnerCustomer ownercustomer { get; set; }
        public RejectedOrdersSubscription subscription { get; set; }
        [JsonProperty(PropertyName = "user-customer")]
        public RejectedOrdersUserCustomer usercustomer { get; set; }
    }

    public class RejectedOrdersOwnerCustomer
    {
        public RejectedOrdersData data { get; set; }
        public RejectedOrdersLinks links { get; set; }
    }

    public class RejectedOrdersData
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class RejectedOrdersLinks
    {
        public string related { get; set; }
    }

    public class RejectedOrdersSubscription
    {
        public RejectedOrdersData1 data { get; set; }
        public RejectedOrdersLinks1 links { get; set; }
    }

    public class RejectedOrdersData1
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class RejectedOrdersLinks1
    {
        public string related { get; set; }
    }

    public class RejectedOrdersUserCustomer
    {
        public RejectedOrdersData2 data { get; set; }
        public RejectedOrdersLinks2 links { get; set; }
    }

    public class RejectedOrdersData2
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class RejectedOrdersLinks2
    {
        public string related { get; set; }
    }

    public class RejectedOrdersLinks3
    {
        public string self { get; set; }
    }



}
