using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    /// <summary>
    ///  This class is used for checking MSISDN number 
    /// </summary>
    public class PaiedMSISDNCheckResponse : RACommonResponse
    {
        /// <summary>
        /// SIM card number (i.e. "981809647747") 
        /// </summary>
        public string sim_number { get; set; }
        /// <summary>
        /// Subscreiption type code (i.e. "")
        /// </summary>
        public string subscription_type_code { get; set; }
        /// <summary>
        /// imsi number (i.e. "470037108801557") 
        /// </summary>
        public string imsi { get; set; }
    }

    public class PaiedMSISDNCheckResponseDataRev
    {
        public PaiedMSISDNCheckResponseRev data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }
    }

    public class PaiedMSISDNCheckResponseDataRevV1
    {
        public PaiedMSISDNCheckResponseRevV1 data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }
    }

    public class PaiedMSISDNCheckResponseRev
    {
        /// <summary>
        /// SIM card number (i.e. "981809647747") 
        /// </summary>
        public string sim_number { get; set; }
        /// <summary>
        /// Subscreiption type code (i.e. "")
        /// </summary>
        public string subscription_type_code { get; set; }
        /// <summary>
        /// imsi number (i.e. "470037108801557") 
        /// </summary>
        public string imsi { get; set; }
    }

    public class PaiedMSISDNCheckResponseRevV1
    {
        /// <summary>
        /// SIM card number (i.e. "981809647747") 
        /// </summary>
        public string sim_number { get; set; }
        /// <summary>
        /// Subscreiption type code (i.e. "")
        /// </summary>
        public string subscription_type_code { get; set; }
        /// <summary>
        /// imsi number (i.e. "470037108801557") 
        /// </summary>
        public string imsi { get; set; }
        public string number_category { get; set; }
        public string message { get; set; }
        public bool isDesiredCategory { get; set; }
    }


    #region Cherish Msisdn

    public class CherishMSISDNCheckResponse : RACommonResponse
    {
        public string retailer_code { get; set; }

        public string number_category { get; set; }
    }

    #endregion



    public class UnpairedMSISDNCheckResponse : RACommonResponse
    {
        public int stock_id { get; set; }
        public string retailer_code { get; set; }

        public string number_category { get; set; }
    }
    public class CherishedMSISDNCheckResponse : UnpairedMSISDNCheckResponse
    {
        public bool isDesiredCategory { get; set; } = false;
        public string category_name { get; set; }
        public string data_message { get; set; }

    }
    public class UnpairedMSISDNStartrekCheckResponse : RACommonResponse
    {
        public int stock_id { get; set; }
        public string retailer_code { get; set; }

        public string number_category { get; set; }
        public string reservation_id { get; set; }
    }

    public class UnpairedMSISDNStartrekCheckResponseV2 : UnpairedMSISDNCheckResponse
    {
        public string retailer_code { get; set; }
        public string reservation_id { get; set; }
        public bool isDesiredCategory { get; set; } = false;
        public string category_name { get; set; }
        public string data_message { get; set; }
    }

    public class UnpairedMSISDNCheckResponseForMNPPortIn : RACommonResponse
    {
        public bool is_controlled { get; set; }//
    }


    public class OldSIMNnumberResponse : RACommonResponse
    {
        public string old_sim_number { get; set; }
    }

    public class DBSSNotificationResponse : RACommonResponse
    {
        public string msisdn_reservation_id { get; set; }
        public bool is_unreservation_needed { get; set; }

        public string bi_token_number { get; set; }
        public string msisdn { get; set; }

        // Property for order request Newly added         
        public string bss_request_id { get; set; } //DBSS Bio Request Id.
        public int purpose_number { get; set; }
        public int sim_category { get; set; }
        public string sim_number { get; set; }
        public string subscription_code { get; set; }// subscription  type code
        public string package_code { get; set; }
        public string dest_doc_type_no { get; set; }
        public string dest_doc_id { get; set; }
        public string dest_dob { get; set; }
        public string customer_name { get; set; }
        public string gender { get; set; }
        public string flat_number { get; set; }
        public string house_number { get; set; }
        public string road_number { get; set; }
        public string village { get; set; }
        public string division_Name { get; set; }
        public string district_Name { get; set; }
        public string thana_Name { get; set; }
        public string postal_code { get; set; }
        public string user_id { get; set; }
        public string port_in_date { get; set; }
        public string alt_msisdn { get; set; }
        public int status { get; set; }
        public long error_id { get; set; }
        public string error_description { get; set; }
        public string create_date { get; set; }
        public string dest_id_type_exp_time { get; set; }
        public string confirmation_code { get; set; }//DBSS Order Confarmation Code.
        public string email { get; set; }
        public string salesman_code { get; set; }
        public string channel_name { get; set; }
        public string center_or_distributor_code { get; set; }
        public string sim_replace_reason { get; set; }
        public int? is_paired { get; set; }
        public int dbss_subscription_id { get; set; }
        public string old_sim_number { get; set; }
        public int sim_replacement_type { get; set; }
        public int src_sim_category { get; set; }
        public string port_in_confirmation_code { get; set; }
        public string payment_type { get; set; }
        public string poc_number { get; set; }


    }

    public class OrderConformationResponse : RACommonResponse
    {
        public string order_conformation_code { get; set; }
    }


    /// <summary>
    /// The implementation of absract class named Data S
    /// </summary>
    public class MSISDNCheckResponseData : Data
    {
    }

    public class OrderInfoResponse : RACommonResponse
    {
        public string alt_msisdn { get; set; }
        public string sim_number { get; set; }
        public string village { get; set; }
        public string gender { get; set; }
        public int thana_id { get; set; }
        public string thana_name { get; set; }
        public string road_number { get; set; }
        public string flat_number { get; set; }
        public string district_name { get; set; }
        public int district_id { get; set; }
        public string customer_name { get; set; }
        public string division_name { get; set; }
        public int division_id { get; set; }
        public string house_number { get; set; }
        public string email { get; set; }
        public string postal_code { get; set; }
        public string subscription_code { get; set; }
        public string subscription_type_id { get; set; }
        public string package_code { get; set; }
        public int package_id { get; set; }
        public int is_urgent { get; set; }
        public string port_in_date { get; set; }
    }

    public class OrderInfoResponseDataRev
    {
        public OrderInfoResponseRev data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }
    }

    public class OrderInfoResponseRev
    {
        public string alt_msisdn { get; set; }
        public string sim_number { get; set; }
        public string village { get; set; }
        public string gender { get; set; }
        public int thana_id { get; set; }
        public string thana_name { get; set; }
        public string road_number { get; set; }
        public string flat_number { get; set; }
        public string district_name { get; set; }
        public int district_id { get; set; }
        public string customer_name { get; set; }
        public string division_name { get; set; }
        public int division_id { get; set; }
        public string house_number { get; set; }
        public string email { get; set; }
        public string postal_code { get; set; }
        public string subscription_code { get; set; }
        public string subscription_type_id { get; set; }
        public string package_code { get; set; }
        public int package_id { get; set; }
        public int is_urgent { get; set; }
        public string port_in_date { get; set; }
    }

    public class RAPassLenResponse : RACommonResponse
    {
        public int length { get; set; }
    }

    public class RAUserInfoForForgetPWDResponse : RAPassLenResponse
    {
        public string mobile_number { get; set; }
    }


    public class RAOTPResponse : RACommonResponse
    {
        public string otp { get; set; }
    }

    public class RAPassLenResponseV2
    {
        public bool isError { get; set; }
        public string message { get; set; }
        public PasswordLenthData data { get; set; }
    }
    public class PasswordLenthData
    {
        public int length { get; set; }
    }
}
