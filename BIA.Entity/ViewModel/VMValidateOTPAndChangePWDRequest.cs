using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class VMValidateOTPAndChangePWDRequest
    {
        [Required]
        public long otp { get; set; }
        [Required]
        public string user_name { get; set; }
        [Required]
        public string new_pwd { get; set; }
    }
}
