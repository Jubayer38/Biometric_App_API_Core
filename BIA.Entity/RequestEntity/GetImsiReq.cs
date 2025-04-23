using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class GetImsiReq
    {
        public string? sim { get; set; }
        public string? msisdn { get; set; }
        public string? purpose_number { get; set; }
        public string? retailer_id { get; set; }
    }
}
