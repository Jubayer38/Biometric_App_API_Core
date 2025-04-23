using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class PurposeNumberReponse
    {
        public List<PurposeNumberReponseData> data { get; set; }
        public bool result { get; set; }
        public string message { get; set; }
    }

    public class PurposeNumberReponseData
    {
        public int purpose_id { get; set; }
        public string purpose_name { get; set; }
    }

    public class PurposeNumberReponseRev
    {
        public List<PurposeNumberReponseDataRev> data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }
    }

    public class PurposeNumberReponseDataRev
    {
        public int purpose_id { get; set; }
        public string purpose_name { get; set; }
    }
}
