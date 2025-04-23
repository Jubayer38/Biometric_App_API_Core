using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    /// <summary>
    /// Order submission resposne type.
    /// </summary>
    public class SendOrderResponse
    {
        /// <summary>
        /// Is ESIM
        /// </summary>
        public int is_esim { get; set; }
        //public SendOrderResponse()
        //{
        //    data = new SendOrderResponseData();
        //}
        /// <summary>
        /// Order submit request Id.
        /// </summary>
        public string request_id { get; set; }
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool is_success { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
    }

    public class DataRes
    {
        public int isEsim { get; set; }
        /// <summary>
        /// Order submit request Id. 
        /// </summary>
        public string request_id { get; set; }

    }

    public class SendOrderResponseRev
    { 
        public DataRes data { get; set; }

        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
    }

    public class SendOrderResponse2
    {
        //public SendOrderResponse()
        //{
        //    data = new SendOrderResponseData();
        //}
        /// <summary>
        /// Order submit request Id.
        /// </summary>
        public string request_id { get; set; }
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool is_success { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
    }

    public class SendOrderResponseRevamp
    {
        /// <summary>
        /// Is ESIM
        /// </summary>
        public int is_esim { get; set; }
        //public SendOrderResponse()
        //{
        //    data = new SendOrderResponseData();
        //}
        /// <summary>
        /// Order submit request Id.
        /// </summary>
        public string request_id { get; set; }
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool isError { get; set; } 
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
    }
    public class SendOrderResponseData
    {
        /// <summary>
        /// The token no which provided for farther status update. 
        /// GENERATED FORMAT: [1 for INDIVIDUAL or 2 for CORPORATE][CURRENT DATE with format as YYYYMMDD][LAST SIX DIGIT with max TOKEN NO + 1]
        /// SAMPLE FOR CORPORATE: 220190627000004 where last one is 120190627000003
        /// </summary>
        public string request_id { get; set; }
    }
}
