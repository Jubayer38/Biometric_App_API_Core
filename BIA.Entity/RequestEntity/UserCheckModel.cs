using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class UserCheckModel
    {
        public string user_name { get; set; }
        public string? bpmsisdn { get; set; } = "";
    }
}
