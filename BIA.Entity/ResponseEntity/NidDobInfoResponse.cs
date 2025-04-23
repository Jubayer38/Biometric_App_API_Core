using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class NidDobInfoResponse
    {
        public string dest_nid { get; set; }
        public string dest_dob { get; set; }
        public string src_nid { get; set; }
        public string src_dob { get; set; }
        public bool result { get; set; }
        public string message { get; set; }
    }
}
