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
using BIA.JWT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace BIA.Controllers
{
    [Route("api/TwoParty")]
    [ApiController]
    public class TwoPartyController : ControllerBase
    {
        private readonly ApiRequest _apiReq;
        private readonly BL_Json _blJson;
        private readonly BLLOrder _orderManager;
        private readonly BLLLog _bllLog;
        private readonly BiometricApiCall _apiCall;
        private readonly BaseController _bio;
        private readonly GeoFencingValidation _geo;
        private readonly BLLDBSSToRAParse _dbssToRaParse;
        private readonly ApiManager _apiManager;
        private readonly IConfiguration _configuration;

        public TwoPartyController(ApiRequest apiReq, BL_Json blJson, BLLOrder orderManager, BLLLog bllLog, BiometricApiCall apiCall, BaseController bio, GeoFencingValidation geo, BLLDBSSToRAParse dbssToRaParse, ApiManager apiManager, IConfiguration configuration)
        {
            _apiReq = apiReq;
            _blJson = blJson; 
            _orderManager = orderManager;
            _bllLog = bllLog;
            _apiCall = apiCall;
            _bio = bio;
            _geo = geo;
            _dbssToRaParse = dbssToRaParse;
            _apiManager = apiManager;
            _configuration = configuration;
        }


        #region TwoParty submit Order API
        /// Send Order
        /// <summary>
        /// This API is used for SimReplacement submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("TwoPartySubmitOrderV1")]
        public async Task<IActionResult> TwoPartySubmitOrderV1([FromBody] RAOrderRequest model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new WebException(MessageCollection.InvalidSecurityToken);

                log.req_time = DateTime.Now;

                #region request_blob
                RAOrderRequest objData = new RAOrderRequest();
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

                //======Only for POC & TwoParty Varification (purpose: SIM Transfer(5), B2C to B2B(11)): MSISDN = POC_NUMBER=======
                //Here, we save data as MSISDN = POC_NUMBER, because for making decission wheather the request contains POC number or not.
                //for purpose Corporate_To_Individual_Transfer if src customer validation is done through OTP, 
                //then we save data as POC_NUMBER = POC_NUMBER. In this case reseller app sends POC_NUMBER.
                switch (Convert.ToInt16(model.purpose_number))
                {
                    case (int)EnumPurposeNumber.SIMTransfer:
                        model.poc_msisdn_number = model.msisdn;
                        break;

                    case (int)EnumPurposeNumber.CorporateToIndividualTransfer:
                        model.poc_msisdn_number = String.IsNullOrEmpty(model.otp) ? model.msisdn : model.poc_msisdn_number;
                        break;

                    case (int)EnumPurposeNumber.IndividualToCorporateTransfer:
                        model.poc_msisdn_number = model.msisdn;
                        break;
                }
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                orderRes = await _orderManager.SubmitOrder3(model);

                if (!orderRes.is_success)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = orderRes.message
                    });
                }
                model.bi_token_number = Convert.ToDouble(orderRes.request_id);
                #endregion
                #region bio verification

                var parsedData = _orderManager.SubmitOrderDataPurse(model);
                BiomerticDataModel dataModel = bioverifyDataMapp(parsedData);
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

                //=====Order submission=====

            }
            catch (Exception ex)
            {
                model.status = 150;
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
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.is_success = false;
                    orderRes.message = ex.Message;
                    model.err_msg = ex.Message;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
            }
            finally
            {
                log.purpose_number = model.purpose_number;
                log.msisdn = _bllLog.FormatMSISDN(model.msisdn);
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
                if(orderRes != null)
                {
                    log.bi_token_number = orderRes.request_id;
                    log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                }
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);                
                log.method_name = "TwoPartySubmitOrderV1";
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
        [HttpPost]
        [Route("TwoPartySubmitOrderV2")]
        public async Task<IActionResult> TwoPartySubmitOrderV2([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
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

                log.req_time = DateTime.Now;

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

                //======Only for POC & TwoParty Varification (purpose: SIM Transfer(5), B2C to B2B(11)): MSISDN = POC_NUMBER=======
                //Here, we save data as MSISDN = POC_NUMBER, because for making decission wheather the request contains POC number or not.
                //for purpose Corporate_To_Individual_Transfer if src customer validation is done through OTP, 
                //then we save data as POC_NUMBER = POC_NUMBER. In this case reseller app sends POC_NUMBER.
                switch (Convert.ToInt16(model.purpose_number))
                {
                    case (int)EnumPurposeNumber.SIMTransfer:
                        model.poc_msisdn_number = model.msisdn;
                        break;

                    case (int)EnumPurposeNumber.CorporateToIndividualTransfer:
                        model.poc_msisdn_number = String.IsNullOrEmpty(model.otp) ? model.msisdn : model.poc_msisdn_number;
                        break;

                    case (int)EnumPurposeNumber.IndividualToCorporateTransfer:
                        model.poc_msisdn_number = model.msisdn;
                        break;
                }

                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                orderRes = await _orderManager.SubmitOrderV4(model, loginProviderId);

                if (!orderRes.is_success)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = orderRes.message
                    });
                }
                model.bi_token_number = Convert.ToDouble(orderRes.request_id);
                #endregion
                #region bio verification

                var parsedData = await _orderManager.SubmitOrderDataPurseV2(model);
                BiomerticDataModel dataModel = bioverifyDataMapp(parsedData);
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

                //=====Order submission=====

            }
            catch (Exception ex)
            {
                model.status = 150;
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
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.is_success = false;
                    orderRes.message = ex.Message;
                    model.err_msg = ex.Message;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
            }
            finally
            {
                log.purpose_number = model.purpose_number;
                log.msisdn = _bllLog.FormatMSISDN(model.msisdn);
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
                if(orderRes != null)
                {
                    log.bi_token_number = orderRes.request_id;
                    log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                }
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.method_name = "TwoPartySubmitOrderV2";
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
        [HttpPost]
        [Route("TwoPartySubmitOrderV3")]
        public async Task<IActionResult> TwoPartySubmitOrderV3([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
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

                log.req_time = DateTime.Now;

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

                //======Only for POC & TwoParty Varification (purpose: SIM Transfer(5), B2C to B2B(11)): MSISDN = POC_NUMBER=======
                //Here, we save data as MSISDN = POC_NUMBER, because for making decission wheather the request contains POC number or not.
                //for purpose Corporate_To_Individual_Transfer if src customer validation is done through OTP, 
                //then we save data as POC_NUMBER = POC_NUMBER. In this case reseller app sends POC_NUMBER.
                switch (Convert.ToInt16(model.purpose_number))
                {
                    case (int)EnumPurposeNumber.SIMTransfer:
                        model.poc_msisdn_number = model.msisdn;
                        break;

                    case (int)EnumPurposeNumber.CorporateToIndividualTransfer:
                        model.poc_msisdn_number = String.IsNullOrEmpty(model.otp) ? model.msisdn : model.poc_msisdn_number;
                        break;

                    case (int)EnumPurposeNumber.IndividualToCorporateTransfer:
                        model.poc_msisdn_number = model.msisdn;
                        break;
                }
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;

                orderRes = await _orderManager.SubmitOrderV5(model, loginProviderId);

                if (!orderRes.is_success)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = orderRes.message
                    });
                }
                model.bi_token_number = Convert.ToDouble(orderRes.request_id);
                #endregion
                #region bio verification

                var parsedData = await _orderManager.SubmitOrderDataPurseV2(model);
                BiomerticDataModel dataModel = bioverifyDataMapp(parsedData);
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

                //=====Order submission=====

            }
            catch (Exception ex)
            {
                model.status = 150;
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
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.is_success = false;
                    orderRes.message = ex.Message;
                    model.err_msg = ex.Message;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
            }
            finally
            {
                log.purpose_number = model.purpose_number;
                log.msisdn = _bllLog.FormatMSISDN(model.msisdn);
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
                if(orderRes != null)
                {
                    log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                    log.bi_token_number = orderRes.request_id;
                }                
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);                
                log.method_name = "TwoPartySubmitOrderV3";
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
        [HttpPost]
        [Route("TwoPartySubmitOrderV4")]
        public async Task<IActionResult> TwoPartySubmitOrderV4([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
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

                log.req_time = DateTime.Now;

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

                //======Only for POC & TwoParty Varification (purpose: SIM Transfer(5), B2C to B2B(11)): MSISDN = POC_NUMBER=======
                //Here, we save data as MSISDN = POC_NUMBER, because for making decission wheather the request contains POC number or not.
                //for purpose Corporate_To_Individual_Transfer if src customer validation is done through OTP, 
                //then we save data as POC_NUMBER = POC_NUMBER. In this case reseller app sends POC_NUMBER.
                switch (Convert.ToInt16(model.purpose_number))
                {
                    case (int)EnumPurposeNumber.SIMTransfer:
                        model.poc_msisdn_number = model.msisdn;
                        break;

                    case (int)EnumPurposeNumber.CorporateToIndividualTransfer:
                        model.poc_msisdn_number = String.IsNullOrEmpty(model.otp) ? model.msisdn : model.poc_msisdn_number;
                        break;

                    case (int)EnumPurposeNumber.IndividualToCorporateTransfer:
                        model.poc_msisdn_number = model.msisdn;
                        break;
                }
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;

                orderRes = await _orderManager.SubmitOrderV7(model, loginProviderId);

                if (orderRes.isError)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = orderRes.message
                    });
                }
                model.bi_token_number = orderRes.data != null ? Convert.ToDouble(orderRes.data.request_id) : 0;
                #endregion
                #region bio verification

                var parsedData = await _orderManager.SubmitOrderDataPurseV2(model);
                BiomerticDataModel dataModel = bioverifyDataMapp(parsedData);
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

                //=====Order submission=====

            }
            catch (Exception ex)
            {
                model.status = 150;
                ErrorDescription error;
                log.is_success = 0;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    if (verifyResp != null)
                    {
                        orderRes.data = new DataRes()
                        {
                            request_id = verifyResp.bss_req_id
                        };
                    }
                    else
                    {
                        orderRes.data.request_id = "";
                    }
                    orderRes.isError = true;
                    orderRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.isError = true;
                    orderRes.message = ex.Message;
                    model.err_msg = ex.Message;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
            }
            finally
            {
                log.purpose_number = model.purpose_number;
                log.msisdn = _bllLog.FormatMSISDN(model.msisdn);
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
                if (orderRes.data != null)
                {
                    log.is_success = orderRes.data.request_id.Length > 1 ? 1 : 0;
                    log.bi_token_number = orderRes.data.request_id;
                }
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.method_name = "TwoPartySubmitOrderV3";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.user_id = model.retailer_id;
                log.remarks = model.bi_token_number != null
                               && model.bi_token_number > 1 ? "Resubmit order" : String.Empty;
                await _bllLog.RAToDBSSLog(log, "", "");
            }
            return Ok(orderRes);
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
            if (!String.IsNullOrEmpty(order.optional3))
                resp.otp_no = order.optional3;
            if (order.src_doc_type_no != null)
                resp.src_doc_type_no = order.src_doc_type_no.ToString();
            if (!String.IsNullOrEmpty(order.dest_nid))
                resp.dest_doc_id = order.dest_nid;
            if (!String.IsNullOrEmpty(order.retailer_id))
                resp.user_id = order.retailer_id;
            if (!String.IsNullOrEmpty(order.msisdn))
                resp.msisdn = order.msisdn;
            if (order.dest_ec_verifi_reqrd != null)
                resp.dest_ec_verification_required = (int)order.dest_ec_verifi_reqrd;
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
            resp.dest_dob = order.dest_dob;
            resp.src_dob = order.src_dob;
            resp.dest_left_thumb = order.dest_left_thumb;
            resp.dest_left_index = order.dest_left_index;
            resp.dest_right_thumb = order.dest_right_thumb;
            resp.dest_right_index = order.dest_right_index;
            resp.src_left_index = order.src_left_index;
            resp.src_left_thumb = order.src_left_thumb;
            resp.src_right_index = order.src_right_index;
            resp.src_right_thumb = order.src_right_thumb;
            if (order.sim_replacement_type != null)
                resp.sim_replacement_type = (int)order.sim_replacement_type;
            if (!String.IsNullOrEmpty(order.src_nid))
                resp.src_doc_id = order.src_nid;
            if (order.src_ec_verifi_reqrd != null)
                resp.src_ec_verification_required = (int)order.src_ec_verifi_reqrd;
            resp.poc_number = order.poc_number;
            return resp;

        }

    }
}
