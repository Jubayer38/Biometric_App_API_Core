using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class UnreserveMSISDNRequestRootobject
    {
        public UnreserveMSISDNRequestData data { get; set; }
    }

    public class UnreserveMSISDNRequestData
    {
        public string id { get; set; } = "";
    }

}
