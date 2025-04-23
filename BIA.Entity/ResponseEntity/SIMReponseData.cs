using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class UnpairedSIMData
    {
        public List<SIMReponseData> data { get; set; }
        public UnpairedSIMData()
        {
            data = new List<SIMReponseData>();
        }
        public bool result { get; set; }
        public string message { get; set; }
    }

    public class SIMReponseData
    {
        public string sim_serial { get; set; }
    }

    public class UnpairedSIMDataRev
    {
        public List<SIMReponseDataRev> data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }
    }

    public class SIMReponseDataRev
    {
        public string sim_serial { get; set; }
    }
}
