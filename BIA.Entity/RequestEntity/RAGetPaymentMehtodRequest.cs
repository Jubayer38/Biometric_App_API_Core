using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class RAGetPaymentMehtodRequest : RACommonRequest
    {
        [Required]
        public int channel_id { get; set; }
    }
}
