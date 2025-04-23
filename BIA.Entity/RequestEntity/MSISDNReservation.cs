using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class MSISDNReservation
    {
        public MSISDN data { get; set; }
    }
    public class MSISDN
    {
        public string msisdn { get; set; }
    }
}
