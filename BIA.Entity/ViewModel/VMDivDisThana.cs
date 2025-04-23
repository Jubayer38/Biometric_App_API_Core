using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class VMDivDisThana
    {
        public int division_id { get; set; }
        public string division_name { get; set; }

        public int district_id { get; set; }
        public string district_name { get; set; }
        public int div_dis_id { get; set; }

        public int thana_id { get; set; }
        public string thana_name { get; set; }
        public int dis_thana_id { get; set; }
    }
}
