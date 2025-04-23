using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class OTPResponse : RACommonResponse
    {
        public bool is_otp_valid { get; set; }
    }

    public class OTPResponseRev
    {
        public bool isError { get; set; }
        public string message { get; set; }
        public OTPRespData data { get; set; }
    }
    public class OTPRespData
    {
        public bool is_otp_valid { get; set; }
    }

    public class DBSSOTPResponseRootobject
    {
        public DBSSOTPResponseRootobjectData data { get; set; }
    }

    public class DBSSOTPResponseRootobjectData
    {
        public string type { get; set; }
        public string id { get; set; }
        public DBSSOTPResponseRootobjectAttributes attributes { get; set; }
    }

    public class DBSSOTPResponseRootobjectAttributes
    {
        public string msisdn { get; set; }
        public int purpose { get; set; }
        public string identifier { get; set; }
        public string otp { get; set; }
    }
}
