using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class SIMValidationRequestRootobject
    {
        public SIMValidationRequestRootobject()
        {

        }
        public SIMValidationRequestRootobject(SIMValidationRequestData data)
        {
            this.data = data;
        }
        public SIMValidationRequestData data { get; set; }
    }

    public class SIMValidationRequestData
    {
        public SIMValidationRequestData(string type, string id, SIMValidationRequestAttributes attributes)
        {
            this.type = type;
            this.id = id;
            this.attributes = attributes;
        }
        public string type { get; set; }
        public string id { get; set; }
        public SIMValidationRequestAttributes attributes { get; set; }
    }

    public class SIMValidationRequestAttributes
    {
        public SIMValidationRequestAttributes(string center_code, string distributor_code
            , string retailer_code, string product_code, string serial_no)
        {
            this.center_code = center_code;
            this.distributor_code = distributor_code;
            this.retailer_code = retailer_code;
            this.product_code = product_code;
            this.serial_no = serial_no;
        }
        public string center_code { get; set; } = "";
        public string distributor_code { get; set; } = "";
        public string retailer_code { get; set; } = "";
        public string product_code { get; set; } = "";
        public string serial_no { get; set; } = "";
    }
}
