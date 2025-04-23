using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class BlackListedWordModel
    {
        public bool isError { get; set; }
        public string message { get; set; } 
        public string[] data { get; set; }
    }
}
