using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class SubscriptionTypeResModel
    {
        public List<SubscriptionTypeResData> data { get; set; }=new List<SubscriptionTypeResData>();
        public bool isError { get; set; }
        public string message { get; set; }
    }

    public class SubscriptionTypeResData
    {
        public int subscription_id { get; set; }
        public string subscription_name { get; set; }
    }
}
