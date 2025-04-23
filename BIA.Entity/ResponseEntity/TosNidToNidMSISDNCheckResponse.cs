using BIA.Entity.CommonEntity;

namespace BIA.Entity.ResponseEntity
{
    public class TosNidToNidMSISDNCheckResponse : RACommonResponse
    {
        /// <summary>
        /// Customer's (parent) NID.
        /// </summary>
        public string doc_id_number { get; set; }
        /// <summary>
        /// Customer's (parent) DOB.
        /// </summary>
        public string dob { get; set; }
        /// <summary>
        /// Customer's () old SIM number.
        /// </summary>
        public string old_sim_number { get; set; }
        /// <summary>
        /// Customer's () old SIM type (i.e. Prepaid = 1, Postpaid = 2).
        /// </summary>
        public string old_sim_type { get; set; }
        /// <summary>
        /// DBSS subacription type.
        /// </summary>
        public long? dbss_subscription_id { get; set; }

        /// <summary>
        /// source coustomer's sim category.
        /// </summary>
        public int src_sim_category { get; set; }

        /// <summary>
        /// source coustomer's owner customer id
        /// </summary>
        public string src_owner_customer_id { get; set; }

        /// <summary>
        /// source coustomer's user customer id.
        /// </summary>
        public string src_user_customer_id { get; set; }

        /// <summary>
        /// source coustomer's payer customer id.
        /// </summary>
        public string src_payer_customer_id { get; set; }
    }
    public class TosNidToNidMSISDNCheckResponseRevamp
    {
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>  
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }

        public TosNidToNidMSISDNCheckResponse data { get; set; }
    }
}
