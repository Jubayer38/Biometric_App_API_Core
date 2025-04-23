using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class RAGetPackageResquest
    {
        public string retailer_id { get; set; }
        /// <summary>
        /// 13/14
        /// </summary>
        public string subscription_id { get; set; }
        /// <summary>
        /// en/bn
        /// </summary>
        public string lan { get; set; }
        /// <summary>
        /// Session Token
        /// </summary>
        public string session_token { get; set; }
    }
    public class RAGetPackageResquestV2
    {
        public string retailer_id { get; set; }
        /// <summary>
        /// 13/14
        /// </summary>
        public string subscription_type_id { get; set; }
        /// <summary>
        /// en/bn
        /// </summary>
        public string lan { get; set; }
        /// <summary>
        /// Session Token
        /// </summary>
        public string session_token { get; set; }
    }

    public class RAGetPackageResquestV3
    {
        public string retailer_id { get; set; }
        /// <summary>
        /// 13/14
        /// </summary>
        public string subscription_id { get; set; }
        /// <summary>
        /// en/bn
        /// </summary>
        public string lan { get; set; }
        /// <summary>
        /// Session Token
        /// </summary>
        public string session_token { get; set; }

        public string category_name { get; set; }
    }
}
