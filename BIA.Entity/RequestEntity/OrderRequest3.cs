using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class OrderRequest4 
    {
        public int? is_esim { get; set; }
        public string? bss_reqId { get; set; }
        public int? status { get; set; }
        public long? error_id { get; set; }
        public string? error_description { get; set; }

        public double? bi_token_number { get; set; }
        public decimal? purpose_number { get; set; }
        public string? msisdn { get; set; }
        public decimal? sim_category { get; set; }
        public string? sim_number { get; set; }
        public decimal? subscription_type_id { get; set; }
        public string? subscription_code { get; set; }
        public decimal? package_id { get; set; }
        public string? package_code { get; set; }
        public decimal? dest_doc_type_no { get; set; }
        public string? dest_nid { get; set; }
        public string? src_nid { get; set; }
        public string? dest_dob { get; set; }
        public decimal? src_doc_type_no { get; set; }
        public string? src_dob { get; set; }
        public string? platform_id { get; set; }
        public string? customer_name { get; set; }
        public string? gender { get; set; }
        public string? flat_number { get; set; }
        public string? house_number { get; set; }
        public string? road_number { get; set; }
        public string? village { get; set; }
        public decimal? division_id { get; set; }
        public decimal? district_id { get; set; }
        public decimal? thana_id { get; set; }
        public string? postal_code { get; set; }
        public string? email { get; set; }
        public string? retailer_code { get; set; }
        public decimal? dest_left_thumb_score { get; set; }
        public byte[] dest_left_thumb { get; set; }
        public decimal? dest_left_index_score { get; set; }
        public byte[] dest_left_index { get; set; }
        public decimal? dest_right_thumb_score { get; set; }
        public byte[] dest_right_thumb { get; set; }
        public decimal? dest_right_index_score { get; set; }
        public byte[] dest_right_index { get; set; }
        public decimal? src_left_thumb_score { get; set; }
        public byte[] src_left_thumb { get; set; }
        public decimal? src_left_index_score { get; set; }
        public byte[] src_left_index { get; set; }
        public decimal? src_right_thumb_score { get; set; }
        public byte[] src_right_thumb { get; set; }
        public decimal? src_right_index_score { get; set; }
        public byte[] src_right_index { get; set; }
        public string retailer_id { get; set; }//user_id
        public DateTime? port_in_date { get; set; }
        public string? alt_msisdn { get; set; }
        public string? poc_number { get; set; }
        public decimal? is_urgent { get; set; }
        public string? optional1 { get; set; }
        public string? optional2 { get; set; }
        public string? optional3 { get; set; }
        public decimal? optional4 { get; set; }
        public decimal? optional5 { get; set; }
        public decimal? optional6 { get; set; }
        public string? note { get; set; }
        public decimal? sim_rep_reason_id { get; set; }
        public string payment_type { get; set; }
        public decimal? is_paired { get; set; }
        public decimal? cahnnel_id { get; set; }

        public string? division_name { get; set; }
        public string? district_name { get; set; }
        public string? thana_name { get; set; }
        public string? center_code { get; set; }
        public string? distributor_code { get; set; }
        public string? sim_replc_reason { get; set; }
        public string? channel_name { get; set; }
        public int? right_id { get; set; }
        public int? sim_replacement_type { get; set; }
        public string? old_sim_number { get; set; }

        public int? src_sim_category { get; set; }
        public string? port_in_confirmation_code { get; set; }
        public int? dest_ec_verifi_reqrd { get; set; }
        public int? src_ec_verifi_reqrd { get; set; }
        public int? dest_foreign_flag { get; set; }
        public int? dbss_subscription_id { get; set; }
        public int? saf_status { get; set; }
        public string? customer_id { get; set; }
        public string? order_confirmation_code { get; set; }
        public string? server_name { get; set; }
        public string? src_owner_customer_id { get; set; }
        public string? src_user_customer_id { get; set; }
        public string? src_payer_customer_id { get; set; }

        public string? dest_imsi { get; set; }
        public string? msisdnReservationId { get; set; }

        public long? loginAttemptId { get; set; }
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public string? btsCode { get; set; }
        public int? lac { get; set; }
        public int? cid { get; set; }
        public string? scanner_id { get; set; }
        public decimal? order_booking_flag { get; set; }
        public int? is_starTrek { get; set; }
        public string? order_id { get; set; } = "";
        public int? is_online_sale { get; set; } = 0;
        public string selected_category { get; set; }

    }
    public class OrderRequest3 //FP in byte[]
    {
        public int? is_esim { get; set; }
        public string? bss_reqId { get; set; }
        public int? status { get; set; }
        public long? error_id { get; set; }
        public string? error_description { get; set; }

        public double? bi_token_number { get; set; }
        public decimal? purpose_number { get; set; }
        public string? msisdn { get; set; }
        public decimal? sim_category { get; set; }
        public string? sim_number { get; set; }
        public decimal? subscription_type_id { get; set; }
        public string? subscription_code { get; set; }
        public decimal? package_id { get; set; }
        public string? package_code { get; set; }
        public decimal? dest_doc_type_no { get; set; }
        public string? dest_nid { get; set; }
        public string? src_nid { get; set; }
        public string? dest_dob { get; set; }
        public decimal? src_doc_type_no { get; set; }
        public string? src_dob { get; set; }
        public string? platform_id { get; set; }
        public string? customer_name { get; set; }
        public string? gender { get; set; }
        public string? flat_number { get; set; }
        public string? house_number { get; set; }
        public string? road_number { get; set; }
        public string? village { get; set; }
        public decimal? division_id { get; set; }
        public decimal? district_id { get; set; }
        public decimal? thana_id { get; set; }
        public string? postal_code { get; set; }
        public string? email { get; set; }
        public string? retailer_code { get; set; }
        public decimal? dest_left_thumb_score { get; set; }
        public byte[] dest_left_thumb { get; set; }
        public decimal? dest_left_index_score { get; set; }
        public byte[] dest_left_index { get; set; }
        public decimal? dest_right_thumb_score { get; set; }
        public byte[] dest_right_thumb { get; set; }
        public decimal? dest_right_index_score { get; set; }
        public byte[] dest_right_index { get; set; }
        public decimal? src_left_thumb_score { get; set; }
        public byte[] src_left_thumb { get; set; }
        public decimal? src_left_index_score { get; set; }
        public byte[] src_left_index { get; set; }
        public decimal? src_right_thumb_score { get; set; }
        public byte[] src_right_thumb { get; set; }
        public decimal? src_right_index_score { get; set; }
        public byte[] src_right_index { get; set; }
        public string retailer_id { get; set; }//user_id
        public DateTime? port_in_date { get; set; }
        public string? alt_msisdn { get; set; }
        public string? poc_number { get; set; }
        public decimal? is_urgent { get; set; }
        public string? optional1 { get; set; }
        public string? optional2 { get; set; }
        public string? optional3 { get; set; }
        public decimal? optional4 { get; set; }
        public decimal? optional5 { get; set; }
        public decimal? optional6 { get; set; }
        public string? note { get; set; }
        public decimal? sim_rep_reason_id { get; set; }
        public string payment_type { get; set; }
        public decimal? is_paired { get; set; }
        public decimal? cahnnel_id { get; set; }

        public string? division_name { get; set; }
        public string? district_name { get; set; }
        public string? thana_name { get; set; }
        public string? center_code { get; set; }
        public string? distributor_code { get; set; }
        public string? sim_replc_reason { get; set; }
        public string? channel_name { get; set; }
        public int? right_id { get; set; }
        public int? sim_replacement_type { get; set; }
        public string? old_sim_number { get; set; }

        public int? src_sim_category { get; set; }
        public string? port_in_confirmation_code { get; set; }
        public int? dest_ec_verifi_reqrd { get; set; }
        public int? src_ec_verifi_reqrd { get; set; }
        public int? dest_foreign_flag { get; set; }
        public int? dbss_subscription_id { get; set; }
        public int? saf_status { get; set; }
        public string? customer_id { get; set; }
        public string? order_confirmation_code { get; set; }
        public string? server_name { get; set; }
        public string? src_owner_customer_id { get; set; }
        public string? src_user_customer_id { get; set; }
        public string? src_payer_customer_id { get; set; }

        public string? dest_imsi { get; set; }
        public string? msisdnReservationId { get; set; }

        public long? loginAttemptId { get; set; }
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public string? btsCode { get; set; }
        public int? lac { get; set; }
        public int? cid { get; set; }
        public string? scanner_id { get; set; }
        public decimal? order_booking_flag { get; set; }
        public int? is_starTrek { get; set; }
        public string? order_id { get; set; } = "";
        public int? is_online_sale { get; set; } = 0;

    }

     
    public class OrderRequest2 //FP in byte[]
    {
        public string bss_reqId { get; set; }
        public int status { get; set; }
        public long error_id { get; set; }
        public string error_description { get; set; }

        public double? bi_token_number { get; set; }
        public decimal? purpose_number { get; set; }
        public string msisdn { get; set; }
        public decimal? sim_category { get; set; }
        public string sim_number { get; set; }
        public decimal? subscription_type_id { get; set; }
        public string subscription_code { get; set; }
        public decimal? package_id { get; set; }
        public string package_code { get; set; }
        public decimal? dest_doc_type_no { get; set; }
        public string dest_nid { get; set; }
        public string src_nid { get; set; }
        public string dest_dob { get; set; }
        public decimal? src_doc_type_no { get; set; }
        public string src_dob { get; set; }
        public string platform_id { get; set; }
        public string customer_name { get; set; }
        public string gender { get; set; }
        public string flat_number { get; set; }
        public string house_number { get; set; }
        public string road_number { get; set; }
        public string village { get; set; }
        public decimal? division_id { get; set; }
        public decimal? district_id { get; set; }
        public decimal? thana_id { get; set; }
        public string postal_code { get; set; }
        public string email { get; set; }
        public string retailer_code { get; set; }
        public decimal? dest_left_thumb_score { get; set; }
        public byte[] dest_left_thumb { get; set; }
        public decimal? dest_left_index_score { get; set; }
        public byte[] dest_left_index { get; set; }
        public decimal? dest_right_thumb_score { get; set; }
        public byte[] dest_right_thumb { get; set; }
        public decimal? dest_right_index_score { get; set; }
        public byte[] dest_right_index { get; set; }
        public decimal? src_left_thumb_score { get; set; }
        public byte[] src_left_thumb { get; set; }
        public decimal? src_left_index_score { get; set; }
        public byte[] src_left_index { get; set; }
        public decimal? src_right_thumb_score { get; set; }
        public byte[] src_right_thumb { get; set; }
        public decimal? src_right_index_score { get; set; }
        public byte[] src_right_index { get; set; }
        public string retailer_id { get; set; }//user_id
        public DateTime? port_in_date { get; set; }
        public string alt_msisdn { get; set; }
        public string poc_number { get; set; }
        public decimal? is_urgent { get; set; }
        public string optional1 { get; set; }
        public string optional2 { get; set; }
        public string optional3 { get; set; }
        public decimal? optional4 { get; set; }
        public decimal? optional5 { get; set; }
        public decimal? optional6 { get; set; }
        public string note { get; set; }
        public decimal? sim_rep_reason_id { get; set; }
        public string payment_type { get; set; }
        public decimal? is_paired { get; set; }
        public decimal? cahnnel_id { get; set; }

        public string division_name { get; set; }
        public string district_name { get; set; }
        public string thana_name { get; set; }
        public string center_code { get; set; }
        public string distributor_code { get; set; }
        public string sim_replc_reason { get; set; }
        public string channel_name { get; set; }
        public int? right_id { get; set; }
        public int? sim_replacement_type { get; set; }
        public string old_sim_number { get; set; }

        public int? src_sim_category { get; set; }
        public string port_in_confirmation_code { get; set; }
        public int? dest_ec_verifi_reqrd { get; set; }
        public int? src_ec_verifi_reqrd { get; set; }
        public int? dest_foreign_flag { get; set; }
        public int? dbss_subscription_id { get; set; }
        public int? saf_status { get; set; }
        public string customer_id { get; set; }
        public string order_confirmation_code { get; set; }
        public string server_name { get; set; }
        public string src_owner_customer_id { get; set; }
        public string src_user_customer_id { get; set; }
        public string src_payer_customer_id { get; set; }

        public string dest_imsi { get; set; }
        public string msisdnReservationId { get; set; }
        public decimal? order_booking_flag { get; set; }

    }




    public class OrderRequest
    {

        public decimal? bi_token_number { get; set; }
        public decimal? purpose_number { get; set; }
        public string msisdn { get; set; }
        public decimal? sim_category { get; set; }
        public string sim_number { get; set; }
        public decimal? subscription_type_id { get; set; }
        public string subscription_code { get; set; }
        public decimal? package_id { get; set; }
        public string package_code { get; set; }
        public decimal? dest_doc_type_no { get; set; }
        public string dest_nid { get; set; }
        public string src_nid { get; set; }
        public string dest_dob { get; set; }
        public decimal? src_doc_type_no { get; set; }
        public string src_dob { get; set; }
        public string platform_id { get; set; }
        public string customer_name { get; set; }
        public string gender { get; set; }
        public string flat_number { get; set; }
        public string house_number { get; set; }
        public string road_number { get; set; }
        public string village { get; set; }
        public decimal? division_id { get; set; }
        public decimal? district_id { get; set; }
        public decimal? thana_id { get; set; }
        public string postal_code { get; set; }
        public string email { get; set; }
        public string salesman_code { get; set; }
        public decimal? dest_left_thumb_score { get; set; }
        public string dest_left_thumb { get; set; }
        public decimal? dest_left_index_score { get; set; }
        public string dest_left_index { get; set; }
        public decimal? dest_right_thumb_score { get; set; }
        public string dest_right_thumb { get; set; }
        public decimal? dest_right_index_score { get; set; }
        public string dest_right_index { get; set; }
        public decimal? src_left_thumb_score { get; set; }
        public string src_left_thumb { get; set; }
        public decimal? src_left_index_score { get; set; }
        public string src_left_index { get; set; }
        public decimal? src_right_thumb_score { get; set; }
        public string src_right_thumb { get; set; }
        public decimal? src_right_index_score { get; set; }
        public string src_right_index { get; set; }
        public string retailer_id { get; set; }//user_id
        public string port_in_date { get; set; }
        public string alt_msisdn { get; set; }
        public string poc_number { get; set; }
        public decimal? is_urgent { get; set; }
        public string optional1 { get; set; }
        public string optional2 { get; set; }
        public string optional3 { get; set; }
        public decimal? optional4 { get; set; }
        public decimal? optional5 { get; set; }
        public decimal? optional6 { get; set; }
        public string note { get; set; }
        public decimal? sim_rep_reason_id { get; set; }
        public string payment_type { get; set; }
        public decimal? is_paired { get; set; }
        public decimal? cahnnel_id { get; set; }

        public string division_name { get; set; }
        public string district_name { get; set; }
        public string thana_name { get; set; }
        public string center_code { get; set; }
        public string distributor_code { get; set; }
        public string sim_replc_reason { get; set; }
        public string channel_name { get; set; }
        public int? right_id { get; set; }
    }
}
