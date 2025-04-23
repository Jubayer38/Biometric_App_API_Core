using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    /// <summary>
    /// Get status response type.
    /// </summary>
    public class GetStatusResponse
    {
        /// <summary>
        /// Order status code. (i.e. 10 = "Order submited", 20 = "Bio verification submitted.")
        /// </summary>
        public int? status { get; set; }
        /// <summary>
        /// Order status name.
        /// </summary>
        public string status_name { get; set; }
        /// <summary>
        /// Mobile number 
        /// </summary>
        public string msisdn { get; set; }
        /// <summary>
        /// Status of if the order have finished or not.
        /// </summary>
        public bool? is_finished { get; set; }
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool result { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
    }
     
    public class GetStatusResponseRevamp
    {
        public GetStatusResponseDataRevamp data { get; set; } 
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
    }
    public class GetStatusResponseDataRevamp
    {
        /// <summary>
        /// Order status code. (i.e. 10 = "Order submited", 20 = "Bio verification submitted.")
        /// </summary>
        public int? status { get; set; }
        /// <summary>
        /// Order status name.
        /// </summary>
        public string status_name { get; set; }
        /// <summary>
        /// Mobile number 
        /// </summary>
        public string msisdn { get; set; }
        /// <summary>
        /// Status of if the order have finished or not.
        /// </summary>
        public bool? is_finished { get; set; }
    }
}
