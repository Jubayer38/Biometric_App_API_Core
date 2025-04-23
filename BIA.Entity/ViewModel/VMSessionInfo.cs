using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class VMSessionInfo
    {
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string role_name { get; set; }
        //public int department_id { get; set; }
        //public string department_name { get; set; }
        //public int device_id { get; set; }
        public bool force_change_confirmed { get; set; }

    }
}
