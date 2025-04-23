using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    /// <summary>
    /// Biometric verification update reposne type.
    /// </summary>
    public class BIFinishNotiResponse
    {
        /// <summary>
        /// Define  passed or failed result of the provided request. 
        /// </summary>
        public bool is_success { get; set; }

        /// <summary>
        /// Define the success or error message of provided request processing result
        /// </summary>
        public string message { get; set; }
    }
}