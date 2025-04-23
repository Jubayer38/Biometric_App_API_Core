using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class RechargeResponseModel
    {
        public bool isError { get; set; }
        public string message { get; set; }
    }
    public class RechargeResponse
    {
        public bool result { get; set; }
        public string message { get; set; } 
    }
}
