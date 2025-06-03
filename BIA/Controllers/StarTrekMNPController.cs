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
using BIA.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
//using BIA.Common.ModelValidation;
using System.Net;
using static BIA.Common.ModelValidation;

namespace BIA.Controllers
{
    [Route("api/StarTrekMNP")]
    [ApiController]
    public class StarTrekMNPController : ControllerBase
    {
        private readonly BLLOrder _orderManager;
        private readonly BLLLog _bllLog;
        private readonly BaseController _bio;
        private readonly GeoFencingValidation _geo;
        private readonly BLLDBSSToRAParse _dbssToRaParse;

        public StarTrekMNPController(BLLOrder orderManager, BLLLog bllLog, BaseController bio, GeoFencingValidation geo, BLLDBSSToRAParse dbssToRaParse)
        {
            _orderManager = orderManager;
            _bllLog = bllLog;
            _bio = bio;
            _geo = geo;
            _dbssToRaParse = dbssToRaParse;
        }

        /// Send Order
        /// <summary>
        /// This API is used for MNP PortIn submit order.
        /// </summary> 
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        //[ResponseType(typeof(SendOrderResponse))]
        [HttpPost]
        [Route("MNPPortInSubmitOrder")]
        public async Task<IActionResult> MNPPortInSubmitOrder([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            BL_Json _blJson = new BL_Json();
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

                var validateResponse = modelValidation.OrderSubmitModelValidation(new ValidationPropertiesModel
                {
                    purpose_number = model.purpose_number,
                    msisdn = model.msisdn,
                    customer_name = model.customer_name,
                    gender = model.gender,
                    division_id = model.division_id,
                    district_id = model.district_id,
                    thana_id = model.thana_id,
                    village = model.village
                });
                if (!validateResponse.result)
                {
                    return Ok(new SendOrderResponseRev()
                    {
                        isError = true,
                        message = validateResponse.message
                    });
                }
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
                    orderRes.data = new DataRes()
                    {
                        request_id = "0"
                    };
                    orderRes.isError = true;
                    orderRes.message = orderValidationResult.message;

                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                #endregion
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                ///if (model.bi_token_number == null || model.bi_token_number == 0)
                //{
                orderRes =await _orderManager.SubmitOrderV7(model, loginProviderId);
                if (orderRes.isError)
                {
                    return Ok(new SendOrderResponseRev()
                    {
                        isError = true,
                        message = orderRes.message
                    });
                }

                string requestIdStr = orderRes?.data?.request_id;
                if (requestIdStr != null && double.TryParse(requestIdStr, out var requestId))
                {
                    model.bi_token_number = requestId;
                }
                else
                {
                    model.bi_token_number = 0;
                }
                //}

                //if (model.bi_token_number != null || model.bi_token_number > 1)
                //{
                try
                {
                    #endregion
                    #region unpaired MSISDN validation (MNP)

                    var channelInfo = await _orderManager.GetInventoryIdByChannelName(model.channel_name);

                    if (SettingsValues.GetRyzeAllowOrNot() == 1)
                    {
                        RACommonResponse msisdnValidationResp = await MNPValidateMSISDN(new UnpairedMSISDNCheckRequest()
                        {
                            mobile_number = model.msisdn,
                            sim_number = model.sim_number,
                            channel_id = channelInfo.Item1,
                            channel_name = model.channel_name,
                            center_code = model.center_code,
                            inventory_id = channelInfo.Item2,
                            purpose_number = model.purpose_number,
                            retailer_id = model.retailer_id,
                            sim_category = model.sim_category
                        });

                        if (msisdnValidationResp.result == false)
                        {
                            model.status = (int)EnumRAOrderStatus.Failed;
                            if (orderRes != null)
                                orderRes.data = new DataRes()
                                {
                                    request_id = "0"
                                };
                            if (orderRes != null)
                            {
                                orderRes.isError = true;
                                orderRes.message = msisdnValidationResp.message;
                            }
                            model.err_msg = msisdnValidationResp.message;
                            return Ok(orderRes);
                        }
                    }
                    else
                    {

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

                    //log.req_blob = _blJson.GetGenericJsonData(Convert.ToString(SubmitOrderRequestBindingForLog(model)));

                    #region Get IMSI
                    var imsiResp = await _bio.GetImsiBySimAsync(new GetImsiReq
                    {
                        purpose_number = model.purpose_number,
                        retailer_id = model.retailer_id,
                        sim = model.sim_number,
                        msisdn = model.msisdn
                    });

                    if (imsiResp.result == false)
                    {
                        model.status = (int)EnumRAOrderStatus.Failed;
                        if (orderRes != null)
                            orderRes.data = new DataRes()
                            {
                                request_id = "0"
                            };
                        if (orderRes != null)
                        {
                            orderRes.isError = true;
                            orderRes.message = imsiResp.message;
                        }
                        model.err_msg = imsiResp.message;
                        return Ok(orderRes);
                    }
                    else
                    {
                        model.dest_imsi = imsiResp.imsi;//[Note: here IMSI is being sent as SIM number as per business requirement]
                    }
                    #endregion

                    #region bio verification

                    var pardedData = await _orderManager.SubmitOrderDataPurseV2(model);

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
                    return Ok(orderRes);
                }
                //}
            }
            catch (Exception ex)
            {
                model.status = (int)EnumRAOrderStatus.Failed;
                log.res_time = DateTime.Now;
                ErrorDescription error;
                log.is_success = 0;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    orderRes.data = new DataRes
                    {
                        request_id = verifyResp != null ? verifyResp.bss_req_id : "0"
                    };
                    //orderRes.data.request_id = verifyResp != null ? verifyResp.bss_req_id : "0";
                    orderRes.isError = true;
                    orderRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.isError = true;
                    orderRes.message = ex.Message;
                    model.err_msg = ex.Message;
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
                log.res_time = DateTime.Now;
                if (orderRes != null)
                    if (orderRes.data != null)
                    {
                        log.is_success = orderRes.data.request_id.Length > 1 ? 1 : 0;
                        log.bi_token_number = orderRes.data.request_id;
                    }
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.method_name = "MNPPortInSubmitOrder";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.user_id = model.retailer_id;
                log.remarks = model.bi_token_number != null
                                               && model.bi_token_number > 1 ? "Resubmit order" : String.Empty;
                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, "", ""));
                //logThread.Start();
                await _bllLog.RAToDBSSLog(log, "", "");
            }
            return Ok(orderRes);
        }


