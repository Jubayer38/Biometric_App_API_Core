using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class DMSRetailerReqModel
    {
        [Required]
        public string userName { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string retailerCode { get; set; }
        public string iTopUpNumber { get; set; }
        [Required]
        public int isActive { get; set; }
        public string typeName { get; set; }
    }
}
