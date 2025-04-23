using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class RAOrderRequest : RACommonRequest
    {
        public bool is_success { get; set; } = false;
        public string err_code { get; set; } = "";
        public string err_msg { get; set; } = "";
        public string bss_reqId { get; set; } = "";
        public int status { get; set; } = 0;
        public long error_id { get; set; } = 0;
        /// <summary>
        /// BI token number. For new order submit it is 0, for re-submit order it will be long like "191230632808". 
        /// </summary>
        //[Required]
        public double? bi_token_number { get; set; } = 0;
        /// <summary>
        /// Currently purpose_number property contains value 
        /// while submitting order for diferrent purpose like new connection, sim replacement.
        //[Required]
        public string purpose_number { get; set; } = "";
        /// <summary>
        /// Mobile number 
        /// </summary>
        public string msisdn { get; set; }
        /// <summary>
        /// SIM category (i.e. Prepaid = 1, Postpaid = 2)
        /// </summary>
        public int? sim_category { get; set; }
        /// <summary>
        /// SIM Number.   
        /// </summary>
        public string sim_number { get; set; } = "";
        /// <summary>
        /// DBSS subscription type id.
        /// </summary>
        public string subscription_type_id { get; set; } = "";
        /// <summary>
        /// DBSS subscription code.
        /// </summary>
        public string subscription_code { get; set; } = "";
        /// <summary>
        /// Subscription package id.
        /// </summary>
        public string package_id { get; set; } = "";
        /// <summary>
        /// Subscription package code.
        /// </summary>
        public string package_code { get; set; } = "";
        /// <summary>
        /// Customer's(Parent) national id.
        /// </summary>
        public string dest_nid { get; set; }
        /// <summary>
        /// Customer's(Child) national id.
        /// </summary>
        public string src_nid { get; set; } = "";
        /// <summary>
        /// Customer's(Parent) date of birth.
        /// </summary>
        public string dest_dob { get; set; }
        /// <summary>
        /// Customer's(Parent) date of birth.
        /// </summary>
        public int? dest_doc_type_no { get; set; }
        /// <summary>
        /// Customer's(Child) date of birth.
        /// </summary>
        public string src_dob { get; set; } = "";
        /// <summary>
        /// Customer's(Child) natinal ID.
        /// </summary>
        public int? src_doc_type_no { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string platform_id { get; set; } = "";
        /// <summary>
        /// Customer's full name.
        /// </summary>
        public string customer_name { get; set; } = "";
        /// <summary>
        /// Customer's gender.
        /// </summary>
        public string gender { get; set; } = "";
        /// <summary>
        /// Customer's resident flat number.
        /// </summary>
        public string flat_number { get; set; } = "";
        /// <summary>
        /// Customer's house number.
        /// </summary>
        public string house_number { get; set; } = "";
        /// <summary>
        /// Road number of customer's house. 
        /// </summary>
        public string road_number { get; set; } = "";
        /// <summary>
        /// Customre's village name.
        /// </summary>
        public string village { get; set; } = "";
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
        public string postal_code { get; set; } = "";
        /// <summary>
        /// Customer's email.
        /// </summary>
        public string email { get; set; } = "";
        //public string retailer_code { get; set; }
        /// <summary>
        /// Customer's(parent) left thumb finger print score.
        /// </summary>
        public int? dest_left_thumb_score { get; set; }
        /// <summary>
        /// Customer's(parent) left thumb finger print.
        /// </summary>
        public string dest_left_thumb { get; set; }
        /// <summary>
        /// Customer's(parent) left index finger print score.
        /// </summary>
        public int? dest_left_index_score { get; set; }
        /// <summary>
        /// Customer's(parent) left index finger print.
        /// </summary>
        public string dest_left_index { get; set; }
        /// <summary>
        /// Customer's(parent) right thumb finger print score.
        /// </summary>
        public int? dest_right_thumb_score { get; set; }
        /// <summary>
        /// Customer's(parent) right thumb finger print.
        /// </summary>
        public string dest_right_thumb { get; set; }
        /// <summary>
        /// Customer's(parent) right index finger print score.
        /// </summary>
        public int? dest_right_index_score { get; set; }
        /// <summary>
        /// Customer's(parent) right index finger print.
        /// </summary>
        public string dest_right_index { get; set; }
        /// <summary>
        /// Customer's(chile) left thumb finger print score.
        /// </summary>
        public int? src_left_thumb_score { get; set; }
        /// <summary>
        /// Customer's(chile) left thumb finger print.
        /// </summary>
        public string src_left_thumb { get; set; } = "";
        /// <summary>
        /// Customer's(chile) left index finger print score.
        /// </summary>
        public int? src_left_index_score { get; set; }
        /// <summary>
        /// Customer's(chile) left index finger print.
        /// </summary>
        public string src_left_index { get; set; } = "";
        /// <summary>
        /// Customer's(chile) right thumb finger print score.
        /// </summary>
        public int? src_right_thumb_score { get; set; }
        /// <summary>
        /// Customer's(chile) right thumb finger print.
        /// </summary>
        public string src_right_thumb { get; set; } = "";
        /// <summary>
        /// Customer's(chile) right index finger print score.
        /// </summary>
        public int? src_right_index_score { get; set; }
        /// <summary>
        /// Customer's(chile) right index finger print.
        /// </summary>
        public string src_right_index { get; set; } = "";
        //[Required]
        /// <summary>
        /// Reseller user name(id) (i.e. "201949")
        /// </summary>
        public string retailer_id { get; set; } = "";//user_id
        /// <summary>
        /// MNP port-in date.
        /// </summary>
        public string port_in_date { get; set; } = "";
        /// <summary>
        /// Customer alternative mobile number.
        /// </summary>
        public string alt_msisdn { get; set; } = "";
        //public string poc_number { get; set; }
        /// <summary>
        /// B2B: POC mobile number.
        /// </summary>
        public string poc_msisdn_number { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public int? is_urgent { get; set; }//0/1
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string optional1 { get; set; } = "";
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string optional2 { get; set; } = "";
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string optional3 { get; set; } = "";
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string optional4 { get; set; } = "";
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string optional5 { get; set; } = "";
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string optional6 { get; set; } = "";
        /// <summary>
        /// Additional field for inserting any note for new record. 
        /// </summary>
        public string note { get; set; } = "";
        /// <summary>
        /// Reason Id for replacing SIM cards. Selected from reseller app UI dropdown.
        /// </summary>
        public string sim_rep_reason_id { get; set; } = "";
        /// <summary>
        /// Prepaid or postpaid.
        /// </summary>
        public string payment_type { get; set; } = "";
        /// <summary>
        /// Is the order is for paired MSISDN or not.
        /// </summary>
        public int? is_paired { get; set; }
        /// <summary>
        /// Reseller's channel ID.
        /// </summary>
        public int? channel_id { get; set; }
        /// <summary>
        /// Customer's division name.
        /// </summary>
        public string division_name { get; set; } = "";
        /// <summary>
        /// Customer's district name.
        /// </summary>
        public string district_name { get; set; } = "";
        /// <summary>
        /// Customer's thana name.
        /// </summary>
        public string thana_name { get; set; } = "";
        /// <summary>
        /// Reseller center code.
        /// </summary>
        public string center_code { get; set; } = "";
        /// <summary>
        /// Reseller distributor code.
        /// </summary>
        public string distributor_code { get; set; } = "";
        /// <summary>
        /// SIM replacement reason.
        /// </summary>
        public string sim_replc_reason { get; set; } = "";
        /// <summary>
        /// Reseller channel name. 
        /// </summary>
        //[Required]
        public string channel_name { get; set; } = "";
        //public int? right_id { get; set; }
        /// <summary>
        /// SIM replacement type.
        /// ByPOC = 1, ByAuthPerson = 2, BulkSIMReplacment = 3
        /// </summary>
        public int? sim_replacement_type { get; set; }
        /// <summary>
        /// Customer's old SIM number. This property used for SIM replacement.
        /// </summary>
        public string old_sim_number { get; set; } = "";
        /// <summary>
        /// Customer's (child) SIM category. (i.e.  Prepaid = 1, Postpaid = 2)
        /// </summary>
        public int? src_sim_category { get; set; }
        /// <summary>
        /// MNP portin DBSS order conformation code. 
        /// </summary>
        public string port_in_confirmation_code { get; set; } = "";
        /// <summary>
        /// Is EC verification required or not of Customer(parent) biometric info. 
        /// </summary>
        public int? dest_ec_verifi_reqrd { get; set; }
        /// <summary>
        /// Is EC verification required or not of Customer(child) biometric info. 
        /// </summary>
        public int? src_ec_verifi_reqrd { get; set; }
        /// <summary>
        /// Customer(parent) is foreign or not.
        /// </summary>
        public int? dest_foreign_flag { get; set; }
        /// <summary>
        /// DBSS subscription ID.
        /// </summary>
        public int? dbss_subscription_id { get; set; }
        /// <summary>
        /// Contains true or false. If Customer info already exists in DBSS DB 
        /// then it contains true, otherwise false.
        /// Used in B2C Sim replacement.
        /// </summary>
        public bool? saf_status { get; set; }
        /// <summary>
        /// Customer id get through DBSS API. 
        /// </summary>
        public string customer_id { get; set; } = "";
        /// <summary>
        /// DBSS order comformation code.
        /// </summary>
        public string order_confirmation_code { get; set; } = "";
        /// <summary>
        /// If the OTP is used for activation process then reseller app should pass the OTP using this filed.
        /// </summary>
        public string otp { get; set; } = "";
        /// <summary>
        /// Srource ownercustomer ID
        /// </summary>
        public string src_owner_customer_id { get; set; } = "";
        /// <summary>
        /// Srource user customer ID
        /// </summary>
        public string src_user_customer_id { get; set; } = "";
        /// <summary>
        /// Srource payer customer ID
        /// </summary>
        public string src_payer_customer_id { get; set; } = "";
        /// <summary>
        /// Dest IMSI number
        /// </summary>
        public string dest_imsi { get; set; } = "";
        /// <summary>
        /// Msisdn Reservation Id 
        /// </summary>
        public string msisdnReservationId { get; set; } = "";
        public decimal? order_booking_flag { get; set; }



        ////============Model Validation============
        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    switch (Convert.ToInt16(purpose_number))
        //    {
        //        case 2:
        //            if (String.IsNullOrEmpty(dest_dob))
        //                yield return new ValidationResult("'dest_dob' is required.");
        //            else if (!dest_doc_type_no.HasValue && dest_doc_type_no <= 0)
        //                yield return new ValidationResult("'dest_doc_type_no' is required. Value must be greater then 0.");
        //            else if (String.IsNullOrEmpty(customer_name))
        //                yield return new ValidationResult("'customer_name' is required.");
        //            else if (String.IsNullOrEmpty(division_name))
        //                yield return new ValidationResult("'division_name' is required.");
        //            else if (String.IsNullOrEmpty(district_name))
        //                yield return new ValidationResult("'district_name' is required.");
        //            else if (String.IsNullOrEmpty(thana_name))
        //                yield return new ValidationResult("'thana_name' is required.");
        //            else if (String.IsNullOrEmpty(village))
        //                yield return new ValidationResult("'village' is required.");
        //            else if (String.IsNullOrEmpty(gender))
        //                yield return new ValidationResult("'gender' is required.");
        //            else if (!dest_left_thumb_score.HasValue && dest_left_thumb_score <= FixedValueCollection.MaxFingerPrintScore)
        //                yield return new ValidationResult(String.Format($"'dest_left_thumb_score' is required. Value must be greater then {0}", FixedValueCollection.MaxFingerPrintScore));
        //            else if (String.IsNullOrEmpty(dest_left_thumb))
        //                yield return new ValidationResult("'dest_left_thumb' is required.");
        //            else if (!dest_left_index_score.HasValue && dest_left_index_score <= 0)
        //                yield return new ValidationResult(String.Format($"'dest_left_index_score' is required. Value must be greater then {0}", FixedValueCollection.MaxFingerPrintScore));
        //            else if (String.IsNullOrEmpty(dest_left_index))
        //                yield return new ValidationResult("'dest_left_index' is required.");
        //            else if (!dest_right_thumb_score.HasValue && dest_right_thumb_score <= 0)
        //                yield return new ValidationResult(String.Format($"'dest_right_thumb_score' is required. Value must be greater then {0}", FixedValueCollection.MaxFingerPrintScore));
        //            else if (String.IsNullOrEmpty(dest_right_thumb))
        //                yield return new ValidationResult("'dest_right_thumb' is required.");
        //            else if (!dest_right_index_score.HasValue && dest_right_index_score <= 0)
        //                yield return new ValidationResult(String.Format($"'dest_right_index_score' is required. Value must be greater then {0}", FixedValueCollection.MaxFingerPrintScore));
        //            else if (String.IsNullOrEmpty(dest_right_index))
        //                yield return new ValidationResult("'dest_right_index' is required.");
        //            break;
        //        case (int)EnumPurposeNumber.DeRegistration:
        //            break;
        //        case (int)EnumPurposeNumber.SIMReplacement:
        //            break;
        //        case (int)EnumPurposeNumber.MNPRegistration:
        //            break;
        //        case (int)EnumPurposeNumber.MNPEmergencyReturn:
        //            break;
        //        case (int)EnumPurposeNumber.MNPDeRegistration:
        //            break;
        //        default:
        //            break;
        //    }
        //}
        ////=============x=================
    }
    public class RAOrderRequestV2 : RACommonRequest
    {
        //public int is_esim { get; set; } 
        public int isBPUser { get; set; } =0;
        public bool is_success { get; set; } = false;
        public string? err_code { get; set; }
        public string? err_msg { get; set; }
        public string? bss_reqId { get; set; } 
        public int? status { get; set; }
        public long? error_id { get; set; }
        /// <summary>
        /// BI token number. For new order submit it is 0, for re-submit order it will be long like "191230632808". 
        /// </summary>
        //[Required]
        public double? bi_token_number { get; set; }
        /// <summary>
        /// Currently purpose_number property contains value 
        /// while submitting order for diferrent purpose like new connection, sim replacement.
        //[Required]
        public string purpose_number { get; set; } = "";
        /// <summary>
        /// Mobile number 
        /// </summary>
        public string msisdn { get; set; }
        /// <summary>
        /// SIM category (i.e. Prepaid = 1, Postpaid = 2)
        /// </summary>
        public int? sim_category { get; set; }
        /// <summary>
        /// SIM Number.   
        /// </summary>
        public string sim_number { get; set; } = "";
        /// <summary>
        /// DBSS subscription type id.
        /// </summary>
        public string? subscription_type_id { get; set; }
        /// <summary>
        /// DBSS subscription code.
        /// </summary>
        public string? subscription_code { get; set; }
        /// <summary>
        /// Subscription package id.
        /// </summary>
        public string? package_id { get; set; }
        /// <summary>
        /// Subscription package code.
        /// </summary>
        public string? package_code { get; set; }
        /// <summary>
        /// Customer's(Parent) national id.
        /// </summary>
        public string dest_nid { get; set; }
        /// <summary>
        /// Customer's(Child) national id.
        /// </summary>
        public string? src_nid { get; set; }
        /// <summary>
        /// Customer's(Parent) date of birth.
        /// </summary>
        public string dest_dob { get; set; }
        /// <summary>
        /// Customer's(Parent) date of birth.
        /// </summary>
        public int dest_doc_type_no { get; set; }
        /// <summary>
        /// Customer's(Child) date of birth.
        /// </summary>
        public string? src_dob { get; set; }
        /// <summary>
        /// Customer's(Child) natinal ID.
        /// </summary>
        public int? src_doc_type_no { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? platform_id { get; set; }
        /// <summary>
        /// Customer's full name.
        /// </summary>
        public string customer_name { get; set; } = "";
        /// <summary>
        /// Customer's gender.
        /// </summary>
        public string gender { get; set; } = "";
        /// <summary>
        /// Customer's resident flat number.
        /// </summary>
        public string? flat_number { get; set; } = "";
        /// <summary>
        /// Customer's house number.
        /// </summary>
        public string? house_number { get; set; } = "";
        /// <summary>
        /// Road number of customer's house. 
        /// </summary>
        public string? road_number { get; set; } = "";
        /// <summary>
        /// Customre's village name.
        /// </summary>
        public string village { get; set; } = "";
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
        public string? postal_code { get; set; } = "";
        /// <summary>
        /// Customer's email.
        /// </summary>
        public string? email { get; set; } = "";
        //public string retailer_code { get; set; }
        /// <summary>
        /// Customer's(parent) left thumb finger print score.
        /// </summary>
        public int? dest_left_thumb_score { get; set; }
        /// <summary>
        /// Customer's(parent) left thumb finger print.
        /// </summary>
        public string? dest_left_thumb { get; set; }
        /// <summary>
        /// Customer's(parent) left index finger print score.
        /// </summary>
        public int? dest_left_index_score { get; set; }
        /// <summary>
        /// Customer's(parent) left index finger print.
        /// </summary>
        public string? dest_left_index { get; set; }
        /// <summary>
        /// Customer's(parent) right thumb finger print score.
        /// </summary>
        public int? dest_right_thumb_score { get; set; }
        /// <summary>
        /// Customer's(parent) right thumb finger print.
        /// </summary>
        public string? dest_right_thumb { get; set; }
        /// <summary>
        /// Customer's(parent) right index finger print score.
        /// </summary>
        public int? dest_right_index_score { get; set; }
        /// <summary>
        /// Customer's(parent) right index finger print.
        /// </summary>
        public string? dest_right_index { get; set; }
        /// <summary>
        /// Customer's(chile) left thumb finger print score.
        /// </summary>
        public int? src_left_thumb_score { get; set; }
        /// <summary>
        /// Customer's(chile) left thumb finger print.
        /// </summary>
        public string? src_left_thumb { get; set; }
        /// <summary>
        /// Customer's(chile) left index finger print score.
        /// </summary>
        public int? src_left_index_score { get; set; }
        /// <summary>
        /// Customer's(chile) left index finger print.
        /// </summary>
        public string? src_left_index { get; set; }
        /// <summary>
        /// Customer's(chile) right thumb finger print score.
        /// </summary>
        public int? src_right_thumb_score { get; set; }
        /// <summary>
        /// Customer's(chile) right thumb finger print.
        /// </summary>
        public string? src_right_thumb { get; set; }
        /// <summary>
        /// Customer's(chile) right index finger print score.
        /// </summary>
        public int? src_right_index_score { get; set; }
        /// <summary>
        /// Customer's(chile) right index finger print.
        /// </summary>
        public string? src_right_index { get; set; }
        //[Required]
        /// <summary>
        /// Reseller user name(id) (i.e. "201949")
        /// </summary>
        public string retailer_id { get; set; } = "";//user_id
        /// <summary>
        /// MNP port-in date.
        /// </summary>
        public string? port_in_date { get; set; }
        /// <summary>
        /// Customer alternative mobile number.
        /// </summary>
        public string? alt_msisdn { get; set; } = "";
        //public string poc_number { get; set; }
        /// <summary>
        /// B2B: POC mobile number.
        /// </summary>
        public string? poc_msisdn_number { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? is_urgent { get; set; }//0/1
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional1 { get; set; }
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional2 { get; set; }
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional3 { get; set; }
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional4 { get; set; }
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional5 { get; set; }
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional6 { get; set; }
        /// <summary>
        /// Additional field for inserting any note for new record. 
        /// </summary>
        public string? note { get; set; }
        /// <summary>
        /// Reason Id for replacing SIM cards. Selected from reseller app UI dropdown.
        /// </summary>
        public string? sim_rep_reason_id { get; set; }
        /// <summary>
        /// Prepaid or postpaid.
        /// </summary>
        public string? payment_type { get; set; }
        /// <summary>
        /// Is the order is for paired MSISDN or not.
        /// </summary>
        public int? is_paired { get; set; }
        /// <summary>
        /// Reseller's channel ID.
        /// </summary>
        public int? channel_id { get; set; }
        /// <summary>
        /// Customer's division name.
        /// </summary>
        public string? division_name { get; set; }
        /// <summary>
        /// Customer's district name.
        /// </summary>
        public string? district_name { get; set; }
        /// <summary>
        /// Customer's thana name.
        /// </summary>
        public string? thana_name { get; set; }
        /// <summary>
        /// Reseller center code.
        /// </summary>
        public string? center_code { get; set; }
        /// <summary>
        /// Reseller distributor code.
        /// </summary>
        public string? distributor_code { get; set; }
        /// <summary>
        /// SIM replacement reason.
        /// </summary>
        public string? sim_replc_reason { get; set; }
        /// <summary>
        /// Reseller channel name. 
        /// </summary>
        //[Required]
        public string channel_name { get; set; } = "";
        //public int? right_id { get; set; }
        /// <summary>
        /// SIM replacement type.
        /// ByPOC = 1, ByAuthPerson = 2, BulkSIMReplacment = 3
        /// </summary>
        public int? sim_replacement_type { get; set; }
        /// <summary>
        /// Customer's old SIM number. This property used for SIM replacement.
        /// </summary>
        public string? old_sim_number { get; set; }
        /// <summary>
        /// Customer's (child) SIM category. (i.e.  Prepaid = 1, Postpaid = 2)
        /// </summary>
        public int? src_sim_category { get; set; }
        /// <summary>
        /// MNP portin DBSS order conformation code. 
        /// </summary>
        public string? port_in_confirmation_code { get; set; }
        /// <summary>
        /// Is EC verification required or not of Customer(parent) biometric info. 
        /// </summary>
        public int? dest_ec_verifi_reqrd { get; set; }
        /// <summary>
        /// Is EC verification required or not of Customer(child) biometric info. 
        /// </summary>
        public int? src_ec_verifi_reqrd { get; set; }
        /// <summary>
        /// Customer(parent) is foreign or not.
        /// </summary>
        public int? dest_foreign_flag { get; set; }
        /// <summary>
        /// DBSS subscription ID.
        /// </summary>
        public int? dbss_subscription_id { get; set; }
        /// <summary>
        /// Contains true or false. If Customer info already exists in DBSS DB 
        /// then it contains true, otherwise false.
        /// Used in B2C Sim replacement.
        /// </summary>
        public bool? saf_status { get; set; }
        /// <summary>
        /// Customer id get through DBSS API. 
        /// </summary>
        public string? customer_id { get; set; }
        /// <summary>
        /// DBSS order comformation code.
        /// </summary>
        public string? order_confirmation_code { get; set; }
        /// <summary>
        /// If the OTP is used for activation process then reseller app should pass the OTP using this filed.
        /// </summary>
        public string? otp { get; set; }
        /// <summary>
        /// Srource ownercustomer ID
        /// </summary>
        public string? src_owner_customer_id { get; set; }
        /// <summary>
        /// Srource user customer ID
        /// </summary>
        public string? src_user_customer_id { get; set; }
        /// <summary>
        /// Srource payer customer ID
        /// </summary>
        public string? src_payer_customer_id { get; set; }
        /// <summary>
        /// Dest IMSI number
        /// </summary>
        public string? dest_imsi { get; set; }
        /// <summary>
        /// Msisdn Reservation Id 
        /// </summary>
        public string? msisdnReservationId { get; set; }
        /// <summary>
        /// lac will send from App to get BTS code
        /// </summary>
        public int? lac { get; set; }

        /// <summary>
        /// cid will send from App to get BTS code
        /// </summary>
        public int? cid { get; set; }

        /// <summary>
        /// latitude
        /// </summary>
        public decimal latitude { get; set; } = 0;

        /// <summary> 
        /// longitude
        /// </summary>
        public decimal longitude { get; set; } = 0;

        /// <summary> 
        /// longitude 
        /// </summary>
        public string? scanner_id { get; set; }
        public int? is_esim { get; set; }
        public decimal order_booking_flag { get; set; } = 0;
        public int? is_starTrek { get; set; } = 0;
        public string? order_id { get; set; } = "";

        public int? is_online_sale { get; set; } = 0;
    }

    public class CherishRequest : RAOrderRequestV2
    {
        public string selected_category { get; set; }
    }

    public class ECVerificationFPBaseReqModel : RACommonRequest
    {
        //public int is_esim { get; set; } 
        public int isBPUser { get; set; } = 0; 
        public bool is_success { get; set; } = false;
        public string? err_code { get; set; }
        public string? err_msg { get; set; }
        public string? bss_reqId { get; set; }
        public int? status { get; set; }
        public long? error_id { get; set; }
        /// <summary>
        /// BI token number. For new order submit it is 0, for re-submit order it will be long like "191230632808". 
        /// </summary>
        //[Required]
        public double? bi_token_number { get; set; }
        /// <summary>
        /// Currently purpose_number property contains value 
        /// while submitting order for diferrent purpose like new connection, sim replacement.
        //[Required]
        public string? purpose_number { get; set; } = "";
        /// <summary>
        /// Mobile number 
        /// </summary>
        public string msisdn { get; set; }
        /// <summary>
        /// SIM category (i.e. Prepaid = 1, Postpaid = 2)
        /// </summary>
        public int? sim_category { get; set; }
        /// <summary>
        /// SIM Number.   
        /// </summary>
        public string? sim_number { get; set; } = "";
        /// <summary>
        /// DBSS subscription type id.
        /// </summary>
        public string? subscription_type_id { get; set; }
        /// <summary>
        /// DBSS subscription code.
        /// </summary>
        public string? subscription_code { get; set; }
        /// <summary>
        /// Subscription package id.
        /// </summary>
        public string? package_id { get; set; }
        /// <summary>
        /// Subscription package code.
        /// </summary>
        public string? package_code { get; set; }
        /// <summary>
        /// Customer's(Parent) national id.
        /// </summary>
        public string? dest_nid { get; set; }
        /// <summary>
        /// Customer's(Child) national id.
        /// </summary>
        public string? src_nid { get; set; }
        /// <summary>
        /// Customer's(Parent) date of birth.
        /// </summary>
        public string? dest_dob { get; set; }
        /// <summary>
        /// Customer's(Parent) date of birth.
        /// </summary>
        public int? dest_doc_type_no { get; set; }
        /// <summary>
        /// Customer's(Child) date of birth.
        /// </summary>
        public string? src_dob { get; set; }
        /// <summary>
        /// Customer's(Child) natinal ID.
        /// </summary>
        public int? src_doc_type_no { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? platform_id { get; set; }
        /// <summary>
        /// Customer's full name.
        /// </summary>
        public string? customer_name { get; set; } = "";
        /// <summary>
        /// Customer's gender.
        /// </summary>
        public string? gender { get; set; } = "";
        /// <summary>
        /// Customer's resident flat number.
        /// </summary>
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
        public string? village { get; set; } = "";
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
        //public string retailer_code { get; set; }
        /// <summary>
        /// Customer's(parent) left thumb finger print score.
        /// </summary>
        public int? dest_left_thumb_score { get; set; }
        /// <summary>
        /// Customer's(parent) left thumb finger print.
        /// </summary>
        public string? dest_left_thumb { get; set; }
        /// <summary>
        /// Customer's(parent) left index finger print score.
        /// </summary>
        public int? dest_left_index_score { get; set; }
        /// <summary>
        /// Customer's(parent) left index finger print.
        /// </summary>
        public string? dest_left_index { get; set; }
        /// <summary>
        /// Customer's(parent) right thumb finger print score.
        /// </summary>
        public int? dest_right_thumb_score { get; set; }
        /// <summary>
        /// Customer's(parent) right thumb finger print.
        /// </summary>
        public string? dest_right_thumb { get; set; }
        /// <summary>
        /// Customer's(parent) right index finger print score.
        /// </summary>
        public int? dest_right_index_score { get; set; }
        /// <summary>
        /// Customer's(parent) right index finger print.
        /// </summary>
        public string? dest_right_index { get; set; }
        /// <summary>
        /// Customer's(chile) left thumb finger print score.
        /// </summary>
        public int? src_left_thumb_score { get; set; }
        /// <summary>
        /// Customer's(chile) left thumb finger print.
        /// </summary>
        public string? src_left_thumb { get; set; }
        /// <summary>
        /// Customer's(chile) left index finger print score.
        /// </summary>
        public int? src_left_index_score { get; set; }
        /// <summary>
        /// Customer's(chile) left index finger print.
        /// </summary>
        public string? src_left_index { get; set; }
        /// <summary>
        /// Customer's(chile) right thumb finger print score.
        /// </summary>
        public int? src_right_thumb_score { get; set; }
        /// <summary>
        /// Customer's(chile) right thumb finger print.
        /// </summary>
        public string? src_right_thumb { get; set; }
        /// <summary>
        /// Customer's(chile) right index finger print score.
        /// </summary>
        public int? src_right_index_score { get; set; }
        /// <summary>
        /// Customer's(chile) right index finger print.
        /// </summary>
        public string? src_right_index { get; set; }
        //[Required]
        /// <summary>
        /// Reseller user name(id) (i.e. "201949")
        /// </summary>
        public string retailer_id { get; set; } = "";//user_id
        /// <summary>
        /// MNP port-in date.
        /// </summary>
        public string? port_in_date { get; set; }
        /// <summary>
        /// Customer alternative mobile number.
        /// </summary>
        public string? alt_msisdn { get; set; }
        //public string poc_number { get; set; }
        /// <summary>
        /// B2B: POC mobile number.
        /// </summary>
        public string? poc_msisdn_number { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? is_urgent { get; set; }//0/1
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional1 { get; set; }
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional2 { get; set; }
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional3 { get; set; }
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional4 { get; set; }
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional5 { get; set; }
        /// <summary>
        /// Optional additional field.
        /// </summary>
        public string? optional6 { get; set; }
        /// <summary>
        /// Additional field for inserting any note for new record. 
        /// </summary>
        public string? note { get; set; }
        /// <summary>
        /// Reason Id for replacing SIM cards. Selected from reseller app UI dropdown.
        /// </summary>
        public string? sim_rep_reason_id { get; set; }
        /// <summary>
        /// Prepaid or postpaid.
        /// </summary>
        public string? payment_type { get; set; }
        /// <summary>
        /// Is the order is for paired MSISDN or not.
        /// </summary>
        public int? is_paired { get; set; }
        /// <summary>
        /// Reseller's channel ID.
        /// </summary>
        public int? channel_id { get; set; }
        /// <summary>
        /// Customer's division name.
        /// </summary>
        public string? division_name { get; set; }
        /// <summary>
        /// Customer's district name.
        /// </summary>
        public string? district_name { get; set; }
        /// <summary>
        /// Customer's thana name.
        /// </summary>
        public string? thana_name { get; set; }
        /// <summary>
        /// Reseller center code.
        /// </summary>
        public string? center_code { get; set; }
        /// <summary>
        /// Reseller distributor code.
        /// </summary>
        public string? distributor_code { get; set; }
        /// <summary>
        /// SIM replacement reason.
        /// </summary>
        public string? sim_replc_reason { get; set; }
        /// <summary>
        /// Reseller channel name. 
        /// </summary>
        //[Required]
        public string? channel_name { get; set; } = "";
        //public int? right_id { get; set; }
        /// <summary>
        /// SIM replacement type.
        /// ByPOC = 1, ByAuthPerson = 2, BulkSIMReplacment = 3
        /// </summary>
        public int? sim_replacement_type { get; set; }
        /// <summary>
        /// Customer's old SIM number. This property used for SIM replacement.
        /// </summary>
        public string? old_sim_number { get; set; }
        /// <summary>
        /// Customer's (child) SIM category. (i.e.  Prepaid = 1, Postpaid = 2)
        /// </summary>
        public int? src_sim_category { get; set; }
        /// <summary>
        /// MNP portin DBSS order conformation code. 
        /// </summary>
        public string? port_in_confirmation_code { get; set; }
        /// <summary>
        /// Is EC verification required or not of Customer(parent) biometric info. 
        /// </summary>
        public int? dest_ec_verifi_reqrd { get; set; }
        /// <summary>
        /// Is EC verification required or not of Customer(child) biometric info. 
        /// </summary>
        public int? src_ec_verifi_reqrd { get; set; }
        /// <summary>
        /// Customer(parent) is foreign or not.
        /// </summary>
        public int? dest_foreign_flag { get; set; }
        /// <summary>
        /// DBSS subscription ID.
        /// </summary>
        public int? dbss_subscription_id { get; set; }
        /// <summary>
        /// Contains true or false. If Customer info already exists in DBSS DB 
        /// then it contains true, otherwise false.
        /// Used in B2C Sim replacement.
        /// </summary>
        public bool? saf_status { get; set; }
        /// <summary>
        /// Customer id get through DBSS API. 
        /// </summary>
        public string? customer_id { get; set; }
        /// <summary>
        /// DBSS order comformation code.
        /// </summary>
        public string? order_confirmation_code { get; set; }
        /// <summary>
        /// If the OTP is used for activation process then reseller app should pass the OTP using this filed.
        /// </summary>
        public string? otp { get; set; }
        /// <summary>
        /// Srource ownercustomer ID
        /// </summary>
        public string? src_owner_customer_id { get; set; }
        /// <summary>
        /// Srource user customer ID
        /// </summary>
        public string? src_user_customer_id { get; set; }
        /// <summary>
        /// Srource payer customer ID
        /// </summary>
        public string? src_payer_customer_id { get; set; }
        /// <summary>
        /// Dest IMSI number
        /// </summary>
        public string? dest_imsi { get; set; }
        /// <summary>
        /// Msisdn Reservation Id 
        /// </summary>
        public string? msisdnReservationId { get; set; }
        /// <summary>
        /// lac will send from App to get BTS code
        /// </summary>
        public int? lac { get; set; }

        /// <summary>
        /// cid will send from App to get BTS code
        /// </summary>
        public int? cid { get; set; }

        /// <summary>
        /// latitude
        /// </summary>
        public decimal latitude { get; set; } = 0;

        /// <summary> 
        /// longitude
        /// </summary>
        public decimal longitude { get; set; } = 0;

        /// <summary> 
        /// longitude 
        /// </summary>
        public string? scanner_id { get; set; }
        public int? is_esim { get; set; }
        public decimal order_booking_flag { get; set; } = 0;
        public int? is_starTrek { get; set; } = 0;
        public string? order_id { get; set; } = "";

        public int? is_online_sale { get; set; } = 0;
    }
}
  