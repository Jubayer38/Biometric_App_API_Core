using BIA.Entity.CommonEntity;

namespace BIA.Entity.ResponseEntity
{
    /// <summary>
    /// B2C SIM replacement reponse type.
    /// </summary>
    public class IndividualSIMReplacementMSISDNCheckResponse : SIMReplacementMSISDNCheckResponse
    {
        /// <summary>
        /// Customer's SAF status.
        /// </summary>
        public bool saf_status { get; set; }
        /// <summary>
        /// DBSS customer id.
        /// </summary>
        public string customer_id { get; set; }
    }

    public class CorporateSIMReplacementCheckResponseWithCustomerId : RACommonResponse
    {
        public string customer_id { get; set; }
        public long dbss_subscription_id { get; set; }
        public string old_sim_number { get; set; }
        public string old_sim_type { get; set; }
    }

    /// <summary>
    /// B2B SIM replacement mobile number validation type. 
    /// </summary>
    public class SIMReplacementMSISDNCheckResponse : RACommonResponse
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
        public long dbss_subscription_id { get; set; }
    }

    public class SIMReplacementMSISDNCheckResponseDataRev
    { 
        public bool isError { get; set; }
        public string message { get; set; }

        public SIMReplacementMSISDNCheckResponseRev data { get; set; }
    }
    public class SIMReplacementMSISDNCheckResponseRev
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
        public long dbss_subscription_id { get; set; }
    }

    public class BioCalcelMSISDNCheckParseResponse
    {
        public string nid { get; set; }
        public string dob { get; set; }
    }

    public class MSISDNCheckResponse : RACommonResponse
    {

        /// <summary>
        /// Customer's (parent) NID.
        /// </summary>
        public string nid { get; set; }
        /// <summary>
        /// Customer's (parent) DOB.
        /// </summary>
        public string dob { get; set; }
        /// <summary>
        /// DBSS subacription type.
        /// </summary>
        public long dbss_subscription_id { get; set; }
        /// <summary>
        /// Customer's SAF status.
        /// </summary>
        public bool saf_status { get; set; }
        /// <summary>
        /// DBSS customer id.
        /// </summary>
        public string customer_id { get; set; }
        /// <summary>
        /// dedicated_Ac_Id. 
        /// </summary>
        public string dedicated_Ac_Id { get; set; }
        /// <summary> 
        /// DBSS customer id.
        /// </summary>
        public decimal amount { get; set; }
    }

    public class MSISDNCheckResponseV2 : RACommonResponse
    {

        /// <summary>
        /// Customer's (parent) NID.
        /// </summary>
        public string nid { get; set; }
        /// <summary>
        /// Customer's (parent) DOB.
        /// </summary>
        public string dob { get; set; }
        /// <summary>
        /// DBSS subacription type.
        /// </summary>
        public long dbss_subscription_id { get; set; }
        /// <summary>
        /// Customer's SAF status.
        /// </summary>
        public bool saf_status { get; set; }
        /// <summary>
        /// DBSS customer id.
        /// </summary>
        public string customer_id { get; set; }

    }

    public class MSISDNCheckResponseRevamp
    {
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }

        public MSISDNCheckResponseV2 data { get; set; }


    }

    public class IndividualSIMReplacementMSISDNCheckResponseRevamp
    {
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }

        public IndividualSIMReplacementMSISDNCheckResponse data { get; set; }


    }

    public class SIMReplacementMSISDNCheckResponseRevamp
    {
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }

        public SIMReplacementMSISDNCheckResponse data { get; set; }
    }
}
