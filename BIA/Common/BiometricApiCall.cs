using BIA.BLL.BLLServices;
using BIA.BLL.Utility;
using BIA.Controllers;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.PopulateModel;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.Utility;
using Dahomey.Cbor.Serialization.Converters;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace BIA.Common
{
    public class BiometricApiCall
    {
        private readonly BL_Json _blJson;
        private readonly BLLLog _bllLog;
        private readonly BLLCommon _bllCommon;
        private readonly BllBiometricBssService _bllObj;
        private readonly BllHandleException _manageExecption;
        private readonly BLLDBSSToRAParse _dbssToRaParse;
        private readonly ApiRequest _apiReq;

        public BiometricApiCall(BL_Json blJson, BLLLog bllLog, BLLCommon bllCommon, BllBiometricBssService bllObj, BllHandleException manageExecption, BLLDBSSToRAParse dbssToRaParse, ApiRequest apiReq)
        {
            _blJson = blJson;
            _bllLog = bllLog;
            _bllCommon = bllCommon;
            _bllObj = bllObj;
            _manageExecption = manageExecption;
            _dbssToRaParse = dbssToRaParse;
            _apiReq = apiReq;
        }

        readonly IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        public static string singleSourceLoginSessionToken = null;
        public async Task<BioVerifyResp> BioVerificationReqToBss(BiomerticDataModel item, object reqModel, string meathodUrl)
        {
            LogModel log = new LogModel();
            object bioResponse = null;
            bool bioLogNeeded = false;
            bool isCdtError = false;
            log.status = item.status;
            BL_Json byteArrayConverter = new BL_Json();
            ApiCall genericApiCall = new ApiCall();
            BioVerifyResp verifyResp = new BioVerifyResp();
            // SingleSourceCheckResponseModel checkResponseModel = new SingleSourceCheckResponseModel();
            SingleSourceCheckResponseModel checkResponseModel = new SingleSourceCheckResponseModel();
            MSISDNReservationResponse reservationResponse = new MSISDNReservationResponse();
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;

            try
            {

                checkResponseModel = await SingleSourceCheckFromBioDB(item.msisdn, item.sim_number, item.purpose_number, item.poc_number, item.sim_replacement_type, item.dest_doc_id, item.dest_dob, item.dest_imsi);

                if (checkResponseModel.Status == 0)
                {
                    verifyResp.is_success = false;
                    verifyResp.err_msg = checkResponseModel.Message;
                    log.message = checkResponseModel.Message;
                    return verifyResp;
                }

                if (string.IsNullOrEmpty(item.poc_number) ||
                    (!string.IsNullOrEmpty(item.poc_number)
                        && item.purpose_number == (int)EnumPurposeNumber.SIMReplacement
                        && (item.sim_replacement_type == (int)EnumSIMReplacementType.ByPOC
                            || item.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                        )
                     )
                {

                    if (item.purpose_number == (int)EnumPurposeNumber.NewRegistration || item.purpose_number == (int)EnumPurposeNumber.MNPRegistration || item.purpose_number == (int)EnumPurposeNumber.MNPEmergencyReturn)
                    {
                        string CDTMessage = await CDT(item);
                        if (!string.IsNullOrEmpty(CDTMessage))
                        {
                            isCdtError = true;
                            verifyResp.err_msg = "DBSS: CDT Operation Fail.";
                            log.message = "DBSS: CDT Operation Fail.";
                            throw new Exception(CDTMessage);
                        }
                    }//######################## CDT Others ###########################
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMReplacement
                            || item.purpose_number == (int)EnumPurposeNumber.DeRegistration)
                    {
                        var res = GetPropertyValue(reqModel, "data.attributes.msisdn");
                        string msisdn = res.ToString();
                        bool isOtherCDTSuccess = await OtherCDT(item);
                        if (!isOtherCDTSuccess)
                        {
                            verifyResp.err_msg = "DBSS: Other CDT Operation Fail.";
                            log.message = "DBSS: Other CDT Operation Fail.";
                            throw new Exception("DBSS: Other CDT Operation Fail.");
                        }
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMTransfer)
                    {
                        RACommonResponse raResp = new RACommonResponse();
                        raResp = await OtherCDTForTOS(item);
                        if (raResp.result == false)
                        {
                            verifyResp.err_msg = raResp.message;
                            log.message = "DBSS: Other CDT Operation Fail.";
                            return verifyResp;
                            //throw new Exception("DBSS: Other CDT Operation for TOS Fail.");
                        }
                    }
                    if (item.is_paired != null)
                    {
                        if (item.is_paired == 0 && item.purpose_number == (int)EnumPurposeNumber.NewRegistration)
                        {
                            try
                            {
                                reservationResponse = await MSISDNReservation(item);
                                if (reservationResponse.IsReserve != true)
                                {
                                    log.message = reservationResponse.Error_message;
                                    verifyResp.err_msg = "DBSS: MSISDN Reservation Fail.";
                                    throw new Exception("DBSS: MSISDN Reservation Fail.");
                                }
                                else
                                {
                                    verifyResp.Reservation_Id = reservationResponse.Reservation_Id;
                                }
                            }
                            catch (Exception ex)
                            {
                                verifyResp.err_msg = ex.Message.ToString();
                                //verifyResp.is_success = false;
                                //return verifyResp;
                            }
                        }
                    }
                }
                bioLogNeeded = true;
                object reqModel_temp = reqModel;
                try
                {
                    string req_string = JsonConvert.SerializeObject(reqModel_temp);
                    JObject parsedObj = JObject.Parse(req_string);
                    if (item.dest_left_thumb != null)
                        parsedObj["data"]["attributes"]["dest_left_thumb"] = null;
                    if (item.dest_left_index != null)
                        parsedObj["data"]["attributes"]["dest_left_index"] = null;
                    if (item.dest_right_thumb != null)
                        parsedObj["data"]["attributes"]["dest_right_thumb"] = null;
                    if (item.dest_right_index != null)
                        parsedObj["data"]["attributes"]["dest_right_index"] = null;
                    if (item.src_left_index != null)
                        parsedObj["data"]["attributes"]["src_left_thumb"] = null;
                    if (item.src_left_thumb != null)
                        parsedObj["data"]["attributes"]["src_left_index"] = null;
                    if (item.src_right_index != null)
                        parsedObj["data"]["attributes"]["src_right_thumb"] = null;
                    if (item.src_right_thumb != null)
                        parsedObj["data"]["attributes"]["src_right_index"] = null;

                    log.req_string = parsedObj.ToString();
                    log.req_blob = byteArrayConverter.GetGenericJsonData(parsedObj.ToString());
                }
                catch (Exception ex)
                { throw new Exception("Error Occurred in FP Set to Null in Log time."); }

                try
                {
                    log.req_time = DateTime.Now;
                    bioResponse = await genericApiCall.HttpPostRequest(reqModel, meathodUrl);
                    log.res_time = DateTime.Now;
                }
                catch (Exception ex)
                {
                    log.message = "DBSS: Bio Req-" + ex.Message.ToString();
                    verifyResp.err_msg = "DBSS: Bio Req-" + ex.Message.ToString();
                    throw new Exception("DBSS: Bio Req-" + ex.Message);
                }

                log.res_string = JsonConvert.SerializeObject(bioResponse.ToString()).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(bioResponse);

                BioResModel? response = null;
                try
                {
                    response = JsonConvert.DeserializeObject<BioResModel>(bioResponse.ToString());
                }
                catch (Exception ex)
                {
                    verifyResp.err_msg = "DBSS: Biometric Api Response Parsing Error.";
                    throw new Exception("DBSS: Biometric Api Response Parsing Error.");
                }

                if (response == null)
                {
                    verifyResp.err_msg = "DBSS: Invalid Biometric Api Response.";
                    throw new Exception("DBSS: Invalid Biometric Api Response.");
                }
                else if (response.data == null)
                {
                    verifyResp.err_msg = "DBSS: Invalid Biometric Api Response.";
                    throw new Exception("DBSS: Invalid Biometric Api Response.");
                }
                else if (response.data.request_id == null)
                {
                    verifyResp.err_msg = "DBSS: Invalid Biometric Api Response.";
                    throw new Exception("DBSS: Invalid Biometric Api Response.");
                }
                // this  is successfull case and bss give us failled response
                item.bss_request_id = response.data.request_id;
                log.is_success = 1;
                verifyResp.is_success = true;
                verifyResp.bss_req_id = response.data.request_id;
                item.bss_request_id = verifyResp.bss_req_id;

                return verifyResp;
            }
            catch (Exception ex)
            {
                if (bioLogNeeded || isCdtError)
                {
                    ErrorDescription error = new ErrorDescription();
                    try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "DBSS Service"); }
                    catch { }
                    log.req_time = reqTime;
                    log.res_time = resTime;
                    log.res_string = JsonConvert.SerializeObject(bioResponse != null ? bioResponse.ToString() : ex.Message).ToString();
                    log.res_blob = byteArrayConverter.GetGenericJsonData(bioResponse != null ? bioResponse.ToString() : ex.Message);
                    log.message = error != null ? error.error_description : null;
                    log.error_code = error != null ? error.error_code : null;
                    log.error_source = error != null ? error.error_source : "DBSS Service";
                    log.is_success = 0;
                    // BIRequset Table Update Status 150 and Error id and description for biometric Failuer
                    item.status = 150;
                    item.error_id = error != null ? error.error_id : 0;
                    item.error_description = error != null ? error.error_description : ex.Message;
                    verifyResp.error_Id = item.error_id;
                    verifyResp.is_success = false;
                    verifyResp.err_code = error != null ? error.error_code : null;
                    verifyResp.err_msg = ex.Message.ToString();
                }
                return verifyResp;
            }
            finally
            {
                if (bioLogNeeded)
                {
                    log.bss_request_id = item.bss_request_id;
                    log.bi_token_number = item.bi_token_number;
                    log.msisdn = item.msisdn;
                    log.user_id = item.user_id;
                    log.integration_point_from = (int)IntegrationPoints.BI;
                    log.integration_point_to = (int)IntegrationPoints.BSS;
                    log.method_name = "BioVerificationReqToBss";
                    log.purpose_number = item.purpose_number.ToString();
                    await _bllLog.BALogInsert(log);
                }
            }
        }

        public async Task<BioVerifyResp> BioVerificationReqToBssV2(BiomerticDataModel item, object reqModel, string meathodUrl)
        {
            LogModel log = new LogModel();
            object bioResponse = null;
            bool bioLogNeeded = false;
            bool isCdtError = false;
            log.status = item.status;
            BL_Json byteArrayConverter = new BL_Json();
            ApiCall genericApiCall = new ApiCall();
            BioVerifyResp verifyResp = new BioVerifyResp();
            // SingleSourceCheckResponseModel checkResponseModel = new SingleSourceCheckResponseModel();
            SingleSourceCheckResponseModelRevamp checkResponseModel = new SingleSourceCheckResponseModelRevamp();
            MSISDNReservationResponse reservationResponse = new MSISDNReservationResponse();
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;

            try
            {
                if (item.purpose_number == (int)EnumPurposeNumber.NewRegistration && String.IsNullOrEmpty(item.poc_number))
                {
                    checkResponseModel = await SingleSourceCheckThroughAPI(item.msisdn, item.user_id);

                    if (checkResponseModel.Status == true)
                    {
                        verifyResp.is_success = false;
                        verifyResp.err_msg = checkResponseModel.Message;
                        log.message = checkResponseModel.Message;
                        return verifyResp;
                    }
                }

                if (string.IsNullOrEmpty(item.poc_number) ||
                    (!string.IsNullOrEmpty(item.poc_number)
                        && item.purpose_number == (int)EnumPurposeNumber.SIMReplacement
                        && (item.sim_replacement_type == (int)EnumSIMReplacementType.ByPOC
                            || item.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                        )
                     )
                {

                    if (item.purpose_number == (int)EnumPurposeNumber.NewRegistration || item.purpose_number == (int)EnumPurposeNumber.MNPRegistration || item.purpose_number == (int)EnumPurposeNumber.MNPEmergencyReturn)
                    {
                        string CDTMessage = await CDT(item);
                        if (!string.IsNullOrEmpty(CDTMessage))
                        {
                            isCdtError = true;
                            verifyResp.err_msg = "DBSS: CDT Operation Fail.";
                            log.message = "DBSS: CDT Operation Fail.";
                            throw new Exception(CDTMessage);
                        }
                    }//######################## CDT Others ###########################
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMReplacement
                            || item.purpose_number == (int)EnumPurposeNumber.DeRegistration)
                    {
                        var res = GetPropertyValue(reqModel, "data.attributes.msisdn");
                        string msisdn = res.ToString();
                        bool isOtherCDTSuccess = await OtherCDT(item);
                        if (!isOtherCDTSuccess)
                        {
                            verifyResp.err_msg = "DBSS: Other CDT Operation Fail.";
                            log.message = "DBSS: Other CDT Operation Fail.";
                            throw new Exception("DBSS: Other CDT Operation Fail.");
                        }
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMTransfer)
                    {
                        RACommonResponse raResp = new RACommonResponse();
                        raResp = await OtherCDTForTOS(item);
                        if (raResp.result == false)
                        {
                            verifyResp.err_msg = raResp.message;
                            log.message = "DBSS: Other CDT Operation Fail.";
                            return verifyResp;
                            //throw new Exception("DBSS: Other CDT Operation for TOS Fail.");
                        }
                    }
                    if (item.is_paired != null)
                    {
                        if (item.is_paired == 0 && item.purpose_number == (int)EnumPurposeNumber.NewRegistration)
                        {
                            try
                            {
                                reservationResponse = await MSISDNReservation(item);
                                if (reservationResponse.IsReserve != true)
                                {
                                    log.message = reservationResponse.Error_message;
                                    verifyResp.err_msg = "DBSS: MSISDN Reservation Fail.";
                                    throw new Exception("DBSS: MSISDN Reservation Fail.");
                                }
                                else
                                {
                                    verifyResp.Reservation_Id = reservationResponse.Reservation_Id;
                                }
                            }
                            catch (Exception ex)
                            {
                                verifyResp.err_msg = ex.Message.ToString();
                                //verifyResp.is_success = false;
                                //return verifyResp;
                            }
                        }
                    }
                }
                bioLogNeeded = true;
                object reqModel_temp = reqModel;
                try
                {
                    string req_string = JsonConvert.SerializeObject(reqModel_temp);
                    JObject parsedObj = JObject.Parse(req_string);

                    if (parsedObj != null)
                    {
                        if (item.dest_left_thumb != null)
                            parsedObj["data"]["attributes"]["dest_left_thumb"] = null;
                        if (item.dest_left_index != null)
                            parsedObj["data"]["attributes"]["dest_left_index"] = null;
                        if (item.dest_right_thumb != null)
                            parsedObj["data"]["attributes"]["dest_right_thumb"] = null;
                        if (item.dest_right_index != null)
                            parsedObj["data"]["attributes"]["dest_right_index"] = null;
                        if (item.src_left_index != null)
                            parsedObj["data"]["attributes"]["src_left_thumb"] = null;
                        if (item.src_left_thumb != null)
                            parsedObj["data"]["attributes"]["src_left_index"] = null;
                        if (item.src_right_index != null)
                            parsedObj["data"]["attributes"]["src_right_thumb"] = null;
                        if (item.src_right_thumb != null)
                            parsedObj["data"]["attributes"]["src_right_index"] = null;
                    }                    

                    log.req_string = parsedObj.ToString();
                    log.req_blob = byteArrayConverter.GetGenericJsonData(parsedObj.ToString());
                }
                catch (Exception ex)
                { throw new Exception("Error Occurred in FP Set to Null in Log time."); }

                try
                {
                    log.req_time = DateTime.Now;
                    bioResponse = await genericApiCall.HttpPostRequest(reqModel, meathodUrl);
                    log.res_time = DateTime.Now;
                }
                catch (Exception ex)
                {
                    log.message = "DBSS: Bio Req-" + ex.Message.ToString();
                    verifyResp.err_msg = "DBSS: Bio Req-" + ex.Message.ToString();
                    throw new Exception("DBSS: Bio Req-" + ex.Message);
                }

                log.res_string = JsonConvert.SerializeObject(bioResponse.ToString()).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(bioResponse);

                BioResModel? response = null;
                try
                {
                    response = JsonConvert.DeserializeObject<BioResModel>(bioResponse.ToString());
                }
                catch (Exception ex)
                {
                    verifyResp.err_msg = "DBSS: Biometric Api Response Parsing Error.";
                    throw new Exception("DBSS: Biometric Api Response Parsing Error.");
                }

                if (response == null)
                {
                    verifyResp.err_msg = "DBSS: Invalid Biometric Api Response.";
                    throw new Exception("DBSS: Invalid Biometric Api Response.");
                }
                else if (response.data == null)
                {
                    verifyResp.err_msg = "DBSS: Invalid Biometric Api Response.";
                    throw new Exception("DBSS: Invalid Biometric Api Response.");
                }
                else if (response.data.request_id == null)
                {
                    verifyResp.err_msg = "DBSS: Invalid Biometric Api Response.";
                    throw new Exception("DBSS: Invalid Biometric Api Response.");
                }
                // this  is successfull case and bss give us failled response
                item.bss_request_id = response.data.request_id;
                log.is_success = 1;
                verifyResp.is_success = true;
                verifyResp.bss_req_id = response.data.request_id;
                item.bss_request_id = verifyResp.bss_req_id;

                return verifyResp;
            }
            catch (Exception ex)
            {
                verifyResp.err_msg = ex.Message.ToString();
                if (bioLogNeeded || isCdtError)
                {
                    ErrorDescription error = new ErrorDescription();
                    try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "DBSS Service"); }
                    catch { }
                    log.req_time = reqTime;
                    log.res_time = resTime;
                    log.res_string = JsonConvert.SerializeObject(bioResponse != null ? bioResponse.ToString() : ex.Message).ToString();
                    log.res_blob = byteArrayConverter.GetGenericJsonData(bioResponse != null ? bioResponse.ToString() : ex.Message);
                    log.message = error != null ? error.error_description : null;
                    log.error_code = error != null ? error.error_code : null;
                    log.error_source = error != null ? error.error_source : "DBSS Service";
                    log.is_success = 0;
                    // BIRequset Table Update Status 150 and Error id and description for biometric Failuer
                    item.status = 150;
                    item.error_id = error != null ? error.error_id : 0;
                    item.error_description = error != null ? error.error_description : ex.Message;
                    verifyResp.error_Id = item.error_id;
                    verifyResp.is_success = false;
                    verifyResp.err_code = error != null ? error.error_code : null;
                    verifyResp.err_msg = ex.Message.ToString();
                }
                return verifyResp;
            }
            finally
            {
                if (bioLogNeeded)
                {
                    log.bss_request_id = item.bss_request_id;
                    log.bi_token_number = item.bi_token_number;
                    log.msisdn = item.msisdn;
                    log.user_id = item.user_id;
                    log.integration_point_from = (int)IntegrationPoints.BI;
                    log.integration_point_to = (int)IntegrationPoints.BSS;
                    log.method_name = "BioVerificationReqToBssV2";
                    log.purpose_number = item.purpose_number.ToString();
                    await _bllLog.BALogInsert(log);
                }
            }
        }

        public async Task<BioVerifyResp> BioVerificationReqToBssV3(BiomerticDataModel item, object reqModel, string meathodUrl)
        {
            LogModel log = new LogModel();
            object bioResponse = null;
            bool bioLogNeeded = false;
            bool isCdtError = false;
            log.status = item.status;
            BL_Json byteArrayConverter = new BL_Json();
            ApiCall genericApiCall = new ApiCall();
            BioVerifyResp verifyResp = new BioVerifyResp();
            // SingleSourceCheckResponseModel checkResponseModel = new SingleSourceCheckResponseModel();
            SingleSourceCheckResponseModelRevamp checkResponseModel = new SingleSourceCheckResponseModelRevamp();
            MSISDNReservationResponse reservationResponse = new MSISDNReservationResponse();
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;

            try
            {
                if (item.purpose_number == (int)EnumPurposeNumber.NewRegistration && String.IsNullOrEmpty(item.poc_number))
                {
                    checkResponseModel = await SingleSourceCheckThroughAPI(item.msisdn, item.user_id);

                    if (checkResponseModel.Status == true)
                    {
                        verifyResp.is_success = false;
                        verifyResp.err_msg = checkResponseModel.Message;
                        log.message = checkResponseModel.Message;
                        return verifyResp;
                    }
                }

                if (string.IsNullOrEmpty(item.poc_number) ||
                    (!string.IsNullOrEmpty(item.poc_number)
                        && item.purpose_number == (int)EnumPurposeNumber.SIMReplacement
                        && (item.sim_replacement_type == (int)EnumSIMReplacementType.ByPOC
                            || item.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                        )
                     )
                {

                    if (item.purpose_number == (int)EnumPurposeNumber.NewRegistration || item.purpose_number == (int)EnumPurposeNumber.MNPRegistration || item.purpose_number == (int)EnumPurposeNumber.MNPEmergencyReturn)
                    {
                        string CDTMessage = await CDT(item);
                        if (!string.IsNullOrEmpty(CDTMessage))
                        {
                            isCdtError = true;
                            verifyResp.err_msg = "DBSS: CDT Operation Fail.";
                            log.message = "DBSS: CDT Operation Fail.";
                            throw new Exception(CDTMessage);
                        }
                    }//######################## CDT Others ###########################
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMReplacement
                            || item.purpose_number == (int)EnumPurposeNumber.DeRegistration)
                    {
                        var res = GetPropertyValue(reqModel, "data.attributes.msisdn");
                        string msisdn = res.ToString();
                        bool isOtherCDTSuccess = await OtherCDT(item);
                        if (!isOtherCDTSuccess)
                        {
                            verifyResp.err_msg = "DBSS: Other CDT Operation Fail.";
                            log.message = "DBSS: Other CDT Operation Fail.";
                            throw new Exception("DBSS: Other CDT Operation Fail.");
                        }
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMTransfer)
                    {
                        RACommonResponse raResp = new RACommonResponse();
                        raResp = await OtherCDTForTOS(item);
                        if (raResp.result == false)
                        {
                            verifyResp.err_msg = raResp.message;
                            log.message = "DBSS: Other CDT Operation Fail.";
                            return verifyResp;
                            //throw new Exception("DBSS: Other CDT Operation for TOS Fail.");
                        }
                    }
                    //if (item.is_paired != null)
                    //{
                    //    if (item.is_paired == 0 && item.purpose_number == (int)EnumPurposeNumber.NewRegistration)
                    //    {
                    //        try
                    //        {
                    //            reservationResponse = await MSISDNReservation(item);
                    //            if (reservationResponse.IsReserve != true)
                    //            {
                    //                log.message = reservationResponse.Error_message;
                    //                verifyResp.err_msg = "DBSS: MSISDN Reservation Fail.";
                    //                throw new Exception("DBSS: MSISDN Reservation Fail.");
                    //            }
                    //            else
                    //            {
                    //                verifyResp.Reservation_Id = reservationResponse.Reservation_Id;
                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            verifyResp.err_msg = ex.Message.ToString();
                    //            //verifyResp.is_success = false;
                    //            //return verifyResp;
                    //        }
                    //    }
                    //}
                }
                bioLogNeeded = true;
                object reqModel_temp = reqModel;
                try
                {
                    string req_string = JsonConvert.SerializeObject(reqModel_temp);
                    JObject parsedObj = JObject.Parse(req_string);

                    if (parsedObj != null)
                    {
                        if (item.dest_left_thumb != null)
                            parsedObj["data"]["attributes"]["dest_left_thumb"] = null;
                        if (item.dest_left_index != null)
                            parsedObj["data"]["attributes"]["dest_left_index"] = null;
                        if (item.dest_right_thumb != null)
                            parsedObj["data"]["attributes"]["dest_right_thumb"] = null;
                        if (item.dest_right_index != null)
                            parsedObj["data"]["attributes"]["dest_right_index"] = null;
                        if (item.src_left_index != null)
                            parsedObj["data"]["attributes"]["src_left_thumb"] = null;
                        if (item.src_left_thumb != null)
                            parsedObj["data"]["attributes"]["src_left_index"] = null;
                        if (item.src_right_index != null)
                            parsedObj["data"]["attributes"]["src_right_thumb"] = null;
                        if (item.src_right_thumb != null)
                            parsedObj["data"]["attributes"]["src_right_index"] = null;
                    }

                    log.req_string = parsedObj.ToString();
                    log.req_blob = byteArrayConverter.GetGenericJsonData(parsedObj.ToString());
                }
                catch (Exception ex)
                { throw new Exception("Error Occurred in FP Set to Null in Log time."); }

                try
                {
                    log.req_time = DateTime.Now;
                    bioResponse = await genericApiCall.HttpPostRequest(reqModel, meathodUrl);
                    log.res_time = DateTime.Now;
                }
                catch (Exception ex)
                {
                    log.message = "DBSS: Bio Req-" + ex.Message.ToString();
                    verifyResp.err_msg = "DBSS: Bio Req-" + ex.Message.ToString();
                    throw new Exception("DBSS: Bio Req-" + ex.Message);
                }

                log.res_string = JsonConvert.SerializeObject(bioResponse.ToString()).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(bioResponse);

                BioResModel? response = null;
                try
                {
                    response = JsonConvert.DeserializeObject<BioResModel>(bioResponse.ToString());
                }
                catch (Exception ex)
                {
                    verifyResp.err_msg = "DBSS: Biometric Api Response Parsing Error.";
                    throw new Exception("DBSS: Biometric Api Response Parsing Error.");
                }

                if (response == null)
                {
                    verifyResp.err_msg = "DBSS: Invalid Biometric Api Response.";
                    throw new Exception("DBSS: Invalid Biometric Api Response.");
                }
                else if (response.data == null)
                {
                    verifyResp.err_msg = "DBSS: Invalid Biometric Api Response.";
                    throw new Exception("DBSS: Invalid Biometric Api Response.");
                }
                else if (response.data.request_id == null)
                {
                    verifyResp.err_msg = "DBSS: Invalid Biometric Api Response.";
                    throw new Exception("DBSS: Invalid Biometric Api Response.");
                }
                // this  is successfull case and bss give us failled response
                item.bss_request_id = response.data.request_id;
                log.is_success = 1;
                verifyResp.is_success = true;
                verifyResp.bss_req_id = response.data.request_id;
                item.bss_request_id = verifyResp.bss_req_id;

                return verifyResp;
            }
            catch (Exception ex)
            {
                verifyResp.err_msg = ex.Message.ToString();
                if (bioLogNeeded || isCdtError)
                {
                    ErrorDescription error = new ErrorDescription();
                    try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "DBSS Service"); }
                    catch { }
                    log.req_time = reqTime;
                    log.res_time = resTime;
                    log.res_string = JsonConvert.SerializeObject(bioResponse != null ? bioResponse.ToString() : ex.Message).ToString();
                    log.res_blob = byteArrayConverter.GetGenericJsonData(bioResponse != null ? bioResponse.ToString() : ex.Message);
                    log.message = error != null ? error.error_description : null;
                    log.error_code = error != null ? error.error_code : null;
                    log.error_source = error != null ? error.error_source : "DBSS Service";
                    log.is_success = 0;
                    // BIRequset Table Update Status 150 and Error id and description for biometric Failuer
                    item.status = 150;
                    item.error_id = error != null ? error.error_id : 0;
                    item.error_description = error != null ? error.error_description : ex.Message;
                    verifyResp.error_Id = item.error_id;
                    verifyResp.is_success = false;
                    verifyResp.err_code = error != null ? error.error_code : null;
                    verifyResp.err_msg = ex.Message.ToString();
                }
                return verifyResp;
            }
            finally
            {
                if (bioLogNeeded)
                {
                    log.bss_request_id = item.bss_request_id;
                    log.bi_token_number = item.bi_token_number;
                    log.msisdn = item.msisdn;
                    log.user_id = item.user_id;
                    log.integration_point_from = (int)IntegrationPoints.BI;
                    log.integration_point_to = (int)IntegrationPoints.BSS;
                    log.method_name = "BioVerificationReqToBssV3";
                    log.purpose_number = item.purpose_number.ToString();
                    await _bllLog.BALogInsert(log);
                }
            }
        }
        #region CDT
        public async Task<string> CDT(BiomerticDataModel item)
        {
            LogModel log = new LogModel();
            string res = null;
            CDTRequestModel cdtReqModel = new CDTRequestModel();
            object cdtResponse = null;
            log.status = item.status;
            BiometricPopulateModel pltObj = new BiometricPopulateModel();
            BL_Json byteArrayConverter = new BL_Json();
            ApiCall genericApiCall = new ApiCall();
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;

            try
            {
                string meathodUrl = "/api/v1/residential-credit-decisions";
                cdtReqModel = pltObj.PopulateCDTRequestModel(item);

                log.req_string = JsonConvert.SerializeObject(cdtReqModel).ToString();
                log.req_blob = byteArrayConverter.GetGenericJsonData(cdtReqModel);
                try
                {
                    log.req_time = DateTime.Now;
                    //cdtResponse = await genericApiCall.HttpPostRequest(cdtReqModel, meathodUrl);
                    cdtResponse = await genericApiCall.HttpPostRequestCDT(cdtReqModel, meathodUrl);
                    log.res_time = DateTime.Now;

                }
                catch (Exception ex)
                { throw new Exception("DBSS: CDT " + ex.Message); }

                log.res_string = JsonConvert.SerializeObject(cdtResponse.ToString()).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(cdtResponse);

                // if "credit_decision" is "ACCEPTED" then CDT return true else false.
                // till now can not get any response so that job is pending, for this reason if get success then pass by default true.

                string desision = null;
                JObject dbssRespObj;
                try
                {
                    dbssRespObj = JObject.Parse(cdtResponse.ToString());

                    desision = (string)dbssRespObj?["data"]?["attributes"]?["credit-decision"];
                    if (string.IsNullOrEmpty(desision))
                        throw new Exception("DBSS: CDT Api Response not Valid.");
                    else if (desision == "ACCEPTED")
                    {
                        res = "";
                    }
                    else if (desision == "REJECTED")
                    {
                        res = (string)dbssRespObj?["data"]?["attributes"]?["business-instruction"];
                        if (string.IsNullOrEmpty(res))
                            throw new Exception("DBSS: CDT Api Response not Valid.");
                    }
                    log.is_success = 1;
                    return res;
                }
                catch (Exception ex)
                { throw new Exception("DBSS: CDT Api Response not Valid."); }

            }

            catch (Exception ex)
            {
                res = ex.Message;
                ErrorDescription error = new ErrorDescription();
                try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "BIA"); }
                catch { }
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(cdtResponse != null ? cdtResponse : ex.Message).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(cdtResponse != null ? cdtResponse : ex.Message);
                log.message = error != null ? error.error_description : "";
                log.error_code = error != null ? error.error_code : "";
                log.error_source = error != null ? error.error_source : "BIA";
                log.is_success = 0;
                // BIRequset Table Update Status 150 and Error id and description for biometric Failuer
                item.status = 150;
                item.error_id = error != null ? error.error_id : 0;
                if(error != null)
                {
                    res = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description;
                    item.error_description = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description;
                }
                return res;
            }
            finally
            {
                log.bss_request_id = item.bss_request_id;
                log.bi_token_number = item.bi_token_number;
                log.msisdn = item.msisdn;
                log.user_id = item.user_id;
                log.integration_point_from = (int)IntegrationPoints.BI;
                log.integration_point_to = (int)IntegrationPoints.BSS;
                log.method_name = "CDT";
                log.purpose_number = item.purpose_number.ToString();

                await _bllLog.BALogInsert(log);
            }
        }

        public async Task<bool> OtherCDT(BiomerticDataModel item)
        {
            BL_Json byteArrayConverter = new BL_Json();
            ApiCall genericApiCall = new ApiCall();
            LogModel log = new LogModel();
            bool res = true;
            string meathodUrl = $"/api/v1/subscriptions?filter%5Bmsisdn%5D={item.msisdn}&include=barrings";
            object otherCdtResponse = null;
            log.status = item.status;
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;
            try
            {
                log.req_time = DateTime.Now;
                log.req_string = meathodUrl;
                log.req_blob = byteArrayConverter.GetGenericJsonData(meathodUrl);
                try
                { otherCdtResponse = genericApiCall.HttpGetRequest(meathodUrl, out reqTime, out resTime); }
                catch (Exception ex)
                { throw new Exception("DBSS: Other CDT " + ex.Message); }
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(otherCdtResponse.ToString()).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(otherCdtResponse);

                OtherCDTResModel? response = new OtherCDTResModel();
                try
                {
                    response = JsonConvert.DeserializeObject<OtherCDTResModel>(otherCdtResponse.ToString());
                }
                catch (Exception ex)
                { throw new Exception("DBSS: Other CDT Api Response Parsing Error."); }

                if (response == null)
                    throw new Exception("DBSS: Other CDT Api Response not Valid.");

                if (response.included != null && response.included.Count > 0)
                    foreach (var item1 in response.included)
                    {
                        if (item1.id == "BAR_EXCEPTION" || item1.id == "BAR_RAFM")
                        {
                            throw new Exception("User is Blocked by " + item1.id + " role.");
                        }
                    }

                log.is_success = 1;
                return res;
            }

            catch (Exception ex)
            {
                res = false;
                ErrorDescription error = new ErrorDescription();
                try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "DBSS Service"); }
                catch { }
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(otherCdtResponse != null ? otherCdtResponse.ToString() : ex.Message).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(otherCdtResponse != null ? otherCdtResponse : ex.Message);
                log.message = error != null ? error.error_description : "";
                log.error_code = error != null ? error.error_code : "";
                log.error_source = error != null ? error.error_source : "DBSS Service";
                log.is_success = 0;
                item.status = 150;
                item.error_id = error != null ? error.error_id : 0;
                if(error != null)
                {
                    item.error_description = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description;
                }
                return res;
            }
            finally
            {
                log.bss_request_id = item.bss_request_id;
                log.bi_token_number = item.bi_token_number;
                log.msisdn = item.msisdn;
                log.user_id = item.user_id;
                log.integration_point_from = (int)IntegrationPoints.BI;
                log.integration_point_to = (int)IntegrationPoints.BSS;
                log.method_name = "OtherCDT";
                log.purpose_number = item.purpose_number.ToString();

                await _bllLog.BALogInsert(log);
            }
        }

        public async Task<RACommonResponse> OtherCDTForTOS(BiomerticDataModel item)
        {
            BL_Json byteArrayConverter = new BL_Json();
            ApiCall genericApiCall = new ApiCall();
            LogModel log = new LogModel();
            RACommonResponse raResponse = new RACommonResponse();

            string meathodUrl = $"/api/v1/subscriptions?filter%5Bmsisdn%5D={item.msisdn}&include=barrings";
            object otherCdtResponse = null;
            log.status = item.status;
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;
            try
            {
                log.req_time = DateTime.Now;
                log.req_string = meathodUrl;
                log.req_blob = byteArrayConverter.GetGenericJsonData(meathodUrl);
                try
                { otherCdtResponse = genericApiCall.HttpGetRequest(meathodUrl, out reqTime, out resTime); }
                catch (Exception ex)
                { throw new Exception("DBSS: Other CDT For TOS " + ex.Message); }
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(otherCdtResponse.ToString()).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(otherCdtResponse);

                OtherCDTResModel? response = new OtherCDTResModel();
                try
                {
                    response = JsonConvert.DeserializeObject<OtherCDTResModel>(otherCdtResponse.ToString());

                }
                catch (Exception ex)
                { throw new Exception("DBSS: Other CDT For TOS Api Response Parsing Error."); }

                if (response == null)
                    throw new Exception("DBSS: Other CDT For TOS Api Response not Valid.");

                if (response.included != null && response.included.Count > 0)
                {
                    foreach (var item1 in response.included)
                    {
                        if (item1.id == "BAR_EXCEPTION" || item1.id == "BAR_RAFM")
                        {
                            raResponse.result = false;
                            throw new Exception("User is Blocked by " + item1.id + " role.");
                        }
                    }

                    #region Cherished number validation for TOS
                    RACommonResponse raCommon = await CheckCherishMSISDNParseForTos(item, "OtherCDTForTOS");

                    if (raCommon.result == true)
                    {
                        if (response.included != null && response.included.Count > 0)
                        {
                            foreach (var item1 in response.included)
                            {
                                if (item1.id.Equals("BAR_PREMIUM"))
                                {
                                    raResponse.result = false;
                                    raResponse.message = "User is Blocked by " + item1.id + " role.";
                                    throw new Exception("User is Blocked by " + item1.id + " role.");
                                }
                            }
                        }
                    }

                    #endregion
                }

                log.is_success = 1;
                raResponse.result = true;
                return raResponse;
            }

            catch (Exception ex)
            {
                raResponse.result = false;
                ErrorDescription error = new ErrorDescription();
                try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "DBSS Service"); }
                catch { }
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(otherCdtResponse != null ? otherCdtResponse.ToString() : ex.Message).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(otherCdtResponse != null ? otherCdtResponse : ex.Message);
                log.message = error != null ? error.error_description : "";
                log.error_code = error != null ? error.error_code : "";
                log.error_source = error != null ? error.error_source : "DBSS Service";
                log.is_success = 0;
                // BIRequset Table Update Status 150 and Error id and description for biometric Failuer
                item.status = 150;
                item.error_id = error != null ? error.error_id : 0;
                item.error_description = error != null ? error.error_description : ex.Message;
                return raResponse;
            }
            finally
            {
                log.bss_request_id = item.bss_request_id;
                log.bi_token_number = item.bi_token_number;
                log.msisdn = item.msisdn;
                log.user_id = item.user_id;
                log.integration_point_from = (int)IntegrationPoints.BI;
                log.integration_point_to = (int)IntegrationPoints.BSS;
                log.method_name = "OtherCDTForTOS";
                log.purpose_number = item.purpose_number.ToString();

                await _bllLog.BALogInsert(log);
            }
        }
        #endregion
        #region Single Source Check
        public async Task<SingleSourceCheckResponseModel> SingleSourceCheckFromBioDB(string msisdn, string sim_number, int purpose_No, string poc_number, int sim_rep_type, string dest_doc_id, string dest_dob, string dest_imsi)
        {
            SingleSourceCheckResponseModel checkResponseModel = new SingleSourceCheckResponseModel();
            try
            {
                checkResponseModel = await _bllObj.SingleSourceCheckFromBioDB(msisdn, sim_number, purpose_No, poc_number, sim_rep_type, dest_doc_id, dest_dob, dest_imsi);
            }
            catch (Exception ex)
            {
                throw;
            }
            return checkResponseModel;
        }

        public async Task<RACommonResponse> CheckCherishMSISDNParseForTos(BiomerticDataModel msisdnCheckReqest, string apiName)
        {
            RACommonResponse raRespModel = new RACommonResponse();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            string number_category = string.Empty;

            try
            {
                if (msisdnCheckReqest.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.msisdn = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.msisdn;
                }

                apiUrl = String.Format(GetAPICollection.CherishMSISDNValidation, msisdnCheckReqest.msisdn);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;

                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsingForTOS(dbssResp, msisdnCheckReqest.user_id);

                if (msisdnResp.result == false)
                {
                    raRespModel.result = false;
                    raRespModel.message = msisdnResp.message;
                    return raRespModel;
                }
                raRespModel.result = true;
                raRespModel.message = msisdnResp.message;

                return raRespModel;

            }
            catch (Exception ex)
            {
                raRespModel.result = false;
                raRespModel.message = ex.Message.ToString();

                log.is_success = 0;

                log.message = ex.Message.ToString();

                return raRespModel;
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.msisdn);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number.ToString();
                log.user_id = msisdnCheckReqest.user_id;
                log.method_name = "CheckCherishMSISDNParseForTos";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);

            }
        }

        public async Task<string> SingleSourceLogin(string msisdn, string userName)
        {
            HttpClient client = new HttpClient();
            LogModel log = new LogModel();
            SingleSourceReqModel singleSourceReqModel = new SingleSourceReqModel();
            SingleSourceLoginReq loginReqModel = new SingleSourceLoginReq();
            SingleSourceReqModel reqModel = new SingleSourceReqModel();
            BL_Json byteArrayConverter = new BL_Json();
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;
            string messages = string.Empty;
            string loginapiUrl = SingleSourceAPI.LoginAPI;
            string apiUrl = SingleSourceAPI.BiometricInfoAPI;
            string loginResponseContent = String.Empty;
            string responseContent = String.Empty;
            SingleSourceLoginRes? loginapiResponse = new SingleSourceLoginRes();
            SingleSourceRes Inforesponse = new SingleSourceRes();
            string token = string.Empty;
            string res = string.Empty;
            try
            {
                StringContent content;

                loginReqModel = new SingleSourceLoginReq()
                {
                    user_name = SettingsValues.GetSingleSourceUserName(),
                    password = SettingsValues.GetSingleSourcePassword()
                };
                messages = SettingsValues.GetSingleSourceMessage();
                log.req_string = JsonConvert.SerializeObject(loginReqModel).ToString();
                log.req_blob = byteArrayConverter.GetGenericJsonData(loginReqModel);

                string jsonData = JsonConvert.SerializeObject(loginReqModel);

                content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync(loginapiUrl, content).Result;
                loginResponseContent = response.Content.ReadAsStringAsync().Result;

                loginapiResponse = JsonConvert.DeserializeObject<SingleSourceLoginRes>(loginResponseContent);

                singleSourceLoginSessionToken = loginapiResponse != null ? loginapiResponse.session_token : "";

                token = singleSourceLoginSessionToken;
                log.req_time = DateTime.Now;
                log.res_string = JsonConvert.SerializeObject(token).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(loginapiResponse);
            }
            catch (Exception ex)
            {
                res = ex.Message;
                ErrorDescription error = new ErrorDescription();
                try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "Single Source"); }
                catch { }
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(Inforesponse != null ? Inforesponse : ex.Message).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(responseContent != null ? responseContent : ex.Message);
                log.message = error != null ? error.error_description : "";
                log.error_code = error != null ? error.error_code : "";
                log.error_source = error != null ? error.error_source : "Single Source";
                log.is_success = 0;
            }
            finally
            {
                log.method_name = "SingleSourceLogin";
                log.msisdn = _bllLog.FormatMSISDN(msisdn);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = userName;

                await _bllLog.BALogInsert(log);
            }
            return token;
        }

        public async Task<SingleSourceCheckResponseModelRevamp> SingleSourceCheckThroughAPI(string msisdn, string userName)
        {
            HttpClient client = new HttpClient();
            LogModel log = new LogModel();
            string res = string.Empty;
            SingleSourceReqModel singleSourceReqModel = new SingleSourceReqModel();
            SingleSourceLoginReq loginReqModel = new SingleSourceLoginReq();
            SingleSourceReqModel reqModel = new SingleSourceReqModel();
            object loginResponse = null;

            BiometricPopulateModel pltObj = new BiometricPopulateModel();
            BL_Json byteArrayConverter = new BL_Json();
            string loginapiUrl = SingleSourceAPI.LoginAPI;
            string apiUrl = SingleSourceAPI.BiometricInfoAPI;
            string loginResponseContent = String.Empty;
            string responseContent = String.Empty;
            SingleSourceLoginRes? loginapiResponse = new SingleSourceLoginRes();
            SingleSourceRes Inforesponse = new SingleSourceRes();
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;
            string messages = string.Empty;
            try
            {
                StringContent content;
                messages = SettingsValues.GetSingleSourceMessage();
                for (int i = 0; i < 3; i++)
                {
                    if(!String.IsNullOrEmpty(singleSourceLoginSessionToken))
                    {
                        log.req_time = DateTime.Now;
                        reqModel = new SingleSourceReqModel { msisdn = msisdn };
                        string jsonData = JsonConvert.SerializeObject(reqModel);
                        log.req_blob = byteArrayConverter.GetGenericJsonData(jsonData);
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer "+singleSourceLoginSessionToken);
                        content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                        HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;
                        responseContent = response.Content.ReadAsStringAsync().Result;

                        Inforesponse = JsonConvert.DeserializeObject<SingleSourceRes>(responseContent);

                        log.res_blob = byteArrayConverter.GetGenericJsonData(responseContent);

                        log.res_time = DateTime.Now;

                        if (Inforesponse != null && !Inforesponse.is_success && Inforesponse.message.Contains("Invalid session token"))
                        {
                            singleSourceLoginSessionToken = await SingleSourceLogin(msisdn, userName);

                            log.req_time = DateTime.Now;
                            reqModel = new SingleSourceReqModel { msisdn = msisdn };
                            jsonData = JsonConvert.SerializeObject(reqModel);
                            log.req_blob = byteArrayConverter.GetGenericJsonData(jsonData);
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + singleSourceLoginSessionToken);
                            content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                            response = client.PostAsync(apiUrl, content).Result;
                            
                            responseContent = response.Content.ReadAsStringAsync().Result;

                            Inforesponse = JsonConvert.DeserializeObject<SingleSourceRes>(responseContent);
                            log.res_time = DateTime.Now;
                            log.res_blob = byteArrayConverter.GetGenericJsonData(responseContent);
                            return new SingleSourceCheckResponseModelRevamp
                            {
                                Status = Inforesponse != null && Inforesponse.Data != null ? Inforesponse.Data.is_active : false,
                                Message = Inforesponse != null && Inforesponse.Data != null && Inforesponse.Data.is_active == true ? messages : Inforesponse.message
                            };
                        }
                        else
                        {
                            return new SingleSourceCheckResponseModelRevamp
                            {
                                Status = Inforesponse != null && Inforesponse.Data != null ? Inforesponse.Data.is_active : false,
                                Message = (Inforesponse != null && Inforesponse.Data != null && Inforesponse.Data.is_active == true)
                                ? messages
                                : (Inforesponse != null ? Inforesponse.message : string.Empty)
                            };
                        }
                    }
                    else
                    {
                        singleSourceLoginSessionToken = await SingleSourceLogin(msisdn, userName);
                    }                   
                }
                return new SingleSourceCheckResponseModelRevamp
                {
                    Status = Inforesponse != null && Inforesponse.Data != null ? Inforesponse.Data.is_active : false,
                    Message = Inforesponse != null && Inforesponse.Data != null && Inforesponse.Data.is_active == true ? messages : Inforesponse.message
                };
            }
            catch (Exception ex)
            {
                res = ex.Message;
                ErrorDescription error = new ErrorDescription();
                try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "Single Source"); }
                catch { }
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(Inforesponse != null ? Inforesponse : ex.Message).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(responseContent != null ? responseContent : ex.Message);
                log.message = error != null ? error.error_description : "";
                log.error_code = error != null ? error.error_code : "";
                log.error_source = error != null ? error.error_source : "Single Source";
                log.is_success = 0;
                throw new Exception("Single Source Status Check " + ex.Message);
            }
            finally
            {
                log.method_name = "SingleSourceCheckThroughAPI";
                log.msisdn = _bllLog.FormatMSISDN(msisdn);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = userName;

                await _bllLog.BALogInsert(log);
            }
        }
        #endregion

        public async Task<RACommonResponse> CherishedNumberValidationForTOS(BiomerticDataModel msisdnCheckReqest, string apiName)
        {
            BL_Json byteArrayConverter = new BL_Json();
            ApiCall genericApiCall = new ApiCall();
            RACommonResponse raRespModel = new RACommonResponse();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            string number_category = string.Empty;

            try
            {
                if (msisdnCheckReqest.msisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.msisdn = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.msisdn;
                }

                apiUrl = String.Format(GetAPICollection.CherishMSISDNValidation, msisdnCheckReqest.msisdn);

                log.req_blob = byteArrayConverter.GetGenericJsonData(apiUrl);

                log.req_time = DateTime.Now;
                dbssResp = (JObject)genericApiCall.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;

                txtResp = Convert.ToString(dbssResp);

                log.res_blob = byteArrayConverter.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.CherishMSISDNReqParsing(dbssResp, msisdnCheckReqest.user_id);

                if (msisdnResp.result == false)
                {
                    raRespModel.result = false;
                    raRespModel.message = msisdnResp.message;

                    return raRespModel;
                }
                raRespModel.result = true;
                raRespModel.message = msisdnResp.message;

                return raRespModel;

            }
            catch (Exception ex)
            {
                raRespModel.result = false;
                raRespModel.message = ex.Message.ToString();

                log.is_success = 0;

                log.message = ex.Message.ToString();

                return raRespModel;
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.msisdn);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number.ToString();
                log.user_id = msisdnCheckReqest.user_id;
                log.method_name = "CherishedNumberValidationForTOS";

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }


        #region Privat Method
        public static object GetPropertyValue(object src, string propName)
        {
            if (src == null) throw new ArgumentException("CDT Other-Value cannot be null.", "src");
            if (propName == null) throw new ArgumentException("CDT Other-Value cannot be null.", "propName");

            if (propName.Contains("."))//complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                return GetPropertyValue(GetPropertyValue(src, temp[0]), temp[1]);
            }
            else
            {
                var prop = src.GetType().GetProperty(propName);
                return prop != null ? prop.GetValue(src, null) : null;
            }
        }
        #endregion
        #region Unreserve MSISDN

        /// <summary>
        /// Unreserve-MSISDN
        /// </summary>
        /// <param name="msisdnReservationId"></param>
        public async Task<RACommonResponse> UnreserveMSISDN(string msisdnReservationId, string sessionToken, string bio_request_id, string bi_token_number, string msisdn)
        {
            ApiRequest apiReq = new ApiRequest();
            BIAToDBSSLog logObj = new BIAToDBSSLog();
            string apiUrl = "", txtResp = "";


            RACommonResponse resp = new RACommonResponse();
            BLLRAToDBSSParse rAParse = new BLLRAToDBSSParse();
            UnreserveMSISDNRequestRootobject reqRootObj = new UnreserveMSISDNRequestRootobject();
            try
            {
                reqRootObj = rAParse.UnreserveMSISDNReqParsing(msisdnReservationId);

                apiUrl = String.Format(DeleteAPICollection.UnreserveMSISDN);

                logObj.req_blob = _blJson.GetGenericJsonData(reqRootObj);
                logObj.req_time = DateTime.Now;

                //object dbssResp = new object();
                object dbssResp = await apiReq.HttpDeleteRequest(reqRootObj, apiUrl);

                logObj.res_blob = _blJson.GetGenericJsonData(dbssResp);
                logObj.res_time = DateTime.Now;
                txtResp = apiUrl + "//" + Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    logObj.is_success = 1;

                    var dbssRespModel = JsonConvert.DeserializeObject<ReserverMSISDNResponseRootobject>(dbssResp.ToString());
                    if (dbssRespModel.data != null)
                    {
                        if (dbssRespModel.data.status == 200)
                        {
                            resp.result = true;
                            resp.message = "MSISDN unreserved successfully.";
                            //ToDo: Update BIReq Tbl remarks column with 200 status.(need to confirm.)
                            //return resp;
                        }
                        else
                        {
                            resp.result = false;
                            resp.message = "MSISDN unreservation failed!";
                            //return resp;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logObj.res_blob = _blJson.GetGenericJsonData(ex.Message);
                logObj.res_time = DateTime.Now;

                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    logObj.is_success = 0;
                    logObj.error_code = error.error_code ?? String.Empty;
                    logObj.error_source = error.error_source ?? String.Empty;
                    logObj.message = error.error_description ?? String.Empty;

                    resp.result = false;
                    resp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    resp.result = false;
                    resp.message = ex.Message;
                }
            }
            finally
            {
                logObj.msisdn = msisdn;
                logObj.bi_token_number = bi_token_number;
                logObj.dbss_request_id = bio_request_id;

                logObj.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                logObj.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                logObj.user_id = _bllCommon.GetUserNameFromSessionToken(sessionToken);
                logObj.method_name = "UnreserveMSISDN";

                await _bllLog.RAToDBSSLog(logObj, apiUrl + Convert.ToString(reqRootObj), txtResp);

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(logObj, apiUrl + Convert.ToString(reqRootObj), txtResp));
                //logThread.Start();
            }
            return resp;
        }

        public async Task<RACommonResponse> UnreserveMSISDNV2(string msisdnReservationId, string sessionToken, string bio_request_id, string bi_token_number, string msisdn)
        {
            ApiRequest apiReq = new ApiRequest();
            BIAToDBSSLog logObj = new BIAToDBSSLog();
            string apiUrl = "", txtResp = "";

            RACommonResponse resp = new RACommonResponse();
            BLLRAToDBSSParse rAParse = new BLLRAToDBSSParse();
            UnreserveMSISDNRequestRootobject reqRootObj = new UnreserveMSISDNRequestRootobject();
            try
            {
                reqRootObj = rAParse.UnreserveMSISDNReqParsing(msisdnReservationId);

                apiUrl = String.Format(DeleteAPICollection.UnreserveMSISDN);

                logObj.req_blob = _blJson.GetGenericJsonData(reqRootObj);
                logObj.req_time = DateTime.Now;

                //object dbssResp = new object();
                object dbssResp = await apiReq.HttpDeleteRequest(reqRootObj, apiUrl);

                logObj.res_blob = _blJson.GetGenericJsonData(dbssResp);
                logObj.res_time = DateTime.Now;
                txtResp = apiUrl + "//" + Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    logObj.is_success = 1;

                    var dbssRespModel = JsonConvert.DeserializeObject<ReserverMSISDNResponseRootobject>(dbssResp.ToString());
                    if (dbssRespModel.data != null)
                    {
                        if (dbssRespModel.data.status == 200)
                        {
                            resp.result = true;
                            resp.message = "MSISDN unreserved successfully.";
                            //ToDo: Update BIReq Tbl remarks column with 200 status.(need to confirm.)
                            //return resp;
                        }
                        else
                        {
                            resp.result = false;
                            resp.message = "MSISDN unreservation failed!";
                            //return resp;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logObj.res_blob = _blJson.GetGenericJsonData(ex.Message);
                logObj.res_time = DateTime.Now;

                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    logObj.is_success = 0;
                    logObj.error_code = error.error_code ?? String.Empty;
                    logObj.error_source = error.error_source ?? String.Empty;
                    logObj.message = error.error_description ?? String.Empty;

                    resp.result = false;
                    resp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    resp.result = false;
                    resp.message = ex.Message;
                }
            }
            finally
            {
                logObj.msisdn = msisdn;
                logObj.bi_token_number = bi_token_number;
                logObj.dbss_request_id = bio_request_id;

                logObj.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                logObj.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                logObj.user_id = _bllCommon.GetUserNameFromSessionTokenV2(sessionToken);
                logObj.method_name = "UnreserveMSISDN";

                await _bllLog.RAToDBSSLog(logObj, apiUrl + Convert.ToString(reqRootObj), txtResp);
            }
            return resp;
        }

        public async Task<RACommonResponse> UnreserveMSISDNStarTrek(string msisdnReservationId, string userId, string bio_request_id, string bi_token_number, string msisdn)
        {
            ApiRequest apiReq = new ApiRequest();
            BIAToDBSSLog logObj = new BIAToDBSSLog();
            string apiUrl = "", txtResp = "";
             
            RACommonResponse resp = new RACommonResponse();
            BLLRAToDBSSParse rAParse = new BLLRAToDBSSParse();
            UnreserveMSISDNRequestRootobject reqRootObj = new UnreserveMSISDNRequestRootobject();
            try
            {
                reqRootObj = rAParse.UnreserveMSISDNPopulate(msisdnReservationId);

                apiUrl = String.Format(DeleteAPICollection.UnreserveMSISDN);

                logObj.req_blob = _blJson.GetGenericJsonData(reqRootObj);
                logObj.req_time = DateTime.Now;

                object dbssResp = await apiReq.HttpDeleteRequest(reqRootObj, apiUrl);

                logObj.res_blob = _blJson.GetGenericJsonData(dbssResp);
                logObj.res_time = DateTime.Now;
                txtResp = apiUrl + "//" + Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    logObj.is_success = 1;

                    var dbssRespModel = JsonConvert.DeserializeObject<ReserverMSISDNResponseRootobject>(dbssResp.ToString());
                    
                    if (dbssRespModel != null && dbssRespModel.data != null)
                    {
                        if (dbssRespModel.data.status == 200)
                        {
                            resp.result = true;
                            resp.message = "MSISDN unreserved successfully.";
                        }
                        else
                        {
                            resp.result = false;
                            resp.message = "MSISDN unreservation failed!";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logObj.res_blob = _blJson.GetGenericJsonData(ex.Message);
                logObj.res_time = DateTime.Now;

                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    logObj.is_success = 0;
                    logObj.error_code = error.error_code ?? String.Empty;
                    logObj.error_source = error.error_source ?? String.Empty;
                    logObj.message = error.error_description ?? String.Empty;

                    resp.result = false;
                    resp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    resp.result = false;
                    resp.message = ex.Message;
                }
            }
            finally
            {
                logObj.msisdn = msisdn;
                logObj.bi_token_number = bi_token_number;
                logObj.dbss_request_id = bio_request_id;

                logObj.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                logObj.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                logObj.user_id = userId;
                logObj.method_name = "UnreserveMSISDNStarTrek";

                await _bllLog.RAToDBSSLog(logObj, apiUrl + Convert.ToString(reqRootObj), txtResp);
            }
            return resp;
        }


        #endregion
        #region MSISDN Reservation
        public async Task<MSISDNReservationResponse> MSISDNReservation(BiomerticDataModel item)
        {
            BiometricPopulateModel pltObj = new BiometricPopulateModel();
            MSISDNReservationResponse response = new MSISDNReservationResponse();
            BL_Json byteArrayConverter = new BL_Json();
            ApiCall genericApiCall = new ApiCall();

            LogModel log = new LogModel();
            bool res = false;
            MSISDNReservation msisdnRes = new MSISDNReservation();
            string meathodUrl = "/api/v1/msisdn-reservations";
            object reservationResponse = null;
            log.status = item.status;
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;
            try
            {
                msisdnRes = pltObj.PopulateMSISDNReservationReqModel(item.msisdn);

                log.req_time = DateTime.Now;
                log.req_string = JsonConvert.SerializeObject(msisdnRes).ToString();
                log.req_blob = byteArrayConverter.GetGenericJsonData(msisdnRes);
                try
                {
                    log.req_time = DateTime.Now; ;

                    reservationResponse = await genericApiCall.HttpPostRequest(msisdnRes, meathodUrl);
                    //reservationResponse = @"{data:{reservation-id:22801bd1-619c-4ba0-86ba-158c561118fb,reserve-valid-for:2021-09-02T06:20:20Z}}";
                    log.res_time = DateTime.Now; ;
                }
                catch (Exception ex)
                {
                    response.Error_message = "DBSS: Reservation " + ex.Message;
                    throw new Exception("DBSS: Reservation " + ex.Message);
                }
                //log.res_time = DateTime.Now;

                log.res_string = JsonConvert.SerializeObject(reservationResponse.ToString()).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(reservationResponse);

                try
                {
                    JObject dbssRespObj = JObject.Parse(reservationResponse.ToString());
                    if (dbssRespObj.ContainsKey("data"))
                        response.Reservation_Id = (string)dbssRespObj["data"]["reservation-id"];
                    if (response.Reservation_Id == null) throw new Exception("DBSS: MSISDN Reservation Id not found.");
                }
                catch (Exception ex)
                { throw new Exception("DBSS: MSISDN Reservation Api Response Parsing Error."); }

                _bllObj.UpdateBioDbForReservation(item.bi_token_number, response.Reservation_Id);

                log.is_success = 1;
                response.IsReserve = true;
                return response;

            }
            catch (Exception ex)
            {
                ErrorDescription error = null;
                try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "DBSS Service"); }
                catch { }
                //log.res_time = DateTime.Now;
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(reservationResponse != null ? reservationResponse : ex.Message).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(reservationResponse != null ? reservationResponse : ex.Message);
                log.message = error != null ? error.error_description : null;
                log.error_code = error != null ? error.error_code : null;
                log.error_source = error != null ? error.error_source : "DBSS Service";
                log.is_success = 0;

                // BIRequset Table Update Status 150 and Error id and description for biometric Failuer
                item.status = 150;
                item.error_id = error != null ? error.error_id : 0;
                item.error_description = error != null ? error.error_description : ex.Message;
                response.Error_message = error != null ? error.error_description : ex.Message;
                //bllObj.UpdateStatusandErrorMessage(item.bi_token_number, item.status, item.error_id, item.error_description);
                return response;
            }
            finally
            {
                log.bss_request_id = item.bss_request_id;
                log.bi_token_number = item.bi_token_number;
                log.msisdn = item.msisdn;
                log.user_id = item.user_id;
                log.integration_point_from = (int)IntegrationPoints.BI;
                log.integration_point_to = (int)IntegrationPoints.BSS;
                log.method_name = "MSISDNReservation";
                log.purpose_number = item.purpose_number.ToString();
                await _bllLog.BALogInsert(log);
            }

        }
        #endregion

    }
}
