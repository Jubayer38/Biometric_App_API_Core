using BIA.BLL.Utility;
using BIA.DAL.Repositories;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.Utility;
using BIA.Entity.ViewModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static BIA.Entity.ResponseEntity.CherishedMSISDNCheckResponse;

namespace BIA.BLL.BLLServices
{
    public class BLLOrder
    {
        
        private readonly LogWriter _logWriter;
        private readonly DALBiometricRepo dataManager;
        private readonly BLLCommon _bllCommon;
        private readonly BLLLog _bLLLog;
        private readonly IConfiguration _configuration;
        public BLLOrder(LogWriter logWriter, DALBiometricRepo _dataManager, BLLCommon bllCommon, BLLLog bLLLog, IConfiguration configuration)
        {
            _logWriter = logWriter;
            dataManager = _dataManager;
            _bllCommon = bllCommon;
            _bLLLog = bLLLog;
            _configuration = configuration;
        }
        // return type SendOrderResponse of this methos
        public async Task<SendOrderResponse> SubmitOrder3(RAOrderRequest model)// Converted FP in byte[].
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            decimal tokenId = 0;
            try
            {
                if (model.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                }

                OrderRequest2 order = new OrderRequest2();
                BL_Json blJson = new BL_Json();

                order.bi_token_number = model.bi_token_number == null ? 0 : model.bi_token_number;
                order.purpose_number = String.IsNullOrEmpty(model.purpose_number) ? null : Convert.ToDecimal(model.purpose_number.Trim());
                order.msisdn = model.msisdn;


                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_category = model.src_sim_category;
                else
                    order.sim_category = model.sim_category;

                if (!String.IsNullOrEmpty(model.sim_number))
                {
                    if (model.sim_number.Substring(0, 6) != FixedValueCollection.SIMCode)
                    {
                        model.sim_number = FixedValueCollection.SIMCode + model.sim_number;
                    }
                }

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_number = model.old_sim_number.Trim();
                else if (!String.IsNullOrEmpty(model.sim_number))
                    order.sim_number = model.sim_number.Trim();
                else
                    order.sim_number = model.sim_number;

                order.subscription_type_id = String.IsNullOrEmpty(model.subscription_type_id) ? null : Convert.ToDecimal(model.subscription_type_id.Trim());
                order.subscription_code = model.subscription_code;
                order.package_id = String.IsNullOrEmpty(model.package_id) ? null : Convert.ToDecimal(model.package_id.Trim());
                order.package_code = model.package_code;
                if (!String.IsNullOrEmpty(model.dest_nid) && !String.IsNullOrEmpty(model.dest_dob))
                {
                    order.dest_nid = (model.dest_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.dest_dob).Year + model.dest_nid : model.dest_nid;
                }

                if (!String.IsNullOrEmpty(model.src_nid) && !String.IsNullOrEmpty(model.src_dob))
                {
                    order.src_nid = (model.src_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.src_dob).Year + model.src_nid : model.src_nid;
                }

                order.dest_dob = String.IsNullOrEmpty(model.dest_dob) ? String.Empty : DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.dest_nid))
                {
                    order.dest_doc_type_no = model.dest_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;
                }

