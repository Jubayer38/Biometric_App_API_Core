using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class RechargeAmountReqModel
    {
        /// <summary>
        /// Security token for validating if the user is valid to get access to the api.
        /// </summary>
        [Required]
        public string session_token { get; set; }
        public string retailer_code { get; set; }
        public string channel_name { get; set; }
    }

    public class RechargeAmountReqModelRev
    {
        /// <summary>
        /// Security token for validating if the user is valid to get access to the api.
        /// </summary>
        [Required]
        public string session_token { get; set; }
        public string retailer_code { get; set; }
        public string channel_name { get; set; }

        [Required]
        public string bi_token_number { get; set; }
    }
}
