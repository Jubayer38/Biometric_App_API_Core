using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    /// <summary>
    /// This class is used for geting the response of subscription type ID.
    /// </summary>
    public class SubscriptionTypeReponse
    {
        public List<SubscriptionTypeReponseData> data { get; set; }
        public SubscriptionTypeReponse()
        {
            data = new List<SubscriptionTypeReponseData>();
        }
        public bool result { get; set; }
        public string message { get; set; }
    }

    public class SubscriptionTypeReponseData
    {
        /// <summary>
        /// 
        /// </summary>
        /// 
        public string subscription_id { get; set; }
        public string subscription_name { get; set; }

    }

    public class UnpairedMSISDNData
    {
        public List<ReponseData> data { get; set; }
        public UnpairedMSISDNData()
        {
            data = new List<ReponseData>();
        }
        public bool result { get; set; }
        public string message { get; set; }
    }

    public class ReponseData
    {
        public string msisdn { get; set; }
    }

    public class UnpairedMSISDNDataRev
    {
        public List<ReponseDataRev> data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }
    }

    public class PairedMSISDNDataRev
    {
        public ReponseDataRev data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }
    }

    public class ReponseDataRev
    {
        public string msisdn { get; set; }
    }

    /// <summary>
    /// This class is used for geting the response of subscription type ID.
    /// </summary>
    public class SubscriptionTypeReponseRev
    {
        public List<SubscriptionTypeReponseDataRev> data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; } 
    } 

    public class SubscriptionTypeReponseDataRev
    {
        /// <summary>
        /// 
        /// </summary>
        /// 
        public string subscription_id { get; set; }
        public string subscription_name { get; set; }

    }
}
