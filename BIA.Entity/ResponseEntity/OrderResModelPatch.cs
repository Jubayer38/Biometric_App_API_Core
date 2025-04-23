using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class OrderResModelPatch
    {
        public List<OrderResPathDatum> data { get; set; }
    }

    public class OrderResPathDatum
    {
        public string type { get; set; }
        public string id { get; set; }
        public OrderResPathAttributes attributes { get; set; }
    }

    public class OrderResPathAttributes
    {
        public string requestid { get; set; }
        public string href { get; set; }
        public DateTime scheduledat { get; set; }
        public string status { get; set; }
    }
}
