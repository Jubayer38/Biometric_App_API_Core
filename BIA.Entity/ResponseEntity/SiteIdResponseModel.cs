using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class SiteIdResponseModel
    {
        public bool isError { get; set; }
        public string message { get; set; }
        public BTSCode data { get; set; }
    }
    public class BTSCode
    {
        public string bts_code { get; set; }
    }
}
