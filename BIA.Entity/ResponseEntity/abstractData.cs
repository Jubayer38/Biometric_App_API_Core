using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    /// <summary>
    /// The Abstract response model after processing API request
    /// </summary>
    public abstract class Data
    {
        public string message { get; set; }
        /// <summary>
        /// Define Operational level passed or failed result of the provided request. Always return true or false. Default value is false
        /// </summary>
        public bool is_success { get; set; }

        /// <summary>
        /// The specified code for error identification
        /// </summary>
        public string error_code { get; set; }
        /// <summary>
        /// The specified error message which is pre-de
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// The source from where error found (like CBVMP, EC, NPGW, BSS etc.)
        /// </summary>
        public string error_source { get; set; }
        /// <summary>
        /// Id number of a request.
        /// </summary>
        public string request_id { get; set; }


    }
}
