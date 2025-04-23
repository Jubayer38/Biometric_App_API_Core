using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.CommonEntity
{
    public class GeofenceReqModel : RACommonResponse
    {
        public double retilerLat { get; set; }
        public double retilerLon { get; set; }
    }
}
 