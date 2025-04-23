using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class APIVersionRequest
    {
        [Required]
        public string username { get; set; }
    }

    /// <summary>
    /// This class is using for check reseller app information. 
    /// </summary>
    public class APIVersionRequestWithAppUpdateCheck
    {
        /// <summary>
        /// Reseller user name. 
        /// </summary>
        [Required]
        public string username { get; set; }

        /// <summary>
        /// Reseller App Apk version.  
        /// </summary>
        public int? appVersion { get; set; }
    }
    public class APIVersionRequestWithAppUpdateCheckForGotPass
    {
        /// <summary>
        /// Reseller user name. 
        /// </summary>
        [Required]
        public string username { get; set; }

    }

}
