using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class SiteIdRequestModel :RACommonRequest
    {
        public decimal lac { get; set; }
        public decimal cid { get; set; }
    }
}
