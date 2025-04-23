using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class ChannelWiseResponse
    {
        public List<ChannelWiseResponseData> data { get; set; }
        public bool result { get; set; }
        public string message { get; set; }
    }

    public class ChannelWiseResponseData
    {

        public string payment_amount { get; set; }
        public string payment_method { get; set; }
    }

    public class ChannelWiseResponseRev
    {
        public List<ChannelWiseResponseDataRev> data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }
    }
     
    public class ChannelWiseResponseDataRev
    {

        public string payment_amount { get; set; }
        public string payment_method { get; set; }
    }
}
