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
    [Route("api/BioCancelation")]
    [ApiController]
    public class BioCancelationController : ControllerBase
    {
        private readonly BLLRAToDBSSParse _raToDBssParse;
        private readonly BLLDBSSToRAParse _dbssToRaParse;
        private readonly ApiRequest _apiReq;
        private readonly BLLCommon _bllCommon;
        private readonly BL_Json _blJson;
        private readonly BLLOrder _bllOrder;
        private readonly ApiManager _apiManager;
        private readonly BLLLog _bllLog;
        private readonly BaseController _bio;
        private readonly GeoFencingValidation _geo;

        public BioCancelationController(BLLRAToDBSSParse raToDBssParse, BLLDBSSToRAParse dbssToRaParse, ApiRequest apiReq, BLLCommon bllCommon, BL_Json blJson, BLLOrder bllOrder, ApiManager apiManager, BLLLog bllLog, BaseController bio, GeoFencingValidation geo)
        {
            this._bllCommon = bllCommon;
            this._raToDBssParse = raToDBssParse;
            this._dbssToRaParse = dbssToRaParse;
            this._apiReq = apiReq;
            this._blJson = blJson;
            this._bllOrder = bllOrder;
            this._apiManager = apiManager;
            this._bllLog = bllLog;
            _bio = bio;
            _geo = geo;
        }

        #region Bio-Cancel MSISDN validation  
        /// <summary>
        /// This API is used for MSISDN validation for paired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForBioCancelV1")]
        public async Task<IActionResult> ValidateMSISDNForBioCancelV1([FromBody] BioCancelMSISDNValidationReq msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            RACommonResponse raRespModel = new RACommonResponse();
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.GetSubscriptionByMSISDNIncludingCustomerInfo, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;
                
                if (dbssResp != null)
                {
                    if (dbssResp["data"] == null || dbssResp["included"] == null)
                    {
                        return Ok(new RACommonResponse()
                        {
                            result = false,
                            message = MessageCollection.MSISDNInvalid
                        });
                    }

                    if (dbssResp.Property("data") != null && dbssResp.Property("included") != null)
                    {
                        VMBioCancelMSISDNValidationReqParsing dbssRespParseResp = _dbssToRaParse.BioCancelMSISDNValidationReqParsing(dbssResp);
                        if (dbssRespParseResp.result == true
                            && !String.IsNullOrEmpty(dbssRespParseResp.nid)
                            && !String.IsNullOrEmpty(dbssRespParseResp.dob))
                        {
                            if (msisdnCheckReqest.nid.Equals(dbssRespParseResp.nid)
                                && DateTime.Parse(msisdnCheckReqest.dob).ToString(StringFormatCollection.DBSSDOBFormat).Equals(dbssRespParseResp.dob))
                            {
                                return Ok(new RABioCancelResp()
                                {
                                    dbss_subscription_id = dbssRespParseResp.subscription_id,
                                    dest_sim_category = dbssRespParseResp.dest_sim_category,
                                    result = true,
                                    message = MessageCollection.MSISDNValid
                                });
                            }
                            else
                            {
                                raRespModel.result = false;
                                raRespModel.message = "NID or DOB didn't match.";
                            }
                        }
                        else
                        {
                            raRespModel.result = false;
                            raRespModel.message = dbssRespParseResp.message;
                        }
                    }
                }                
                else
                {
                    raRespModel.result = false;
                    raRespModel.message = "No data found!";
                }
            }
            catch (Exception ex)
            {

                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;

                raRespModel.result = false;
                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidateMSISDNForBioCancelV1";

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);

            }
            return Ok(raRespModel);
        }

        /// <summary>
        /// This API is used for MSISDN validation for paired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForBioCancelV2")]
        public async Task<IActionResult> ValidateMSISDNForBioCancelV2([FromBody] BioCancelMSISDNValidationReq msisdnCheckReqest)
        {
            string apiUrl = "", txtResp = "";
            RACommonResponse raRespModel = new RACommonResponse();
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.GetSubscriptionByMSISDNIncludingCustomerInfo, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = MessageCollection.MSISDNInvalid
                    });
                }

                if (dbssResp.Property("data").HasValues && dbssResp.Property("included").HasValues)
                {
                    VMBioCancelMSISDNValidationReqParsing dbssRespParseResp = _dbssToRaParse.BioCancelMSISDNValidationReqParsing(dbssResp);
                    if (dbssRespParseResp.result == true
                        && !String.IsNullOrEmpty(dbssRespParseResp.nid)
                        && !String.IsNullOrEmpty(dbssRespParseResp.dob))
                    {
                        if (msisdnCheckReqest.nid.Equals(dbssRespParseResp.nid)
                            && DateTime.Parse(msisdnCheckReqest.dob).ToString(StringFormatCollection.DBSSDOBFormat).Equals(dbssRespParseResp.dob))
                        {
                            return Ok(new RABioCancelResp()
                            {
                                dbss_subscription_id = dbssRespParseResp.subscription_id,
                                dest_sim_category = dbssRespParseResp.dest_sim_category,
                                result = true,
                                message = MessageCollection.MSISDNValid
                            });
                        }
                        else
                        {
                            raRespModel.result = false;
                            raRespModel.message = "NID or DOB didn't match.";
                        }
                    }
                    else
                    {
                        raRespModel.result = false;
                        raRespModel.message = dbssRespParseResp.message;
                    }
                }
                else
                {
                    raRespModel.result = false;
                    raRespModel.message = "No data found!";
                }
            }
            catch (Exception ex)
            {

                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;

                raRespModel.result = false;
                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
            }

            finally
            {

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidateMSISDNForBioCancelV1";

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);

            }
            return Ok(raRespModel);
        }

        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForBioCancelV3")]
        public async Task<IActionResult> ValidateMSISDNForBioCancelV3([FromBody] BioCancelMSISDNValidationReq msisdnCheckReqest)
        {
            string apiUrl = "", txtResp = "";
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

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

                apiUrl = String.Format(GetAPICollection.GetSubscriptionByMSISDNIncludingCustomerInfo, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = MessageCollection.MSISDNInvalid
                    });
                }

                if (dbssResp.Property("data").HasValues && dbssResp.Property("included").HasValues)
                {
                    VMBioCancelMSISDNValidationReqParsing dbssRespParseResp = _dbssToRaParse.BioCancelMSISDNValidationReqParsing(dbssResp);
                    if (dbssRespParseResp.result == true
                        && !String.IsNullOrEmpty(dbssRespParseResp.nid)
                        && !String.IsNullOrEmpty(dbssRespParseResp.dob))
                    {
                        if (msisdnCheckReqest.nid.Equals(dbssRespParseResp.nid)
                            && DateTime.Parse(msisdnCheckReqest.dob).ToString(StringFormatCollection.DBSSDOBFormat).Equals(dbssRespParseResp.dob))
                        {
                            return Ok(new RABioCancelRespRev()
                            {
                                isError = false,
                                message = MessageCollection.MSISDNValid,
                                data = new RABioCancelResData()
                                {
                                    dbss_subscription_id = dbssRespParseResp.subscription_id,
                                    dest_sim_category = dbssRespParseResp.dest_sim_category
                                }
                            });
                        }
                        else
                        {
                            raRespModel.isError = true;
                            raRespModel.message = "NID or DOB didn't match.";
                        }
                    }
                    else
                    {
                        raRespModel.isError = true;
                        raRespModel.message = dbssRespParseResp.message;
                    }
                }
                else
                {
                    raRespModel.isError = true;
                    raRespModel.message = "No data found!";
                }
            }
            catch (Exception ex)
            {

                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;

                raRespModel.isError = true;
                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
            }

            finally
            {

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidateMSISDNForBioCancelV3";

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);

            }
            return Ok(raRespModel);
        }


        #endregion


        #region Bio Cancel Submit-Order API
        /// Send Order
        /// <summary>
        /// This API is used for all kind of submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("BioCancelSubmitOrderV1")]
        public async Task<IActionResult> BioCancelSubmitOrderV1([FromBody] RAOrderRequest model)
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

                //=========Hard Coded: Because at this moment(01-12-19 : 5.48pm) from RA right is comming with wrong number. 
                model.right_id = 33;//right id for registration cancellation

                //==== Check if submitted order is already in process or not.=====
                var orderValidationResult = await _bllOrder.ValidateOrder(new VMValidateOrder { msisdn = model.msisdn, sim_number = model.sim_number, purpose_number = Convert.ToInt32(model.purpose_number), retailer_id = model.retailer_id, dest_dob = DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat) });
                if (orderValidationResult.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }

                #region Insert_Order
                model.status = model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                orderRes = await _bllOrder.SubmitOrder3(model);

                if (!orderRes.is_success)
                {
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = orderRes.message
                    });
                }
                model.bi_token_number = Convert.ToDouble(orderRes.request_id);
                #endregion
                #region bio verification

                var pardedData = _bllOrder.SubmitOrderDataPurse(model);
                BiomerticDataModel dataModel = bioverifyDataMapp(pardedData);
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
                model.status = 150;
                ErrorDescription error;
                log.is_success = 0;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    orderRes.request_id = null;
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
                log.msisdn = model.msisdn;
                log.req_time = DateTime.Now;
                response2 = await _bllOrder.UpdateOrder(new RAOrderRequestUpdate
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

                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = model.bi_token_number.ToString();
                log.method_name = "BioCancelSubmitOrderV1";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.user_id = model.retailer_id;
                log.remarks = model.bi_token_number != null
                               && model.bi_token_number > 1 ? "Resubmit order" : String.Empty;

                await _bllLog.RAToDBSSLog(log, "", "");
            }
            return Ok(orderRes);
        }


        [HttpPost("BioCancelSubmitOrderV2")]
        public async Task<IActionResult> BioCancelSubmitOrderV2([FromBody] RAOrderRequestV2 model)
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
                RAOrderRequestV2 objData = model;
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

                //=========Hard Coded: Because at this moment(01-12-19 : 5.48pm) from RA right is coming with the wrong number. 
                model.right_id = 33; // right id for registration cancellation

                //==== Check if the submitted order is already in process or not.=====
                var orderValidationResult = await _bllOrder.ValidateOrder(new VMValidateOrder
                {
                    msisdn = model.msisdn,
                    sim_number = model.sim_number,
                    purpose_number = Convert.ToInt32(model.purpose_number),
                    retailer_id = model.retailer_id,
                    dest_dob = DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat)
                });

                if (orderValidationResult.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }

                #region Insert_Order
                model.status = model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                orderRes = await _bllOrder.SubmitOrderV4(model, loginProviderId);

                if (!orderRes.is_success)
                {
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = orderRes.message
                    });
                }
                model.bi_token_number = Convert.ToDouble(orderRes.request_id);
                #endregion

                #region bio verification
                var pardedData = await _bllOrder.SubmitOrderDataPurseV2(model);
                BiomerticDataModel dataModel = bioverifyDataMapp(pardedData);
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
                model.status = 150;
                ErrorDescription error;
                log.is_success = 0;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    orderRes.request_id = null;
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
                log.msisdn = model.msisdn;
                log.req_time = DateTime.Now;
                response2 = await _bllOrder.UpdateOrder(new RAOrderRequestUpdate
                {
                    bi_token_number = model.bi_token_number,
                    dest_imsi = model.dest_imsi,
                    user_name = model.retailer_id,
                    msidn = model.msisdn,
                    status = model.status,
                    bss_reqId = model.bss_reqId,
                    error_id = model.error_id,
                    err_msg = model.err_msg,
                });
                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = model.bi_token_number.ToString();
                log.method_name = "BioCancelSubmitOrderV2";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.user_id = model.retailer_id;
                log.remarks = model.bi_token_number != null && model.bi_token_number > 1 ? "Resubmit order" : String.Empty;

                await _bllLog.RAToDBSSLog(log, "", "");
            }

            return Ok(orderRes);
        }


        [HttpPost]
        [Route("BioCancelSubmitOrderV3")]
        public async Task<IActionResult> BioCancelSubmitOrderV3([FromBody] RAOrderRequestV2 model)
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
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = "Invalid Security Token",
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
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

                //=========Hard Coded: Because at this moment(01-12-19 : 5.48pm) from RA right is comming with wrong number. 
                model.right_id = 33;//right id for registration cancellation

                //==== Check if submitted order is already in process or not.=====
                var orderValidationResult = await _bllOrder.ValidateOrder(new VMValidateOrder { msisdn = model.msisdn, sim_number = model.sim_number, purpose_number = Convert.ToInt32(model.purpose_number), retailer_id = model.retailer_id, dest_dob = DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat) });
                if (orderValidationResult.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                #region Insert_Order
                model.status = model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                orderRes = await _bllOrder.SubmitOrderV5(model, loginProviderId);

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

                var pardedData = await _bllOrder.SubmitOrderDataPurseV2(model);
                BiomerticDataModel dataModel = bioverifyDataMapp(pardedData);
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

                    orderRes.request_id = null;
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
                log.msisdn = model.msisdn;
                log.req_time = DateTime.Now;
                response2 = await _bllOrder.UpdateOrder(new RAOrderRequestUpdate
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

                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = model.bi_token_number.ToString();
                log.method_name = "BioCancelSubmitOrderV3";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.user_id = model.retailer_id;
                log.remarks = model.bi_token_number != null
                               && model.bi_token_number > 1 ? "Resubmit order" : String.Empty;

                await _bllLog.RAToDBSSLog(log, "", "");

            }
            return Ok(orderRes);
        }

        [HttpPost]
        [Route("BioCancelSubmitOrderV4")]
        public async Task<IActionResult> BioCancelSubmitOrderV4([FromBody] RAOrderRequestV2 model)
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

                //=========Hard Coded: Because at this moment(01-12-19 : 5.48pm) from RA right is comming with wrong number. 
                model.right_id = 33;//right id for registration cancellation

                //==== Check if submitted order is already in process or not.=====
                var orderValidationResult = await _bllOrder.ValidateOrder(new VMValidateOrder { msisdn = model.msisdn, sim_number = model.sim_number, purpose_number = Convert.ToInt32(model.purpose_number), retailer_id = model.retailer_id, dest_dob = DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat) });
                if (orderValidationResult.result == false)
                {
                    orderRes.data = new DataRes { request_id = "0" };
                    orderRes.isError = true;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                #region Insert_Order
                model.status = model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                orderRes = await _bllOrder.SubmitOrderV7(model, loginProviderId);

                if (orderRes.isError)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = orderRes.message
                    });
                }
                model.bi_token_number = Convert.ToDouble(orderRes.data.request_id);
                #endregion
                #region bio verification

                var pardedData = await _bllOrder.SubmitOrderDataPurseV2(model);
                BiomerticDataModel dataModel = bioverifyDataMapp(pardedData);
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

                    orderRes.data.request_id = verifyResp != null ? verifyResp.bss_req_id : "";
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
                log.msisdn = model.msisdn;
                log.req_time = DateTime.Now;
                if (model.bi_token_number != null && model.bi_token_number > 0)
                {
                    response2 = await _bllOrder.UpdateOrder(new RAOrderRequestUpdate
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
                log.is_success = orderRes.data.request_id.Length > 1 ? 1 : 0;
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = model.bi_token_number.ToString();
                log.method_name = "BioCancelSubmitOrderV3";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.user_id = model.retailer_id;
                log.remarks = model.bi_token_number != null
                               && model.bi_token_number > 1 ? "Resubmit order" : String.Empty;

                await _bllLog.RAToDBSSLog(log, "", "");
            }
            return Ok(new RACommonResponseRevamp()
            {
                isError = false,
                message = orderRes.message,
                data = new Datas()
                {
                    isEsim = 0,
                    request_id = orderRes.data.request_id,
                }
            });
        }


        #endregion  
        public BiomerticDataModel bioverifyDataMapp(OrderRequest2 order)
        {
            BiomerticDataModel resp = new BiomerticDataModel();

            resp.purpose_number = (int)order.purpose_number;
            resp.dest_doc_type_no = order.dest_doc_type_no.ToString();
            if (!String.IsNullOrEmpty(order.optional3))
            {
                resp.otp_no = order.optional3;
            }
            if (order.src_doc_type_no != null)
            {
                resp.src_doc_type_no = order.src_doc_type_no.ToString();
            }
            resp.dest_doc_id = order.dest_nid;
            resp.user_id = order.retailer_id;
            resp.msisdn = order.msisdn;
            resp.dest_ec_verification_required = (int)order.dest_ec_verifi_reqrd;
            resp.dest_imsi = order.dest_imsi;
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
            {
                resp.sim_replacement_type = (int)order.sim_replacement_type;
            }
            if (!String.IsNullOrEmpty(order.src_nid))
            {
                resp.src_doc_id = order.src_nid;
            }
            if (order.src_ec_verifi_reqrd != null)
            {
                resp.src_ec_verification_required = (int)order.src_ec_verifi_reqrd;
            }
            return resp;
        }

    }
}
