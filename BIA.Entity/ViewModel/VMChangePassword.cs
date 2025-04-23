using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class VMChangePassword
    {
        public string username { get; set; }
        public string old_password { get; set; }
        public string new_password { get; set; }
    }
}
