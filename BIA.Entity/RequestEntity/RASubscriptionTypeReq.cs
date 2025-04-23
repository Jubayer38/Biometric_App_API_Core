using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class RASubscriptionTypeReq
    {
        public string retailer_id { get; set; }
        /// <summary>
        /// prepaid/postpaid
        /// </summary>
        public string subscription_type { get; set; } = "";
        /// <summary>
        /// en/bn
        /// </summary>
        public string? lan { get; set; }
        /// <summary>
        /// Session Token
        /// </summary>
        public string session_token { get; set; }
        public string channel_name { get; set; } = "";
    }
}
