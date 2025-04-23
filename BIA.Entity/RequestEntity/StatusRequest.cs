using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    /// <summary>
    /// Get order current status request type.
    /// </summary>
    public class StatusRequest : RACommonRequest
    {
        /// <summary>
        /// Order request Id or Token.
        /// </summary>
        public string request_id { get; set; }
    }
}

