using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class RABioCancelResp : RACommonResponse
    {
        public long dbss_subscription_id { get; set; }
        public int dest_sim_category { get; set; }
    }

    public class RABioCancelRespRev
    {
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>  
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }

        public RABioCancelResData data { get; set; }
    }
    public class RABioCancelResData
    {
        public long dbss_subscription_id { get; set; }
        public int dest_sim_category { get; set; }
    }


    public class BioCancelationResponse : ResponseData
    {
        /// <summary>
        /// The response information of BI 
        /// </summary>
        public BioCanlcelData data { get; set; }
    }
    /// <summary>
    /// The implementation of absract class named Data with new column(s)
    /// </summary>
    public class BioCanlcelData : Data
    {

        /// <summary>
        /// The token no which provided for farther status update. 
        /// GENERATED FORMAT: [1 for INDIVIDUAL or 2 for CORPORATE][CURRENT DATE with format as YYYYMMDD][LAST SIX DIGIT with max TOKEN NO + 1]
        /// SAMPLE FOR CORPORATE: 220190627000004 where last one is 120190627000003
        /// </summary>
        public long RequestID { get; set; }
    }
}
