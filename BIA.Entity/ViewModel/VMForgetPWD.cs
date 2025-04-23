using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class VMForgetPWD
    {
        public long user_id { get; set; }
        public string mobile_no { get; set; }
        public string new_pwd { get; set; }
        public string new_hashed_pwd { get; set; }
    }
}
