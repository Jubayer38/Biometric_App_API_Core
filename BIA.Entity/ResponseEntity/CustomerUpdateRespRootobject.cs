using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class CustomerUpdateRespRootobject
    {
        public List<CustomerUpdateRespDatum> data { get; set; }
    }

    public class CustomerUpdateRespDatum
    {
        public string type { get; set; }
        public string id { get; set; }
        public CustomerUpdateRespAttributes attributes { get; set; }
    }

    public class CustomerUpdateRespAttributes
    {
        public string requestid { get; set; }
        public string href { get; set; }
        public DateTime scheduledat { get; set; }
        public string status { get; set; }
    }
}
