using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    /// <summary>
    /// This class is used for SIM number validation.
    /// </summary>
    public class SIMNumberCheckRequest
    {
        /// <summary>
        /// SIM Number, which is unique.   
        /// </summary>
        public string sim_number { get; set; }//in DBSS API it is mapped with serial_no.
        /// <summary>
        /// Language selected by App user.
        /// </summary>
        public string lan { get; set; } = "";

        public string center_code { get; set; } = "";
        public string distributor_code { get; set; } = "";
        public string retailer_id { get; set; } = "";
        public string product_code { get; set; } = "";
        public string purpose_number { get; set; } = "";
        public string session_token { get; set; }
        public string channel_name { get; set; } = "";
        public int? inventory_id { get; set; }
        public string msisdn { get; set; } = "";
    }
}
