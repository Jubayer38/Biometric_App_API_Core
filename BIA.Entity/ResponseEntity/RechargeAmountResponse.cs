using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class RechargeAmountData
    {
        public List<RechargeAmountResponse> data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; } 
    }
    public class RechargeAmountResponse
    {
        public double amountId { get; set; }

        public double rechargeAmount { get; set; }
    }
}
