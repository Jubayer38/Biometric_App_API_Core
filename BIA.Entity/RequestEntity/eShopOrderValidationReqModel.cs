using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class eShopOrderValidationReqModel : RACommonRequest
    {
        public string orderId { get; set; } 
        public string msisdn { get; set; } 
        public string retailer_id { get; set; } 
    }
}
 