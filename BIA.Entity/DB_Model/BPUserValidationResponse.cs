using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.DB_Model
{
    public class BPUserValidationResponse
    {
        public bool is_valid { get; set; }
        public string err_msg { get; set; }
    }
}
