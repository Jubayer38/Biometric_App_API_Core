using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class DBSSOTPValidationRequest
    {
        public string otp { get; set; }
        public string poc_msisdn { get; set; }
        public string auth_msisdn { get; set; }
        public int purpose { get; set; }
    }
    public class DBSSOTPValidationRequestRootobject
    {
        public DBSSOTPValidationRequestData data { get; set; }
    }
    public class DBSSOTPValidationRequestData
    {
        public int id { get; set; }
        public string type { get; set; }
        public DBSSOTPValidationRequestAttributes attributes { get; set; }
    }
    public class DBSSOTPValidationRequestAttributes
    {
        public string otp { get; set; }
        public string msisdn { get; set; }
        public string identifier { get; set; }
        public int purpose { get; set; }
    }
}
