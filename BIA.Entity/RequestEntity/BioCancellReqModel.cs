using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class BioCancellReqModel
    {
        public BioCancellData data { get; set; }
        public BioCancellMeta meta { get; set; }
    }

    public class BioCancellData
    {
        public string type { get; set; } = "";
        public string id { get; set; } = "";
        public BioCancellAttributes attributes { get; set; }
    }

    public class BioCancellAttributes
    {
        public string biometric_request_id { get; set; }
        public int status { get; set; }=0;
    }

    public class BioCancellMeta
    {
        public string reason { get; set; } = "";
        public string channel { get; set; } = "";
        public string reseller { get; set; } = "";
        public string salesman { get; set; } = "";
    }
}
