using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class DBSSOTPValidationReq : RACommonRequest
    {
        [Required]
        public string otp { get; set; }

        [Required, MinLength(11), MaxLength(13)]
        public string src_msisdn { get; set; }

        [Required, MinLength(11), MaxLength(13)]
        public string dest_msisdn { get; set; }

        [Required]
        public int? purpose_number { get; set; }

        [Required]
        public string retailer_id { get; set; }
    }
}
