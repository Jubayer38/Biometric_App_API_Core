using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    /// <summary>
    /// Rejected orders request type.
    /// </summary>
    public class RejectedOrdersRequest : RACommonRequest
    {
        /// <summary>
        /// Reseller name/id. 
        /// </summary>
        public string retailer_id { get; set; }
        /// <summary>
        /// Reseller app language.
        /// </summary>
        public string lan { get; set; } = "";
    }
}
