using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    /// <summary>
    /// Common MSISDN request chaeck type. 
    /// </summary>
    public class MSISDNCheckRequest : RACommonRequest
    {
        /// <summary>
        /// MSISDN number which starts with 880. 
        /// </summary>
        [Required]
        public string mobile_number { get; set; }
        /// <summary>
        /// Language that defines in which language user wants to use device.
        /// </summary>
        public string lan { get; set; } = "";
        /// <summary>
        /// Define Purpose Number to understand validation type. Currently purpose_number property contains value 
        /// while submitting order for diferrent purpose like new connection, sim replacement.
        /// For validation api request purpose_number inserted 0 from code level while log insert.
        /// </summary>
        [Required]
        public string? purpose_number { get; set; }

        /// <summary>
        /// Reseller user name (id) (i.e. "201949")
        /// </summary>
        [Required]
        public string retailer_id { get; set; }

        /// <summary>
        /// Reseller channel name (i.e. "RESELLER", "Corporate")
        /// </summary>
        [Required]
        public string channel_name { get; set; }
        /// <summary>
        /// Reseller inventory id.
        /// </summary>
        //[Required]
        public int? inventory_id { get; set; }
        /// <summary>
        /// Reseller center code.
        /// </summary>
        public string? center_code { get; set; } = "";
        /// <summary>
        /// SIM category (i.e. Prepaid = 1, Postpaid = 2)
        /// </summary> 
        public int? sim_category { get; set; }
    }

    /// <summary>
    /// Paired MSISDN validation request type.
    /// </summary>
    public class PairedMSISDNCheckRequest : MSISDNCheckRequest
    {
        //public int? sim_category { get; set; }
    }



    /// <summary>
    /// Unpaired MSISDN validation request type.
    /// </summary>
    public class UnpairedMSISDNCheckRequest : MSISDNCheckRequest
    {
        /// <summary>
        /// SIM Number, which is unique.   
        /// </summary>
        [Required, StringLength(12, ErrorMessage = "SIM number must be 12 digit number.")]
        public string sim_number { get; set; }//in DBSS API it is mapped with serial_no. 

        /// <summary>
        /// Reseller channel id.
        /// </summary>
        //[Required]
        public int channel_id { get; set; } = 0;
    }

    public class CherishMSISDNCheckRequest : UnpairedMSISDNCheckRequest
    {
        [Required]
        public string selected_category { get; set; }
    }
    public class UnpairedMSISDNCheckRequestOnline : MSISDNCheckRequest
    {
        /// <summary> 
        /// SIM Number, which is unique.   
        /// </summary>
        [Required, StringLength(12, ErrorMessage = "SIM number must be 12 digit number.")]
        public string sim_number { get; set; }//in DBSS API it is mapped with serial_no. 

        /// <summary>
        /// Reseller channel id.
        /// </summary>
        //[Required]
        public int channel_id { get; set; } = 0;
        public string? order_id { get; set; } = "";
    }

    /// <summary>
    /// B2B SIM replacement mobbile number validation request type.
    /// </summary>
    public class CorporateMSISDNCheckRequest : MSISDNCheckRequest
    {
        /// <summary>
        /// Customer(Parent) mobile number.
        /// </summary>
        [Required]
        public string poc_msisdn_number { get; set; }
        /// <summary>
        /// New SIM number.
        /// </summary>
        [Required, StringLength(12, ErrorMessage = "SIM number must be 12 digit number.")]
        public string sim_number { get; set; }//in DBSS API it is mapped with serial_no.
    }

    /// <summary>
    /// B2C SIM replacement mobile number validation requestt type.
    /// </summary>
    public class IndividualSIMReplsMSISDNCheckRequest : MSISDNCheckRequest
    {
        /// <summary>
        /// SIM number.
        /// </summary>
        [Required, StringLength(12, ErrorMessage = "SIM number must be 12 digit number.")]
        public string sim_number { get; set; }//in DBSS API it is mapped with serial_no.
    }
    public class IndividualSIMReplsMSISDNCheckRequestOnline : MSISDNCheckRequest
    {
        /// <summary>
        /// SIM number.
        /// </summary>
        [Required, StringLength(12, ErrorMessage = "SIM number must be 12 digit number.")]
        public string sim_number { get; set; }//in DBSS API it is mapped with serial_no.
        public string order_id { get; set; }
    }

    /// <summary>
    /// B2B SIM replacement mobile number validation by auth persion reqest type.
    /// </summary>
    public class CorporateMSISDNCheckWithOTPRequest : CorporateMSISDNCheckRequest
    {
        /// <summary>
        /// DBSS one time password.   
        /// </summary>
        [Required]
        public string otp { get; set; }
    }


    public class BioCancelMSISDNValidationReq : MSISDNCheckRequest
    {
        public string nid { get; set; } = "";
        public string dob { get; set; } = "";
    }
}
