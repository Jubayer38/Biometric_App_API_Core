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
    [Route("api/POC")]
    [ApiController]
    public class POCController : ControllerBase
    {
        private readonly BLLRAToDBSSParse _raToDBssParse;
        private readonly BLLDBSSToRAParse _dbssToRaParse;
        private readonly ApiRequest _apiReq;
        private readonly BL_Json _blJson;
        private readonly BLLOrder _orderManager;
        private readonly BLLLog _bllLog;
        private readonly BaseController _bio;
        private readonly ApiManager _apiManager;
        private readonly GeoFencingValidation _geo;

        public POCController(BLLRAToDBSSParse raToDBssParse, BLLDBSSToRAParse dbssToRaParse, ApiRequest apiReq, BL_Json blJson, BLLOrder orderManager, BLLLog bllLog, BaseController bio, ApiManager apiManager, GeoFencingValidation geo)
        {
            _raToDBssParse = raToDBssParse;
            _dbssToRaParse = dbssToRaParse;
            _apiReq = apiReq;
            _blJson = blJson; 
            _orderManager = orderManager;
            _bllLog = bllLog;
            _bio = bio;
            _apiManager = apiManager;
            _geo = geo;
        }

        #region POC Send Order API
        /// Send Order
        /// <summary>
        /// This API is used for POC submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("POCSubmitOrderV1")]
        public async Task<IActionResult> POCSubmitOrderV1([FromBody] RAOrderRequest model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new WebException(MessageCollection.InvalidSecurityToken);

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
                model.poc_msisdn_number = model.msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number)
                && Convert.ToInt16(model.purpose_number).Equals((int)EnumPurposeNumber.SIMReplacement))
                {
                    model.sim_replacement_type = (int)EnumSIMReplacementType.BulkSIMReplacment;
                }
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                orderRes = await _orderManager.SubmitOrder3(model);

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

                        var pursedData = _orderManager.SubmitOrderDataPurse(model);
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
                ErrorDescription error;
                log.is_success = 0;
                model.status = (int)EnumRAOrderStatus.Failed;

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
                log.req_time = DateTime.Now;
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
                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.request_id;
                log.method_name = "POCSubmitOrderV1";
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
        /// This API is used for POC submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns> 
        [HttpPost]
        [Route("POCSubmitOrderV2")]
        public async Task<IActionResult> POCSubmitOrderV2([FromBody] RAOrderRequestV2 model)
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
                model.poc_msisdn_number = model.msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number)
                && Convert.ToInt16(model.purpose_number).Equals((int)EnumPurposeNumber.SIMReplacement))
                {
                    model.sim_replacement_type = (int)EnumSIMReplacementType.BulkSIMReplacment;
                }
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                orderRes = await _orderManager.SubmitOrderV4(model, loginProviderId);

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
                model.status = (int)EnumRAOrderStatus.Failed;
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
                log.req_time = DateTime.Now;
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
                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.request_id;
                log.method_name = "POCSubmitOrderV1";
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
        /// This API is used for POC submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns> 
        [HttpPost]
        [Route("POCSubmitOrderV3")]
        public async Task<IActionResult> POCSubmitOrderV3([FromBody] RAOrderRequestV2 model)
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
                model.poc_msisdn_number = model.msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number)
                && Convert.ToInt16(model.purpose_number).Equals((int)EnumPurposeNumber.SIMReplacement))
                {
                    model.sim_replacement_type = (int)EnumSIMReplacementType.BulkSIMReplacment;
                }
                #region Insert_Order
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
                model.status = (int)EnumRAOrderStatus.Failed;
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
                log.req_time = DateTime.Now;
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
                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.request_id;
                log.method_name = "POCSubmitOrderV3";
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
        /// This API is used for POC submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns> 
        [HttpPost]
        [Route("POCSubmitOrderV4")]
        public async Task<IActionResult> POCSubmitOrderV4([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
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
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
                    allowedDistance = Convert.ToDouble(configuration.GetSection("AppSettings:GeofencingDistance").Value);
                    geoFencEnable = Convert.ToInt32(configuration.GetSection("AppSettings:GeofencingDistanceCalculateEnable").Value);
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
                model.poc_msisdn_number = model.msisdn;

                if (!String.IsNullOrEmpty(model.poc_msisdn_number)
                && Convert.ToInt16(model.purpose_number).Equals((int)EnumPurposeNumber.SIMReplacement))
                {
                    model.sim_replacement_type = (int)EnumSIMReplacementType.BulkSIMReplacment;
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
                        message = orderRes.message,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
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
                            isError = orderRes.isError,
                            message = orderRes.message,
                            data = new Datas()
                            {
                                isEsim = 0,
                                request_id = orderRes.data != null ? orderRes.data.request_id : "0",
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                model.status = (int)EnumRAOrderStatus.Failed;
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
                        orderRes.data = new DataRes()
                        {
                            request_id = ""
                        };
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
                log.req_time = DateTime.Now;
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
                log.method_name = "POCSubmitOrderV4";
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
                    request_id = orderRes.data != null ? orderRes.data.request_id : "0",
                }
            });
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
            resp.user_id = order.retailer_id;
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
            resp.dest_left_thumb = order.dest_left_thumb;
            resp.dest_left_index = order.dest_left_index;
            resp.dest_right_thumb = order.dest_right_thumb;
            resp.dest_right_index = order.dest_right_index;

            if (!String.IsNullOrEmpty(order.poc_number))
                resp.poc_number = order.poc_number;
            if (order.sim_replacement_type != null)
                resp.sim_replacement_type = (int)order.sim_replacement_type;
            if (order.src_doc_type_no != null)
                resp.src_doc_type_no = order.src_doc_type_no.ToString();

            return resp;
        }

    }
}
