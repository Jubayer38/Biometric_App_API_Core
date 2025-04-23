using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    /// <summary>
    /// The model for changing authentication password from external user
    /// </summary>
    public class ChangePasswordRequests
    {
        [Required]
        public string session_token { get; set; }
        /// <summary>
        /// The password which currently used by user
        /// </summary>
        /// 
        [Required]
        public string old_password { get; set; }
        /// <summary>
        /// The new password which will be applied for next login
        /// </summary>
        [Required]
        public string new_password { get; set; }
        [Required]
        public string user_id { get; set; }
    }


    public class ChangePasswordRequestsByOTP
    {
        /// <summary>
        /// The new password which will be applied for next login
        /// </summary>
        [Required]
        public string new_password { get; set; }
        [Required]
        public string user_id { get; set; }
    }
}
