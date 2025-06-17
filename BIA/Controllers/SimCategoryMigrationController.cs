using BIA.BLL.BLLServices;
using BIA.BLL.Utility;
using BIA.Common;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.Utility;
using BIA.Entity.ViewModel;
using BIA.Helper;
using BIA.JWT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace BIA.Controllers
{
    [Route("api/SimCategoryMigration")]
    [ApiController]
    public class SimCategoryMigrationController : ControllerBase
    {
        private readonly BLLDBSSToRAParse _dbssToRaParse;
        private readonly ApiRequest _apiReq;
        private readonly BL_Json _blJson;
        private readonly BLLLog _bllLog;
        private readonly ApiManager _apiManager;
        private readonly BLLOrder _orderManager;
        private readonly BaseController _bio;  
        private readonly GeoFencingValidation _geo; 
        private readonly BllHandleException _manageExecption;
        private readonly IConfiguration _configuration;

        public SimCategoryMigrationController(BLLDBSSToRAParse dbssToRaParse, ApiRequest apiReq, BL_Json blJson, BLLLog bllLog, ApiManager apiManager, BLLOrder orderManager, BaseController bio, GeoFencingValidation geo, BllHandleException manageExecption, IConfiguration configuration)
        {
            _dbssToRaParse = dbssToRaParse;
            _apiReq = apiReq;
            _blJson = blJson;
            _bllLog = bllLog;
            _apiManager = apiManager;
            _orderManager = orderManager;
            _bio = bio;
            _geo = geo;
            _manageExecption = manageExecption;
            _configuration = configuration;
        }

        #region MSISDN validation NEW 
        /// <summary>
        /// This API is used for MSISDN validation for paired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(MSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForPreToPost")]
        public async Task<IActionResult> ValidateMSISDNForPreToPost([FromBody] MSISDNValidationReqForMigration msisdnCheckReqest)
        {
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }
                int purpose = (int)EnumPurposeNumber.SIMCategoryMigration;
                msisdnCheckReqest.purpose_number = purpose.ToString();

                apiUrl = String.Format(GetAPICollection.GetSubscriptionIdWithValidateMSISDN, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    log.remarks = MessageCollection.SIMReplNoDataFound;
                    return Ok(new MSISDNCheckResponse()
                    {
                        result = false,
                        message = MessageCollection.SIMReplNoDataFound,

                    });
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.PreToPostMSISDNReqParsing(dbssResp);

                if (msisdnResp.result == false)
                {
                    log.remarks = msisdnResp.message;
                    return Ok(new MSISDNCheckResponse()
                    {
                        result = false,
                        message = FixedValueCollection.MSISDNError + msisdnResp.message
                    });
                }

                msisdnCheckReqest.dbss_subscription_id = msisdnResp.dbss_subscription_id;

                bool isBarException = await CheckIsUserInBarrier(msisdnCheckReqest);

                if (!isBarException)
                {
                    return Ok(new MSISDNCheckResponse()
                    {
                        result = false,
                        message = "Customer is barred. Pls contact with your supervisor."
                    });
                }

                var resp = new MSISDNCheckResponseV2()
                {
                    dbss_subscription_id = msisdnResp.dbss_subscription_id,
                    nid = "**********",
                    dob = "**/**/*****",
                    result = true,
                    message = MessageCollection.MSISDNValid,
                    saf_status = msisdnResp.saf_status,
                    customer_id = msisdnResp.customer_id
                };
                return Ok(resp);
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.is_success = 0;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(new MSISDNCheckResponse()
                    {
                        result = false,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new MSISDNCheckResponse()
                    {
                        result = false,
                        message = ex.Message
                    });
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidateMSISDNForPreToPost";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }


        /// <summary>
        /// This API is used for MSISDN validation for paired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(MSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForPreToPostV2")]
        public async Task<IActionResult> ValidateMSISDNForPreToPostV2([FromBody] MSISDNValidationReqForMigration msisdnCheckReqest)
        {
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            try
            {
                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(msisdnCheckReqest.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;

                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }
                int purpose = (int)EnumPurposeNumber.SIMCategoryMigration;
                msisdnCheckReqest.purpose_number = purpose.ToString();

                apiUrl = String.Format(GetAPICollection.GetSubscriptionIdWithValidateMSISDN, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    log.remarks = MessageCollection.SIMReplNoDataFound;
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = MessageCollection.SIMReplNoDataFound,

                    });
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.PreToPostMSISDNReqParsing(dbssResp);

                if (msisdnResp.result == false)
                {
                    log.remarks = msisdnResp.message;
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = FixedValueCollection.MSISDNError + msisdnResp.message
                    });
                }

                msisdnCheckReqest.dbss_subscription_id = msisdnResp.dbss_subscription_id;

                bool isBarException = await CheckIsUserInBarrier(msisdnCheckReqest);

                if (!isBarException)
                {
                    return Ok(new RACommonResponseRevamp() { isError = true, message = "Customer is barred. Pls contact with your supervisor." });
                }

                var resp = new MSISDNCheckResponseV2()
                {
                    dbss_subscription_id = msisdnResp.dbss_subscription_id,
                    nid = "**********",
                    dob = "**/**/*****",
                    result = true,
                    message = MessageCollection.MSISDNValid,
                    saf_status = msisdnResp.saf_status,
                    customer_id = msisdnResp.customer_id
                };
                return Ok(new MSISDNCheckResponseRevamp()
                {
                    isError = false,
                    message = resp.message,
                    data = resp
                });

            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.is_success = 0;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(new MSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg,
                        data = null
                    });
                }
                catch (Exception ex2)
                {
                    return Ok(new MSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = ex2.Message,
                        data = null
                    });
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidateMSISDNForPreToPost";

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        #endregion

        #region Pre to Post Order Submission NEW
        /// Send Order
        /// <summary>
        /// This API is used for SimReplacement submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        //[ResponseType(typeof(SendOrderResponse))]
        [HttpPost]
        [Route("SubmitOrder")]
        public async Task<IActionResult> SubmitOrder([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            NidDobInfoResponse dobInfoResponse = new NidDobInfoResponse();
            MSISDNCheckRequest msisdnCheckReqest = new MSISDNCheckRequest();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new WebException(MessageCollection.InvalidSecurityToken);

                string loginProviderId = _bio.GetDecryptedSecurityToken(model.session_token);

                if (loginProviderId.Equals("Fail"))
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = "Invalid Security Token"
                    });
                }


                int purpose = (int)EnumPurposeNumber.SIMCategoryMigration;
                model.purpose_number = purpose.ToString();

                #region Get NID DOB
                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.retailer_id = model.retailer_id;
                dobInfoResponse = await GetNidDobForSimCategoryMigration(msisdnCheckReqest);

                if (dobInfoResponse.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = dobInfoResponse.message;
                    log.remarks = dobInfoResponse.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.dest_nid = dobInfoResponse.dest_nid;
                    model.dest_dob = dobInfoResponse.dest_dob;
                }
                #endregion

                #region request_blob
                RAOrderRequestV2 objData = new RAOrderRequestV2();
                objData = model;
                string req_string = JsonConvert.SerializeObject(objData);
                JObject parsedObj = JObject.Parse(req_string);
                if (model.dest_left_thumb != null)
                    parsedObj["dest_left_thumb"] = null;
                if (model.dest_left_index != null)
                    parsedObj["dest_left_index"] = null;
                if (model.dest_right_thumb != null)
                    parsedObj["dest_right_thumb"] = null;
                if (model.dest_right_index != null)
                    parsedObj["dest_right_index"] = null;
                if (model.src_left_index != null)
                    parsedObj["src_left_thumb"] = null;
                if (model.src_left_thumb != null)
                    parsedObj["src_left_index"] = null;
                if (model.src_right_index != null)
                    parsedObj["src_right_thumb"] = null;
                if (model.src_right_thumb != null)
                    parsedObj["src_right_index"] = null;

                log.req_blob = _blJson.GetGenericJsonData(parsedObj.ToString());
                #endregion

                #region Check if submitted order is already in process or not.
                var orderValidationResult = await _orderManager.ValidateOrder(new VMValidateOrder
                {
                    msisdn = model.msisdn,
                    sim_number = model.sim_number,
                    purpose_number = Convert.ToInt32(model.purpose_number),
                    is_corporate = 0,
                    retailer_id = model.retailer_id,
                    dest_dob = DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat)
                });
                if (orderValidationResult.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = orderValidationResult.message;
                    log.remarks = orderValidationResult.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                orderRes = await _orderManager.SubmitOrderV5(model, loginProviderId);
                if (!orderRes.is_success)
                {
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = orderRes.message
                    });
                }
                else
                {
                    try
                    {
                        model.bi_token_number = Convert.ToDouble(orderRes.request_id);
                        #endregion

                        #region bio verification

                        var pursedData = await _orderManager.SubmitOrderDataPurseV2(model);
                        BiomerticDataModel dataModel = bioverifyDataMapp(pursedData);

                        verifyResp = await _bio.BssServiceProcess(dataModel);

                        if (verifyResp.is_success == true)
                        {
                            model.bss_reqId = verifyResp.bss_req_id;
                            model.status = (int)EnumRAOrderStatus.BioVerificationSubmitted;
                        }
                        else
                        {
                            model.status = (int)EnumRAOrderStatus.Failed;
                            model.err_code = verifyResp.err_code;
                            model.err_msg = verifyResp.err_msg;
                            model.error_id = verifyResp.error_Id;
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        ErrorDescription errorDescription = new ErrorDescription();
                        model.status = (int)EnumRAOrderStatus.Failed;
                        errorDescription = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                        model.err_code = errorDescription.error_code;
                        model.error_id = errorDescription.error_id;
                        orderRes.message = String.IsNullOrEmpty(errorDescription.error_custom_msg) ? errorDescription.error_description : errorDescription.error_custom_msg;
                        model.err_msg = String.IsNullOrEmpty(errorDescription.error_custom_msg) ? errorDescription.error_description : errorDescription.error_custom_msg;
                        orderRes.is_success = false;
                        return Ok(orderRes);
                    }

                }

            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error;
                log.is_success = 0;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    if (verifyResp != null)
                    {
                        orderRes.request_id = verifyResp.bss_req_id;
                    }
                    else
                    {
                        orderRes.request_id = "";
                    }

                    orderRes.is_success = false;
                    orderRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    log.res_time = DateTime.Now;
                    orderRes.is_success = false;
                    orderRes.message = ex.Message;

                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
            }
            finally
            {
                if (model.bi_token_number != null && model.bi_token_number > 1)
                {
                    response2 = await _orderManager.UpdateOrder(new RAOrderRequestUpdate
                    {
                        bi_token_number = model.bi_token_number,
                        msidn = model.msisdn,
                        user_name = model.retailer_id,
                        dest_imsi = model.dest_imsi,
                        status = model.status,
                        bss_reqId = model.bss_reqId,
                        error_id = model.error_id,
                        err_msg = model.err_msg,
                    });
                }
                log.purpose_number = model.purpose_number;
                log.msisdn = _bllLog.FormatMSISDN(model.msisdn);
                log.bi_token_number = orderRes.request_id;
                log.method_name = "SubmitOrder";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.user_id = model.retailer_id;
                log.remarks = model.bi_token_number != null
                                && model.bi_token_number > 1 ? "Resubmit order" : String.Empty;

                await _bllLog.RAToDBSSLog(log, "", "");
            }
            return Ok(orderRes);
        }

        /// Send Order
        /// <summary>
        /// This API is used for SimReplacement submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        //[ResponseType(typeof(SendOrderResponse))]
        [HttpPost]
        [Route("SubmitOrderV2")]
        public async Task<IActionResult> SubmitOrderV2([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            NidDobInfoResponse dobInfoResponse = new NidDobInfoResponse();
            MSISDNCheckRequest msisdnCheckReqest = new MSISDNCheckRequest();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            GeoFencing geoFencing = new GeoFencing();
            GeofenceReqModel geofenceReqModel = new GeofenceReqModel();
            try
            {
                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;
                double allowedDistance = 0;
                int geoFencEnable = 0;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                try
                {
                    allowedDistance = Convert.ToDouble(_configuration.GetSection("AppSettings:GeofencingDistance").Value);
                    geoFencEnable = Convert.ToInt32(_configuration.GetSection("AppSettings:GeofencingDistanceCalculateEnable").Value);
                }
                catch
                { }

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(model.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;
                        model.distributor_code = security.DistributorCode;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                #region Geo fencing BP user
                if (geoFencEnable == 1)
                {
                    if (model.isBPUser == 1)
                    {
                        RACommonResponseRevamp responseRevamp = await _geo.GeoFencingBPUser(model);

                        if (responseRevamp != null && responseRevamp.isError == true)
                        {
                            return Ok(responseRevamp);
                        }
                    }
                }
                #endregion

                int purpose = (int)EnumPurposeNumber.SIMCategoryMigration;
                model.purpose_number = purpose.ToString();

                #region Get NID DOB
                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.retailer_id = model.retailer_id;
                dobInfoResponse = await GetNidDobForSimCategoryMigration(msisdnCheckReqest);

                if (dobInfoResponse.result == false)
                {
                    //orderRes.request_id = "0";
                    orderRes.isError = true;
                    orderRes.message = dobInfoResponse.message;
                    log.remarks = dobInfoResponse.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = orderRes.message,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = " "
                        }
                    });
                }
                else
                {
                    model.dest_nid = dobInfoResponse.dest_nid;
                    model.dest_dob = dobInfoResponse.dest_dob;
                }
                #endregion

                #region request_blob
                RAOrderRequestV2 objData = new RAOrderRequestV2();
                objData = model;
                string req_string = JsonConvert.SerializeObject(objData);
                JObject parsedObj = JObject.Parse(req_string);
                if (model.dest_left_thumb != null)
                    parsedObj["dest_left_thumb"] = null;
                if (model.dest_left_index != null)
                    parsedObj["dest_left_index"] = null;
                if (model.dest_right_thumb != null)
                    parsedObj["dest_right_thumb"] = null;
                if (model.dest_right_index != null)
                    parsedObj["dest_right_index"] = null;
                if (model.src_left_index != null)
                    parsedObj["src_left_thumb"] = null;
                if (model.src_left_thumb != null)
                    parsedObj["src_left_index"] = null;
                if (model.src_right_index != null)
                    parsedObj["src_right_thumb"] = null;
                if (model.src_right_thumb != null)
                    parsedObj["src_right_index"] = null;

                log.req_blob = _blJson.GetGenericJsonData(parsedObj.ToString());
                #endregion

                #region Check if submitted order is already in process or not.
                var orderValidationResult = await _orderManager.ValidateOrder(new VMValidateOrder
                {
                    msisdn = model.msisdn,
                    sim_number = model.sim_number,
                    purpose_number = Convert.ToInt32(model.purpose_number),
                    is_corporate = 0,
                    retailer_id = model.retailer_id,
                    dest_dob = DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat)
                });
                if (orderValidationResult.result == false)
                {
                    //orderRes.request_id = "0";
                    orderRes.isError = true;
                    orderRes.message = orderValidationResult.message;
                    log.remarks = orderValidationResult.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = orderRes.message,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = " "
                        }
                    });
                }
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                orderRes = await _orderManager.SubmitOrderV7(model, loginProviderId);
                if (orderRes.isError)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = orderRes.message,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = " "
                        }
                    });
                }
                else
                {
                    try
                    {
                        model.bi_token_number = Convert.ToDouble(orderRes.data.request_id);
                        #endregion

                        #region bio verification

                        var pursedData = await _orderManager.SubmitOrderDataPurseV2(model);
                        BiomerticDataModel dataModel = bioverifyDataMapp(pursedData);

                        verifyResp = await _bio.BssServiceProcessV2(dataModel);

                        if (verifyResp.is_success == true)
                        {
                            model.bss_reqId = verifyResp.bss_req_id;
                            model.status = (int)EnumRAOrderStatus.BioVerificationSubmitted;
                        }
                        else
                        {
                            model.status = (int)EnumRAOrderStatus.Failed;
                            model.err_code = verifyResp.err_code;
                            model.err_msg = verifyResp.err_msg;
                            model.error_id = verifyResp.error_Id;
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        ErrorDescription errorDescription = new ErrorDescription();
                        model.status = (int)EnumRAOrderStatus.Failed;
                        errorDescription = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                        model.err_code = errorDescription.error_code;
                        model.error_id = errorDescription.error_id;
                        orderRes.message = String.IsNullOrEmpty(errorDescription.error_custom_msg) ? errorDescription.error_description : errorDescription.error_custom_msg;
                        model.err_msg = String.IsNullOrEmpty(errorDescription.error_custom_msg) ? errorDescription.error_description : errorDescription.error_custom_msg;
                        orderRes.isError = true;
                        return Ok(new RACommonResponseRevamp()
                        {
                            isError = true,
                            message = orderRes.message,
                            data = new Datas()
                            {
                                isEsim = 0,
                                request_id = "0"
                            }
                        });
                    }

                }

            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error;
                log.is_success = 0;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    if (verifyResp != null)
                    {
                        orderRes.data.request_id = verifyResp.bss_req_id;
                    }
                    else
                    {
                        orderRes.data.request_id = "";
                    }

                    orderRes.isError = true;
                    orderRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    log.res_time = DateTime.Now;
                    orderRes.isError = true;
                    orderRes.message = ex.Message;

                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
            }
            finally
            {
                if (model.bi_token_number != null && model.bi_token_number > 1)
                {
                    response2 = await _orderManager.UpdateOrder(new RAOrderRequestUpdate
                    {
                        bi_token_number = model.bi_token_number,
                        msidn = model.msisdn,
                        user_name = model.retailer_id,
                        dest_imsi = model.dest_imsi,
                        status = model.status,
                        bss_reqId = model.bss_reqId,
                        error_id = model.error_id,
                        err_msg = model.err_msg,
                    });
                }
                log.purpose_number = model.purpose_number;
                log.msisdn = _bllLog.FormatMSISDN(model.msisdn);
                log.bi_token_number = orderRes.data == null ? "0" : orderRes.data.request_id;
                log.method_name = "SubmitOrderV2";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.user_id = model.retailer_id;
                log.remarks = model.bi_token_number != null
                                && model.bi_token_number > 1 ? "Resubmit order" : String.Empty;

                await _bllLog.RAToDBSSLog(log, "", "");
            }
            return Ok(new RACommonResponseRevamp()
            {
                isError = orderRes.isError,
                message = orderRes.message,
                data = new Datas()
                {
                    isEsim = 0,
                    request_id = orderRes.data.request_id != null ? orderRes.data.request_id : "0",
                }
            });
        }
        #endregion

        #region GET NID DOB FOR SIM CATEGORY MIGRATION
        public async Task<NidDobInfoResponse> GetNidDobForSimCategoryMigration(MSISDNCheckRequest msisdnCheckReqest)
        {
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            BIAToDBSSLog log = new BIAToDBSSLog();
            string apiUrl = string.Empty;
            string txtResp = string.Empty;

            try
            {
                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }
                int purpose = (int)EnumPurposeNumber.SIMCategoryMigration;
                msisdnCheckReqest.purpose_number = purpose.ToString();

                apiUrl = String.Format(GetAPICollection.GetSubscriptionIdWithValidateMSISDN, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.PreToPostMSISDNReqParsing(dbssResp);

                if (msisdnResp.result == false)
                {
                    nidDobInfo.result = false;
                    nidDobInfo.message = msisdnResp.message;
                    return nidDobInfo;
                }
                nidDobInfo.dest_nid = msisdnResp.nid;
                nidDobInfo.dest_dob = msisdnResp.dob;
                nidDobInfo.result = true;
                nidDobInfo.message = "";
                return nidDobInfo;
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.is_success = 0;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    nidDobInfo.result = false;
                    nidDobInfo.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return nidDobInfo;

                }
                catch (Exception)
                {
                    nidDobInfo.result = false;
                    nidDobInfo.message = ex.Message;
                    return nidDobInfo;
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "GetNidDobForSimCategoryMigration";
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        #endregion

        public BiomerticDataModel bioverifyDataMapp(OrderRequest2 order)
        {
            BiomerticDataModel resp = new BiomerticDataModel();

            resp.status = order.status;
            resp.create_date = DateTime.Now.ToString();

            if (order.purpose_number != null)
                resp.purpose_number = (int)order.purpose_number;
            if (order.dest_doc_type_no != null)
                resp.dest_doc_type_no = order.dest_doc_type_no.ToString();
            if (!String.IsNullOrEmpty(order.dest_nid))
                resp.dest_doc_id = order.dest_nid;
            if (!String.IsNullOrEmpty(order.retailer_id))
                resp.user_id = order.retailer_id;
            if (!String.IsNullOrEmpty(order.msisdn))
                resp.msisdn = order.msisdn;

            resp.dest_ec_verification_required = 0;
            //if (order.dest_ec_verifi_reqrd != null)
            //    resp.dest_ec_verification_required = (int)order.dest_ec_verifi_reqrd;
            if (!String.IsNullOrEmpty(order.dest_imsi))
                resp.dest_imsi = order.dest_imsi;
            if (order.dest_foreign_flag != null)
                resp.dest_foreign_flag = (int)order.dest_foreign_flag;
            if (order.sim_category != null)
            {
                resp.sim_category = (int)order.sim_category;
            }
            else
            {
                resp.sim_category = 0;
            }
            if (!String.IsNullOrEmpty(order.poc_number))
                resp.poc_number = order.poc_number;

            resp.dest_dob = order.dest_dob;
            resp.dest_left_thumb = order.dest_left_thumb;
            resp.dest_left_index = order.dest_left_index;
            resp.dest_right_thumb = order.dest_right_thumb;
            resp.dest_right_index = order.dest_right_index;

            if (order.src_doc_type_no != null)
                resp.src_doc_type_no = order.src_doc_type_no.ToString();
            if (order.src_ec_verifi_reqrd != null)
                resp.src_ec_verification_required = (int)order.src_ec_verifi_reqrd;
            if (order.sim_replacement_type != null)
                resp.sim_replacement_type = (int)order.sim_replacement_type;
            if (!String.IsNullOrEmpty(order.src_dob))
                resp.src_dob = order.src_dob;
            if (!String.IsNullOrEmpty(order.src_nid))
                resp.src_doc_id = order.src_nid;

            return resp;

        }

        public async Task<bool> CheckIsUserInBarrier(MSISDNValidationReqForMigration mSISDNCheckRequest)
        {

            BL_Json byteArrayConverter = new BL_Json();
            ApiCall genericApiCall = new ApiCall();
            LogModel log = new LogModel();
            bool res = true;
            string meathodUrl = String.Format(GetAPICollection.GetBARExceptionChecking, mSISDNCheckRequest.dbss_subscription_id);
            JObject dbssResp = new JObject();

            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;
            try
            {
                log.req_string = meathodUrl;
                log.req_blob = byteArrayConverter.GetGenericJsonData(meathodUrl);

                try
                {
                    dbssResp = (JObject)genericApiCall.HttpGetRequest(meathodUrl, out reqTime, out resTime);
                }
                catch (Exception ex)
                {
                    throw new Exception("DBSS: BAR " + ex.Message);
                }

                log.req_time = reqTime;
                log.res_time = resTime;

                if (dbssResp == null)
                {
                    log.res_time = resTime;
                    log.res_string = "DBSS: BAR dbssResp is null";
                    log.res_blob = byteArrayConverter.GetGenericJsonData("DBSS: BAR dbssResp is null");
                    log.is_success = 0;
                    return false;
                }

                log.res_string = JsonConvert.SerializeObject(dbssResp.ToString()).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(dbssResp);


                if (dbssResp["data"] != null)
                {
                    int total = dbssResp["data"].Count();

                    if (total > 0)
                    {
                        for (int i = 0; i < total; i++)
                        {
                            var exception = "";
                            try
                            {
                                exception = dbssResp["data"][i]["relationships"]["barring"]["data"]["id"].ToString();
                            }
                            catch (Exception)
                            {
                                throw new Exception("Data not found in relationships field!");
                            }
                            if (exception != null)
                            {
                                if (exception.Contains("BARALL") || exception.Contains("BAR_EXCEPTION"))
                                {
                                    throw new Exception("Customer is barred. Pls contact with your supervisor.");
                                }
                            }
                        }

                    }
                }

                log.is_success = 1;
                return res;
            }
            catch (Exception ex)
            {
                res = false;
                ErrorDescription error = null;
                try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "BIA"); }
                catch { }
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(dbssResp != null ? dbssResp.ToString() : ex.Message).ToString();
                string errorMessage = dbssResp != null ? dbssResp.ToString() : ex.Message;
                log.res_blob = byteArrayConverter.GetGenericJsonData(errorMessage);
                log.message = error != null ? error.error_description : null;
                log.error_code = error != null ? error.error_code : null;
                log.error_source = error != null ? error.error_source : "DBSS";
                log.is_success = 0;
                return res;
            }
            finally
            {
                log.msisdn = mSISDNCheckRequest.mobile_number;
                log.purpose_number = mSISDNCheckRequest.purpose_number;
                log.user_id = mSISDNCheckRequest.retailer_id;
                log.integration_point_from = (int)IntegrationPoints.BI;
                log.integration_point_to = (int)IntegrationPoints.BSS;
                log.method_name = "CheckIsUserInBarrier";
                await _bllLog.BALogInsert(log);
            }
        }
    }
}
