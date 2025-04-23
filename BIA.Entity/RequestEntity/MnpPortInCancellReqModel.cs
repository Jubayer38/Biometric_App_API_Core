using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class MnpPortInCancellReqModel
    {
        public MnpPortInCancellData data { get; set; }
    }
    public class MnpPortInCancellData
    {
        public string type { get; set; }
        public string id { get; set; }
        public MnpPortInCancellAttributes attributes { get; set; }
    }

    public class MnpPortInCancellAttributes
    {
        public string id { get; set; }
        public string biometric_request_id { get; set; }
    }
}
