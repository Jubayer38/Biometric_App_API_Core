using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class eShopOrderResponseModel
    {
        public string status { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public eShopRespData data { get; set; }
    }
    public class eShopRespData
    {
        public int status_code { get; set; }
        public bool is_reserved { get; set; }
        public string? reservation_id { get; set; }
    }
}
