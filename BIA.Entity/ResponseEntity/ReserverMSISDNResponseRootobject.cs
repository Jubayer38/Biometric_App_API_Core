using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class ReserverMSISDNResponseRootobject
    {
        public ReserverMSISDNResponseData data { get; set; }
    }

    public class ReserverMSISDNResponseData
    {
        public int status { get; set; }
    }
}
