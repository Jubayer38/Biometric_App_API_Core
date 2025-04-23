using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class BPOtpValidationReq : RACommonRequest
    {
        public string bp_otp { get; set; }
        public string retailer_otp { get; set; }
    }
}