        /// Send Order
        /// <summary>
        /// This API is used for MNP PortIn submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        //[ResponseType(typeof(SendOrderResponse))]
        [HttpPost]
        [Route("MNPPortInSubmitOrder_ESIM")]
        public async Task<IActionResult> MNPPortInSubmitOrder_ESIM([FromBody] RAOrderRequestV2 model)
        {
            BL_Json _blJson = new BL_Json();
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

                var validateResponse = modelValidation.OrderSubmitModelValidation(new ValidationPropertiesModel
                {
                    purpose_number = model.purpose_number,
                    msisdn = model.msisdn,
                    customer_name = model.customer_name,
                    gender = model.gender,
                    division_id = model.division_id,
                    district_id = model.district_id,
                    thana_id = model.thana_id,
                    village = model.village
                });
                if (!validateResponse.result)
                {
                    return Ok(new SendOrderResponseRev()
                    {
                        isError = true,
                        message = validateResponse.message
                    });
                }
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
                    orderRes.data = new DataRes { request_id = "0" };
                    orderRes.isError = true;
                    orderRes.message = orderValidationResult.message;

                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                #endregion
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                model.is_esim = 1;
                orderRes = await _orderManager.SubmitOrderV7(model, loginProviderId);
                if (orderRes.isError)
                {
                    return Ok(new SendOrderResponseRev()
                    {
                        isError = true,
                        message = orderRes.message
                    });
                }

                string requestIdStr = orderRes?.data?.request_id;
                if (requestIdStr != null && double.TryParse(requestIdStr, out var requestId))
                {
                    model.bi_token_number = requestId;
                }
                else
                {
                    model.bi_token_number = 0;
                }

                #endregion
                #region unpaired MSISDN validation (MNP)

                var channelInfo = await _orderManager.GetInventoryIdByChannelName(model.channel_name);

                RACommonResponseRevamp msisdnValidationResp = await MNPValidateMSISDNESim(new UnpairedMSISDNCheckRequest()
                {
                    mobile_number = model.msisdn,
                    sim_number = model.sim_number,
                    channel_id = channelInfo.Item1,
                    channel_name = model.channel_name,
                    center_code = model.center_code,
                    inventory_id = channelInfo.Item2,
                    purpose_number = model.purpose_number,
                    retailer_id = model.retailer_id,
                    sim_category = model.sim_category
                });

                if (msisdnValidationResp.isError == true)
                {
                    model.status = (int)EnumRAOrderStatus.Failed;
                    orderRes.data = new DataRes { request_id = "0" };
                    orderRes.isError = true;
                    orderRes.message = msisdnValidationResp.message;
                    return Ok(orderRes);
                }
                #endregion

                #region request_blob
                //RAOrderRequestV2 objData = new RAOrderRequestV2();
                //objData = model;
                //string req_string = JsonConvert.SerializeObject(objData);
                //JObject parsedObj = JObject.Parse(req_string);
                //if (model.dest_left_thumb != null)
                //    parsedObj["dest_left_thumb"] = null;
                //if (model.dest_left_index != null)
                //    parsedObj["dest_left_index"] = null;
                //if (model.dest_right_thumb != null)
                //    parsedObj["dest_right_thumb"] = null;
                //if (model.dest_right_index != null)
                //    parsedObj["dest_right_index"] = null;
                //if (model.src_left_index != null)
                //    parsedObj["src_left_thumb"] = null;
                //if (model.src_left_thumb != null)
                //    parsedObj["src_left_index"] = null;
                //if (model.src_right_index != null)
                //    parsedObj["src_right_thumb"] = null;
                //if (model.src_right_thumb != null)
                //    parsedObj["src_right_index"] = null;

                //log.req_blob = _blJson.GetGenericJsonData(parsedObj.ToString());
                #endregion

                //log.req_blob = _blJson.GetGenericJsonData(Convert.ToString(SubmitOrderRequestBindingForLog(model)));



                #region Get IMSI
                var imsiResp = await _bio.GetImsiBySimAsync(new GetImsiReq
                {
                    purpose_number = model.purpose_number,
                    retailer_id = model.retailer_id,
                    sim = model.sim_number,
                    msisdn = model.msisdn
                });

                if (imsiResp.result == false)
                {
                    model.status = (int)EnumRAOrderStatus.Failed;
                    orderRes.data = new DataRes { request_id = "0" };
                    orderRes.isError = true;
                    orderRes.message = imsiResp.message;
                    model.err_msg = imsiResp.message;
                    return Ok(orderRes);
                }
                else
                {
                    model.dest_imsi = imsiResp.imsi;//[Note: here IMSI is being sent as SIM number as per business requirement]
                }
                #endregion

                #region bio verification

                var pardedData = await _orderManager.SubmitOrderDataPurseV2(model);
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
                }
                #endregion

