using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    /// <summary>
    /// TOS NID to NID Src MSISDN Check Model Class. 
    /// </summary>
    public class TosNidToNidMsisdnCheckRequest : RACommonRequest
    {
        /// <summary>
        /// MSISDN number which starts with 880. 
        /// </summary>
        [Required]
        public string mobile_number { get; set; }
        /// <summary>
        /// Language that defines in which language user wants to use device.
        /// </summary>
        public string lan { get; set; } = "";
        /// <summary>
        /// Define Purpose Number to understand validation type. Currently purpose_number property contains value 
        /// while submitting order for diferrent purpose like new connection, sim replacement.
        /// For validation api request purpose_number inserted 0 from code level while log insert.
        /// </summary>
        [Required]
        public string purpose_number { get; set; }
        /// <summary>
        /// Reseller user name (id) (i.e. "10001")
        /// </summary>
        [Required]
        public string retailer_id { get; set; }
        /// <summary>
        /// Reseller channel name (i.e. "RESELLER", "Corporate")
        /// </summary>
        [Required]
        public string channel_name { get; set; } = "";
        /// <summary>
        /// Reseller center code.
        /// </summary>
        public string center_code { get; set; } = "";
    }
}
