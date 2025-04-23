using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class UnpairedSIMreqRootModel
    {
        public string retailer_code { get; set; }
        public string sim_serial { get; set; }
        public string[] product_category { get; set; } = new string[0];
        public string[] product_code { get; set; }= new string[0];
        public string user_name { get; set; } = "";
        public string password { get; set; } = "";


    }
    public class UnpairedSIMRespRootData
    {
        public object data { get; set; }
    }
}
