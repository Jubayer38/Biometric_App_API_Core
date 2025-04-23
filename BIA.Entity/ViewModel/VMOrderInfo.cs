using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class VMOrderInfo
    {
        public string alt_msisdn { get; set; }
        public string village { get; set; }
        public string gender { get; set; }
        public int thana_id { get; set; }
        public string thana_name { get; set; }
        public string road_number { get; set; }
        public string flat_number { get; set; }
        public string district_name { get; set; }
        public int district_id { get; set; }
        public string customer_name { get; set; }
        public string division_name { get; set; }
        public int division_id { get; set; }
        public string house_number { get; set; }
        public string email { get; set; }
        public string postal_code { get; set; }
        public string subscription_code { get; set; }
        public string subscription_type_id { get; set; }
        public string package_code { get; set; }
        public int package_id { get; set; }
        public string salesman_code { get; set; }
        public int is_urgent { get; set; }
        public string port_in_date { get; set; }
    }
}
