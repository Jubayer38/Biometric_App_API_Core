using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class QCStatusResponseRootobject
    {
        public List<QCStatusResponseDatum> data { get; set; }
    }

    public class QCStatusResponseDatum
    {
        public string type { get; set; }
        public string id { get; set; }
        public QCStatusResponseAttributes attributes { get; set; }
    }

    public class QCStatusResponseAttributes
    {
        public string requestid { get; set; }
        public string href { get; set; }
        public DateTime scheduledat { get; set; }
        public string status { get; set; }
    }
}
