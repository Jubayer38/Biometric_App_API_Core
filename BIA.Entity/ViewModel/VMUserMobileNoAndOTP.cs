using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class VMUserMobileNoAndOTP : RACommonResponse
    {
        public long user_id { get; set; }
        public string mobile_no { get; set; }
        public string PWD { get; set; }
    }
}
