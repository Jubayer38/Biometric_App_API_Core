namespace BIA.Entity.ResponseEntity
{
    public class ResubmitResponseModel
    {

        public ResubmitResponseModelData data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }
    }
    public class ResubmitResponseModelData
    {
        public string bi_token_number { get; set; }
        public string bss_request_id { get; set; }
        public string customer_name { get; set; }
        public string purpose_number { get; set; }
        public string msisdn { get; set; }
        public string dest_sim_category { get; set; }
        public string dest_doc_type_no { get; set; }
        public string dest_doc_id { get; set; }
        public string dest_dob { get; set; }
        public string src_doc_id { get; set; }
        public string src_doc_type_no { get; set; }
        public string src_dob { get; set; }
        public string platform_id { get; set; }
        public string payment_type { get; set; }
        public int? is_paired { get; set; }
        public int? channel_id { get; set; }
        public string sim_replc_reason { get; set; }
        public int? right_id { get; set; }
        public string alt_msisdn { get; set; }
        public string dest_sim_number { get; set; }
        public string gender { get; set; }
        public string? flat_number { get; set; }
        /// <summary>
        /// Customer's house number.
        /// </summary>
        public string? house_number { get; set; }
        /// <summary>
        /// Road number of customer's house. 
        /// </summary>
        public string? road_number { get; set; }
        /// <summary>
        /// Customre's village name.
        /// </summary>
        public string village { get; set; }
        /// <summary>
        /// Customer's division id.
        /// </summary>
        public int? division_id { get; set; }
        /// <summary>
        /// Customer's district id.
        /// </summary>
        public int? district_id { get; set; }
        /// <summary>
        /// Customer's thana Id.
        /// </summary>
        public int? thana_id { get; set; }
        /// <summary>
        /// Customer's living area's postal code.
        /// </summary>
        public string? postal_code { get; set; }
        /// <summary>
        /// Customer's email.
        /// </summary>
        public string? email { get; set; }


        public string? port_in_date { get; set; }

        public int? is_urgent { get; set; }//0/1

        public string? division_name { get; set; }

        public string? district_name { get; set; }

        public string? thana_name { get; set; }

        public string subscription_code { get; set; }
        public int? subscription_type_id { get; set; }
        public string package_code { get; set; }
        public int? package_id { get; set; }
        public string? order_id { get; set; }
    }
} 
