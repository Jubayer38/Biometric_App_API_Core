using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class QCStatusUpdateRequestRootobject
    {
        public QCStatusUpdateRequestData data { get; set; }
    }

    public class QCStatusUpdateRequestData
    {
        public string type { get; set; }
        public string id { get; set; }
        public QCStatusUpdateRequestAttributes attributes { get; set; }
    }

    public class QCStatusUpdateRequestAttributes
    {
        public string status { get; set; }
        public string reseller { get; set; }
    }
}
