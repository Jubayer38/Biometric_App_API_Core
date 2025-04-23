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
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static BIA.Common.ModelValidation;

namespace BIA.Controllers
{
    [Route("api/StarTrekCherishNewConn")]
    [ApiController]
    public class StartTrekCherishNewConnection : ControllerBase
    {
        private readonly ApiRequest _apiReq;
        private readonly BL_Json _blJson;
        private readonly BLLOrder _orderManager;
        private readonly BLLLog _bllLog;
        private readonly BiometricApiCall _apiCall;
        private readonly BaseController _bio;
        private readonly GeoFencingValidation _geo;
        private readonly StarTrekCommonController _trekCommonController;

        public StartTrekCherishNewConnection(ApiRequest apiReq, BL_Json blJson, BLLOrder orderManager, BLLLog bllLog, BiometricApiCall apiCall, BaseController bio, GeoFencingValidation geo, StarTrekCommonController trekCommonController)
        {  
            _apiReq = apiReq;
            _blJson = blJson;
            _orderManager=orderManager;
            _bllLog = bllLog;
            _apiCall = apiCall;
            _bio = bio;
            _geo = geo;
            _trekCommonController = trekCommonController;
        }

        [HttpPost]
        [Route("CherishNewConnectionRYZE")]
        public async Task<IActionResult> NewConnectionSubmitOrder([FromBody] CherishRequest model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            GeoFencing geoFencing = new GeoFencing();
            GeofenceReqModel geofenceReqModel = new GeofenceReqModel();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
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

                #region Validate token
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
                #endregion
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
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = validateResponse.message,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = " "
                        }
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
                    orderRes.isError = true;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);

                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = orderRes.isError,
                        message = orderValidationResult.message,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = " "
                        }
                    });
                }
                #endregion
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                model.is_starTrek = 1;
                
                orderRes = await _orderManager.SubmitOrderV9(model, loginProviderId);
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
                model.bi_token_number = Convert.ToDouble(orderRes.data.request_id);

                try
                {
                    #region unpaired MSISDN validation 

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

                    if (model.is_paired == 0)
                    {
                        var channelInfo = await _orderManager.GetInventoryIdByChannelName(model.channel_name);

                        string centerCode = "";

                        if (channelInfo.Item2 == (int)EnumInventoryId.POS) //for channel POS, eshop.
                        {
                            centerCode = await _orderManager.GetCenterCodeByUserName(model.retailer_id);//here retailer_id==userName

                            if (String.IsNullOrEmpty(centerCode))
                            {
                                orderRes.isError = true;
                                orderRes.message = "Retailer's center code not found!";
                                orderRes.data = new DataRes
                                {
                                    isEsim = 0,
                                    request_id = "0"
                                };
                                model.err_msg = orderRes.message;
                            }
                        }

                        if (SettingsValues.GetRyzeAllowOrNot() == 1)
                        {
                            RACommonResponseRevamp msisdnValidationResp = await _trekCommonController.ValidateUnpairedMSISDNSTartTrek(new UnpairedMSISDNCheckRequest()
                            {
                                mobile_number = model.msisdn,
                                sim_number = model.sim_number,
                                channel_id = channelInfo.Item1,
                                channel_name = model.channel_name,
                                center_code = centerCode,
                                inventory_id = channelInfo.Item2,
                                purpose_number = model.purpose_number,
                                retailer_id = model.retailer_id,
                                sim_category = model.sim_category

                            }, "ValidateUnpairedMSISDNSTartTrek");

                            if (msisdnValidationResp.isError == true)
                            {
                                model.status = (int)EnumRAOrderStatus.Failed;
                                orderRes.data = new DataRes()
                                {
                                    request_id = "0"
                                };
                                orderRes.isError = true;
                                orderRes.message = msisdnValidationResp.message;
                                model.err_msg = orderRes.message;

                                return Ok(new RACommonResponseRevamp()
                                {
                                    isError = true,
                                    message = msisdnValidationResp.message,
                                    data = new Datas()
                                    {
                                        isEsim = 0,
                                        request_id = " "
                                    }
                                });
                            }
                        }
                        else
                        {
                            RACommonResponseRevamp msisdnValidationResp = await _trekCommonController.ValidateMSISDNSTartTrekCherishV2(new CherishMSISDNCheckRequest()
                            {
                                mobile_number = model.msisdn,
                                sim_number = model.sim_number,
                                channel_id = channelInfo.Item1,
                                channel_name = model.channel_name,
                                center_code = centerCode,
                                inventory_id = channelInfo.Item2,
                                purpose_number = model.purpose_number,
                                retailer_id = model.retailer_id,
                                sim_category = model.sim_category,
                                selected_category=model.selected_category,

                            }, "ValidateUnpairedMSISDNSTartTrekV2");

                            if (msisdnValidationResp.isError == true)
                            {
                                model.status = (int)EnumRAOrderStatus.Failed;
                                orderRes.data = new DataRes()
                                {
                                    request_id = "0"
                                };
                                orderRes.isError = true;
                                orderRes.message = msisdnValidationResp.message;
                                model.err_msg = orderRes.message;

                                return Ok(new RACommonResponseRevamp()
                                {
                                    isError = true,
                                    message = msisdnValidationResp.message,
                                    data = new Datas()
                                    {
                                        isEsim = 0,
                                        request_id = " "
                                    }
                                });
                            }
                        }
                    }
                    #endregion

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
                        orderRes.isError = true;
                        orderRes.message = imsiResp.message;
                        model.err_msg = orderRes.message;

                        return Ok(new RACommonResponseRevamp()
                        {
                            isError = true,
                            message = imsiResp.message,
                            data = new Datas()
                            {
                                isEsim = 0,
                                request_id = " "
                            }
                        });
                    }
                    else
                    {
                        model.dest_imsi = imsiResp.imsi;//[Note: here IMSI is being sent as SIM number as per business requirement]
                    }
                    #endregion

                    #region bio verification

                    var pursedData = await _orderManager.SubmitOrderDataPurseV2(model);
                    BiomerticDataModel dataModel = bioverifyDataMapp(pursedData);

                    verifyResp = await _bio.BssServiceProcessStarTrek(dataModel, model.msisdnReservationId, model.retailer_id, 0);

                    if (verifyResp.is_success == true)
                    {
                        model.bss_reqId = verifyResp.bss_req_id;
                        model.status = (int)EnumRAOrderStatus.BioVerificationSubmitted;
                        model.msisdnReservationId = verifyResp.Reservation_Id;
                    }
                    else
                    {
                        if (verifyResp.Reservation_Id != null)
                        {
                            if (model.is_paired == 0 && Convert.ToInt32(model.purpose_number) == (int)EnumPurposeNumber.NewRegistration)
                            {
                                await _apiCall.UnreserveMSISDNV2(verifyResp.Reservation_Id, model.session_token, "", model.bi_token_number.ToString(), model.msisdn);
                            }
                        }
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
                        message = String.IsNullOrEmpty(errorDescription.error_custom_msg) ? errorDescription.error_description : errorDescription.error_custom_msg,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = " "
                        }
                    });
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
            catch (Exception ex)
            {
                if (verifyResp != null)
                {
                    if (!String.IsNullOrEmpty(verifyResp.Reservation_Id))
                    {
                        if (model.is_paired == 0 && Convert.ToInt32(model.purpose_number) == (int)EnumPurposeNumber.NewRegistration)
                        {
                            await _apiCall.UnreserveMSISDNV2(verifyResp.Reservation_Id, model.session_token, "", model.bi_token_number.ToString(), model.msisdn);
                        }
                    }
                }
                log.res_time = DateTime.Now;
                ErrorDescription error;
                log.is_success = 0;
                model.status = (int)EnumRAOrderStatus.Failed;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    if (verifyResp != null)
                    {
                        orderRes.data = new DataRes
                        {
                            request_id = verifyResp.bss_req_id != null ? verifyResp.bss_req_id : "0"
                        };
                    }
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
                        dest_imsi = model.dest_imsi,
                        status = model.status,
                        bss_reqId = model.bss_reqId,
                        error_id = model.error_id,
                        err_msg = model.err_msg,
                    });
                }
                if (orderRes != null)
                {
                    if (orderRes.data != null)
                    {
                        log.bi_token_number = orderRes.data.request_id;
                        log.is_success = orderRes.data.request_id.Length > 1 ? 1 : 0;
                    }
                }
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.method_name = "NewConnectionSubmitOrder";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.user_id = model.retailer_id;
                log.remarks = model.bi_token_number != null
                               && model.bi_token_number > 1 ? "Resubmit order" : String.Empty;

                await _bllLog.RAToDBSSLog(log, "", "");
            }
        }

        [HttpPost]
        [Route("CherishNewConnectionRYZEESIM")]
        public async Task<IActionResult> NewConnectionSubmitOrder_ESIM([FromBody] CherishRequest model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            GeoFencing geoFencing = new GeoFencing();
            GeofenceReqModel geofenceReqModel = new GeofenceReqModel();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = validateResponse.message,
                        data = new Datas() { }
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
                    orderRes.data.request_id = "0";
                    orderRes.isError = true;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);

                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = orderRes.isError,
                        message = orderRes.message,
                        data = new Datas() { }
                    });
                }
                #endregion
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                model.is_esim = 1;
                model.is_starTrek = 1;

                orderRes = await _orderManager.SubmitOrderV9(model, loginProviderId);
                if (orderRes.isError)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = orderRes.message,
                        data = new Datas() { }
                    });
                }
                model.bi_token_number = Convert.ToDouble(orderRes.data.request_id);

                #endregion
                #region unpaired MSISDN validation 

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

                if (model.is_paired == 0)
                {
                    var channelInfo = await _orderManager.GetInventoryIdByChannelName(model.channel_name);

                    string centerCode = "";

                    if (channelInfo.Item2 == (int)EnumInventoryId.POS) //for channel POS, eshop.
                    {
                        centerCode = await _orderManager.GetCenterCodeByUserName(model.retailer_id);//here retailer_id==userName

                        if (String.IsNullOrEmpty(centerCode))
                        {
                            orderRes.isError = true;
                            orderRes.message = "Retailer's center code not found!";
                            model.err_msg = orderRes.message;
                            orderRes.data = new DataRes
                            {
                                request_id = "0"
                            };
                        }
                    }

                    if (SettingsValues.GetRyzeAllowOrNot() == 1)
                    {
                        RACommonResponseRevamp msisdnValidationResp = await _trekCommonController.ValidateUnpairedMSISDNESIM(new UnpairedMSISDNCheckRequest()
                        {
                            mobile_number = model.msisdn,
                            sim_number = model.sim_number,
                            channel_id = channelInfo.Item1,
                            channel_name = model.channel_name,
                            center_code = centerCode,
                            inventory_id = channelInfo.Item2,
                            purpose_number = model.purpose_number,
                            retailer_id = model.retailer_id,
                            sim_category = model.sim_category

                        }, "ValidateUnpairedMSISDN_ESIMV2");

                        if (msisdnValidationResp.isError == true)
                        {
                            model.status = (int)EnumRAOrderStatus.Failed;
                            orderRes.data = new DataRes
                            {
                                request_id = "0"
                            };
                            orderRes.isError = true;
                            orderRes.message = msisdnValidationResp.message;
                            model.err_msg = orderRes.message;

                            return Ok(new RACommonResponseRevamp()
                            {
                                isError = orderRes.isError,
                                message = orderRes.message,
                                data = new Datas()
                                {
                                    request_id = orderRes.data.request_id,
                                    isEsim = 1
                                }
                            });
                        }
                    }
                    else
                    {
                        RACommonResponseRevamp msisdnValidationResp = await _trekCommonController.ValidateUnpairedMSISDNESIMV3(new CherishMSISDNCheckRequest()
                        {
                            mobile_number = model.msisdn,
                            sim_number = model.sim_number,
                            channel_id = channelInfo.Item1,
                            channel_name = model.channel_name,
                            center_code = centerCode,
                            inventory_id = channelInfo.Item2,
                            purpose_number = model.purpose_number,
                            retailer_id = model.retailer_id,
                            sim_category = model.sim_category,
                            selected_category= model.selected_category,

                        }, "ValidateUnpairedMSISDN_ESIMV3");

                        if (msisdnValidationResp.isError == true)
                        {
                            model.status = (int)EnumRAOrderStatus.Failed;
                            orderRes.data = new DataRes
                            {
                                request_id = "0"
                            };
                            orderRes.isError = true;
                            orderRes.message = msisdnValidationResp.message;
                            model.err_msg = orderRes.message;

                            return Ok(new RACommonResponseRevamp()
                            {
                                isError = orderRes.isError,
                                message = orderRes.message,
                                data = new Datas()
                                {
                                    request_id = orderRes.data.request_id,
                                    isEsim = 1

                                }
                            });
                        }
                    }
                }
                #endregion                

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
                    orderRes.data.request_id = "0";
                    orderRes.isError = true;
                    orderRes.message = imsiResp.message;
                    model.err_msg = orderRes.message;
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = orderRes.isError,
                        message = orderRes.message,
                        data = new Datas()
                        {
                            request_id = orderRes.data.request_id,
                            isEsim = 1
                        }
                    });
                }
                else
                {
                    model.dest_imsi = imsiResp.imsi;//[Note: here IMSI is being sent as SIM number as per business requirement]
                }
                #endregion

                #region bio verification

                var pursedData = await _orderManager.SubmitOrderDataPurseV2(model);
                BiomerticDataModel dataModel = bioverifyDataMapp(pursedData);

                verifyResp = await _bio.BssServiceProcessStarTrek(dataModel, model.msisdnReservationId, model.retailer_id, 0);

                if (verifyResp.is_success == true)
                {
                    model.bss_reqId = verifyResp.bss_req_id;
                    model.status = (int)EnumRAOrderStatus.BioVerificationSubmitted;
                    model.msisdnReservationId = verifyResp.Reservation_Id;
                }
                else
                {
                    if (verifyResp.Reservation_Id != null)
                    {
                        if (model.is_paired == 0 && Convert.ToInt32(model.purpose_number) == (int)EnumPurposeNumber.NewRegistration)
                        {
                            await _apiCall.UnreserveMSISDNV2(verifyResp.Reservation_Id, model.session_token, "", model.bi_token_number.ToString(), model.msisdn);
                        }
                    }

                    model.status = (int)EnumRAOrderStatus.Failed;
                    model.err_code = verifyResp.err_code;
                    model.err_msg = verifyResp.err_msg;
                    model.error_id = verifyResp.error_Id;
                }
                #endregion

            }
            catch (Exception ex)
            {
                if (verifyResp != null)
                {
                    if (String.IsNullOrEmpty(verifyResp.Reservation_Id))
                    {
                        if (model.is_paired == 0 && Convert.ToInt32(model.purpose_number) == (int)EnumPurposeNumber.NewRegistration)
                        {
                            await _apiCall.UnreserveMSISDNV2(verifyResp.Reservation_Id, model.session_token, "", model.bi_token_number.ToString(), model.msisdn);
                        }
                    }
                }
                log.res_time = DateTime.Now;
                ErrorDescription error;
                log.is_success = 0;
                model.status = (int)EnumRAOrderStatus.Failed;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    orderRes.data = new DataRes()
                    {
                        request_id = verifyResp != null ? verifyResp.bss_req_id : ""
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
                if (model.bi_token_number != null || model.bi_token_number > 1)
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

                if (orderRes != null)
                {
                    if (orderRes.data != null)
                    {
                        log.bi_token_number = orderRes.data.request_id;
                        log.is_success = orderRes.data.request_id.Length > 1 ? 1 : 0;
                    }
                }
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.method_name = "CherishNewConnectionRYZEESIM";
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
                    request_id = orderRes.data.request_id,
                    isEsim = 1
                }
            });
        }

        public BiomerticDataModel bioverifyDataMapp(OrderRequest2 order)
        {
            BiomerticDataModel resp = new BiomerticDataModel();
            if (order.purpose_number != null)
                resp.purpose_number = (int)order.purpose_number;
            if (order.dest_doc_type_no != null)
                resp.dest_doc_type_no = order.dest_doc_type_no.ToString();
            if (!String.IsNullOrEmpty(order.dest_nid))
                resp.dest_doc_id = order.dest_nid;
            if (!String.IsNullOrEmpty(order.retailer_id))
                resp.user_id = order.retailer_id;
            resp.msisdn = order.msisdn;
            if (order.dest_ec_verifi_reqrd != null)
                resp.dest_ec_verification_required = (int)order.dest_ec_verifi_reqrd;
            if (!String.IsNullOrEmpty(order.dest_imsi))
                resp.dest_imsi = order.dest_imsi;
            resp.dest_foreign_flag = 0;
            resp.status = order.status;
            if (order.sim_category != null)
                resp.sim_category = (int)order.sim_category;
            resp.dest_dob = order.dest_dob;
            resp.create_date = DateTime.Now.ToString();
            resp.dest_left_thumb = order.dest_left_thumb;
            resp.dest_left_index = order.dest_left_index;
            resp.dest_right_thumb = order.dest_right_thumb;
            resp.dest_right_index = order.dest_right_index;
            if (!String.IsNullOrEmpty(order.sim_number))
                resp.sim_number = order.sim_number;

            if (order.is_paired != null)
            {
                resp.is_paired = (int)order.is_paired;
            }
            else
            {
                resp.is_paired = 0;
            }

            if (order.src_doc_type_no != null)
                resp.src_doc_type_no = order.src_doc_type_no.ToString();
            return resp;
        }
    }
}