                order.src_dob = String.IsNullOrEmpty(model.src_dob) ? String.Empty : DateTime.Parse(model.src_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.src_nid))
                {
                    order.src_doc_type_no = model.src_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;//(int)DOCTypeNo.nid;//1 for nid. /*model.src_doc_type_no;*/
                }
                else
                {
                    order.src_doc_type_no = null;
                }
                order.platform_id = model.platform_id;

                if (!String.IsNullOrEmpty(model.customer_name) &&
                        isCustomerNameValid(model.customer_name))
                {
                    order.customer_name = model.customer_name;
                }

                if (!String.IsNullOrEmpty(model.gender))
                {
                    order.gender = model.gender.ToLower() == "others" ? "notitle" : model.gender.ToLower();
                }

                order.flat_number = model.flat_number;
                order.house_number = model.house_number;
                order.road_number = model.road_number;
                order.village = model.village;
                order.division_id = model.division_id;
                order.district_id = model.district_id.Equals(null) ? null : model.district_id;
                order.thana_id = model.thana_id.Equals(null) ? null : model.thana_id;
                order.postal_code = String.IsNullOrEmpty(model.postal_code) ? "0" : model.postal_code;

                //=====temp modelfication (as RA apk is unable to update [15-12-19])============
                if (!String.IsNullOrEmpty(model.email))
                {
                    order.email = isEmailValid(model.email.ToLower()) == true ? model.email : String.Empty;
                }
                else
                {
                    order.email = String.Empty;
                }
                //===================

                order.retailer_id = model.retailer_id;
                //note: For retailer App dest_doc_id and src_doc_id is dest_nid and src_nid namely. 
                order.dest_left_thumb_score = model.dest_left_thumb_score.Equals(null) ? (decimal?)null : model.dest_left_thumb_score;
                order.dest_left_thumb = null;
                order.dest_left_index_score = model.dest_left_index_score.Equals(null) ? (decimal?)null : model.dest_left_index_score;
                order.dest_left_index = null;
                order.dest_right_thumb_score = model.dest_right_thumb_score.Equals(null) ? (decimal?)null : model.dest_right_thumb_score;
                order.dest_right_index = null;
                order.dest_right_index_score = model.dest_right_index_score.Equals(null) ? (decimal?)null : model.dest_right_index_score;
                order.dest_right_thumb = null;
                order.src_left_thumb_score = model.src_left_thumb_score.Equals(null) ? (decimal?)null : model.src_left_thumb_score;
                order.src_left_thumb = null;
                order.src_left_index_score = model.src_left_index_score.Equals(null) ? (decimal?)null : model.src_left_index_score;
                order.src_left_index = null;
                order.src_right_thumb_score = model.src_right_thumb_score.Equals(null) ? (decimal?)null : model.src_right_thumb_score;
                order.src_right_thumb = null;
                order.src_right_index_score = model.src_right_index_score.Equals(null) ? (decimal?)null : model.src_right_index_score;
                order.src_right_index = null;
                order.retailer_code = (model.channel_name == FixedValueCollection.ResellerChannel
                                                            || model.channel_name == FixedValueCollection.CorporateChannel
                                                            || model.channel_name == FixedValueCollection.SMEChannel)
                                                            ? FixedValueCollection.ResellerCodeText + model.retailer_id : model.retailer_id;
                order.port_in_date = String.IsNullOrEmpty(model.port_in_date) ? null : DateTime.Parse(model.port_in_date);
                order.alt_msisdn = model.alt_msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    if (model.poc_msisdn_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                    {
                        order.poc_number = FixedValueCollection.MSISDNCountryCode + model.poc_msisdn_number;
                    }
                }
                order.is_urgent = model.is_urgent.Equals(null) ? null : model.is_urgent;
                order.optional1 = model.optional1;
                order.optional2 = model.optional2;
                order.optional3 = model.otp; // For any kind of activation is processed through OTP then OTP will be insert in option3.
                order.optional4 = String.IsNullOrEmpty(model.optional4) ? null : Convert.ToDecimal(model.optional4.Trim());
                order.optional5 = String.IsNullOrEmpty(model.optional5) ? null : Convert.ToDecimal(model.optional5.Trim());
                order.optional6 = String.IsNullOrEmpty(model.optional6) ? null : Convert.ToDecimal(model.optional6.Trim());
                order.note = model.note;
                order.sim_rep_reason_id = String.IsNullOrEmpty(model.sim_rep_reason_id) ? null : Convert.ToDecimal(model.sim_rep_reason_id.Trim());
                order.payment_type = model.payment_type;
                order.is_paired = model.is_paired.Equals(null) ? null : model.is_paired;
                order.cahnnel_id = model.channel_id.Equals(null) ? null : model.channel_id;

                order.division_name = String.IsNullOrEmpty(model.division_name) ? model.division_name : ConverterHelper.UpperLowerWithSpaceConverter(model.division_name);
                order.district_name = String.IsNullOrEmpty(model.district_name) ? model.district_name : ConverterHelper.UpperLowerWithSpaceConverter(model.district_name);
                order.thana_name = String.IsNullOrEmpty(model.thana_name) ? model.thana_name : ConverterHelper.UpperLowerWithSpaceConverter(model.thana_name);

                //here distributor code is mapped with "P_CENTER_OR_DISTRIBUTOR_CODE" in DB. 
                order.distributor_code = String.IsNullOrEmpty(model.distributor_code) ? _bllCommon.GetDistributorCodeFromSessionToken(model.session_token) : model.distributor_code;
                order.sim_replc_reason = model.sim_replc_reason;
                order.channel_name = model.channel_name;
                order.right_id = model.right_id.Equals(null) ? null : model.right_id;

                order.sim_replacement_type = model.sim_replacement_type.Equals(null) ? null : model.sim_replacement_type;

                order.old_sim_number = String.IsNullOrEmpty(model.old_sim_number) ? null : model.old_sim_number;

                order.src_sim_category = model.src_sim_category;
                order.port_in_confirmation_code = model.port_in_confirmation_code;

                order.dest_ec_verifi_reqrd = !String.IsNullOrEmpty(model.dest_nid) ? 1 : 0;// 

                //If any trunsection porpuse have done by OTP, then the bio verification for src customer is not needed.
                //For example: For "two party EC verification" if the purpose is "B2B to B2C" with OTP, then the value "src_ec_verifi_reqrd" will 0.    
                //Here reseller app is sending OTP in "optional3" for "Corporate To Individual Transfer with OTP" [03 MAY 2020]
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.CorporateToIndividualTransfer
                    && !String.IsNullOrEmpty(model.otp))
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2COnePaertyValidationOTPNote' key may be missing within appSettings file.");
                    }

                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                else
                {
                    order.src_ec_verifi_reqrd = !String.IsNullOrEmpty(model.src_nid) ? 1 : 0;
                }
                #region One Party EC Verification
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer
                   && model.src_ec_verifi_reqrd == 0)
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2COnePaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                #endregion
                order.dest_foreign_flag = model.dest_foreign_flag;
                order.dbss_subscription_id = model.dbss_subscription_id.Equals(null) ? null : model.dbss_subscription_id;
                order.customer_id = model.customer_id;
                order.order_confirmation_code = model.order_confirmation_code;
                order.server_name = Environment.MachineName;

                //for sim repalcement customer update is enabled here.
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMReplacement &&
                    model.saf_status.HasValue)
                {
                    order.saf_status = model.saf_status == false ? 1 : 0;
                }
                else if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                            String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    order.saf_status = 1;
                }
                else
                {
                    order.saf_status = 0;
                }
                if (!String.IsNullOrEmpty(model.msisdnReservationId))
                {
                    order.msisdnReservationId = model.msisdnReservationId;
                }

                order.src_owner_customer_id = model.src_owner_customer_id;
                order.src_user_customer_id = model.src_user_customer_id;
                order.src_payer_customer_id = model.src_payer_customer_id;
                order.dest_imsi = model.dest_imsi;
                order.status = model.status;
                order.bss_reqId = model.bss_reqId;
                order.error_id = model.error_id;
                order.error_description = model.err_msg;

                if (model.order_booking_flag != null)
                {
                    order.order_booking_flag = model.order_booking_flag;
                }
                //dataManager = new DALBiometricRepo();
                tokenId = await dataManager.SubmitOrder2(order);

                //=========Makeing Response Decission=========
                if (tokenId > 0)
                {
                    orderRes.request_id = tokenId.ToString();
                    orderRes.is_success = true;
                    orderRes.message = MessageCollection.OrderSubmitSuccessfull;
                }
                else
                {
                    orderRes.request_id = !String.IsNullOrEmpty(tokenId.ToString()) ? tokenId.ToString() : "0";
                    orderRes.is_success = false;
                    orderRes.message = MessageCollection.OrderCreationFaild;
                }
                return orderRes;
            }
            catch (Exception ex)
            {
                orderRes.request_id = !String.IsNullOrEmpty(tokenId.ToString()) ? tokenId.ToString() : "0";
                orderRes.is_success = false;
                orderRes.message = ex.Message.ToString();

                return orderRes;

            }
        }

        public async Task<SendOrderResponse> SubmitOrderV4(RAOrderRequestV2 model, string loginProvider)// Converted FP in byte[].
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            decimal tokenId = 0;
            try
            {
                if (model.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                }

                OrderRequest3 order = new OrderRequest3();
                BL_Json blJson = new BL_Json();

                order.bi_token_number = model.bi_token_number == null ? 0 : model.bi_token_number;
                order.purpose_number = String.IsNullOrEmpty(model.purpose_number) ? null : Convert.ToDecimal(model.purpose_number.Trim());
                order.msisdn = model.msisdn;

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_category = model.src_sim_category;
                else
                    order.sim_category = model.sim_category;

                if (!String.IsNullOrEmpty(model.sim_number))
                {
                    if (model.sim_number.Substring(0, 6) != FixedValueCollection.SIMCode)
                    {
                        model.sim_number = FixedValueCollection.SIMCode + model.sim_number;
                    }
                }

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_number = model.old_sim_number != null ? model.old_sim_number.Trim() : "";
                else if (!String.IsNullOrEmpty(model.sim_number))
                    order.sim_number = model.sim_number.Trim();
                else
                    order.sim_number = model.sim_number;

                order.subscription_type_id = String.IsNullOrEmpty(model.subscription_type_id) ? null : Convert.ToDecimal(model.subscription_type_id.Trim());
                order.subscription_code = model.subscription_code;
                order.package_id = String.IsNullOrEmpty(model.package_id) ? null : Convert.ToDecimal(model.package_id.Trim());
                order.package_code = model.package_code;
                if (!String.IsNullOrEmpty(model.dest_nid) && !String.IsNullOrEmpty(model.dest_dob))
                {
                    order.dest_nid = (model.dest_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.dest_dob).Year + model.dest_nid : model.dest_nid;
                }

                if (!String.IsNullOrEmpty(model.src_nid) && !String.IsNullOrEmpty(model.src_dob))
                {
                    order.src_nid = (model.src_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.src_dob).Year + model.src_nid : model.src_nid;
                }

                order.dest_dob = String.IsNullOrEmpty(model.dest_dob) ? String.Empty : DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.dest_nid))
                {
                    order.dest_doc_type_no = model.dest_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;
                }

                order.src_dob = String.IsNullOrEmpty(model.src_dob) ? String.Empty : DateTime.Parse(model.src_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.src_nid))
                {
                    order.src_doc_type_no = model.src_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;//(int)DOCTypeNo.nid;//1 for nid. /*model.src_doc_type_no;*/
                }
                else
                {
                    order.src_doc_type_no = null;
                }
                order.platform_id = model.platform_id;

                if (!String.IsNullOrEmpty(model.customer_name) &&
                        isCustomerNameValid(model.customer_name))
                {
                    order.customer_name = model.customer_name;
                }

                if (!String.IsNullOrEmpty(model.gender))
                {
                    order.gender = model.gender.ToLower() == "others" ? "notitle" : model.gender.ToLower();
                }

                order.flat_number = model.flat_number;
                order.house_number = model.house_number;
                order.road_number = model.road_number;
                order.village = model.village;
                order.division_id = model.division_id;
                order.district_id = model.district_id;
                order.thana_id = model.thana_id;
                order.postal_code = String.IsNullOrEmpty(model.postal_code) ? "0" : model.postal_code;

                //=====temp modelfication (as RA apk is unable to update [15-12-19])============
                if (!String.IsNullOrEmpty(model.email))
                {
                    order.email = isEmailValid(model.email.ToLower()) == true ? model.email : String.Empty;
                }
                else
                {
                    order.email = String.Empty;
                }
                //===================

                order.retailer_id = model.retailer_id;
                //note: For retailer App dest_doc_id and src_doc_id is dest_nid and src_nid namely. 
                order.dest_left_thumb_score = model.dest_left_thumb_score.Equals(null) ? (decimal?)null : model.dest_left_thumb_score;
                order.dest_left_thumb = null;
                order.dest_left_index_score = model.dest_left_index_score.Equals(null) ? (decimal?)null : model.dest_left_index_score;
                order.dest_left_index = null;
                order.dest_right_thumb_score = model.dest_right_thumb_score.Equals(null) ? (decimal?)null : model.dest_right_thumb_score;
                order.dest_right_index = null;
                order.dest_right_index_score = model.dest_right_index_score.Equals(null) ? (decimal?)null : model.dest_right_index_score;
                order.dest_right_thumb = null;
                order.src_left_thumb_score = model.src_left_thumb_score.Equals(null) ? (decimal?)null : model.src_left_thumb_score;
                order.src_left_thumb = null;
                order.src_left_index_score = model.src_left_index_score.Equals(null) ? (decimal?)null : model.src_left_index_score;
                order.src_left_index = null;
                order.src_right_thumb_score = model.src_right_thumb_score.Equals(null) ? (decimal?)null : model.src_right_thumb_score;
                order.src_right_thumb = null;
                order.src_right_index_score = model.src_right_index_score.Equals(null) ? (decimal?)null : model.src_right_index_score;
                order.src_right_index = null;
                order.retailer_code = (model.channel_name == FixedValueCollection.ResellerChannel
                                                            || model.channel_name == FixedValueCollection.CorporateChannel
                                                            || model.channel_name == FixedValueCollection.SMEChannel)
                                                            ? FixedValueCollection.ResellerCodeText + model.retailer_id : model.retailer_id;
                order.port_in_date = String.IsNullOrEmpty(model.port_in_date) ? null : DateTime.Parse(model.port_in_date);
                order.alt_msisdn = model.alt_msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    if (model.poc_msisdn_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                    {
                        order.poc_number = FixedValueCollection.MSISDNCountryCode + model.poc_msisdn_number;
                    }
                }
                order.is_urgent = model.is_urgent.Equals(null) ? null : model.is_urgent;
                order.optional1 = model.optional1;
                order.optional2 = model.optional2;
                order.optional3 = model.otp; // For any kind of activation is processed through OTP then OTP will be insert in option3.
                order.optional4 = String.IsNullOrEmpty(model.optional4) ? null : Convert.ToDecimal(model.optional4.Trim());
                order.optional5 = String.IsNullOrEmpty(model.optional5) ? null : Convert.ToDecimal(model.optional5.Trim());
                order.optional6 = String.IsNullOrEmpty(model.optional6) ? null : Convert.ToDecimal(model.optional6.Trim());
                order.note = model.note;
                order.sim_rep_reason_id = String.IsNullOrEmpty(model.sim_rep_reason_id) ? null : Convert.ToDecimal(model.sim_rep_reason_id.Trim());
                order.payment_type = model.payment_type;
                order.is_paired = model.is_paired.Equals(null) ? null : model.is_paired;
                order.cahnnel_id = model.channel_id.Equals(null) ? null : model.channel_id;

                order.division_name = String.IsNullOrEmpty(model.division_name) ? model.division_name : ConverterHelper.UpperLowerWithSpaceConverter(model.division_name);
                order.district_name = String.IsNullOrEmpty(model.district_name) ? model.district_name : ConverterHelper.UpperLowerWithSpaceConverter(model.district_name);
                order.thana_name = String.IsNullOrEmpty(model.thana_name) ? model.thana_name : ConverterHelper.UpperLowerWithSpaceConverter(model.thana_name);

                //here distributor code is mapped with "P_CENTER_OR_DISTRIBUTOR_CODE" in DB. 
                order.distributor_code = model.distributor_code; // String.IsNullOrEmpty(model.distributor_code) ? _bllCommon.GetDistributorCodeFromSessionTokenV2(model.session_token, model.retailer_id) : model.distributor_code;
                order.sim_replc_reason = model.sim_replc_reason;
                order.channel_name = model.channel_name;
                order.right_id = model.right_id.Equals(null) ? null : model.right_id;

                order.sim_replacement_type = model.sim_replacement_type.Equals(null) ? null : model.sim_replacement_type;

                order.old_sim_number = String.IsNullOrEmpty(model.old_sim_number) ? null : model.old_sim_number;

                order.src_sim_category = model.src_sim_category;
                order.port_in_confirmation_code = model.port_in_confirmation_code;

                order.dest_ec_verifi_reqrd = !String.IsNullOrEmpty(model.dest_nid) ? 1 : 0;// 

                //If any trunsection porpuse have done by OTP, then the bio verification for src customer is not needed.
                //For example: For "two party EC verification" if the purpose is "B2B to B2C" with OTP, then the value "src_ec_verifi_reqrd" will 0.    
                //Here reseller app is sending OTP in "optional3" for "Corporate To Individual Transfer with OTP" [03 MAY 2020]
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.CorporateToIndividualTransfer
                    && !String.IsNullOrEmpty(model.otp))
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2CTwoPaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                else
                {
                    order.src_ec_verifi_reqrd = !String.IsNullOrEmpty(model.src_nid) ? 1 : 0;
                }
                #region One Party EC Verification
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer
                   && model.src_ec_verifi_reqrd == 0)
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2COnePaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                #endregion
                order.dest_foreign_flag = model.dest_foreign_flag;
                order.dbss_subscription_id = model.dbss_subscription_id.Equals(null) ? null : model.dbss_subscription_id;
                order.customer_id = model.customer_id;
                order.order_confirmation_code = model.order_confirmation_code;
                order.server_name = Environment.MachineName;

                //for sim repalcement customer update is enabled here.
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMReplacement &&
                    model.saf_status.HasValue)
                {
                    order.saf_status = model.saf_status == false ? 1 : 0;
                }
                else if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer && String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    order.saf_status = 1;
                }
                else if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMCategoryMigration && model.saf_status.HasValue)
                {
                    order.saf_status = model.saf_status == false ? 1 : 0;
                }
                else
                {
                    order.saf_status = 0;
                }
                if (!String.IsNullOrEmpty(model.msisdnReservationId))
                {
                    order.msisdnReservationId = model.msisdnReservationId;
                }

                order.src_owner_customer_id = model.src_owner_customer_id;
                order.src_user_customer_id = model.src_user_customer_id;
                order.src_payer_customer_id = model.src_payer_customer_id;
                order.dest_imsi = model.dest_imsi;
                order.status = model.status;
                order.bss_reqId = model.bss_reqId;
                order.error_id = model.error_id;
                order.error_description = model.err_msg;

                order.loginAttemptId = LoginProviderInfo.login_attempt_id;
                order.latitude = model.latitude;
                order.longitude = model.longitude;


                if (model.lac != null)
                {
                    order.lac = (int)model.lac;
                }

                if (model.cid != null)
                {
                    order.cid = (int)model.cid;
                }

                if (model.order_booking_flag != null)
                {
                    order.order_booking_flag = model.order_booking_flag;
                }

                //dataManager = new DALBiometricRepo();
                tokenId = await dataManager.SubmitOrderV3(order, loginProvider);

                //=========Makeing Response Decission=========
                if (tokenId > 0)
                {
                    orderRes.request_id = tokenId.ToString();
                    orderRes.is_success = true;
                    orderRes.message = MessageCollection.OrderSubmitSuccessfull;
                }
                else
                {
                    orderRes.request_id = !String.IsNullOrEmpty(tokenId.ToString()) ? tokenId.ToString() : "0"; ;
                    orderRes.is_success = false;
                    orderRes.message = MessageCollection.OrderCreationFaild;
                }
                return orderRes;
            }
            catch (Exception ex)
            {
                orderRes.request_id = !String.IsNullOrEmpty(tokenId.ToString()) ? tokenId.ToString() : "0";
                orderRes.is_success = false;
                orderRes.message = ex.Message.ToString();

                return orderRes;

            }
        }

        public async Task<SendOrderResponse> SubmitOrderV5(RAOrderRequestV2 model, string loginProvider)// Converted FP in byte[].
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            decimal tokenId = 0;
            try
            {
                if (model.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                }

                OrderRequest3 order = new OrderRequest3();
                BL_Json blJson = new BL_Json();

                order.bi_token_number = model.bi_token_number == null ? 0 : model.bi_token_number;
                order.purpose_number = String.IsNullOrEmpty(model.purpose_number) ? null : Convert.ToDecimal(model.purpose_number.Trim());
                order.msisdn = model.msisdn;

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_category = model.src_sim_category;
                else
                    order.sim_category = model.sim_category;

                if (!String.IsNullOrEmpty(model.sim_number))
                {
                    if (model.sim_number.Substring(0, 6) != FixedValueCollection.SIMCode)
                    {
                        model.sim_number = FixedValueCollection.SIMCode + model.sim_number;
                    }
                }

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_number = model.old_sim_number != null ? model.old_sim_number.Trim() : "";
                else if (!String.IsNullOrEmpty(model.sim_number))
                    order.sim_number = model.sim_number.Trim();
                else
                    order.sim_number = model.sim_number;

                order.subscription_type_id = String.IsNullOrEmpty(model.subscription_type_id) ? null : Convert.ToDecimal(model.subscription_type_id.Trim());
                order.subscription_code = model.subscription_code;
                order.package_id = String.IsNullOrEmpty(model.package_id) ? null : Convert.ToDecimal(model.package_id.Trim());
                order.package_code = model.package_code;
                if (!String.IsNullOrEmpty(model.dest_nid) && !String.IsNullOrEmpty(model.dest_dob))
                {
                    order.dest_nid = (model.dest_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.dest_dob).Year + model.dest_nid : model.dest_nid;
                }

                if (!String.IsNullOrEmpty(model.src_nid) && !String.IsNullOrEmpty(model.src_dob))
                {
                    order.src_nid = (model.src_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.src_dob).Year + model.src_nid : model.src_nid;
                }

                order.dest_dob = String.IsNullOrEmpty(model.dest_dob) ? String.Empty : DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.dest_nid))
                {
                    order.dest_doc_type_no = model.dest_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;
                }

                order.src_dob = String.IsNullOrEmpty(model.src_dob) ? String.Empty : DateTime.Parse(model.src_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.src_nid))
                {
                    order.src_doc_type_no = model.src_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;//(int)DOCTypeNo.nid;//1 for nid. /*model.src_doc_type_no;*/
                }
                else
                {
                    order.src_doc_type_no = null;
                }
                order.platform_id = model.platform_id;

                if (!String.IsNullOrEmpty(model.customer_name) &&
                        isCustomerNameValid(model.customer_name))
                {
                    order.customer_name = model.customer_name;
                }

                if (!String.IsNullOrEmpty(model.gender))
                {
                    order.gender = model.gender.ToLower() == "others" ? "notitle" : model.gender.ToLower();
                }

                order.flat_number = model.flat_number;
                order.house_number = model.house_number;
                order.road_number = model.road_number;
                order.village = model.village;
                order.division_id = model.division_id.Equals(null) ? null : model.division_id;
                order.district_id = model.district_id.Equals(null) ? null : model.district_id;
                order.thana_id = model.thana_id.Equals(null) ? null : model.thana_id;
                order.postal_code = String.IsNullOrEmpty(model.postal_code) ? "0" : model.postal_code;

                //=====temp modelfication (as RA apk is unable to update [15-12-19])============
                if (!String.IsNullOrEmpty(model.email))
                {
                    order.email = isEmailValid(model.email.ToLower()) == true ? model.email : String.Empty;
                }
                else
                {
                    order.email = String.Empty;
                }
                //===================

                order.retailer_id = model.retailer_id;
                //note: For retailer App dest_doc_id and src_doc_id is dest_nid and src_nid namely. 
                order.dest_left_thumb_score = model.dest_left_thumb_score.Equals(null) ? (decimal?)null : model.dest_left_thumb_score;
                order.dest_left_thumb = null;
                order.dest_left_index_score = model.dest_left_index_score.Equals(null) ? (decimal?)null : model.dest_left_index_score;
                order.dest_left_index = null;
                order.dest_right_thumb_score = model.dest_right_thumb_score.Equals(null) ? (decimal?)null : model.dest_right_thumb_score;
                order.dest_right_index = null;
                order.dest_right_index_score = model.dest_right_index_score.Equals(null) ? (decimal?)null : model.dest_right_index_score;
                order.dest_right_thumb = null;
                order.src_left_thumb_score = model.src_left_thumb_score.Equals(null) ? (decimal?)null : model.src_left_thumb_score;
                order.src_left_thumb = null;
                order.src_left_index_score = model.src_left_index_score.Equals(null) ? (decimal?)null : model.src_left_index_score;
                order.src_left_index = null;
                order.src_right_thumb_score = model.src_right_thumb_score.Equals(null) ? (decimal?)null : model.src_right_thumb_score;
                order.src_right_thumb = null;
                order.src_right_index_score = model.src_right_index_score.Equals(null) ? (decimal?)null : model.src_right_index_score;
                order.src_right_index = null;
                order.retailer_code = (model.channel_name == FixedValueCollection.ResellerChannel
                                                            || model.channel_name == FixedValueCollection.CorporateChannel
                                                            || model.channel_name == FixedValueCollection.SMEChannel)
                                                            ? FixedValueCollection.ResellerCodeText + model.retailer_id : model.retailer_id;
                order.port_in_date = String.IsNullOrEmpty(model.port_in_date) ? null : DateTime.Parse(model.port_in_date);
                order.alt_msisdn = model.alt_msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    if (model.poc_msisdn_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                    {
                        order.poc_number = FixedValueCollection.MSISDNCountryCode + model.poc_msisdn_number;
                    }
                }
                order.is_urgent = model.is_urgent.Equals(null) ? null : model.is_urgent;
                order.optional1 = model.optional1;
                order.optional2 = model.optional2;
                order.optional3 = model.otp; // For any kind of activation is processed through OTP then OTP will be insert in option3.
                order.optional4 = String.IsNullOrEmpty(model.optional4) ? null : Convert.ToDecimal(model.optional4.Trim());
                order.optional5 = String.IsNullOrEmpty(model.optional5) ? null : Convert.ToDecimal(model.optional5.Trim());
                order.optional6 = String.IsNullOrEmpty(model.optional6) ? null : Convert.ToDecimal(model.optional6.Trim());
                order.note = model.note;
                order.sim_rep_reason_id = String.IsNullOrEmpty(model.sim_rep_reason_id) ? null : Convert.ToDecimal(model.sim_rep_reason_id.Trim());
                order.payment_type = model.payment_type;
                order.is_paired = model.is_paired.Equals(null) ? null : model.is_paired;
                order.cahnnel_id = model.channel_id.Equals(null) ? null : model.channel_id;

                order.division_name = String.IsNullOrEmpty(model.division_name) ? model.division_name : ConverterHelper.UpperLowerWithSpaceConverter(model.division_name);
                order.district_name = String.IsNullOrEmpty(model.district_name) ? model.district_name : ConverterHelper.UpperLowerWithSpaceConverter(model.district_name);
                order.thana_name = String.IsNullOrEmpty(model.thana_name) ? model.thana_name : ConverterHelper.UpperLowerWithSpaceConverter(model.thana_name);

                //here distributor code is mapped with "P_CENTER_OR_DISTRIBUTOR_CODE" in DB. 
                order.distributor_code = String.IsNullOrEmpty(model.distributor_code) ? await _bllCommon.GetDistributorCodeFromSessionTokenV2(model.session_token, model.retailer_id) : model.distributor_code;
                order.sim_replc_reason = model.sim_replc_reason;
                order.channel_name = model.channel_name;
                order.right_id = model.right_id.Equals(null) ? null : model.right_id;

                order.sim_replacement_type = model.sim_replacement_type.Equals(null) ? null : model.sim_replacement_type;

                order.old_sim_number = String.IsNullOrEmpty(model.old_sim_number) ? null : model.old_sim_number;

                order.src_sim_category = model.src_sim_category;
                order.port_in_confirmation_code = model.port_in_confirmation_code;

                order.dest_ec_verifi_reqrd = !String.IsNullOrEmpty(model.dest_nid) ? 1 : 0;// 

                //If any trunsection porpuse have done by OTP, then the bio verification for src customer is not needed.
                //For example: For "two party EC verification" if the purpose is "B2B to B2C" with OTP, then the value "src_ec_verifi_reqrd" will 0.    
                //Here reseller app is sending OTP in "optional3" for "Corporate To Individual Transfer with OTP" [03 MAY 2020]
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.CorporateToIndividualTransfer
                    && !String.IsNullOrEmpty(model.otp))
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2CTwoPaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                else
                {
                    order.src_ec_verifi_reqrd = !String.IsNullOrEmpty(model.src_nid) ? 1 : 0;
                }
                #region One Party EC Verification
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer
                   && model.src_ec_verifi_reqrd == 0)
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2COnePaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                #endregion
                order.dest_foreign_flag = model.dest_foreign_flag;
                order.dbss_subscription_id = model.dbss_subscription_id.Equals(null) ? null : model.dbss_subscription_id;
                order.customer_id = model.customer_id;
                order.order_confirmation_code = model.order_confirmation_code;
                order.server_name = Environment.MachineName;

                //for sim repalcement customer update is enabled here.
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMReplacement &&
                    model.saf_status.HasValue)
                {
                    order.saf_status = model.saf_status == false ? 1 : 0;
                }
                else if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                            String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    order.saf_status = 1;
                }
                else
                {
                    order.saf_status = 0;
                }
                if (!String.IsNullOrEmpty(model.msisdnReservationId))
                {
                    order.msisdnReservationId = model.msisdnReservationId;
                }

                order.src_owner_customer_id = model.src_owner_customer_id;
                order.src_user_customer_id = model.src_user_customer_id;
                order.src_payer_customer_id = model.src_payer_customer_id;
                order.dest_imsi = model.dest_imsi;
                order.status = model.status;
                order.bss_reqId = model.bss_reqId;
                order.error_id = model.error_id;
                order.error_description = model.err_msg;

                order.loginAttemptId = LoginProviderInfo.login_attempt_id;
                order.latitude = model.latitude;
                order.longitude = model.longitude;

                if (model.lac != null)
                {
                    order.lac = (int)model.lac;
                }

                if (model.cid != null)
                {
                    order.cid = (int)model.cid;
                }

                order.scanner_id = model.scanner_id;

                if (model.order_booking_flag != null)
                {
                    order.order_booking_flag = model.order_booking_flag;
                }

                //dataManager = new DALBiometricRepo();
                tokenId = await dataManager.SubmitOrderV4(order, loginProvider);

                //=========Makeing Response Decission=========
                if (tokenId > 0)
                {
                    orderRes.request_id = tokenId.ToString();
                    orderRes.is_success = true;
                    orderRes.message = MessageCollection.OrderSubmitSuccessfull;
                }
                else
                {
                    orderRes.request_id = !String.IsNullOrEmpty(tokenId.ToString()) ? tokenId.ToString() : "0";
                    orderRes.is_success = false;
                    orderRes.message = MessageCollection.OrderCreationFaild;
                }
                return orderRes;
            }
            catch (Exception ex)
            {
                orderRes.request_id = !String.IsNullOrEmpty(tokenId.ToString()) ? tokenId.ToString() : "0";
                orderRes.is_success = false;
                orderRes.message = ex.Message.ToString();

                return orderRes;
            }
        }
        public async Task<SendOrderResponse> SubmitOrderV6(RAOrderRequestV2 model, string loginProvider)// Converted FP in byte[].
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            decimal tokenId = 0;
            try
            {
                if (model.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                }

                OrderRequest3 order = new OrderRequest3();
                BL_Json blJson = new BL_Json();

                order.bi_token_number = model.bi_token_number == null ? 0 : model.bi_token_number;
                order.purpose_number = String.IsNullOrEmpty(model.purpose_number) ? null : Convert.ToDecimal(model.purpose_number.Trim());
                order.msisdn = model.msisdn;

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_category = model.src_sim_category;
                else
                    order.sim_category = model.sim_category;

                if (!String.IsNullOrEmpty(model.sim_number))
                {
                    if (model.sim_number.Substring(0, 6) != FixedValueCollection.SIMCode)
                    {
                        model.sim_number = FixedValueCollection.SIMCode + model.sim_number;
                    }
                }

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_number = model.old_sim_number != null ? model.old_sim_number.Trim() : "";
                else if (!String.IsNullOrEmpty(model.sim_number))
                    order.sim_number = model.sim_number.Trim();
                else
                    order.sim_number = model.sim_number;

                order.subscription_type_id = String.IsNullOrEmpty(model.subscription_type_id) ? null : Convert.ToDecimal(model.subscription_type_id.Trim());
                order.subscription_code = model.subscription_code;
                order.package_id = String.IsNullOrEmpty(model.package_id) ? null : Convert.ToDecimal(model.package_id.Trim());
                order.package_code = model.package_code;
                if (!String.IsNullOrEmpty(model.dest_nid) && !String.IsNullOrEmpty(model.dest_dob))
                {
                    order.dest_nid = (model.dest_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.dest_dob).Year + model.dest_nid : model.dest_nid;
                }

                if (!String.IsNullOrEmpty(model.src_nid) && !String.IsNullOrEmpty(model.src_dob))
                {
                    order.src_nid = (model.src_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.src_dob).Year + model.src_nid : model.src_nid;
                }

                order.dest_dob = String.IsNullOrEmpty(model.dest_dob) ? String.Empty : DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.dest_nid))
                {
                    order.dest_doc_type_no = model.dest_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;
                }

                order.src_dob = String.IsNullOrEmpty(model.src_dob) ? String.Empty : DateTime.Parse(model.src_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.src_nid))
                {
                    order.src_doc_type_no = model.src_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;//(int)DOCTypeNo.nid;//1 for nid. /*model.src_doc_type_no;*/
                }
                else
                {
                    order.src_doc_type_no = null;
                }
                order.platform_id = model.platform_id;

                if (!String.IsNullOrEmpty(model.customer_name) &&
                        isCustomerNameValid(model.customer_name))
                {
                    order.customer_name = model.customer_name;
                }

                if (!String.IsNullOrEmpty(model.gender))
                {
                    order.gender = model.gender.ToLower() == "others" ? "notitle" : model.gender.ToLower();
                }

                order.flat_number = model.flat_number;
                order.house_number = model.house_number;
                order.road_number = model.road_number;
                order.village = model.village;
                order.division_id = model.division_id;
                order.district_id = model.district_id;
                order.thana_id = model.thana_id;
                order.postal_code = String.IsNullOrEmpty(model.postal_code) ? "0" : model.postal_code;

                //=====temp modelfication (as RA apk is unable to update [15-12-19])============
                if (!String.IsNullOrEmpty(model.email))
                {
                    order.email = isEmailValid(model.email.ToLower()) == true ? model.email : String.Empty;
                }
                else
                {
                    order.email = String.Empty;
                }
                //===================

                order.retailer_id = model.retailer_id;
                //note: For retailer App dest_doc_id and src_doc_id is dest_nid and src_nid namely. 
                order.dest_left_thumb_score = model.dest_left_thumb_score.Equals(null) ? (decimal?)null : model.dest_left_thumb_score;
                order.dest_left_thumb = null;
                order.dest_left_index_score = model.dest_left_index_score.Equals(null) ? (decimal?)null : model.dest_left_index_score;
                order.dest_left_index = null;
                order.dest_right_thumb_score = model.dest_right_thumb_score.Equals(null) ? (decimal?)null : model.dest_right_thumb_score;
                order.dest_right_index = null;
                order.dest_right_index_score = model.dest_right_index_score.Equals(null) ? (decimal?)null : model.dest_right_index_score;
                order.dest_right_thumb = null;
                order.src_left_thumb_score = model.src_left_thumb_score.Equals(null) ? (decimal?)null : model.src_left_thumb_score;
                order.src_left_thumb = null;
                order.src_left_index_score = model.src_left_index_score.Equals(null) ? (decimal?)null : model.src_left_index_score;
                order.src_left_index = null;
                order.src_right_thumb_score = model.src_right_thumb_score.Equals(null) ? (decimal?)null : model.src_right_thumb_score;
                order.src_right_thumb = null;
                order.src_right_index_score = model.src_right_index_score.Equals(null) ? (decimal?)null : model.src_right_index_score;
                order.src_right_index = null;
                order.retailer_code = (model.channel_name == FixedValueCollection.ResellerChannel
                                                            || model.channel_name == FixedValueCollection.CorporateChannel
                                                            || model.channel_name == FixedValueCollection.SMEChannel)
                                                            ? FixedValueCollection.ResellerCodeText + model.retailer_id : model.retailer_id;
                order.port_in_date = String.IsNullOrEmpty(model.port_in_date) ? null : DateTime.Parse(model.port_in_date);
                order.alt_msisdn = model.alt_msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    if (model.poc_msisdn_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                    {
                        order.poc_number = FixedValueCollection.MSISDNCountryCode + model.poc_msisdn_number;
                    }
                }
                order.is_urgent = model.is_urgent.Equals(null) ? null : model.is_urgent;
                order.optional1 = model.optional1;
                order.optional2 = model.optional2;
                order.optional3 = model.otp; // For any kind of activation is processed through OTP then OTP will be insert in option3.
                order.optional4 = String.IsNullOrEmpty(model.optional4) ? null : Convert.ToDecimal(model.optional4.Trim());
                order.optional5 = String.IsNullOrEmpty(model.optional5) ? null : Convert.ToDecimal(model.optional5.Trim());
                order.optional6 = String.IsNullOrEmpty(model.optional6) ? null : Convert.ToDecimal(model.optional6.Trim());
                order.note = model.note;
                order.sim_rep_reason_id = String.IsNullOrEmpty(model.sim_rep_reason_id) ? null : Convert.ToDecimal(model.sim_rep_reason_id.Trim());
                order.payment_type = model.payment_type;
                order.is_paired = model.is_paired.Equals(null) ? null : model.is_paired;
                order.cahnnel_id = model.channel_id.Equals(null) ? null : model.channel_id;

                order.division_name = String.IsNullOrEmpty(model.division_name) ? model.division_name : ConverterHelper.UpperLowerWithSpaceConverter(model.division_name);
                order.district_name = String.IsNullOrEmpty(model.district_name) ? model.district_name : ConverterHelper.UpperLowerWithSpaceConverter(model.district_name);
                order.thana_name = String.IsNullOrEmpty(model.thana_name) ? model.thana_name : ConverterHelper.UpperLowerWithSpaceConverter(model.thana_name);

                //here distributor code is mapped with "P_CENTER_OR_DISTRIBUTOR_CODE" in DB. 
                order.distributor_code = String.IsNullOrEmpty(model.distributor_code) ? await _bllCommon.GetDistributorCodeFromSessionTokenV2(model.session_token, model.retailer_id) : model.distributor_code;
                order.sim_replc_reason = model.sim_replc_reason;
                order.channel_name = model.channel_name;
                order.right_id = model.right_id.Equals(null) ? null : model.right_id;

                order.sim_replacement_type = model.sim_replacement_type.Equals(null) ? null : model.sim_replacement_type;

                order.old_sim_number = String.IsNullOrEmpty(model.old_sim_number) ? null : model.old_sim_number;

                order.src_sim_category = model.src_sim_category;
                order.port_in_confirmation_code = model.port_in_confirmation_code;

                order.dest_ec_verifi_reqrd = !String.IsNullOrEmpty(model.dest_nid) ? 1 : 0;// 

                //If any trunsection porpuse have done by OTP, then the bio verification for src customer is not needed.
                //For example: For "two party EC verification" if the purpose is "B2B to B2C" with OTP, then the value "src_ec_verifi_reqrd" will 0.    
                //Here reseller app is sending OTP in "optional3" for "Corporate To Individual Transfer with OTP" [03 MAY 2020]
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.CorporateToIndividualTransfer
                    && !String.IsNullOrEmpty(model.otp))
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2CTwoPaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                }
                else
                {
                    order.src_ec_verifi_reqrd = !String.IsNullOrEmpty(model.src_nid) ? 1 : 0;
                }
                #region One Party EC Verification
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer
                   && model.src_ec_verifi_reqrd == 0)
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2COnePaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                #endregion
                order.dest_foreign_flag = model.dest_foreign_flag;
                order.dbss_subscription_id = model.dbss_subscription_id.Equals(null) ? null : model.dbss_subscription_id;
                order.customer_id = model.customer_id;
                order.order_confirmation_code = model.order_confirmation_code;
                order.server_name = Environment.MachineName;

                //for sim repalcement customer update is enabled here.
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMReplacement &&
                    model.saf_status.HasValue)
                {
                    order.saf_status = model.saf_status == false ? 1 : 0;
                }
                else if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                            String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    order.saf_status = 1;
                }
                else
                {
                    order.saf_status = 0;
                }
                if (!String.IsNullOrEmpty(model.msisdnReservationId))
                {
                    order.msisdnReservationId = model.msisdnReservationId;
                }

                order.src_owner_customer_id = model.src_owner_customer_id;
                order.src_user_customer_id = model.src_user_customer_id;
                order.src_payer_customer_id = model.src_payer_customer_id;
                order.dest_imsi = model.dest_imsi;
                order.status = model.status;
                order.bss_reqId = model.bss_reqId;
                order.error_id = model.error_id;
                order.error_description = model.err_msg;

                order.loginAttemptId = LoginProviderInfo.login_attempt_id;
                order.latitude = model.latitude;
                order.longitude = model.longitude;

                if (model.lac != null)
                {
                    order.lac = (int)model.lac;
                }

                if (model.cid != null)
                {
                    order.cid = (int)model.cid;
                }

                order.scanner_id = model.scanner_id;

                if (model.order_booking_flag != null)
                {
                    order.order_booking_flag = model.order_booking_flag;
                }

                order.is_esim = model.is_esim;

                //dataManager = new DALBiometricRepo();
                tokenId = await dataManager.SubmitOrderV5(order, loginProvider);

                //=========Makeing Response Decission=========
                if (tokenId > 0)
                {
                    orderRes.request_id = tokenId.ToString();
                    orderRes.is_success = true;
                    orderRes.message = MessageCollection.OrderSubmitSuccessfull;
                }
                else
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = MessageCollection.OrderCreationFaild;
                }
                return orderRes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SendOrderResponseRev> SubmitOrderV7(RAOrderRequestV2 model, string loginProvider)// Converted FP in byte[].
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            // decimal tokenId = 0;
            //dataManager = new DALBiometricRepo();
            try
            {
                if (model.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                }

                OrderRequest3 order = new OrderRequest3();
                BL_Json blJson = new BL_Json();

                order.bi_token_number = model.bi_token_number == null ? 0 : model.bi_token_number;
                order.purpose_number = String.IsNullOrEmpty(model.purpose_number) ? null : Convert.ToDecimal(model.purpose_number.Trim());
                order.msisdn = model.msisdn;

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_category = model.src_sim_category;
                else
                    order.sim_category = model.sim_category;

                if (!String.IsNullOrEmpty(model.sim_number))
                {
                    if (model.sim_number.Substring(0, 6) != FixedValueCollection.SIMCode)
                    {
                        model.sim_number = FixedValueCollection.SIMCode + model.sim_number;
                    }
                }

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_number = model.old_sim_number != null ? model.old_sim_number.Trim() : "";
                else if (!String.IsNullOrEmpty(model.sim_number))
                    order.sim_number = model.sim_number.Trim();
                else
                    order.sim_number = model.sim_number;

                order.subscription_type_id = String.IsNullOrEmpty(model.subscription_type_id) ? null : Convert.ToDecimal(model.subscription_type_id.Trim());
                order.subscription_code = model.subscription_code;
                order.package_id = String.IsNullOrEmpty(model.package_id) ? null : Convert.ToDecimal(model.package_id.Trim());
                order.package_code = model.package_code;
                if (!String.IsNullOrEmpty(model.dest_nid) && !String.IsNullOrEmpty(model.dest_dob))
                {
                    order.dest_nid = (model.dest_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.dest_dob).Year + model.dest_nid : model.dest_nid;
                }

                if (!String.IsNullOrEmpty(model.src_nid) && !String.IsNullOrEmpty(model.src_dob))
                {
                    order.src_nid = (model.src_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.src_dob).Year + model.src_nid : model.src_nid;
                }

                order.dest_dob = String.IsNullOrEmpty(model.dest_dob) ? String.Empty : DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.dest_nid))
                {
                    order.dest_doc_type_no = model.dest_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;
                }

                order.src_dob = String.IsNullOrEmpty(model.src_dob) ? String.Empty : DateTime.Parse(model.src_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.src_nid))
                {
                    order.src_doc_type_no = model.src_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;//(int)DOCTypeNo.nid;//1 for nid. /*model.src_doc_type_no;*/
                }
                else
                {
                    order.src_doc_type_no = null;
                }
                order.platform_id = model.platform_id;

                if (!String.IsNullOrEmpty(model.customer_name) &&
                        isCustomerNameValid(model.customer_name))
                {
                    order.customer_name = model.customer_name;
                }

                if (!String.IsNullOrEmpty(model.gender))
                {
                    order.gender = model.gender.ToLower() == "others" ? "notitle" : model.gender.ToLower();
                }

                order.flat_number = model.flat_number;
                order.house_number = model.house_number;
                order.road_number = model.road_number;
                order.village = model.village;
                order.division_id = model.division_id.Equals(null) ? null : model.division_id;
                order.district_id = model.district_id.Equals(null) ? null : model.district_id;
                order.thana_id = model.thana_id.Equals(null) ? null : model.thana_id;
                order.postal_code = String.IsNullOrEmpty(model.postal_code) ? "0" : model.postal_code;

                //=====temp modelfication (as RA apk is unable to update [15-12-19])============
                if (!String.IsNullOrEmpty(model.email))
                {
                    order.email = isEmailValid(model.email.ToLower()) == true ? model.email : String.Empty;
                }
                else
                {
                    order.email = String.Empty;
                }
                //===================

                order.retailer_id = model.retailer_id;
                //note: For retailer App dest_doc_id and src_doc_id is dest_nid and src_nid namely. 
                order.dest_left_thumb_score = model.dest_left_thumb_score.Equals(null) ? (decimal?)null : model.dest_left_thumb_score;
                order.dest_left_thumb = null;
                order.dest_left_index_score = model.dest_left_index_score.Equals(null) ? (decimal?)null : model.dest_left_index_score;
                order.dest_left_index = null;
                order.dest_right_thumb_score = model.dest_right_thumb_score.Equals(null) ? (decimal?)null : model.dest_right_thumb_score;
                order.dest_right_index = null;
                order.dest_right_index_score = model.dest_right_index_score.Equals(null) ? (decimal?)null : model.dest_right_index_score;
                order.dest_right_thumb = null;
                order.src_left_thumb_score = model.src_left_thumb_score.Equals(null) ? (decimal?)null : model.src_left_thumb_score;
                order.src_left_thumb = null;
                order.src_left_index_score = model.src_left_index_score.Equals(null) ? (decimal?)null : model.src_left_index_score;
                order.src_left_index = null;
                order.src_right_thumb_score = model.src_right_thumb_score.Equals(null) ? (decimal?)null : model.src_right_thumb_score;
                order.src_right_thumb = null;
                order.src_right_index_score = model.src_right_index_score.Equals(null) ? (decimal?)null : model.src_right_index_score;
                order.src_right_index = null;
                order.retailer_code = (model.channel_name == FixedValueCollection.ResellerChannel
                                                            || model.channel_name == FixedValueCollection.CorporateChannel
                                                            || model.channel_name == FixedValueCollection.SMEChannel)
                                                            ? FixedValueCollection.ResellerCodeText + model.retailer_id : model.retailer_id;
                order.port_in_date = String.IsNullOrEmpty(model.port_in_date) ? null : DateTime.Parse(model.port_in_date);
                order.alt_msisdn = model.alt_msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    if (model.poc_msisdn_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                    {
                        order.poc_number = FixedValueCollection.MSISDNCountryCode + model.poc_msisdn_number;
                    }
                    else
                    {
                        order.poc_number = model.poc_msisdn_number;
                    }
                }
                order.is_urgent = model.is_urgent.Equals(null) ? null : model.is_urgent;
                order.optional1 = model.optional1;
                order.optional2 = model.optional2;
                order.optional3 = model.otp; // For any kind of activation is processed through OTP then OTP will be insert in option3.
                order.optional4 = String.IsNullOrEmpty(model.optional4) ? null : Convert.ToDecimal(model.optional4.Trim());
                order.optional5 = String.IsNullOrEmpty(model.optional5) ? null : Convert.ToDecimal(model.optional5.Trim());
                order.optional6 = String.IsNullOrEmpty(model.optional6) ? null : Convert.ToDecimal(model.optional6.Trim());
                order.note = model.note;
                order.sim_rep_reason_id = String.IsNullOrEmpty(model.sim_rep_reason_id) ? null : Convert.ToDecimal(model.sim_rep_reason_id.Trim());
                order.payment_type = model.payment_type;
                order.is_paired = model.is_paired.Equals(null) ? null : model.is_paired;
                order.cahnnel_id = model.channel_id.Equals(null) ? null : model.channel_id;

                order.division_name = String.IsNullOrEmpty(model.division_name) ? model.division_name : ConverterHelper.UpperLowerWithSpaceConverter(model.division_name);
                order.district_name = String.IsNullOrEmpty(model.district_name) ? model.district_name : ConverterHelper.UpperLowerWithSpaceConverter(model.district_name);
                order.thana_name = String.IsNullOrEmpty(model.thana_name) ? model.thana_name : ConverterHelper.UpperLowerWithSpaceConverter(model.thana_name);

                //here distributor code is mapped with "P_CENTER_OR_DISTRIBUTOR_CODE" in DB. 
                order.distributor_code = String.IsNullOrEmpty(model.distributor_code) ? await _bllCommon.GetDistributorCodeFromSessionTokenV2(model.session_token, model.retailer_id) : model.distributor_code;
                order.sim_replc_reason = model.sim_replc_reason;
                order.channel_name = model.channel_name;
                order.right_id = model.right_id.Equals(null) ? null : model.right_id;

                order.sim_replacement_type = model.sim_replacement_type.Equals(null) ? null : model.sim_replacement_type;

                order.old_sim_number = String.IsNullOrEmpty(model.old_sim_number) ? null : model.old_sim_number;

                order.src_sim_category = model.src_sim_category;
                order.port_in_confirmation_code = model.port_in_confirmation_code;

                order.dest_ec_verifi_reqrd = !String.IsNullOrEmpty(model.dest_nid) ? 1 : 0;// 

                //If any trunsection porpuse have done by OTP, then the bio verification for src customer is not needed.
                //For example: For "two party EC verification" if the purpose is "B2B to B2C" with OTP, then the value "src_ec_verifi_reqrd" will 0.    
                //Here reseller app is sending OTP in "optional3" for "Corporate To Individual Transfer with OTP" [03 MAY 2020]
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.CorporateToIndividualTransfer
                    && !String.IsNullOrEmpty(model.otp))
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2CTwoPaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                }
                else
                {
                    order.src_ec_verifi_reqrd = !String.IsNullOrEmpty(model.src_nid) ? 1 : 0;
                }
                #region One Party EC Verification
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer
                   && model.src_ec_verifi_reqrd == 0)
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2COnePaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                #endregion
                order.dest_foreign_flag = model.dest_foreign_flag;
                order.dbss_subscription_id = model.dbss_subscription_id.Equals(null) ? null : model.dbss_subscription_id;
                order.customer_id = model.customer_id;
                order.order_confirmation_code = model.order_confirmation_code;
                order.server_name = Environment.MachineName;

                //for sim repalcement customer update is enabled here.
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMReplacement &&
                    model.saf_status.HasValue)
                {
                    order.saf_status = model.saf_status == false ? 1 : 0;
                }
                else if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                            String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    order.saf_status = 1;
                }
                else
                {
                    order.saf_status = 0;
                }
                if (!String.IsNullOrEmpty(model.msisdnReservationId))
                {
                    order.msisdnReservationId = model.msisdnReservationId;
                }

                order.src_owner_customer_id = model.src_owner_customer_id;
                order.src_user_customer_id = model.src_user_customer_id;
                order.src_payer_customer_id = model.src_payer_customer_id;
                order.dest_imsi = model.dest_imsi;
                order.status = model.status;
                order.bss_reqId = model.bss_reqId;
                order.error_id = model.error_id;
                order.error_description = model.err_msg;

                order.loginAttemptId = LoginProviderInfo.login_attempt_id;
                order.latitude = model.latitude;
                order.longitude = model.longitude;

                if (model.lac != null)
                {
                    order.lac = (int)model.lac;
                }

                if (model.cid != null)
                {
                    order.cid = (int)model.cid;
                }

                order.scanner_id = model.scanner_id;

                if (model.order_booking_flag != null)
                {
                    order.order_booking_flag = model.order_booking_flag;
                }

                order.is_esim = model.is_esim;

                //dataManager = new DALBiometricRepo();
                var dataRow = await dataManager.SubmitOrderV6(order, loginProvider);

                //=========Makeing Response Decission=========
                if (dataRow.Rows.Count > 0)
                {
                    double tokenId = Convert.ToDouble(dataRow.Rows[0]["po_PKValue"] == DBNull.Value ? null : dataRow.Rows[0]["po_PKValue"]);
                    string message = Convert.ToString(dataRow.Rows[0]["po_message"] == DBNull.Value ? null : dataRow.Rows[0]["po_message"]);
                    if (tokenId > 0)
                    {
                        orderRes = new SendOrderResponseRev()
                        {
                            data = new DataRes()
                            {
                                request_id = tokenId.ToString()
                            }
                        };
                        //orderRes.data = new DataRes()
                        //{
                        //    request_id = tokenId.ToString()
                        //};
                        orderRes.isError = false;
                        orderRes.message = MessageCollection.OrderSubmitSuccessfull;
                    }
                    else
                    {
                        orderRes = new SendOrderResponseRev()
                        {
                            data = new DataRes()
                            {
                                request_id = "0"
                            }
                        };
                        //orderRes.data = new DataRes()
                        //{
                        //    request_id = "0"
                        //};
                        //orderRes.data.request_id = "0";
                        orderRes.isError = true;
                        orderRes.message = message;
                    }
                }

                return orderRes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SendOrderResponseRev> SubmitOrderForRegistration(RAOrderRequestV2 model, string loginProvider, int isregisteredReq)// Converted FP in byte[].
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev(); 
            // decimal tokenId = 0;
            //dataManager = new DALBiometricRepo();
            try
            {
                if (model.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                }

                OrderRequest3 order = new OrderRequest3();
                BL_Json blJson = new BL_Json();

                order.bi_token_number = model.bi_token_number == null ? 0 : model.bi_token_number;
                order.purpose_number = String.IsNullOrEmpty(model.purpose_number) ? null : Convert.ToDecimal(model.purpose_number.Trim());
                order.msisdn = model.msisdn;

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_category = model.src_sim_category;
                else
                    order.sim_category = model.sim_category;

                if (!String.IsNullOrEmpty(model.sim_number))
                {
                    if (model.sim_number.Substring(0, 6) != FixedValueCollection.SIMCode)
                    {
                        model.sim_number = FixedValueCollection.SIMCode + model.sim_number;
                    }
                }

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_number = model.old_sim_number != null ? model.old_sim_number.Trim() : "";
                else if (!String.IsNullOrEmpty(model.sim_number))
                    order.sim_number = model.sim_number.Trim();
                else
                    order.sim_number = model.sim_number;

                order.subscription_type_id = String.IsNullOrEmpty(model.subscription_type_id) ? null : Convert.ToDecimal(model.subscription_type_id.Trim());
                order.subscription_code = model.subscription_code;
                order.package_id = String.IsNullOrEmpty(model.package_id) ? null : Convert.ToDecimal(model.package_id.Trim());
                order.package_code = model.package_code;
                if (!String.IsNullOrEmpty(model.dest_nid) && !String.IsNullOrEmpty(model.dest_dob))
                {
                    order.dest_nid = (model.dest_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.dest_dob).Year + model.dest_nid : model.dest_nid;
                }

                if (!String.IsNullOrEmpty(model.src_nid) && !String.IsNullOrEmpty(model.src_dob))
                {
                    order.src_nid = (model.src_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.src_dob).Year + model.src_nid : model.src_nid;
                }

                order.dest_dob = String.IsNullOrEmpty(model.dest_dob) ? String.Empty : DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.dest_nid))
                {
                    order.dest_doc_type_no = model.dest_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;
                }

                order.src_dob = String.IsNullOrEmpty(model.src_dob) ? String.Empty : DateTime.Parse(model.src_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.src_nid))
                {
                    order.src_doc_type_no = model.src_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;//(int)DOCTypeNo.nid;//1 for nid. /*model.src_doc_type_no;*/
                }
                else
                {
                    order.src_doc_type_no = null;
                }
                order.platform_id = model.platform_id;

                if (!String.IsNullOrEmpty(model.customer_name) &&
                        isCustomerNameValid(model.customer_name))
                {
                    order.customer_name = model.customer_name;
                }

                if (!String.IsNullOrEmpty(model.gender))
                {
                    order.gender = model.gender.ToLower() == "others" ? "notitle" : model.gender.ToLower();
                }

                order.flat_number = model.flat_number;
                order.house_number = model.house_number;
                order.road_number = model.road_number;
                order.village = model.village;
                order.division_id = model.division_id.Equals(null) ? null : model.division_id;
                order.district_id = model.district_id.Equals(null) ? null : model.district_id;
                order.thana_id = model.thana_id.Equals(null) ? null : model.thana_id;
                order.postal_code = String.IsNullOrEmpty(model.postal_code) ? "0" : model.postal_code;

                //=====temp modelfication (as RA apk is unable to update [15-12-19])============
                if (!String.IsNullOrEmpty(model.email))
                {
                    order.email = isEmailValid(model.email.ToLower()) == true ? model.email : String.Empty;
                }
                else
                {
                    order.email = String.Empty;
                }
                //===================

                order.retailer_id = model.retailer_id;
                //note: For retailer App dest_doc_id and src_doc_id is dest_nid and src_nid namely. 
                order.dest_left_thumb_score = model.dest_left_thumb_score.Equals(null) ? (decimal?)null : model.dest_left_thumb_score;
                order.dest_left_thumb = null;
                order.dest_left_index_score = model.dest_left_index_score.Equals(null) ? (decimal?)null : model.dest_left_index_score;
                order.dest_left_index = null;
                order.dest_right_thumb_score = model.dest_right_thumb_score.Equals(null) ? (decimal?)null : model.dest_right_thumb_score;
                order.dest_right_index = null;
                order.dest_right_index_score = model.dest_right_index_score.Equals(null) ? (decimal?)null : model.dest_right_index_score;
                order.dest_right_thumb = null;
                order.src_left_thumb_score = model.src_left_thumb_score.Equals(null) ? (decimal?)null : model.src_left_thumb_score;
                order.src_left_thumb = null;
                order.src_left_index_score = model.src_left_index_score.Equals(null) ? (decimal?)null : model.src_left_index_score;
                order.src_left_index = null;
                order.src_right_thumb_score = model.src_right_thumb_score.Equals(null) ? (decimal?)null : model.src_right_thumb_score;
                order.src_right_thumb = null;
                order.src_right_index_score = model.src_right_index_score.Equals(null) ? (decimal?)null : model.src_right_index_score;
                order.src_right_index = null;
                order.retailer_code = (model.channel_name == FixedValueCollection.ResellerChannel
                                                            || model.channel_name == FixedValueCollection.CorporateChannel
                                                            || model.channel_name == FixedValueCollection.SMEChannel)
                                                            ? FixedValueCollection.ResellerCodeText + model.retailer_id : model.retailer_id;
                order.port_in_date = String.IsNullOrEmpty(model.port_in_date) ? null : DateTime.Parse(model.port_in_date);
                order.alt_msisdn = model.alt_msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    if (model.poc_msisdn_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                    {
                        order.poc_number = FixedValueCollection.MSISDNCountryCode + model.poc_msisdn_number;
                    }
                    else
                    {
                        order.poc_number = model.poc_msisdn_number;
                    }
                }
                order.is_urgent = model.is_urgent.Equals(null) ? null : model.is_urgent;
                order.optional1 = model.optional1;
                order.optional2 = model.optional2;
                order.optional3 = model.otp; // For any kind of activation is processed through OTP then OTP will be insert in option3.
                order.optional4 = String.IsNullOrEmpty(model.optional4) ? null : Convert.ToDecimal(model.optional4.Trim());
                order.optional5 = String.IsNullOrEmpty(model.optional5) ? null : Convert.ToDecimal(model.optional5.Trim());
                order.optional6 = String.IsNullOrEmpty(model.optional6) ? null : Convert.ToDecimal(model.optional6.Trim());
                order.note = model.note;
                order.sim_rep_reason_id = String.IsNullOrEmpty(model.sim_rep_reason_id) ? null : Convert.ToDecimal(model.sim_rep_reason_id.Trim());
                order.payment_type = model.payment_type;
                order.is_paired = model.is_paired.Equals(null) ? null : model.is_paired;
                order.cahnnel_id = model.channel_id.Equals(null) ? null : model.channel_id;

                order.division_name = String.IsNullOrEmpty(model.division_name) ? model.division_name : ConverterHelper.UpperLowerWithSpaceConverter(model.division_name);
                order.district_name = String.IsNullOrEmpty(model.district_name) ? model.district_name : ConverterHelper.UpperLowerWithSpaceConverter(model.district_name);
                order.thana_name = String.IsNullOrEmpty(model.thana_name) ? model.thana_name : ConverterHelper.UpperLowerWithSpaceConverter(model.thana_name);

                //here distributor code is mapped with "P_CENTER_OR_DISTRIBUTOR_CODE" in DB. 
                order.distributor_code = String.IsNullOrEmpty(model.distributor_code) ? await _bllCommon.GetDistributorCodeFromSessionTokenV2(model.session_token, model.retailer_id) : model.distributor_code;
                order.sim_replc_reason = model.sim_replc_reason;
                order.channel_name = model.channel_name;
                order.right_id = model.right_id.Equals(null) ? null : model.right_id;

                order.sim_replacement_type = model.sim_replacement_type.Equals(null) ? null : model.sim_replacement_type;

                order.old_sim_number = String.IsNullOrEmpty(model.old_sim_number) ? null : model.old_sim_number;

                order.src_sim_category = model.src_sim_category;
                order.port_in_confirmation_code = model.port_in_confirmation_code;

                order.dest_ec_verifi_reqrd = !String.IsNullOrEmpty(model.dest_nid) ? 1 : 0;// 

                //If any trunsection porpuse have done by OTP, then the bio verification for src customer is not needed.
                //For example: For "two party EC verification" if the purpose is "B2B to B2C" with OTP, then the value "src_ec_verifi_reqrd" will 0.    
                //Here reseller app is sending OTP in "optional3" for "Corporate To Individual Transfer with OTP" [03 MAY 2020]
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.CorporateToIndividualTransfer
                    && !String.IsNullOrEmpty(model.otp))
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2CTwoPaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                }
                else
                {
                    order.src_ec_verifi_reqrd = !String.IsNullOrEmpty(model.src_nid) ? 1 : 0;
                }
                #region One Party EC Verification
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer
                   && model.src_ec_verifi_reqrd == 0)
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2COnePaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                #endregion
                order.dest_foreign_flag = model.dest_foreign_flag;
                order.dbss_subscription_id = model.dbss_subscription_id.Equals(null) ? null : model.dbss_subscription_id;
                order.customer_id = model.customer_id;
                order.order_confirmation_code = model.order_confirmation_code;
                order.server_name = Environment.MachineName;

                //for sim repalcement customer update is enabled here.
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMReplacement &&
                    model.saf_status.HasValue)
                {
                    order.saf_status = model.saf_status == false ? 1 : 0;
                }
                else if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                            String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    order.saf_status = 1;
                }
                else
                {
                    order.saf_status = 0;
                }
                if (!String.IsNullOrEmpty(model.msisdnReservationId))
                {
                    order.msisdnReservationId = model.msisdnReservationId;
                }

                order.src_owner_customer_id = model.src_owner_customer_id;
                order.src_user_customer_id = model.src_user_customer_id;
                order.src_payer_customer_id = model.src_payer_customer_id;
                order.dest_imsi = model.dest_imsi;
                order.status = model.status;
                order.bss_reqId = model.bss_reqId;
                order.error_id = model.error_id;
                order.error_description = model.err_msg;

                order.loginAttemptId = LoginProviderInfo.login_attempt_id;
                order.latitude = model.latitude;
                order.longitude = model.longitude;

                if (model.lac != null)
                {
                    order.lac = (int)model.lac;
                }

                if (model.cid != null)
                {
                    order.cid = (int)model.cid;
                }

                order.scanner_id = model.scanner_id;

                if (model.order_booking_flag != null)
                {
                    order.order_booking_flag = model.order_booking_flag;
                }

                order.is_esim = model.is_esim;

                //dataManager = new DALBiometricRepo();
                var dataRow = await dataManager.SubmitOrderRegistrationReq(order, loginProvider, isregisteredReq);

                //=========Makeing Response Decission=========
                if (dataRow.Rows.Count > 0)
                {
                    double tokenId = Convert.ToDouble(dataRow.Rows[0]["po_PKValue"] == DBNull.Value ? null : dataRow.Rows[0]["po_PKValue"]);
                    string message = Convert.ToString(dataRow.Rows[0]["po_message"] == DBNull.Value ? null : dataRow.Rows[0]["po_message"]);
                    if (tokenId > 0)
                    {
                        orderRes = new SendOrderResponseRev()
                        {
                            data = new DataRes()
                            {
                                request_id = tokenId.ToString()
                            }
                        };
                        //orderRes.data = new DataRes()
                        //{
                        //    request_id = tokenId.ToString()
                        //};
                        orderRes.isError = false;
                        orderRes.message = MessageCollection.OrderSubmitSuccessfull;
                    }
                    else
                    {
                        orderRes = new SendOrderResponseRev()
                        {
                            data = new DataRes()
                            {
                                request_id = "0"
                            }
                        };
                        //orderRes.data = new DataRes()
                        //{
                        //    request_id = "0"
                        //};
                        //orderRes.data.request_id = "0";
                        orderRes.isError = true;
                        orderRes.message = message;
                    }
                }

                return orderRes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SendOrderResponseRev> SubmitOrderV8(RAOrderRequestV2 model, string loginProvider)// Converted FP in byte[].
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            // decimal tokenId = 0;  
            //dataManager = new DALBiometricRepo();
            try
            {
                if (model.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                }

                OrderRequest3 order = new OrderRequest3();
                BL_Json blJson = new BL_Json();

                order.bi_token_number = model.bi_token_number == null ? 0 : model.bi_token_number;
                order.purpose_number = String.IsNullOrEmpty(model.purpose_number) ? null : Convert.ToDecimal(model.purpose_number.Trim());
                order.msisdn = model.msisdn;

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_category = model.src_sim_category;
                else
                    order.sim_category = model.sim_category;

                if (!String.IsNullOrEmpty(model.sim_number))
                {
                    if (model.sim_number.Substring(0, 6) != FixedValueCollection.SIMCode)
                    {
                        model.sim_number = FixedValueCollection.SIMCode + model.sim_number;
                    }
                }

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_number = model.old_sim_number != null ? model.old_sim_number.Trim() : "";
                else if (!String.IsNullOrEmpty(model.sim_number))
                    order.sim_number = model.sim_number.Trim();
                else
                    order.sim_number = model.sim_number;

                order.subscription_type_id = String.IsNullOrEmpty(model.subscription_type_id) ? null : Convert.ToDecimal(model.subscription_type_id.Trim());
                order.subscription_code = model.subscription_code;
                order.package_id = String.IsNullOrEmpty(model.package_id) ? null : Convert.ToDecimal(model.package_id.Trim());
                order.package_code = model.package_code;
                if (!String.IsNullOrEmpty(model.dest_nid) && !String.IsNullOrEmpty(model.dest_dob))
                {
                    order.dest_nid = (model.dest_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.dest_dob).Year + model.dest_nid : model.dest_nid;
                }

                if (!String.IsNullOrEmpty(model.src_nid) && !String.IsNullOrEmpty(model.src_dob))
                {
                    order.src_nid = (model.src_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.src_dob).Year + model.src_nid : model.src_nid;
                }

                order.dest_dob = String.IsNullOrEmpty(model.dest_dob) ? String.Empty : DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.dest_nid))
                {
                    order.dest_doc_type_no = model.dest_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;
                }

                order.src_dob = String.IsNullOrEmpty(model.src_dob) ? String.Empty : DateTime.Parse(model.src_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.src_nid))
                {
                    order.src_doc_type_no = model.src_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;//(int)DOCTypeNo.nid;//1 for nid. /*model.src_doc_type_no;*/
                }
                else
                {
                    order.src_doc_type_no = null;
                }
                order.platform_id = model.platform_id;

                if (!String.IsNullOrEmpty(model.customer_name) &&
                        isCustomerNameValid(model.customer_name))
                {
                    order.customer_name = model.customer_name;
                }

                if (!String.IsNullOrEmpty(model.gender))
                {
                    order.gender = model.gender.ToLower() == "others" ? "notitle" : model.gender.ToLower();
                }

                order.flat_number = model.flat_number;
                order.house_number = model.house_number;
                order.road_number = model.road_number;
                order.village = model.village;
                order.division_id = model.division_id;
                order.district_id = model.district_id;
                order.thana_id = model.thana_id;
                order.postal_code = String.IsNullOrEmpty(model.postal_code) ? "0" : model.postal_code;

                //=====temp modelfication (as RA apk is unable to update [15-12-19])============
                if (!String.IsNullOrEmpty(model.email))
                {
                    order.email = isEmailValid(model.email.ToLower()) == true ? model.email : String.Empty;
                }
                else
                {
                    order.email = String.Empty;
                }
                //===================

                order.retailer_id = model.retailer_id;
                //note: For retailer App dest_doc_id and src_doc_id is dest_nid and src_nid namely. 
                order.dest_left_thumb_score = model.dest_left_thumb_score.Equals(null) ? (decimal?)null : model.dest_left_thumb_score;
                order.dest_left_thumb = null;
                order.dest_left_index_score = model.dest_left_index_score.Equals(null) ? (decimal?)null : model.dest_left_index_score;
                order.dest_left_index = null;
                order.dest_right_thumb_score = model.dest_right_thumb_score.Equals(null) ? (decimal?)null : model.dest_right_thumb_score;
                order.dest_right_index = null;
                order.dest_right_index_score = model.dest_right_index_score.Equals(null) ? (decimal?)null : model.dest_right_index_score;
                order.dest_right_thumb = null;
                order.src_left_thumb_score = model.src_left_thumb_score.Equals(null) ? (decimal?)null : model.src_left_thumb_score;
                order.src_left_thumb = null;
                order.src_left_index_score = model.src_left_index_score.Equals(null) ? (decimal?)null : model.src_left_index_score;
                order.src_left_index = null;
                order.src_right_thumb_score = model.src_right_thumb_score.Equals(null) ? (decimal?)null : model.src_right_thumb_score;
                order.src_right_thumb = null;
                order.src_right_index_score = model.src_right_index_score.Equals(null) ? (decimal?)null : model.src_right_index_score;
                order.src_right_index = null;
                order.retailer_code = (model.channel_name == FixedValueCollection.ResellerChannel
                                                            || model.channel_name == FixedValueCollection.CorporateChannel
                                                            || model.channel_name == FixedValueCollection.SMEChannel)
                                                            ? FixedValueCollection.ResellerCodeText + model.retailer_id : model.retailer_id;
                order.port_in_date = String.IsNullOrEmpty(model.port_in_date) ? null : DateTime.Parse(model.port_in_date);
                order.alt_msisdn = model.alt_msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    if (model.poc_msisdn_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                    {
                        order.poc_number = FixedValueCollection.MSISDNCountryCode + model.poc_msisdn_number;
                    }
                    else
                    {
                        order.poc_number = model.poc_msisdn_number;
                    }
                }
                order.is_urgent = model.is_urgent.Equals(null) ? null : model.is_urgent;
                order.optional1 = model.optional1;
                order.optional2 = model.optional2;
                order.optional3 = model.otp; // For any kind of activation is processed through OTP then OTP will be insert in option3.
                order.optional4 = String.IsNullOrEmpty(model.optional4) ? null : Convert.ToDecimal(model.optional4.Trim());
                order.optional5 = String.IsNullOrEmpty(model.optional5) ? null : Convert.ToDecimal(model.optional5.Trim());
                order.optional6 = String.IsNullOrEmpty(model.optional6) ? null : Convert.ToDecimal(model.optional6.Trim());
                order.note = model.note;
                order.sim_rep_reason_id = String.IsNullOrEmpty(model.sim_rep_reason_id) ? null : Convert.ToDecimal(model.sim_rep_reason_id.Trim());
                order.payment_type = model.payment_type;
                order.is_paired = model.is_paired.Equals(null) ? null : model.is_paired;
                order.cahnnel_id = model.channel_id.Equals(null) ? null : model.channel_id;

                order.division_name = String.IsNullOrEmpty(model.division_name) ? model.division_name : ConverterHelper.UpperLowerWithSpaceConverter(model.division_name);
                order.district_name = String.IsNullOrEmpty(model.district_name) ? model.district_name : ConverterHelper.UpperLowerWithSpaceConverter(model.district_name);
                order.thana_name = String.IsNullOrEmpty(model.thana_name) ? model.thana_name : ConverterHelper.UpperLowerWithSpaceConverter(model.thana_name);

                //here distributor code is mapped with "P_CENTER_OR_DISTRIBUTOR_CODE" in DB. 
                order.distributor_code = String.IsNullOrEmpty(model.distributor_code) ? await _bllCommon.GetDistributorCodeFromSessionTokenV2(model.session_token, model.retailer_id) : model.distributor_code;
                order.sim_replc_reason = model.sim_replc_reason;
                order.channel_name = model.channel_name;
                order.right_id = model.right_id.Equals(null) ? null : model.right_id;

                order.sim_replacement_type = model.sim_replacement_type.Equals(null) ? null : model.sim_replacement_type;

                order.old_sim_number = String.IsNullOrEmpty(model.old_sim_number) ? null : model.old_sim_number;

                order.src_sim_category = model.src_sim_category;
                order.port_in_confirmation_code = model.port_in_confirmation_code;

                order.dest_ec_verifi_reqrd = !String.IsNullOrEmpty(model.dest_nid) ? 1 : 0;// 

                //If any trunsection porpuse have done by OTP, then the bio verification for src customer is not needed.
                //For example: For "two party EC verification" if the purpose is "B2B to B2C" with OTP, then the value "src_ec_verifi_reqrd" will 0.    
                //Here reseller app is sending OTP in "optional3" for "Corporate To Individual Transfer with OTP" [03 MAY 2020]
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.CorporateToIndividualTransfer
                    && !String.IsNullOrEmpty(model.otp))
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2CTwoPaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                }
                else
                {
                    order.src_ec_verifi_reqrd = !String.IsNullOrEmpty(model.src_nid) ? 1 : 0;
                }
                #region One Party EC Verification
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer
                   && model.src_ec_verifi_reqrd == 0)
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2COnePaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                #endregion
                order.dest_foreign_flag = model.dest_foreign_flag;
                order.dbss_subscription_id = model.dbss_subscription_id.Equals(null) ? null : model.dbss_subscription_id;
                order.customer_id = model.customer_id;
                order.order_confirmation_code = model.order_confirmation_code;
                order.server_name = Environment.MachineName;

                //for sim repalcement customer update is enabled here.
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMReplacement &&
                    model.saf_status.HasValue)
                {
                    order.saf_status = model.saf_status == false ? 1 : 0;
                }
                else if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                            String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    order.saf_status = 1;
                }
                else
                {
                    order.saf_status = 0;
                }
                if (!String.IsNullOrEmpty(model.msisdnReservationId))
                {
                    order.msisdnReservationId = model.msisdnReservationId;
                }

                order.src_owner_customer_id = model.src_owner_customer_id;
                order.src_user_customer_id = model.src_user_customer_id;
                order.src_payer_customer_id = model.src_payer_customer_id;
                order.dest_imsi = model.dest_imsi;
                order.status = model.status;
                order.bss_reqId = model.bss_reqId;
                order.error_id = model.error_id;
                order.error_description = model.err_msg;

                order.loginAttemptId = LoginProviderInfo.login_attempt_id;
                order.latitude = model.latitude;
                order.longitude = model.longitude;

                if (model.lac != null)
                {
                    order.lac = (int)model.lac;
                }

                if (model.cid != null)
                {
                    order.cid = (int)model.cid;
                }

                order.scanner_id = model.scanner_id;

                if (model.order_booking_flag != null)
                {
                    order.order_booking_flag = model.order_booking_flag;
                }

                order.is_esim = model.is_esim;

                if (model.is_starTrek != null)
                {
                    order.is_starTrek = model.is_starTrek;
                }
                order.order_id = model.order_id;

                if(model.is_online_sale != null)
                {
                    order.is_online_sale = model.is_online_sale;
                }

                //dataManager = new DALBiometricRepo();
                var dataRow = await dataManager.SubmitOrderV7(order, loginProvider);

                //=========Makeing Response Decission=========
                if (dataRow.Rows.Count > 0)
                {
                    double tokenId = Convert.ToDouble(dataRow.Rows[0]["po_PKValue"] == DBNull.Value ? null : dataRow.Rows[0]["po_PKValue"]);
                    string message = Convert.ToString(dataRow.Rows[0]["po_message"] == DBNull.Value ? null : dataRow.Rows[0]["po_message"]);
                    if (tokenId > 0)
                    {
                        orderRes.data = new DataRes()
                        {
                            request_id = tokenId.ToString()
                        };
                        orderRes.isError = false;
                        orderRes.message = MessageCollection.OrderSubmitSuccessfull;
                    }
                    else
                    {
                        orderRes.data = new DataRes()
                        {
                            request_id = "0"
                        };
                        //orderRes.data.request_id = "0";
                        orderRes.isError = true;
                        orderRes.message = message;
                    }
                }

                return orderRes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SendOrderResponseRev> SubmitOrderV9(CherishRequest model, string loginProvider)// Converted FP in byte[].
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            // decimal tokenId = 0;
            //dataManager = new DALBiometricRepo();
            try
            {
                if (model.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                }

                OrderRequest4 order = new OrderRequest4();
                BL_Json blJson = new BL_Json();

                order.bi_token_number = model.bi_token_number == null ? 0 : model.bi_token_number;
                order.purpose_number = String.IsNullOrEmpty(model.purpose_number) ? null : Convert.ToDecimal(model.purpose_number.Trim());
                order.msisdn = model.msisdn;
                order.selected_category = model.selected_category;

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_category = model.src_sim_category;
                else
                    order.sim_category = model.sim_category;

                if (!String.IsNullOrEmpty(model.sim_number))
                {
                    if (model.sim_number.Substring(0, 6) != FixedValueCollection.SIMCode)
                    {
                        model.sim_number = FixedValueCollection.SIMCode + model.sim_number;
                    }
                }

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_number = model.old_sim_number != null ? model.old_sim_number.Trim() : "";
                else if (!String.IsNullOrEmpty(model.sim_number))
                    order.sim_number = model.sim_number.Trim();
                else
                    order.sim_number = model.sim_number;

                order.subscription_type_id = String.IsNullOrEmpty(model.subscription_type_id) ? null : Convert.ToDecimal(model.subscription_type_id.Trim());
                order.subscription_code = model.subscription_code;
                order.package_id = String.IsNullOrEmpty(model.package_id) ? null : Convert.ToDecimal(model.package_id.Trim());
                order.package_code = model.package_code;
                if (!String.IsNullOrEmpty(model.dest_nid) && !String.IsNullOrEmpty(model.dest_dob))
                {
                    order.dest_nid = (model.dest_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.dest_dob).Year + model.dest_nid : model.dest_nid;
                }

                if (!String.IsNullOrEmpty(model.src_nid) && !String.IsNullOrEmpty(model.src_dob))
                {
                    order.src_nid = (model.src_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.src_dob).Year + model.src_nid : model.src_nid;
                }

                order.dest_dob = String.IsNullOrEmpty(model.dest_dob) ? String.Empty : DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.dest_nid))
                {
                    order.dest_doc_type_no = model.dest_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;
                }

                order.src_dob = String.IsNullOrEmpty(model.src_dob) ? String.Empty : DateTime.Parse(model.src_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.src_nid))
                {
                    order.src_doc_type_no = model.src_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;//(int)DOCTypeNo.nid;//1 for nid. /*model.src_doc_type_no;*/
                }
                else
                {
                    order.src_doc_type_no = null;
                }
                order.platform_id = model.platform_id;

                if (!String.IsNullOrEmpty(model.customer_name) &&
                        isCustomerNameValid(model.customer_name))
                {
                    order.customer_name = model.customer_name;
                }

                if (!String.IsNullOrEmpty(model.gender))
                {
                    order.gender = model.gender.ToLower() == "others" ? "notitle" : model.gender.ToLower();
                }

                order.flat_number = model.flat_number;
                order.house_number = model.house_number;
                order.road_number = model.road_number;
                order.village = model.village;
                order.division_id = model.division_id;
                order.district_id = model.district_id.Equals(null) ? null : model.district_id;
                order.thana_id = model.thana_id.Equals(null) ? null : model.thana_id;
                order.postal_code = String.IsNullOrEmpty(model.postal_code) ? "0" : model.postal_code;

                //=====temp modelfication (as RA apk is unable to update [15-12-19])============
                if (!String.IsNullOrEmpty(model.email))
                {
                    order.email = isEmailValid(model.email.ToLower()) == true ? model.email : String.Empty;
                }
                else
                {
                    order.email = String.Empty;
                }
                //===================

                order.retailer_id = model.retailer_id;
                //note: For retailer App dest_doc_id and src_doc_id is dest_nid and src_nid namely. 
                order.dest_left_thumb_score = model.dest_left_thumb_score.Equals(null) ? (decimal?)null : model.dest_left_thumb_score;
                order.dest_left_thumb = null;
                order.dest_left_index_score = model.dest_left_index_score.Equals(null) ? (decimal?)null : model.dest_left_index_score;
                order.dest_left_index = null;
                order.dest_right_thumb_score = model.dest_right_thumb_score.Equals(null) ? (decimal?)null : model.dest_right_thumb_score;
                order.dest_right_index = null;
                order.dest_right_index_score = model.dest_right_index_score.Equals(null) ? (decimal?)null : model.dest_right_index_score;
                order.dest_right_thumb = null;
                order.src_left_thumb_score = model.src_left_thumb_score.Equals(null) ? (decimal?)null : model.src_left_thumb_score;
                order.src_left_thumb = null;
                order.src_left_index_score = model.src_left_index_score.Equals(null) ? (decimal?)null : model.src_left_index_score;
                order.src_left_index = null;
                order.src_right_thumb_score = model.src_right_thumb_score.Equals(null) ? (decimal?)null : model.src_right_thumb_score;
                order.src_right_thumb = null;
                order.src_right_index_score = model.src_right_index_score.Equals(null) ? (decimal?)null : model.src_right_index_score;
                order.src_right_index = null;
                order.retailer_code = (model.channel_name == FixedValueCollection.ResellerChannel
                                                            || model.channel_name == FixedValueCollection.CorporateChannel
                                                            || model.channel_name == FixedValueCollection.SMEChannel)
                                                            ? FixedValueCollection.ResellerCodeText + model.retailer_id : model.retailer_id;
                order.port_in_date = String.IsNullOrEmpty(model.port_in_date) ? null : DateTime.Parse(model.port_in_date);
                order.alt_msisdn = model.alt_msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    if (model.poc_msisdn_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                    {
                        order.poc_number = FixedValueCollection.MSISDNCountryCode + model.poc_msisdn_number;
                    }
                    else
                    {
                        order.poc_number = model.poc_msisdn_number;
                    }
                }
                order.is_urgent = model.is_urgent.Equals(null) ? null : model.is_urgent;
                order.optional1 = model.optional1;
                order.optional2 = model.optional2;
                order.optional3 = model.otp; // For any kind of activation is processed through OTP then OTP will be insert in option3.
                order.optional4 = String.IsNullOrEmpty(model.optional4) ? null : Convert.ToDecimal(model.optional4.Trim());
                order.optional5 = String.IsNullOrEmpty(model.optional5) ? null : Convert.ToDecimal(model.optional5.Trim());
                order.optional6 = String.IsNullOrEmpty(model.optional6) ? null : Convert.ToDecimal(model.optional6.Trim());
                order.note = model.note;
                order.sim_rep_reason_id = String.IsNullOrEmpty(model.sim_rep_reason_id) ? null : Convert.ToDecimal(model.sim_rep_reason_id.Trim());
                order.payment_type = model.payment_type;
                order.is_paired = model.is_paired.Equals(null) ? null : model.is_paired;
                order.cahnnel_id = model.channel_id.Equals(null) ? null : model.channel_id;

                order.division_name = String.IsNullOrEmpty(model.division_name) ? model.division_name : ConverterHelper.UpperLowerWithSpaceConverter(model.division_name);
                order.district_name = String.IsNullOrEmpty(model.district_name) ? model.district_name : ConverterHelper.UpperLowerWithSpaceConverter(model.district_name);
                order.thana_name = String.IsNullOrEmpty(model.thana_name) ? model.thana_name : ConverterHelper.UpperLowerWithSpaceConverter(model.thana_name);

                //here distributor code is mapped with "P_CENTER_OR_DISTRIBUTOR_CODE" in DB. 
                order.distributor_code = String.IsNullOrEmpty(model.distributor_code) ? await _bllCommon.GetDistributorCodeFromSessionTokenV2(model.session_token, model.retailer_id) : model.distributor_code;
                order.sim_replc_reason = model.sim_replc_reason;
                order.channel_name = model.channel_name;
                order.right_id = model.right_id.Equals(null) ? null : model.right_id;

                order.sim_replacement_type = model.sim_replacement_type.Equals(null) ? null : model.sim_replacement_type;

                order.old_sim_number = String.IsNullOrEmpty(model.old_sim_number) ? null : model.old_sim_number;

                order.src_sim_category = model.src_sim_category;
                order.port_in_confirmation_code = model.port_in_confirmation_code;

                order.dest_ec_verifi_reqrd = !String.IsNullOrEmpty(model.dest_nid) ? 1 : 0;// 

                //If any trunsection porpuse have done by OTP, then the bio verification for src customer is not needed.
                //For example: For "two party EC verification" if the purpose is "B2B to B2C" with OTP, then the value "src_ec_verifi_reqrd" will 0.    
                //Here reseller app is sending OTP in "optional3" for "Corporate To Individual Transfer with OTP" [03 MAY 2020]
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.CorporateToIndividualTransfer
                    && !String.IsNullOrEmpty(model.otp))
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2CTwoPaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                }
                else
                {
                    order.src_ec_verifi_reqrd = !String.IsNullOrEmpty(model.src_nid) ? 1 : 0;
                }
                #region One Party EC Verification
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer
                   && model.src_ec_verifi_reqrd == 0)
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2COnePaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                #endregion
                order.dest_foreign_flag = model.dest_foreign_flag;
                order.dbss_subscription_id = model.dbss_subscription_id.Equals(null) ? null : model.dbss_subscription_id;
                order.customer_id = model.customer_id;
                order.order_confirmation_code = model.order_confirmation_code;
                order.server_name = Environment.MachineName;

                //for sim repalcement customer update is enabled here.
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMReplacement &&
                    model.saf_status.HasValue)
                {
                    order.saf_status = model.saf_status == false ? 1 : 0;
                }
                else if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                            String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    order.saf_status = 1;
                }
                else
                {
                    order.saf_status = 0;
                }
                if (!String.IsNullOrEmpty(model.msisdnReservationId))
                {
                    order.msisdnReservationId = model.msisdnReservationId;
                }

                order.src_owner_customer_id = model.src_owner_customer_id;
                order.src_user_customer_id = model.src_user_customer_id;
                order.src_payer_customer_id = model.src_payer_customer_id;
                order.dest_imsi = model.dest_imsi;
                order.status = model.status;
                order.bss_reqId = model.bss_reqId;
                order.error_id = model.error_id;
                order.error_description = model.err_msg;

                order.loginAttemptId = LoginProviderInfo.login_attempt_id;
                order.latitude = model.latitude;
                order.longitude = model.longitude;

                if (model.lac != null)
                {
                    order.lac = (int)model.lac;
                }

                if (model.cid != null)
                {
                    order.cid = (int)model.cid;
                }

                order.scanner_id = model.scanner_id;

                if (model.order_booking_flag != null)
                {
                    order.order_booking_flag = model.order_booking_flag;
                }

                order.is_esim = model.is_esim;

                //dataManager = new DALBiometricRepo();
                var dataRow = await dataManager.SubmitOrderV8(order, loginProvider);

                //=========Makeing Response Decission=========
                if (dataRow.Rows.Count > 0)
                {
                    double tokenId = Convert.ToDouble(dataRow.Rows[0]["po_PKValue"] == DBNull.Value ? null : dataRow.Rows[0]["po_PKValue"]);
                    string message = Convert.ToString(dataRow.Rows[0]["po_message"] == DBNull.Value ? null : dataRow.Rows[0]["po_message"]);
                    if (tokenId > 0)
                    {
                        orderRes = new SendOrderResponseRev()
                        {
                            data = new DataRes()
                            {
                                request_id = tokenId.ToString()
                            }
                        };
                        //orderRes.data = new DataRes()
                        //{
                        //    request_id = tokenId.ToString()
                        //};
                        orderRes.isError = false;
                        orderRes.message = MessageCollection.OrderSubmitSuccessfull;
                    }
                    else
                    {
                        orderRes = new SendOrderResponseRev()
                        {
                            data = new DataRes()
                            {
                                request_id = "0"
                            }
                        };
                        //orderRes.data = new DataRes()
                        //{
                        //    request_id = "0"
                        //};
                        //orderRes.data.request_id = "0";
                        orderRes.isError = true;
                        orderRes.message = message;
                    }
                }

                return orderRes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public OrderRequest2 SubmitOrderDataPurse(RAOrderRequest model)// Converted FP in byte[].
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            decimal tokenId = 0;
            try
            {
                if (model.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                }

                OrderRequest2 order = new OrderRequest2();
                BL_Json blJson = new BL_Json();

                order.bi_token_number = model.bi_token_number == null ? 0 : model.bi_token_number;
                order.purpose_number = String.IsNullOrEmpty(model.purpose_number) ? null : Convert.ToDecimal(model.purpose_number.Trim());
                order.msisdn = model.msisdn;

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_category = model.src_sim_category;
                else
                    order.sim_category = model.sim_category;


                if (!String.IsNullOrEmpty(model.sim_number))
                {
                    if (model.sim_number.Substring(0, 6) != FixedValueCollection.SIMCode)
                    {
                        model.sim_number = FixedValueCollection.SIMCode + model.sim_number;
                    }
                }

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_number = model.old_sim_number.Trim();
                else if (!String.IsNullOrEmpty(model.sim_number))
                    order.sim_number = model.sim_number.Trim();
                else
                    order.sim_number = model.sim_number;
                order.subscription_type_id = String.IsNullOrEmpty(model.subscription_type_id) ? null : Convert.ToDecimal(model.subscription_type_id.Trim());
                order.subscription_code = model.subscription_code;
                order.package_id = String.IsNullOrEmpty(model.package_id) ? null : Convert.ToDecimal(model.package_id.Trim());
                order.package_code = model.package_code;
                if (!String.IsNullOrEmpty(model.dest_nid) && !String.IsNullOrEmpty(model.dest_dob))
                {
                    order.dest_nid = (model.dest_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.dest_dob).Year + model.dest_nid : model.dest_nid;
                }

                if (!String.IsNullOrEmpty(model.src_nid) && !String.IsNullOrEmpty(model.src_dob))
                {
                    order.src_nid = (model.src_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.src_dob).Year + model.src_nid : model.src_nid;
                }

                order.dest_dob = String.IsNullOrEmpty(model.dest_dob) ? String.Empty : DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.dest_nid))
                {
                    order.dest_doc_type_no = model.dest_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;
                }

                order.src_dob = String.IsNullOrEmpty(model.src_dob) ? String.Empty : DateTime.Parse(model.src_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.src_nid))
                {
                    order.src_doc_type_no = model.src_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;//(int)DOCTypeNo.nid;//1 for nid. /*model.src_doc_type_no;*/
                }
                else
                {
                    order.src_doc_type_no = null;
                }
                order.platform_id = model.platform_id;

                if (!String.IsNullOrEmpty(model.customer_name) &&
                        isCustomerNameValid(model.customer_name))
                {
                    order.customer_name = model.customer_name;
                }

                if (!String.IsNullOrEmpty(model.gender))
                {
                    order.gender = model.gender.ToLower() == "others" ? "notitle" : model.gender.ToLower();
                }
                order.status = 20;

                order.flat_number = model.flat_number;
                order.house_number = model.house_number;
                order.road_number = model.road_number;
                order.village = model.village;
                order.division_id = model.division_id;
                order.district_id = model.district_id.Equals(null) ? null : model.district_id;
                order.thana_id = model.thana_id.Equals(null) ? null : model.thana_id;
                order.postal_code = String.IsNullOrEmpty(model.postal_code) ? "0" : model.postal_code;

                //=====temp modelfication (as RA apk is unable to update [15-12-19])============
                if (!String.IsNullOrEmpty(model.email))
                {
                    order.email = isEmailValid(model.email.ToLower()) == true ? model.email : String.Empty;
                }
                else
                {
                    order.email = String.Empty;
                }
                //===================

                order.retailer_id = model.retailer_id;
                //note: For retailer App dest_doc_id and src_doc_id is dest_nid and src_nid namely. 
                order.dest_left_thumb_score = model.dest_left_thumb_score.Equals(null) ? (decimal?)null : model.dest_left_thumb_score;
                order.dest_left_thumb = String.IsNullOrEmpty(model.dest_left_thumb) ? Convert.FromBase64String("") : Convert.FromBase64String(model.dest_left_thumb.Trim());
                order.dest_left_index_score = model.dest_left_index_score.Equals(null) ? (decimal?)null : model.dest_left_index_score;
                order.dest_left_index = String.IsNullOrEmpty(model.dest_left_index) ? Convert.FromBase64String("") : Convert.FromBase64String(model.dest_left_index.Trim());
                order.dest_right_thumb_score = model.dest_right_thumb_score.Equals(null) ? (decimal?)null : model.dest_right_thumb_score;
                order.dest_right_index = String.IsNullOrEmpty(model.dest_right_index) ? Convert.FromBase64String("") : Convert.FromBase64String(model.dest_right_index.Trim());
                order.dest_right_index_score = model.dest_right_index_score.Equals(null) ? (decimal?)null : model.dest_right_index_score;
                order.dest_right_thumb = String.IsNullOrEmpty(model.dest_right_thumb) ? Convert.FromBase64String("") : Convert.FromBase64String(model.dest_right_thumb.Trim());
                order.src_left_thumb_score = model.src_left_thumb_score.Equals(null) ? (decimal?)null : model.src_left_thumb_score;
                order.src_left_thumb = String.IsNullOrEmpty(model.src_left_thumb) ? Convert.FromBase64String("") : Convert.FromBase64String(model.src_left_thumb.Trim());
                order.src_left_index_score = model.src_left_index_score.Equals(null) ? (decimal?)null : model.src_left_index_score;
                order.src_left_index = String.IsNullOrEmpty(model.src_left_index) ? Convert.FromBase64String("") : Convert.FromBase64String(model.src_left_index.Trim());
                order.src_right_thumb_score = model.src_right_thumb_score.Equals(null) ? (decimal?)null : model.src_right_thumb_score;
                order.src_right_thumb = String.IsNullOrEmpty(model.src_right_thumb) ? Convert.FromBase64String("") : Convert.FromBase64String(model.src_right_thumb.Trim());
                order.src_right_index_score = model.src_right_index_score.Equals(null) ? (decimal?)null : model.src_right_index_score;
                order.src_right_index = String.IsNullOrEmpty(model.src_right_index) ? Convert.FromBase64String("") : Convert.FromBase64String(model.src_right_index.Trim());
                order.retailer_code = (model.channel_name == FixedValueCollection.ResellerChannel
                                                            || model.channel_name == FixedValueCollection.CorporateChannel
                                                            || model.channel_name == FixedValueCollection.SMEChannel)
                                                            ? FixedValueCollection.ResellerCodeText + model.retailer_id : model.retailer_id;
                order.port_in_date = String.IsNullOrEmpty(model.port_in_date) ? null : DateTime.Parse(model.port_in_date);
                order.alt_msisdn = model.alt_msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    if (model.poc_msisdn_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                    {
                        order.poc_number = FixedValueCollection.MSISDNCountryCode + model.poc_msisdn_number;
                    }
                }
                order.is_urgent = model.is_urgent.Equals(null) ? null : model.is_urgent;
                order.optional1 = model.optional1;
                order.optional2 = model.optional2;
                order.optional3 = model.otp; // For any kind of activation is processed through OTP then OTP will be insert in option3.
                order.optional4 = String.IsNullOrEmpty(model.optional4) ? null : Convert.ToDecimal(model.optional4.Trim());
                order.optional5 = String.IsNullOrEmpty(model.optional5) ? null : Convert.ToDecimal(model.optional5.Trim());
                order.optional6 = String.IsNullOrEmpty(model.optional6) ? null : Convert.ToDecimal(model.optional6.Trim());
                order.note = model.note;
                order.sim_rep_reason_id = String.IsNullOrEmpty(model.sim_rep_reason_id) ? null : Convert.ToDecimal(model.sim_rep_reason_id.Trim());
                order.payment_type = model.payment_type;
                order.is_paired = model.is_paired.Equals(null) ? null : model.is_paired;
                order.cahnnel_id = model.channel_id.Equals(null) ? null : model.channel_id;

                order.division_name = String.IsNullOrEmpty(model.division_name) ? model.division_name : ConverterHelper.UpperLowerWithSpaceConverter(model.division_name);
                order.district_name = String.IsNullOrEmpty(model.district_name) ? model.district_name : ConverterHelper.UpperLowerWithSpaceConverter(model.district_name);
                order.thana_name = String.IsNullOrEmpty(model.thana_name) ? model.thana_name : ConverterHelper.UpperLowerWithSpaceConverter(model.thana_name);

                //here distributor code is mapped with "P_CENTER_OR_DISTRIBUTOR_CODE" in DB. 
                order.distributor_code = String.IsNullOrEmpty(model.distributor_code) ? _bllCommon.GetDistributorCodeFromSessionToken(model.session_token) : model.distributor_code;
                order.sim_replc_reason = model.sim_replc_reason;
                order.channel_name = model.channel_name;
                order.right_id = model.right_id.Equals(null) ? null : model.right_id;

                order.sim_replacement_type = model.sim_replacement_type.Equals(null) ? null : model.sim_replacement_type;

                order.old_sim_number = String.IsNullOrEmpty(model.old_sim_number) ? null : model.old_sim_number;

                order.src_sim_category = model.src_sim_category;
                order.port_in_confirmation_code = model.port_in_confirmation_code;

                order.dest_ec_verifi_reqrd = !String.IsNullOrEmpty(model.dest_nid) ? 1 : 0;// 

                //If any trunsection porpuse have done by OTP, then the bio verification for src customer is not needed.
                //For example: For "two party EC verification" if the purpose is "B2B to B2C" with OTP, then the value "src_ec_verifi_reqrd" will 0.    
                //Here reseller app is sending OTP in "optional3" for "Corporate To Individual Transfer with OTP" [03 MAY 2020]
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.CorporateToIndividualTransfer
                    && !String.IsNullOrEmpty(model.otp))
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;
                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2CTwoPaertyValidationOTPNote' key may be missing within appSettings file.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                else
                {
                    order.src_ec_verifi_reqrd = !String.IsNullOrEmpty(model.src_nid) ? 1 : 0;
                }

                order.dest_foreign_flag = 0;
                order.dbss_subscription_id = model.dbss_subscription_id.Equals(null) ? null : model.dbss_subscription_id;
                order.customer_id = model.customer_id;
                order.order_confirmation_code = model.order_confirmation_code;
                order.server_name = Environment.MachineName;

                //This commented out if condition is being commented out because 
                //at this moment customer update of SIM replacement is not working. After some days this customer 
                //update feature will work again. Now by deafult this value assigned 0.
                //if (model.saf_status.HasValue)
                //{
                //    order.saf_status = model.saf_status == false ? 1 : 0;
                //}
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                            String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    order.saf_status = 1;
                }
                else
                {
                    order.saf_status = 0;
                }

                order.src_owner_customer_id = model.src_owner_customer_id;
                order.src_user_customer_id = model.src_user_customer_id;
                order.src_payer_customer_id = model.src_payer_customer_id;
                order.dest_imsi = model.dest_imsi;

                return order;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<OrderRequest2> SubmitOrderDataPurseV2(RAOrderRequestV2 model)// Converted FP in byte[].
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            decimal tokenId = 0;
            try
            {
                if (model.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                }
                
                OrderRequest2 order = new OrderRequest2();
                BL_Json blJson = new BL_Json();

                order.bi_token_number = model.bi_token_number == null ? 0 : model.bi_token_number;
                order.purpose_number = String.IsNullOrEmpty(model.purpose_number) ? null : Convert.ToDecimal(model.purpose_number.Trim());
                order.msisdn = model.msisdn;

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_category = model.src_sim_category;
                else
                    order.sim_category = model.sim_category;


                if (!String.IsNullOrEmpty(model.sim_number))
                {
                    if (model.sim_number.Substring(0, 6) != FixedValueCollection.SIMCode)
                    {
                        model.sim_number = FixedValueCollection.SIMCode + model.sim_number;
                    }
                }

                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                      String.IsNullOrEmpty(model.poc_msisdn_number))
                    order.sim_number = model.old_sim_number != null ? model.old_sim_number.Trim() : "";
                else if (!String.IsNullOrEmpty(model.sim_number))
                    order.sim_number = model.sim_number.Trim();
                else
                    order.sim_number = model.sim_number;
                order.subscription_type_id = String.IsNullOrEmpty(model.subscription_type_id) ? null : Convert.ToDecimal(model.subscription_type_id.Trim());
                order.subscription_code = model.subscription_code != null ? model.subscription_code : "";
                order.package_id = String.IsNullOrEmpty(model.package_id) ? null : Convert.ToDecimal(model.package_id.Trim());
                order.package_code = model.package_code != null ? model.package_code : "";
                if (!String.IsNullOrEmpty(model.dest_nid) && !String.IsNullOrEmpty(model.dest_dob))
                {
                    order.dest_nid = (model.dest_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.dest_dob).Year + model.dest_nid : model.dest_nid;
                }

                if (!String.IsNullOrEmpty(model.src_nid) && !String.IsNullOrEmpty(model.src_dob))
                {
                    order.src_nid = (model.src_nid.Length == (int)NIDLength.Length_13) ? DateTime.Parse(model.src_dob).Year + model.src_nid : model.src_nid;
                }

                order.dest_dob = String.IsNullOrEmpty(model.dest_dob) ? String.Empty : DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.dest_nid))
                {
                    order.dest_doc_type_no = model.dest_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;
                }

                order.src_dob = String.IsNullOrEmpty(model.src_dob) ? String.Empty : DateTime.Parse(model.src_dob).ToString(StringFormatCollection.DBSSDOBFormat);

                if (!String.IsNullOrEmpty(model.src_nid))
                {
                    order.src_doc_type_no = model.src_nid.Length == (int)NIDLength.Length_10 ? (int)DOCTypeIDNoNIDLengthWise.NID_Length_10 : (int)DOCTypeIDNoNIDLengthWise.NID_Length_Others;//(int)DOCTypeNo.nid;//1 for nid. /*model.src_doc_type_no;*/
                }
                else
                {
                    order.src_doc_type_no = null;
                }
                order.platform_id = model.platform_id != null ? model.platform_id : "";

                if (!String.IsNullOrEmpty(model.customer_name) &&
                        isCustomerNameValid(model.customer_name))
                {
                    order.customer_name = model.customer_name;
                }

                if (!String.IsNullOrEmpty(model.gender))
                {
                    order.gender = model.gender.ToLower() == "others" ? "notitle" : model.gender.ToLower();
                }
                order.status = 20;

                order.flat_number = model.flat_number != null ? model.flat_number : "";
                order.house_number = model.house_number != null ? model.house_number : "";
                order.road_number = model.road_number != null ? model.road_number : "";
                order.village = model.village;
                order.division_id = model.division_id.Equals(null) ? null : model.division_id;
                order.district_id = model.district_id.Equals(null) ? null : model.district_id;
                order.thana_id = model.thana_id.Equals(null) ? null : model.thana_id;
                order.postal_code = String.IsNullOrEmpty(model.postal_code) ? "0" : model.postal_code;

                //=====temp modelfication (as RA apk is unable to update [15-12-19])============
                if (!String.IsNullOrEmpty(model.email))
                {
                    order.email = isEmailValid(model.email.ToLower()) == true ? model.email : String.Empty;
                }
                else
                {
                    order.email = String.Empty;
                }
                //===================

                order.retailer_id = model.retailer_id;
                //note: For retailer App dest_doc_id and src_doc_id is dest_nid and src_nid namely. 
                order.dest_left_thumb_score = model.dest_left_thumb_score.Equals(null) ? (decimal?)null : model.dest_left_thumb_score;
                order.dest_left_thumb = String.IsNullOrEmpty(model.dest_left_thumb) ? Convert.FromBase64String("") : Convert.FromBase64String(model.dest_left_thumb.Trim());
                order.dest_left_index_score = model.dest_left_index_score.Equals(null) ? (decimal?)null : model.dest_left_index_score;
                order.dest_left_index = String.IsNullOrEmpty(model.dest_left_index) ? Convert.FromBase64String("") : Convert.FromBase64String(model.dest_left_index.Trim());
                order.dest_right_thumb_score = model.dest_right_thumb_score.Equals(null) ? (decimal?)null : model.dest_right_thumb_score;
                order.dest_right_index = String.IsNullOrEmpty(model.dest_right_index) ? Convert.FromBase64String("") : Convert.FromBase64String(model.dest_right_index.Trim());
                order.dest_right_index_score = model.dest_right_index_score.Equals(null) ? (decimal?)null : model.dest_right_index_score;
                order.dest_right_thumb = String.IsNullOrEmpty(model.dest_right_thumb) ? Convert.FromBase64String("") : Convert.FromBase64String(model.dest_right_thumb.Trim());
                order.src_left_thumb_score = model.src_left_thumb_score.Equals(null) ? (decimal?)null : model.src_left_thumb_score;
                order.src_left_thumb = String.IsNullOrEmpty(model.src_left_thumb) ? Convert.FromBase64String("") : Convert.FromBase64String(model.src_left_thumb.Trim());
                order.src_left_index_score = model.src_left_index_score.Equals(null) ? (decimal?)null : model.src_left_index_score;
                order.src_left_index = String.IsNullOrEmpty(model.src_left_index) ? Convert.FromBase64String("") : Convert.FromBase64String(model.src_left_index.Trim());
                order.src_right_thumb_score = model.src_right_thumb_score.Equals(null) ? (decimal?)null : model.src_right_thumb_score;
                order.src_right_thumb = String.IsNullOrEmpty(model.src_right_thumb) ? Convert.FromBase64String("") : Convert.FromBase64String(model.src_right_thumb.Trim());
                order.src_right_index_score = model.src_right_index_score.Equals(null) ? (decimal?)null : model.src_right_index_score;
                order.src_right_index = String.IsNullOrEmpty(model.src_right_index) ? Convert.FromBase64String("") : Convert.FromBase64String(model.src_right_index.Trim());
                order.retailer_code = (model.channel_name == FixedValueCollection.ResellerChannel
                                                            || model.channel_name == FixedValueCollection.CorporateChannel
                                                            || model.channel_name == FixedValueCollection.SMEChannel)
                                                            ? FixedValueCollection.ResellerCodeText + model.retailer_id : model.retailer_id;
                order.port_in_date = String.IsNullOrEmpty(model.port_in_date) ? null : DateTime.Parse(model.port_in_date);
                order.alt_msisdn = model.alt_msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    if (model.poc_msisdn_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                    {
                        order.poc_number = FixedValueCollection.MSISDNCountryCode + model.poc_msisdn_number;
                    }
                    else
                    {
                        order.poc_number = model.poc_msisdn_number;
                    }
                }
                order.is_urgent = model.is_urgent.Equals(null) ? null : model.is_urgent;
                order.optional1 = model.optional1;
                order.optional2 = model.optional2;
                order.optional3 = model.otp; // For any kind of activation is processed through OTP then OTP will be insert in option3.
                order.optional4 = String.IsNullOrEmpty(model.optional4) ? null : Convert.ToDecimal(model.optional4.Trim());
                order.optional5 = String.IsNullOrEmpty(model.optional5) ? null : Convert.ToDecimal(model.optional5.Trim());
                order.optional6 = String.IsNullOrEmpty(model.optional6) ? null : Convert.ToDecimal(model.optional6.Trim());
                order.note = model.note;
                order.sim_rep_reason_id = String.IsNullOrEmpty(model.sim_rep_reason_id) ? null : Convert.ToDecimal(model.sim_rep_reason_id.Trim());
                order.payment_type = model.payment_type;
                order.is_paired = model.is_paired.Equals(null) ? null : model.is_paired;
                order.cahnnel_id = model.channel_id.Equals(null) ? null : model.channel_id;

                order.division_name = String.IsNullOrEmpty(model.division_name) ? model.division_name : ConverterHelper.UpperLowerWithSpaceConverter(model.division_name);
                order.district_name = String.IsNullOrEmpty(model.district_name) ? model.district_name : ConverterHelper.UpperLowerWithSpaceConverter(model.district_name);
                order.thana_name = String.IsNullOrEmpty(model.thana_name) ? model.thana_name : ConverterHelper.UpperLowerWithSpaceConverter(model.thana_name);

                //here distributor code is mapped with "P_CENTER_OR_DISTRIBUTOR_CODE" in DB. 
                order.distributor_code = String.IsNullOrEmpty(model.distributor_code) ? await _bllCommon.GetDistributorCodeFromSessionTokenV2(model.session_token, model.retailer_id) : model.distributor_code;
                order.sim_replc_reason = model.sim_replc_reason;
                order.channel_name = model.channel_name;
                order.right_id = model.right_id.Equals(null) ? null : model.right_id;

                order.sim_replacement_type = model.sim_replacement_type.Equals(null) ? null : model.sim_replacement_type;

                order.old_sim_number = String.IsNullOrEmpty(model.old_sim_number) ? null : model.old_sim_number;

                order.src_sim_category = model.src_sim_category;
                order.port_in_confirmation_code = model.port_in_confirmation_code;

                order.dest_ec_verifi_reqrd = !String.IsNullOrEmpty(model.dest_nid) ? 1 : 0;// 

                //If any trunsection porpuse have done by OTP, then the bio verification for src customer is not needed.
                //For example: For "two party EC verification" if the purpose is "B2B to B2C" with OTP, then the value "src_ec_verifi_reqrd" will 0.    
                //Here reseller app is sending OTP in "optional3" for "Corporate To Individual Transfer with OTP" [03 MAY 2020]
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.CorporateToIndividualTransfer
                    && !String.IsNullOrEmpty(model.otp))
                {
                    order.src_ec_verifi_reqrd = 0;
                    //If src customer is validated by OTP, then we keep a note in DB column.
                    try
                    {
                        try
                        {
                            order.note = _configuration.GetSection("AppSettings:B2BtoB2CTwoPaertyValidationOTPNote").Value;

                        }
                        catch (Exception) { throw new Exception("Key not found in appsettings"); }
                    }
                    catch (NullReferenceException)
                    {
                        throw new Exception("'B2BtoB2CTwoPaertyValidationOTPNote' key may be missing within appSettings in Web.Config file.");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                }
                else
                {
                    order.src_ec_verifi_reqrd = !String.IsNullOrEmpty(model.src_nid) ? 1 : 0;
                }

                order.dest_foreign_flag = 0;
                order.dbss_subscription_id = model.dbss_subscription_id.Equals(null) ? null : model.dbss_subscription_id;
                order.customer_id = model.customer_id;
                order.order_confirmation_code = model.order_confirmation_code;
                order.server_name = Environment.MachineName;

                //This commented out if condition is being commented out because 
                //at this moment customer update of SIM replacement is not working. After some days this customer 
                //update feature will work again. Now by deafult this value assigned 0.
                //if (model.saf_status.HasValue)
                //{
                //    order.saf_status = model.saf_status == false ? 1 : 0;
                //}
                if (Convert.ToInt16(model.purpose_number) == (int)EnumPurposeNumber.SIMTransfer &&
                            String.IsNullOrEmpty(model.poc_msisdn_number))
                {
                    order.saf_status = 1;
                }
                else
                {
                    order.saf_status = 0;
                }

                order.src_owner_customer_id = model.src_owner_customer_id;
                order.src_user_customer_id = model.src_user_customer_id;
                order.src_payer_customer_id = model.src_payer_customer_id;
                order.dest_imsi = model.dest_imsi;

                return order;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<SendOrderResponse2> UpdateOrder(RAOrderRequestUpdate model)// Converted FP in byte[].
        {
            SendOrderResponse2 orderRes = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BL_Json _blJson = new BL_Json();
            decimal tokenId = 0;
            try
            {
                log.req_blob = _blJson.GetGenericJsonData(model);
                OrderRequest3 order = new OrderRequest3();
                order.bi_token_number = model.bi_token_number == null ? 0 : model.bi_token_number;
                order.dest_imsi = model.dest_imsi;
                order.status = model.status;
                order.bss_reqId = model.bss_reqId;
                order.error_id = model.error_id;
                order.error_description = model.err_msg;
                order.retailer_code = model.user_name;

                log.bi_token_number = model.bi_token_number != null ? model.bi_token_number.ToString() : "0";

                log.dbss_request_id = model.bss_reqId ?? "";

                ///if (model != null)
                if (!String.IsNullOrEmpty(model.msisdnReservationId))
                {
                    order.msisdnReservationId = model.msisdnReservationId;
                }

                //dataManager = new DALBiometricRepo();

                tokenId = await dataManager.UpdateOrder(order);

                //=========Makeing Response Decission=========
                if (tokenId > 0)
                {
                    orderRes.request_id = tokenId.ToString();
                    orderRes.is_success = true;
                    orderRes.message = MessageCollection.OrderUpdateSuccessfull;
                }
                else
                {
                    tokenId = await dataManager.UpdateOrder(order);

                    var json = JsonConvert.SerializeObject(model);

                    if (tokenId > 0)
                    {
                        orderRes.request_id = tokenId.ToString();
                        orderRes.is_success = true;
                        orderRes.message = MessageCollection.OrderUpdateSuccessfull;
                    }
                    else
                    {
                        string? text = Convert.ToString(new
                        {
                            retailer_id = "",
                            request_time = DateTime.Now,
                            request_model = Convert.ToString(json),
                            method_name = "UpdateOrder",
                            procedure_name = "UPDATEORDER",
                            error_source = "BIA",
                            error_code = "",
                            error_description = MessageCollection.OrderUpdateFaild,
                            server_name = ""
                        });
                        _logWriter.WriteDailyLog2(text == null ? "" : text);

                        orderRes.request_id = "0";
                        orderRes.is_success = false;
                        orderRes.message = MessageCollection.OrderUpdateFaild;
                    }
                }
                return orderRes;
            }
            catch (Exception ex)
            {
                ErrorDescription error = new ErrorDescription();
                error = await _bLLLog.ManageException(ex.Message, ex.HResult, "BIA");

                orderRes.request_id = "0";
                orderRes.is_success = false;
                orderRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                var json2 = JsonConvert.SerializeObject(model);

                string? text2 = Convert.ToString(new
                {
                    retailer_id = "",
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(json2),
                    method_name = "UpdateOrder",
                    procedure_name = "UPDATEORDER",
                    error_source = "BIA",
                    error_code = "",
                    error_description = ex.Message.ToString(),
                    server_name = ""
                });
                _logWriter.WriteDailyLog2(text2 == null ? "" : text2);

                return orderRes;
            }
            finally
            {
                log.user_id = model?.user_name ?? "";
                log.msisdn = model?.msidn ?? "";
                log.req_time = DateTime.Now;
                log.bi_token_number = orderRes.request_id;
                log.res_time = DateTime.Now;
                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.method_name = "UpdateOrder";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.remarks = "Order updated";

                await _bLLLog.RAToDBSSLog(log, "", "");

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, "", ""));
                //logThread.Start();
            }
        }

        private bool isCustomerNameValid(string name)
        {
            bool result = false;

            if (name.Length < 2)
                throw new Exception("Customer name must have minimum 2 characters.");
            else if (name.Length >= 2 && name.Trim().Length < 2)
                throw new Exception("Invalid customer name! Customer name must have minimum 2 characters.");

            result = true;
            return result;
        }

        #region is email valid

        private bool isEmailValid(string email)
        {
            try
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
        #endregion


        public async Task<GetStatusResponse> GetStatus(StatusRequest model)
        {
            try
            {
                GetStatusResponse statusObj = new GetStatusResponse();

                var dataRow = await dataManager.GetStatus(model);

                int statusCode = 0;
                if (dataRow.Rows.Count > 0)
                {
                    statusCode = Convert.ToInt32(dataRow.Rows[0]["STATUS"] == DBNull.Value ? null : dataRow.Rows[0]["STATUS"]);
                    statusObj.status = statusCode;
                    statusObj.status_name = Convert.ToString(dataRow.Rows[0]["STATUS_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["STATUS_NAME"]);
                    statusObj.msisdn = Convert.ToString(dataRow.Rows[0]["MSISDN"] == DBNull.Value ? null : dataRow.Rows[0]["MSISDN"]);
                    statusObj.result = true;
                    statusObj.message = MessageCollection.Success;
                    //order submission finish status.
                    statusObj.is_finished = statusCode == 50 || statusCode == 150 ? true : false;


                }
                else
                {
                    statusObj.result = false;
                    statusObj.message = MessageCollection.NoDataFound;
                }
                return statusObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<GetStatusResponseRevamp> GetStatusV2(StatusRequest model)
        {
            try
            {
                GetStatusResponseRevamp statusObj = new GetStatusResponseRevamp();
                GetStatusResponseDataRevamp data = new GetStatusResponseDataRevamp();

                var dataRow = await dataManager.GetStatus(model);

                int statusCode = 0;
                if (dataRow.Rows.Count > 0)
                {
                    statusCode = Convert.ToInt32(dataRow.Rows[0]["STATUS"] == DBNull.Value ? null : dataRow.Rows[0]["STATUS"]);
                    data.status = statusCode;
                    data.status_name = Convert.ToString(dataRow.Rows[0]["STATUS_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["STATUS_NAME"]);
                    data.msisdn = Convert.ToString(dataRow.Rows[0]["MSISDN"] == DBNull.Value ? null : dataRow.Rows[0]["MSISDN"]);
                    data.is_finished = statusCode == 50 || statusCode == 150 ? true : false;
                    statusObj.isError = false;
                    statusObj.message = MessageCollection.Success;
                    statusObj.data = data;
                }
                else
                {
                    statusObj.isError = true;
                    statusObj.message = MessageCollection.NoDataFound;
                }
                return statusObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Save FP base64 data in a file and save the path in DB.
        /// </summary>
        /// <param name="dest_doc_id"></param>
        /// <param name="dest_left_thumb"></param>
        /// <param name="msisdn"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetWsqFilePath(string dest_doc_id, string dest_left_thumb, string msisdn, string fileName)
        {
            string FilePath = string.Empty;
            try
            {
                byte[] allbites;
                try
                {
                    allbites = Convert.FromBase64String(dest_left_thumb);
                }
                catch (Exception ex)
                {
                    throw new Exception(fileName + " Fingerprint is in not Valid Base64 Format.");
                }
                string basePath = String.Empty;

                try
                {
                    basePath = _configuration.GetSection("AppSettings:FingerpritnFilePath").Value;

                }
                catch (Exception) { throw new Exception("Key not found in appsettings"); }



                string FolderName = string.Format("{0:yyyyMMdd}", DateTime.Now);
                var path = Path.Combine(basePath + FolderName + "/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string timeStamp = string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);

                FilePath = path + dest_doc_id + "_" + msisdn + "_" + timeStamp + "_" + fileName + ".wsq";

                File.WriteAllBytes(FilePath, allbites);
            }
            catch (Exception ex)
            {
                //ErrorDescription error = ManageException(ex.Message, ex.HResult, "BA");
                //BLLLog bllLog = new BLLLog();
                //BiometricAdapterLog baLog = new BiometricAdapterLog();
                //baLog.method_name = "GetWsqFilePath";
                //baLog.error_id = error.error_id;
                //baLog.error_source = "GetWsqFilePath Method";
                //baLog.message = error.error_description;
                //baLog.is_success = 0;
                //baLog.msisdn = msisdn;
                //baLog.res_time = DateTime.Now;
                //bllLog.BALogInsert(baLog);
            }

            return FilePath;
        }


        public async Task<RACommonResponse> CheckBIAToken(string token_id)
        {
            long result = 0;
            RACommonResponse response = null;
            try
            {
                result = await dataManager.CheckBIAToken(token_id);
                if (result < 1)
                {
                    return response = new RACommonResponse
                    {
                        result = false,
                        message = "No token id found!"
                    };
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response = new RACommonResponse
            {
                result = true,
                message = "Token id found."
            };
        }

        public async Task<OrderInfoResponse> GetOrderInfoByTokenNo(string token_id)
        {
            //OrderInfoResponse response = null;
            OrderInfoResponse oi = new OrderInfoResponse();
            try
            {
                //dataManager = new DALBiometricRepo();

                var dataRow = await dataManager.GetOrderInfoByTokenNo(Convert.ToDecimal(token_id));

                if (dataRow.Rows.Count > 0)
                {
                    string altMsisdn = Convert.ToString(dataRow.Rows[0]["ALT_MSISDN"] == DBNull.Value ? null : dataRow.Rows[0]["ALT_MSISDN"]);
                    if (!String.IsNullOrEmpty(altMsisdn))
                    {
                        oi.alt_msisdn = altMsisdn.Substring(0, 2) == "88" ? altMsisdn.Remove(0, 2) : altMsisdn;
                    }
                    else
                    {
                        oi.alt_msisdn = altMsisdn;
                    }
                    oi.sim_number = Convert.ToString(dataRow.Rows[0]["DEST_SIM_NUMBER"] == DBNull.Value ? "" : dataRow.Rows[0]["SIM_NUMBER"]).Remove(0, 6);
                    oi.village = Convert.ToString(dataRow.Rows[0]["VILLAGE"] == DBNull.Value ? null : dataRow.Rows[0]["VILLAGE"]);
                    oi.gender = Convert.ToString(dataRow.Rows[0]["GENDER"] == DBNull.Value ? null : dataRow.Rows[0]["GENDER"]);
                    oi.thana_id = Convert.ToInt32(dataRow.Rows[0]["THANA_ID"] == DBNull.Value ? null : dataRow.Rows[0]["THANA_ID"]);
                    oi.thana_name = Convert.ToString(dataRow.Rows[0]["THANA_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["THANA_NAME"]);
                    oi.road_number = Convert.ToString(dataRow.Rows[0]["ROAD_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["ROAD_NUMBER"]);
                    oi.flat_number = Convert.ToString(dataRow.Rows[0]["FLAT_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["FLAT_NUMBER"]);
                    oi.district_name = Convert.ToString(dataRow.Rows[0]["DISTRICT_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["DISTRICT_NAME"]);
                    oi.district_id = Convert.ToInt32(dataRow.Rows[0]["DISTRICT_ID"] == DBNull.Value ? null : dataRow.Rows[0]["DISTRICT_ID"]);
                    oi.customer_name = Convert.ToString(dataRow.Rows[0]["CUSTOMER_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["CUSTOMER_NAME"]);
                    oi.division_id = Convert.ToInt32(dataRow.Rows[0]["DIVISION_ID"] == DBNull.Value ? null : dataRow.Rows[0]["DIVISION_ID"]);
                    oi.division_name = Convert.ToString(dataRow.Rows[0]["DIVISION_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["DIVISION_NAME"]);
                    oi.house_number = Convert.ToString(dataRow.Rows[0]["HOUSE_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["HOUSE_NUMBER"]);
                    oi.email = Convert.ToString(dataRow.Rows[0]["EMAIL"] == DBNull.Value ? null : dataRow.Rows[0]["EMAIL"]);
                    oi.postal_code = Convert.ToString(dataRow.Rows[0]["POSTAL_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["POSTAL_CODE"]);
                    oi.subscription_code = Convert.ToString(dataRow.Rows[0]["SUBSCRIPTION_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["SUBSCRIPTION_CODE"]);
                    oi.subscription_type_id = Convert.ToString(dataRow.Rows[0]["SUBSCRIPTION_TYPE_ID"] == DBNull.Value ? null : dataRow.Rows[0]["SUBSCRIPTION_TYPE_ID"]);
                    oi.package_code = Convert.ToString(dataRow.Rows[0]["PACKAGE_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["PACKAGE_CODE"]);
                    oi.package_id = Convert.ToInt32(dataRow.Rows[0]["PACKAGE_ID"] == DBNull.Value ? null : dataRow.Rows[0]["PACKAGE_ID"]);
                    oi.is_urgent = Convert.ToInt32(dataRow.Rows[0]["IS_URGENT"] == DBNull.Value ? null : dataRow.Rows[0]["IS_URGENT"]);
                    oi.port_in_date = Convert.ToString(dataRow.Rows[0]["PORT_IN_DATE"] == DBNull.Value ? null : dataRow.Rows[0]["PORT_IN_DATE"]);

                    oi.result = true;
                    oi.message = "Data found.";
                    return oi;
                }
                else
                {
                    oi.result = false;
                    oi.message = "No data found!";
                    return oi;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<OrderInfoResponseDataRev> GetOrderInfoByTokenNoV2(string token_id)
        {
            //OrderInfoResponse response = null;
            OrderInfoResponseDataRev oi = new OrderInfoResponseDataRev();
            try
            {
                //dataManager = new DALBiometricRepo();

                var dataRow = await dataManager.GetOrderInfoByTokenNo(Convert.ToDecimal(token_id));

                if (dataRow.Rows.Count > 0)
                {
                    string altMsisdn = Convert.ToString(dataRow.Rows[0]["ALT_MSISDN"] == DBNull.Value ? null : dataRow.Rows[0]["ALT_MSISDN"]);
                    if (!String.IsNullOrEmpty(altMsisdn))
                    {
                        oi.data.alt_msisdn = altMsisdn.Substring(0, 2) == "88" ? altMsisdn.Remove(0, 2) : altMsisdn;
                    }
                    else
                    {
                        oi.data.alt_msisdn = altMsisdn;
                    }
                    oi.data.sim_number = Convert.ToString(dataRow.Rows[0]["DEST_SIM_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["SIM_NUMBER"]).Remove(0, 6);
                    oi.data.village = Convert.ToString(dataRow.Rows[0]["VILLAGE"] == DBNull.Value ? null : dataRow.Rows[0]["VILLAGE"]);
                    oi.data.gender = Convert.ToString(dataRow.Rows[0]["GENDER"] == DBNull.Value ? null : dataRow.Rows[0]["GENDER"]);
                    oi.data.thana_id = Convert.ToInt32(dataRow.Rows[0]["THANA_ID"] == DBNull.Value ? null : dataRow.Rows[0]["THANA_ID"]);
                    oi.data.thana_name = Convert.ToString(dataRow.Rows[0]["THANA_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["THANA_NAME"]);
                    oi.data.road_number = Convert.ToString(dataRow.Rows[0]["ROAD_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["ROAD_NUMBER"]);
                    oi.data.flat_number = Convert.ToString(dataRow.Rows[0]["FLAT_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["FLAT_NUMBER"]);
                    oi.data.district_name = Convert.ToString(dataRow.Rows[0]["DISTRICT_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["DISTRICT_NAME"]);
                    oi.data.district_id = Convert.ToInt32(dataRow.Rows[0]["DISTRICT_ID"] == DBNull.Value ? null : dataRow.Rows[0]["DISTRICT_ID"]);
                    oi.data.customer_name = Convert.ToString(dataRow.Rows[0]["CUSTOMER_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["CUSTOMER_NAME"]);
                    oi.data.division_id = Convert.ToInt32(dataRow.Rows[0]["DIVISION_ID"] == DBNull.Value ? null : dataRow.Rows[0]["DIVISION_ID"]);
                    oi.data.division_name = Convert.ToString(dataRow.Rows[0]["DIVISION_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["DIVISION_NAME"]);
                    oi.data.house_number = Convert.ToString(dataRow.Rows[0]["HOUSE_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["HOUSE_NUMBER"]);
                    oi.data.email = Convert.ToString(dataRow.Rows[0]["EMAIL"] == DBNull.Value ? null : dataRow.Rows[0]["EMAIL"]);
                    oi.data.postal_code = Convert.ToString(dataRow.Rows[0]["POSTAL_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["POSTAL_CODE"]);
                    oi.data.subscription_code = Convert.ToString(dataRow.Rows[0]["SUBSCRIPTION_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["SUBSCRIPTION_CODE"]);
                    oi.data.subscription_type_id = Convert.ToString(dataRow.Rows[0]["SUBSCRIPTION_TYPE_ID"] == DBNull.Value ? null : dataRow.Rows[0]["SUBSCRIPTION_TYPE_ID"]);
                    oi.data.package_code = Convert.ToString(dataRow.Rows[0]["PACKAGE_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["PACKAGE_CODE"]);
                    oi.data.package_id = Convert.ToInt32(dataRow.Rows[0]["PACKAGE_ID"] == DBNull.Value ? null : dataRow.Rows[0]["PACKAGE_ID"]);
                    oi.data.is_urgent = Convert.ToInt32(dataRow.Rows[0]["IS_URGENT"] == DBNull.Value ? null : dataRow.Rows[0]["IS_URGENT"]);
                    oi.data.port_in_date = Convert.ToString(dataRow.Rows[0]["PORT_IN_DATE"] == DBNull.Value ? null : dataRow.Rows[0]["PORT_IN_DATE"]);

                    oi.isError = false;
                    oi.message = "Data found.";
                    return oi;
                }
                else
                {
                    oi.isError = true;
                    oi.message = "No data found!";
                    return oi;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> GetPortInOrderConfirmCode(int purposeId, string msisdn)
        {
            string orderConfirmCode = "";
            try
            {
                //dataManager = new DALBiometricRepo();
                var dataRow = await dataManager.GetPortInOrderConfirmCode(purposeId, msisdn);
                if (dataRow.Rows.Count > 0)
                {
                    orderConfirmCode = Convert.ToString(dataRow.Rows[0]["CONFIRMATION_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["CONFIRMATION_CODE"]);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return orderConfirmCode;
        }

        public async Task<OrderConformationResponse> GetPortInOrderConfirmCode_v1(int purposeId, string msisdn)
        {
            string orderConfirmCode = "";
            OrderConformationResponse response = new OrderConformationResponse();
            try
            {
                //dataManager = new DALBiometricRepo();
                var dataRow = await dataManager.GetPortInOrderConfirmCode(purposeId, msisdn);

                if (dataRow.Rows.Count > 0)
                {
                    orderConfirmCode = Convert.ToString(dataRow.Rows[0]["CONFIRMATION_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["CONFIRMATION_CODE"]);
                }
                else
                {
                    response.order_conformation_code = null;
                    response.result = false;
                    response.message = "No port-in order found againest this msisdn that is eligable for cancle port-in!";
                    return response;
                }

                if (String.IsNullOrEmpty(orderConfirmCode))
                {
                    response.order_conformation_code = null;
                    response.result = false;
                    response.message = "Order conformation code not found!";
                    return response;
                }
                else
                {
                    response.order_conformation_code = orderConfirmCode;
                    response.result = true;
                    response.message = MessageCollection.Success;
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Checks if submitted order is in process or not.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ValidateOrderResponse> ValidateOrder(VMValidateOrder model)
        {
            ValidateOrderResponse resp = new ValidateOrderResponse();

            try
            {
                //#region moc test
                //model.msisdn = "01911010101";
                //model.sim_number = "981606508111";
                //#endregion

                resp = OrderRequestModelValidation(model);
                if (resp.result == false)
                {
                    return resp;
                }

                if (model.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                }

                if (!String.IsNullOrEmpty(model.sim_number) && model.sim_number.Substring(0, 6) != FixedValueCollection.SIMCode)
                {
                    model.sim_number = FixedValueCollection.SIMCode + model.sim_number;
                }

                var dataRow = await dataManager.ValidateOrder(model);

                if (dataRow.Rows.Count > 0)
                {
                    resp.result = Convert.ToBoolean(dataRow.Rows[0]["RESULT"] == DBNull.Value ? null : dataRow.Rows[0]["RESULT"]);
                    resp.message = Convert.ToString(dataRow.Rows[0]["MSG"] == DBNull.Value ? null : dataRow.Rows[0]["MSG"]);
                    resp.error_code = Convert.ToInt16(dataRow.Rows[0]["ERROR_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["ERROR_CODE"]);
                }

                return resp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private ValidateOrderResponse OrderRequestModelValidation(VMValidateOrder model)
        {
            ValidateOrderResponse resp = new ValidateOrderResponse();
            try
            {
                resp.result = false;
                if (String.IsNullOrEmpty(model.msisdn))
                {
                    resp.message = "'msisdn' is required!";
                }
                //if (String.IsNullOrEmpty(model.sim_number))
                //{
                //    resp.message = "'sim_number' is required!";
                //}
                else
                {
                    resp.result = true;
                    resp.message = "Validation successful!";
                }

                return resp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<Tuple<int, int>> GetInventoryIdByChannelName(string channelName)
        {
            int inventoryId = 0;
            int channelId = 0;
            var mocTestChannelName = "Reseller";

            try
            {

                var dataRow = await dataManager.GetInventoryIdByChannelName(channelName);

                if (dataRow.Rows.Count > 0)
                {
                    inventoryId = Convert.ToInt16(dataRow.Rows[0]["INVENTORY_ID"] == DBNull.Value ? null : dataRow.Rows[0]["INVENTORY_ID"]);
                    channelId = Convert.ToInt16(dataRow.Rows[0]["CHANNELID"] == DBNull.Value ? null : dataRow.Rows[0]["CHANNELID"]);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Tuple.Create(channelId, inventoryId);
        }

        public async Task<string> GetCenterCodeByUserName(string userlName)
        {
            string centerCode = "";

            try
            {
                //dataManager = new DALBiometricRepo();
                var dataRow = await dataManager.GetCenterCodeByUserName(userlName);

                if (dataRow.Rows.Count > 0)
                {
                    centerCode = Convert.ToString(dataRow.Rows[0]["CENTER_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["CENTER_CODE"]);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return centerCode;
        }
    }
}
