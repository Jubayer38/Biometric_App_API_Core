using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.DB_Model
{
    public class ErrorDescription
    {
        public long error_id { get; set; }
        public string error_code { get; set; }
        public string error_description { get; set; }
        public string error_custom_msg { get; set; }
        public string error_source { get; set; }
        public int error_type_id { get; set; }
    }
}
