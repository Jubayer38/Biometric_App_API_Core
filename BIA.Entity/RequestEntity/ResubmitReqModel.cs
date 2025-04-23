using BIA.Entity.CommonEntity;

namespace BIA.Entity.RequestEntity
{
    public class ResubmitReqModel : RACommonRequest
    {
        public string bi_token_number { get; set; }
        public string? retailer_id { get; set; }//user_id
        public string? distributor_code { get; set; }
        public int isBPUser { get; set; }
        public decimal? latitude { get; set; }

        /// <summary> 
        /// longitude
        /// </summary>
        public decimal? longitude { get; set; }
    }
}
