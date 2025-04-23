using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class MSISDNValidationReqForMigration : RACommonRequest
    {
        /// <summary>
        /// MSISDN number which starts with 880. 
        /// </summary>
        [Required]
        public string mobile_number { get; set; }
        /// <summary>
        /// Language that defines in which language user wants to use device.
        /// </summary>
        public string lan { get; set; }
        /// <summary>
        /// Define Purpose Number to understand validation type. Currently purpose_number property contains value 
        /// while submitting order for diferrent purpose like new connection, sim replacement.
        /// For validation api request purpose_number inserted 0 from code level while log insert.
        /// </summary>
        public string purpose_number { get; set; }

        /// <summary>
        /// Reseller user name (id) (i.e. "201949")
        /// </summary>
        [Required]
        public string retailer_id { get; set; }

        /// <summary>
        /// SIM category (i.e. Prepaid = 1, Postpaid = 2)
        /// </summary> 
        public int? sim_category { get; set; }
        /// <summary>
        /// dbss_subscription_id
        /// </summary> 
        public long dbss_subscription_id { get; set; }
    }
}
