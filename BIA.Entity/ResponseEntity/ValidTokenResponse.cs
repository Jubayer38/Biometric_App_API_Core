using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class ValidTokenResponse
    {
        public string LoginProviderId { get; set; }
        public string ChannelName { get; set; }
        public string UserName { get; set; }
        public string DistributorCode { get; set; }
        public string CenterCode { get; set; }
        public bool IsVallid { get; set; }
        public string Message { get; set; }
    }
}
