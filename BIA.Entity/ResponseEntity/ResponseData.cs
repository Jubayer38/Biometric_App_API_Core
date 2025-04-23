using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    /// <summary>
    /// The absract model for common response for each request
    /// </summary>
    public abstract class ResponseData
    {
        /// <summary>
        /// Define API level passed or failed result of the provided request. Always return true or false. Default value is false
        /// </summary>
        public bool is_api_success { get; set; }

        /// <summary>
        /// Define the success or error message of provided request processing result
        /// </summary>
        public string message { get; set; }


    }
}
