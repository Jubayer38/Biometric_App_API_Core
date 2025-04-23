using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class BPOtpValidationRes
    {
        public bool is_otp_valid { get; set; }
        public string err_msg { get; set; }
    }
}
