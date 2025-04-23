using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class TOSLoanStatusResponse : RACommonResponse
    {
        public decimal dueLoanAmount { get; set; }
        public string dedicated_Ac_Id { get; set; }
        public decimal amount { get; set; }

    }
    public class TOSDebtStatusResponse : RACommonResponse
    {
        public decimal debt { get; set; }
    }
}
