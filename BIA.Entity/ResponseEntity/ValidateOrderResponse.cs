using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class ValidateOrderResponse
    {
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool result { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// If order is in ongoing process then validation Error code will be -111 or -222 or -333 or -999, 
        /// else it will 1.
        /// </summary>
        public int error_code { get; set; }
    }
}
