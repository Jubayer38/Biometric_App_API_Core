using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class UnpairedSIMsearchReqModel : RACommonRequest
    {
        public string? user_name { get; set; }
        public string? password { get; set; }
        public string? product_category_prepaid { get; set; }
        public string? product_category_postpaid { get; set; }
        public string? product_category_simReplacement { get; set; }
        public string? product_code_simReplacement { get; set; }
        public string? product_code_prepaid { get; set; }
        public string? product_code_StarTrekPrepaid { get; set; }
        public string? product_code_StarTrekEsim { get; set; }
        public string? product_category_StarTrekEsim { get; set; }
        public string? product_category_StarTrekPrepaid { get; set; }
        public string? product_code_postpaid { get; set; }
        public string? retailer_code { get; set; }
        [Required]
        public string sim_serial { get; set; }
    }

    public class UnpairedSIMsearchReqStarTrekModel : RACommonRequest
    {
        public string? user_name { get; set; } 
        public string? password { get; set; }
        public string? product_category_prepaid { get; set; }
        public string? product_category_postpaid { get; set; }
        public string? product_category_simReplacement { get; set; }
        public string? product_code_simReplacement { get; set; }
        public string? product_code_prepaid { get; set; }
        public string? product_code_postpaid { get; set; }
        public string? retailer_code { get; set; }
        [Required]
        public string sim_serial { get; set; }
    }
}
