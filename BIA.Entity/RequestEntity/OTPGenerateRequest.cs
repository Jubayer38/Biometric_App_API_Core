using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class OTPGenerateRequest
    {
        [Required]
        public string mobile_number { get; set; }
        [Required]
        public string user_name { get; set; }
        [Required]
        public string module_name { get; set; }
        public string lan { get; set; }
    }


    public class ValidateOTPAndChangePWDRequest
    {
        [Required]
        public string otp { get; set; }
        [Required]
        public string user_name { get; set; }
        [Required]
        public string new_pwd { get; set; }
    }
}
