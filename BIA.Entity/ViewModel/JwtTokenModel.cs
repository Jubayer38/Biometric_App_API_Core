using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class JwtTokenModel
    {
        public string ITopUpNumber { get; set; }
        public string RetailerCode { get; set; }
        public string DeviceId { get; set; }
        public string LoginProvider { get; set; }
        public string UserId { get; set; }

    }
}
