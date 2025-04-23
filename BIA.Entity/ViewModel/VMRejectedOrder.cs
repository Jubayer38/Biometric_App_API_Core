using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class VMRejectedOrder
    {
        public string quality_control_id { get; set; }
        public string customer_id { get; set; }
        public string mobile_number { get; set; }
        public string customer_name { get; set; }

        public int division_id { get; set; }
        public string division_name { get; set; }

        public int district_id { get; set; }
        public string district_name { get; set; }

        public int thana_id { get; set; }
        public string thana_name { get; set; }
        public string village { get; set; }

        public string alt_msisdn { get; set; }
        public string reject_reason { get; set; }
        public string rejection_date { get; set; }
        public string gender { get; set; }

        public string road_number { get; set; }
        public string house_number { get; set; }
        public string flat_number { get; set; }
        public string email { get; set; }
        public string postal_code { get; set; }
        public int is_over_due { get; set; }
    }
}
