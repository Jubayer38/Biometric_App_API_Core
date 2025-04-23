using BIA.BLL.BLLServices;
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
using BIA.BLL.Utility;
using BIA.Entity.Interfaces;
using System.Net;

namespace BIA.Controllers
{
    [Route("api/StarTrekSIMReplacement")]
    [ApiController]
    public class StarTrekSIMReplacementController : ControllerBase
    {
        private readonly BLLOrder _orderManager;
        private readonly BLLLog _bllLog;
        private readonly BaseController _bio;
        private readonly BLLCommon _bllCommon;
        private readonly GeoFencingValidation _geo;
        private readonly BLLDBSSToRAParse _dbssToRaParse;
        public StarTrekSIMReplacementController(BLLOrder orderManager, BLLLog bllLog, BaseController bio, BLLCommon bllCommon, GeoFencingValidation geo, BLLDBSSToRAParse dbssToRaParse)
        {
            _orderManager = orderManager;
            _bllLog = bllLog;
            _bio = bio;
            _bllCommon = bllCommon;
            _geo = geo;
            _dbssToRaParse = dbssToRaParse;
        }

        [HttpPost]
        [Route("SIMReplacementSubmit")]
        public async Task<IActionResult> SIMReplacementSubmitOrder([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev(); 
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse dobInfoResponse = new NidDobInfoResponse();
            IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest = new IndividualSIMReplsMSISDNCheckRequest();
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

                #region Get_Data_from_Validation

                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.purpose_number = model.purpose_number;
                msisdnCheckReqest.retailer_id = model.retailer_id;

                dobInfoResponse = await GetNidDob(msisdnCheckReqest);

                if (dobInfoResponse.result == false)
                {
                    //orderRes.request_id = "0";
                    orderRes.isError = true;
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
                    //orderRes.request_id = "0";
                    orderRes.isError = true;
                    orderRes.message = orderValidationResult.message;
                    log.remarks = orderValidationResult.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                #endregion
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                model.is_starTrek = 1;
                orderRes = await _orderManager.SubmitOrderV7(model, loginProviderId);

                if (orderRes.isError)
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
                        model.bi_token_number = Convert.ToDouble(orderRes.data.request_id);
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
                            // orderRes.request_id = "0";
                            orderRes.isError = true;
                            orderRes.message = imsiResp.message;
                            model.err_msg = imsiResp.message;
                            log.remarks = imsiResp.message;
                            return Ok(orderRes);
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
                        return Ok(orderRes);
                    }
                }
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
                            request_id = "0"
                        };
                    }
                    orderRes.isError = true;
                    orderRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    log.res_time = DateTime.Now;
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
                if (orderRes.data != null)
                {
                    log.is_success = orderRes.data.request_id.Length > 1 ? 1 : 0;
                    log.bi_token_number = orderRes.data.request_id;
                }
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.method_name = "SIMReplacementSubmitStarTrek";
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
        [Route("SIMReplacementSubmit-esim")]
        public async Task<IActionResult> SIMReplacementSubmitOrder_ESIM([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse dobInfoResponse = new NidDobInfoResponse();
            IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest = new IndividualSIMReplsMSISDNCheckRequest();
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

                #region Get_Data_from_Validation

                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.purpose_number = model.purpose_number;
                msisdnCheckReqest.retailer_id = model.retailer_id;

                dobInfoResponse = await GetNidDob(msisdnCheckReqest);

                if (dobInfoResponse.result == false)
                {
                    // orderRes.request_id = "0";
                    orderRes.isError = true;
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
                    // orderRes.request_id = "0";
                    orderRes.isError = true;
                    orderRes.message = orderValidationResult.message;
                    log.remarks = orderValidationResult.message;
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
                model.is_starTrek = 1;
                orderRes = await _orderManager.SubmitOrderV7(model, loginProviderId);

                if (orderRes.isError)
                {
                    return Ok(new SendOrderResponseRev()
                    {
                        isError = true,
                        message = orderRes.message
                    });
                }
                model.bi_token_number = Convert.ToDouble(orderRes.data.request_id);
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
                    // orderRes.request_id = "0";
                    orderRes.isError = true;
                    orderRes.message = imsiResp.message;
                    model.err_msg = imsiResp.message;
                    log.remarks = imsiResp.message;
                    return Ok(orderRes);
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
                model.status = (int)EnumRAOrderStatus.Failed;
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
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    log.res_time = DateTime.Now;
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
                        dest_imsi = model.dest_imsi,
                        status = model.status,
                        bss_reqId = model.bss_reqId,
                        error_id = model.error_id,
                        err_msg = model.err_msg,
                    });
                }

                log.res_time = DateTime.Now;
                if (orderRes != null)
                {
                    log.is_success = orderRes.data != null && orderRes.data.request_id.Length > 1 ? 1 : 0;
                    log.bi_token_number = orderRes.data != null ? orderRes.data.request_id : "";
                }
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.method_name = "SIMReplacementSubmitOrder_ESIM_StarTrek";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BI);
                log.user_id = model.retailer_id;
                log.remarks = model.bi_token_number != null
                                && model.bi_token_number > 1 ? "Resubmit order" : String.Empty;
                await _bllLog.RAToDBSSLog(log, "", "");

            }
            return Ok(orderRes);
        }

        public async Task<NidDobInfoResponse> GetNidDob(IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            BIAToDBSSLog log = new BIAToDBSSLog();
            string apiUrl = string.Empty;
            string? txtResp = string.Empty;
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
            try
            {
                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.GetSubscriptionByMSISDNIncludingOwnerCustomerUserCustomerSimCardInfo, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    nidDobInfo.result = false;
                    nidDobInfo.message = MessageCollection.SIMReplNoDataFound;
                    return nidDobInfo;
                }
                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.IndividualSIMReplacementMSISDNReqParsingV3(dbssResp);

                if (msisdnResp.result == false)
                {
                    nidDobInfo.result = false;
                    nidDobInfo.message = MessageCollection.SIMReplNoDataFound;
                    return nidDobInfo;
                }
                nidDobInfo.dest_nid = msisdnResp.doc_id_number;
                nidDobInfo.dest_dob = msisdnResp.dob;
                nidDobInfo.result = true;
                nidDobInfo.message = "";

                return nidDobInfo;
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;
                nidDobInfo.result = false;
                nidDobInfo.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                log.res_blob = _blJson.GetGenericJsonData(nidDobInfo);
                return nidDobInfo;
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "GetNidDob";
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
            if (order.dest_doc_type_no != null)
                resp.dest_doc_type_no = order.dest_doc_type_no.ToString();
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
            if (order.dbss_subscription_id != 0)
                resp.dbss_subscription_id = (int)order.dbss_subscription_id;
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

    }
}
