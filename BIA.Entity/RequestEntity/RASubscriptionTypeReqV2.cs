using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class RASubscriptionTypeReqV2
    {
        public string retailer_id { get; set; }
        /// <summary>
        /// prepaid/postpaid
        /// </summary>
        //public string subscription_type { get; set; }
        /// <summary>
        /// en/bn
        /// </summary>
        public string lan { get; set; } = "";
        /// <summary>
        /// Session Token
        /// </summary>
        public string session_token { get; set; }
        /// <summary>
        /// channel_name
        /// </summary>
        //public string channel_name { get; set; }
        /// <summary>
        /// dbss_subscription_id
        /// </summary>
        public long dbss_subscription_id { get; set; } = 0;
    }
}