                //=====Order submission=====

            }
            catch (Exception ex)
            {
                model.status = (int)EnumRAOrderStatus.Failed;
                log.res_time = DateTime.Now;
                ErrorDescription error;
                log.is_success = 0;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    // orderRes.data.request_id = verifyResp != null ? verifyResp.bss_req_id : "0";
                    orderRes.data = new DataRes
                    {
                        request_id = verifyResp != null ? verifyResp.bss_req_id : "0"
                    };
                    orderRes.isError = true;
                    orderRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.isError = true;
                    orderRes.message = ex.Message;
                    model.err_msg = ex.Message;
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
                        dest_imsi = model.dest_imsi,
                        msidn = model.msisdn,
                        user_name = model.retailer_id,
                        status = model.status,
                        bss_reqId = model.bss_reqId,
                        error_id = model.error_id,
                        err_msg = model.err_msg,
                    });
                }

                log.res_time = DateTime.Now;
                log.is_success = orderRes.data.request_id.Length > 1 ? 1 : 0;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.data.request_id;
                log.method_name = "MNPPortInSubmitOrder_ESIM";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.user_id = model.retailer_id;
                log.remarks = model.bi_token_number != null
                                               && model.bi_token_number > 1 ? "Resubmit order" : String.Empty;
                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, "", ""));
                //logThread.Start();
                await _bllLog.RAToDBSSLog(log, "", "");
            }
            return Ok(orderRes);
        }

        public async Task<RACommonResponse> MNPValidateMSISDN(UnpairedMSISDNCheckRequest msisdnCheckReqest)
        {
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            BL_Json _blJson = new BL_Json();
            ApiRequest _apiReq = new ApiRequest();
            RACommonResponse raRespModel = new RACommonResponse();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                var dbssReqModel = _raToDBssParse.ValidateMSISDNReqParsing(msisdnCheckReqest);

                if (dbssReqModel.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    dbssReqModel = FixedValueCollection.MSISDNCountryCode + dbssReqModel;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, dbssReqModel);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                JObject dbssResp = new JObject();

                try
                {
                    log.req_time = DateTime.Now;
                    dbssResp = (JObject)await _apiReq.HttpGetRequestForMNPPortIn(apiUrl);
                    log.res_time = DateTime.Now;
                }
                catch (WebException ex)
                {
                    log.res_time = DateTime.Now;
                    txtResp = Convert.ToString(ex.Message);
                    log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var ErrorResponse = ex.Response as HttpWebResponse;
                        if (ErrorResponse != null && (int)ErrorResponse.StatusCode == 404)
                        {
                            //var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);
                            log.is_success = 1;
                            var simResp = await _bio.CheckSIMNumber3(new SIMNumberCheckRequest()
                            {
                                center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                                distributor_code = "",
                                channel_name = msisdnCheckReqest.channel_name,
                                session_token = msisdnCheckReqest.session_token,
                                sim_number = msisdnCheckReqest.sim_number,
                                retailer_id = msisdnCheckReqest.retailer_id,
                                product_code = "",
                                inventory_id = msisdnCheckReqest.inventory_id,
                                msisdn = msisdnCheckReqest.mobile_number
                            }, (int)EnumPurposeOfSIMCheck.NewConnection, false, msisdnCheckReqest.sim_category, "");

                            if (simResp.result == false)
                            {
                                raRespModel.result = false;
                                raRespModel.message = simResp.message;
                                return raRespModel;
                            }

                            raRespModel.result = true;
                            raRespModel.message = MessageCollection.MSISDNandSIMBothValid;

                            return raRespModel;
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    else
                    {
                        throw ex;
                    }
                }
                //======If DBSS api returnd success==========
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                var simResp2 = await _bio.CheckSIMNumber3(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id
                }, (int)EnumPurposeOfSIMCheck.NewConnection, false, msisdnCheckReqest.sim_category, "");

                if (simResp2.result == false)
                {
                    raRespModel.result = false;
                    raRespModel.message = simResp2.message;
                    return raRespModel;
                }

                raRespModel.result = true;
                raRespModel.message = MessageCollection.MSISDNandSIMBothValid;

                return raRespModel;
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                try
                {
                    var simResp2 = await _bio.CheckSIMNumber3(new SIMNumberCheckRequest()
                    {
                        center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                        distributor_code = "",
                        channel_name = msisdnCheckReqest.channel_name,
                        session_token = msisdnCheckReqest.session_token,
                        sim_number = msisdnCheckReqest.sim_number,
                        retailer_id = msisdnCheckReqest.retailer_id,
                        product_code = "",
                        inventory_id = msisdnCheckReqest.inventory_id
                    }, (int)EnumPurposeOfSIMCheck.NewConnection, false, msisdnCheckReqest.sim_category, "");

                    if (simResp2.result == false)
                    {
                        raRespModel.result = false;
                        raRespModel.message = simResp2.message;
                        return raRespModel;
                    }

                    raRespModel.result = true;
                    raRespModel.message = MessageCollection.MSISDNandSIMBothValid;

                    return raRespModel;
                }
                catch { }
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.is_success = 0;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raRespModel.result = false;
                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return raRespModel;
                }
                catch (Exception)
                {
                    raRespModel.result = false;
                    raRespModel.message = ex.Message;
                    return raRespModel;
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;//userName
                log.method_name = "MNPValidateMSISDN";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);

            }
        }

        public BiomerticDataModel bioverifyDataMapp(OrderRequest2 order)
        {
            BiomerticDataModel resp = new BiomerticDataModel();

            resp.status = order.status;
            resp.create_date = DateTime.Now.ToString();
            if (order.purpose_number != null)
                resp.purpose_number = (int)order.purpose_number;
            resp.dest_doc_type_no = order.dest_doc_type_no.ToString();
            resp.dest_doc_id = order.dest_nid;
            resp.user_id = order.retailer_id;
            resp.msisdn = order.msisdn;
            if (order.dest_ec_verifi_reqrd != null)
                resp.dest_ec_verification_required = (int)order.dest_ec_verifi_reqrd;
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
            if (order.src_doc_type_no != null)
                resp.src_doc_type_no = order.src_doc_type_no.ToString();
            if (order.src_ec_verifi_reqrd != null)
                resp.src_ec_verification_required = (int)order.src_ec_verifi_reqrd;
            if (!String.IsNullOrEmpty(order.src_nid))
                resp.src_doc_id = order.src_nid;
            return resp;

        }
        public async Task<RACommonResponseRevamp> MNPValidateMSISDNESim(UnpairedMSISDNCheckRequest msisdnCheckReqest)
        {
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            BL_Json _blJson = new BL_Json();
            ApiRequest _apiReq = new ApiRequest();
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                var dbssReqModel = _raToDBssParse.ValidateMSISDNReqParsing(msisdnCheckReqest);

                if (dbssReqModel.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    dbssReqModel = FixedValueCollection.MSISDNCountryCode + dbssReqModel;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, dbssReqModel);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                JObject dbssResp = new JObject();

                try
                {
                    log.req_time = DateTime.Now;
                    dbssResp = (JObject)await _apiReq.HttpGetRequestForMNPPortIn(apiUrl);
                    log.res_time = DateTime.Now;
                }
                catch (WebException ex)
                {
                    log.res_time = DateTime.Now;
                    txtResp = Convert.ToString(ex.Message);
                    log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var ErrorResponse = ex.Response as HttpWebResponse;
                        if (ErrorResponse != null && (int)ErrorResponse.StatusCode == 404)
                        {
                            //var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);
                            log.is_success = 1;
                            var simResp = await _bio.CheckSIMNumber4(new SIMNumberCheckRequest()
                            {
                                center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                                distributor_code = "",
                                channel_name = msisdnCheckReqest.channel_name,
                                session_token = msisdnCheckReqest.session_token,
                                sim_number = msisdnCheckReqest.sim_number,
                                retailer_id = msisdnCheckReqest.retailer_id,
                                product_code = "",
                                inventory_id = msisdnCheckReqest.inventory_id,
                                msisdn = msisdnCheckReqest.mobile_number
                            }, (int)EnumPurposeOfSIMCheck.NewConnection, false, msisdnCheckReqest.sim_category, "");

                            if (simResp.result == false)
                            {
                                raRespModel.isError = true;
                                raRespModel.message = simResp.message;
                                return raRespModel;
                            }

                            raRespModel.isError = false;
                            raRespModel.message = MessageCollection.MSISDNandSIMBothValid;

                            return raRespModel;
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    else
                    {
                        throw ex;
                    }
                }
                catch (Exception ex2)
                {
                    log.res_time = DateTime.Now;
                    txtResp = Convert.ToString(ex2.Message);
                    log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                    //var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);
                    log.is_success = 1;
                    var simResp = await _bio.CheckSIMNumber4(new SIMNumberCheckRequest()
                    {
                        center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                        distributor_code = "",
                        channel_name = msisdnCheckReqest.channel_name,
                        session_token = msisdnCheckReqest.session_token,
                        sim_number = msisdnCheckReqest.sim_number,
                        retailer_id = msisdnCheckReqest.retailer_id,
                        product_code = "",
                        inventory_id = msisdnCheckReqest.inventory_id,
                        msisdn = msisdnCheckReqest.mobile_number
                    }, (int)EnumPurposeOfSIMCheck.NewConnection, false, msisdnCheckReqest.sim_category, "");

                    if (simResp.result == false)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = simResp.message;
                        return raRespModel;
                    }

                    raRespModel.isError = false;
                    raRespModel.message = MessageCollection.MSISDNandSIMBothValid;

                    return raRespModel;

                }
                //======If DBSS api returnd success==========
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                var msisdnResp2 = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);

                if (msisdnResp2.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.MSISDNAlreadyExists;
                    return raRespModel;
                }

                var simResp2 = await _bio.CheckSIMNumber4(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id
                }, (int)EnumPurposeOfSIMCheck.NewConnection, false, msisdnCheckReqest.sim_category, "");

                if (simResp2.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = simResp2.message;
                    return raRespModel;
                }

                raRespModel.isError = false;
                raRespModel.message = MessageCollection.MSISDNandSIMBothValid;

                return raRespModel;
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

                    raRespModel.isError = true;
                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return raRespModel;
                }
                catch (Exception)
                {
                    raRespModel.isError = true;
                    raRespModel.message = ex.Message;
                    return raRespModel;
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;//userName
                log.method_name = "MNPValidateMSISDNESim";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);

            }
        }
    }
}

