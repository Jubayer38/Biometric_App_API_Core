using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class BioResModel
    {
        public ResData data { get; set; }
    }
    public class ResData
    {
        public string request_id { get; set; }
    }
}
