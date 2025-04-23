using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.CommonEntity
{
    /// <summary>
    /// Common request object type for reseller app.
    /// </summary>
    public class RACommonRequest
    {
        /// <summary>
        /// Security token for validating if the user is valid to get access to the api.
        /// </summary>
        [Required]
        public string session_token { get; set; }
        /// <summary>
        /// User role right id. This property is not currently using. But this property can be ueful in future.
        /// </summary>
        public int? right_id { get; set; }
    }

    public class RAGetCustomerInfoByTokenNoRequest : RACommonRequest
    {
        [Required]
        public string token_id { get; set; }
    }

    public class RAGetPurposeRequest : RACommonRequest
    {
        [Required]
        public int case_id { get; set; }
    }

    /// <summary>
    /// Get activity log, pending list and activation list request data type.
    /// </summary>
    public class RAOrderActivityRequest : RACommonRequest
    {
        /// <summary>
        /// Type: ACTIVITY LOG = 1, PENDING LIST = 2, ACTIVATION LIST = 3
        /// </summary>
        [Required]
        public int activity_type_id { get; set; }
    }

    public class RAGetSubscriptionsRequest 
    {
        [Required]
        public string channel_name { get; set; }
    }
}
