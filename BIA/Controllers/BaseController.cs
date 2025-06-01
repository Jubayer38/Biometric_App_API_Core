using BIA.BLL.BLLServices;
using BIA.BLL.Utility;
using BIA.Common;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.PopulateModel;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Reflection;


namespace BIA.Controllers
{
    public class BaseController : ControllerBase
    {
        private readonly BLLRAToDBSSParse _raToDBssParse;
        private readonly BLLDBSSToRAParse _dbssToRaParse;
        private readonly ApiRequest _apiReq;
        private readonly BLLCommon _bllCommon;
        private readonly BL_Json _blJson;
        private readonly BLLLog _bllLog;
        private readonly BiometricApiCall _apiCall;
        public BaseController(BLLRAToDBSSParse raToDBssParse, BLLDBSSToRAParse dbssToRaParse, ApiRequest apiReq, BLLCommon bllCommon, BL_Json blJson, BLLLog bllLog, BiometricApiCall apiCall)
        {
            _bllCommon = bllCommon;
            _raToDBssParse = raToDBssParse;
            _dbssToRaParse = dbssToRaParse;
            _apiReq = apiReq;
            _blJson = blJson;
            _bllLog = bllLog;
            _apiCall = apiCall;
        }

        #region  MSISDN validation Unpaired
        /// <summary>
        /// This method is used for MSISDN validation for unpaired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>

        public async Task<RACommonResponse> ValidateUnpairedMSISDN(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponse raRespModel = new RACommonResponse();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();

            try
            {
                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;

                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                    if (dbssResp["data"] == null)
                    {
                        log.is_success = 0;
                        raRespModel.result = false;
                        raRespModel.message = "MSISDN: " + MessageCollection.NoDataFound;
                        return raRespModel;
                    }           

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsingV2(dbssResp, msisdnCheckReqest.retailer_id);

                if (msisdnResp.result == false)
                {
                    raRespModel.result = false;
                    raRespModel.message = msisdnResp.message;
                    return raRespModel;
                }

                var stockCheck = await _bllCommon.IsStockAvailable(msisdnResp.stock_id, Convert.ToInt32(msisdnCheckReqest.channel_id));

                if (stockCheck == false)
                {
                    raRespModel.result = false;
                    raRespModel.message = MessageCollection.StockIDMismatch;
                    return raRespModel;
                }


                var simResp = await CheckSIMNumber3(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = string.Empty,
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = string.Empty,
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
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
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.result = false;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }

                string resp = string.Empty;
                if (ex.Response != null)
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return raRespModel;
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.result = false;
                            if (isDBSSErrorOccurred(ex))
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return raRespModel;
                        }
                        catch (Exception)
                        {
                            raRespModel.result = false;
                            raRespModel.message = ex.Message;
                            if (error != null)
                            {
                                log.error_code = error.error_code ?? String.Empty;
                                log.error_source = error.error_source ?? String.Empty;
                                log.message = error.error_description ?? String.Empty;
                            }
                            else
                            {
                                log.error_code = String.Empty;
                                log.error_source = String.Empty;
                                log.message = String.Empty;

                            }
                            return raRespModel;
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.result = false;
                        raRespModel.message = ex.Message;

                        log.error_code = error != null ? error.error_code : String.Empty;
                        log.error_source = error != null ? error.error_source : String.Empty;
                        log.message = error != null ? error.error_description : String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raRespModel.result = false;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

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
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidateUnpairedMSISDN";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        public async Task<RACommonResponse> ValidateUnpairedMSISDNV2(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponse raRespModel = new RACommonResponse();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();

            try
            {
                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;

                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp["data"] == null)
                {
                    log.is_success = 0;
                    raRespModel.result = false;
                    raRespModel.message = "MSISDN: " + MessageCollection.NoDataFound;
                    return raRespModel;
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsingV2(dbssResp, msisdnCheckReqest.retailer_id);

                if (msisdnResp.result == false)
                {
                    raRespModel.result = false;
                    raRespModel.message = msisdnResp.message;
                    return raRespModel;
                }

                var stockCheck = await _bllCommon.IsStockAvailable(msisdnResp.stock_id, Convert.ToInt32(msisdnCheckReqest.channel_id));

                if (stockCheck == false)
                {
                    raRespModel.result = false;
                    raRespModel.message = MessageCollection.StockIDMismatch;
                    return raRespModel;
                }


                var simResp = await CheckSIMNumber3(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = string.Empty,
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = string.Empty,
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
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
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.result = false;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }

                string resp = string.Empty;
                if (ex.Response != null)
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return raRespModel;
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.result = false;
                            if (isDBSSErrorOccurred(ex))
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return raRespModel;
                        }
                        catch (Exception)
                        {
                            raRespModel.result = false;
                            raRespModel.message = ex.Message;

                            log.error_code = error != null ? error.error_code : String.Empty;
                            log.error_source = error != null ? error.error_source : String.Empty;
                            log.message = error != null ? error.error_description : String.Empty;

                            return raRespModel;
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.result = false;
                        raRespModel.message = ex.Message;

                        log.error_code = error != null ? error.error_code : String.Empty;
                        log.error_source = error != null ? error.error_source : String.Empty;
                        log.message = error != null ? error.error_description : String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raRespModel.result = false;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

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
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = apiName;

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNV4(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();

            try
            {
                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;

                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp["data"] == null)
                {
                    log.is_success = 0;
                    raRespModel.isError = true;
                    raRespModel.message = "MSISDN: " + MessageCollection.NoDataFound;
                    return raRespModel;
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsingV2(dbssResp, msisdnCheckReqest.retailer_id);

                if (msisdnResp.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = msisdnResp.message;
                    return raRespModel;
                }

                var stockCheck = await _bllCommon.IsStockAvailable(msisdnResp.stock_id, Convert.ToInt32(msisdnCheckReqest.channel_id));

                if (stockCheck == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.StockIDMismatch;
                    return raRespModel;
                }


                var simResp = await CheckSIMNumber3(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = string.Empty,
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = string.Empty,
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
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
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }

                string resp = string.Empty;
                if (ex.Response != null)
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return raRespModel;
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.isError = true;
                            if (isDBSSErrorOccurred(ex))
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return raRespModel;
                        }
                        catch (Exception)
                        {
                            raRespModel.isError = true;
                            raRespModel.message = ex.Message;

                            log.error_code = error != null ? error.error_code : String.Empty;
                            log.error_source = error != null ? error.error_source : String.Empty;
                            log.message = error != null ? error.error_description : String.Empty;

                            return raRespModel;
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.error_code = error != null ? error.error_code : String.Empty;
                        log.error_source = error != null ? error.error_source : String.Empty;
                        log.message = error != null ? error.error_description : String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raRespModel.isError = true;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

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
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = apiName;

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        public async Task<RACommonResponseRevampV3> ValidateUnpairedMSISDNV6(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevampV3 raRespModel = new RACommonResponseRevampV3();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();

            try
            {
                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                //dbssResp = JObject.Parse("{\"data\":{\"attributes\":{\"currency\":null,\"is-controlled\":true,\"msisdn\":\"8801959592207\",\"number-category\":\"S1\",\"price\":null,\"reserved-for\":\"\",\"salesman-id\":null,\"status\":\"available\",\"stock\":16},\"id\":\"8801959592207\",\"links\":{\"self\":\"/api/v1/msisdns/8801959592207\"},\"relationships\":{\"inventory-sim-card\":{\"data\":null,\"links\":{\"related\":\"/api/v1/msisdns/8801959592207/inventory-sim-card\"}}},\"type\":\"msisdns\"}}");
                log.res_time = DateTime.Now;

                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp["data"] == null)
                {
                    log.is_success = 0;
                    raRespModel.isError = true;
                    raRespModel.message = "MSISDN: " + MessageCollection.NoDataFound;
                    return raRespModel;
                }

                log.is_success = 1;

                var msisdnResp = await _dbssToRaParse.UnpairedMSISDNReqParsingV3(dbssResp, msisdnCheckReqest.retailer_id,msisdnCheckReqest.channel_name);

                if (msisdnResp.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = msisdnResp.message;
                    raRespModel.data = new DesiredCategoryData()
                    {
                        isDesiredCategory = msisdnResp.isDesiredCategory,
                        category = msisdnResp.category_name,
                        message = msisdnResp.data_message,
                    };
                    return raRespModel;
                }

                var stockCheck = await _bllCommon.IsStockAvailable(msisdnResp.stock_id, Convert.ToInt32(msisdnCheckReqest.channel_id));

                if (stockCheck == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.StockIDMismatch;
                    raRespModel.data = new DesiredCategoryData()
                    {
                        isDesiredCategory = msisdnResp.isDesiredCategory,
                        category = msisdnResp.category_name,
                        message = msisdnResp.data_message,
                    };
                    return raRespModel;
                }


                var simResp = await CheckSIMNumber3(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = string.Empty,
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = string.Empty,
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.NewConnection, false, msisdnCheckReqest.sim_category, "");


                if (simResp.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = simResp.message;
                    raRespModel.data = new DesiredCategoryData()
                    {
                        isDesiredCategory = msisdnResp.isDesiredCategory,
                        category = msisdnResp.category_name,
                        message = msisdnResp.data_message,
                    };
                    return raRespModel;
                }
                raRespModel.isError = false;
                raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                raRespModel.data = new DesiredCategoryData()
                {
                    isDesiredCategory = msisdnResp.isDesiredCategory,
                    category = msisdnResp.category_name,
                    message = msisdnResp.data_message,
                };
                return raRespModel;
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }

                string resp = string.Empty;
                if (ex.Response != null)
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return raRespModel;
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.isError = true;
                            if (isDBSSErrorOccurred(ex))
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return raRespModel;
                        }
                        catch (Exception)
                        {
                            raRespModel.isError = true;
                            raRespModel.message = ex.Message;

                            log.error_code = error != null ? error.error_code : String.Empty;
                            log.error_source = error != null ? error.error_source : String.Empty;
                            log.message = error != null ? error.error_description : String.Empty;

                            return raRespModel;
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.error_code = error != null ? error.error_code : String.Empty;
                        log.error_source = error != null ? error.error_source : String.Empty;
                        log.message = error != null ? error.error_description : String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raRespModel.isError = true;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

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
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = apiName;


                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        #endregion

        #region Cherish MSISDN check and validation Unpaired
        /// <summary>
        /// This method is used for MSISDN validation for unpaired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>

        public async Task<RACommonResponse> ValidateUnpairedMSISDNV3(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponse raRespModel = new RACommonResponse();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();

            try
            {
                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;

                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                if (dbssResp["data"] == null)
                {
                    log.is_success = 0;
                    raRespModel.result = false;
                    raRespModel.message = "MSISDN: " + MessageCollection.NoDataFound;
                    return raRespModel;
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsing(dbssResp, msisdnCheckReqest.retailer_id);

                if (msisdnResp.result == false)
                {
                    raRespModel.result = false;
                    raRespModel.message = msisdnResp.message;
                    return raRespModel;
                }

                var stockCheck = await _bllCommon.IsStockAvailable(msisdnResp.stock_id, Convert.ToInt32(msisdnCheckReqest.channel_id));

                if (stockCheck == false)
                {
                    raRespModel.result = false;
                    raRespModel.message = MessageCollection.StockIDMismatch;
                    return raRespModel;
                }


                var simResp = await CheckSIMNumber4(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
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
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.result = false;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }

                string resp = string.Empty;
                if (ex.Response != null)
                    new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return raRespModel;
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.result = false;

                            if (isDBSSErrorOccurred(ex))
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return raRespModel;
                        }
                        catch (Exception)
                        {
                            raRespModel.result = false;
                            raRespModel.message = ex.Message;

                            log.error_code = error != null ? error.error_code : String.Empty;
                            log.error_source = error != null ? error.error_source : String.Empty;
                            log.message = error != null ? error.error_description : String.Empty;

                            return raRespModel;
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.result = false;
                        raRespModel.message = ex.Message;

                        log.error_code = error != null ? error.error_code : String.Empty;
                        log.error_source = error != null ? error.error_source : String.Empty;
                        log.message = error != null ? error.error_description : String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raRespModel.result = false;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

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
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = apiName;

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        /// <summary>
        /// This method is used for MSISDN validation for unpaired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param> 
        /// <returns>Success/ Failure</returns>

        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNV5(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();

            try
            {
                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;

                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                if (dbssResp["data"] == null)
                {
                    log.is_success = 0;
                    raRespModel.isError = true;
                    raRespModel.message = "MSISDN: " + MessageCollection.NoDataFound;
                    return raRespModel;
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsing(dbssResp, msisdnCheckReqest.retailer_id);

                if (msisdnResp.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = msisdnResp.message;
                    return raRespModel;
                }

                var stockCheck = await _bllCommon.IsStockAvailable(msisdnResp.stock_id, Convert.ToInt32(msisdnCheckReqest.channel_id));

                if (stockCheck == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.StockIDMismatch;
                    return raRespModel;
                }


                var simResp = await CheckSIMNumber4(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
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
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }

                string resp = string.Empty;
                if(ex.Response!= null)
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return raRespModel;
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.isError = true;
                            if (isDBSSErrorOccurred(ex))
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return raRespModel;
                        }
                        catch (Exception)
                        {
                            raRespModel.isError = true;
                            raRespModel.message = ex.Message;

                            log.error_code = error != null ? error.error_code : String.Empty;
                            log.error_source = error != null ? error.error_source : String.Empty;
                            log.message = error != null ? error.error_description : String.Empty;

                            return raRespModel;
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.error_code = error != null ? error.error_code : String.Empty;
                        log.error_source = error != null ? error.error_source : String.Empty;
                        log.message = error != null ? error.error_description : String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raRespModel.isError = true;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

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
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidateUnpairedMSISDNV5";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }       

        #endregion


        #region Cherish MSISDN check and validation Paired
        /// <summary>
        /// This method is used for MSISDN validation for unpaired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns> 

        public async Task<RACommonResponse> CheckCherishedNumber(PairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponse raRespModel = new RACommonResponse();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            string number_category = string.Empty;

            try
            {
                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.CherishMSISDNValidation, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;

                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.CherishMSISDNReqParsing(dbssResp, msisdnCheckReqest.retailer_id);

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
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "CheckCherishedNumber";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }


        }


        #endregion


        #region Validate-Corporate-MSISDN
        internal async Task<SIMReplacementMSISDNCheckResponse> ValidateCorporateMSISDN(CorporateMSISDNCheckRequest msisdnCheckReqest
                                                                                             , string apiName)
        {
            SIMReplacementMSISDNCheckResponse raRespModel = new SIMReplacementMSISDNCheckResponse();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
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


                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    return raRespModel = new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = MessageCollection.SIMReplNoDataFound
                    };
                }

                log.is_success = 1;

                CorporateSIMReplacementCheckResponseWithCustomerId msisdnResp = _dbssToRaParse.CorporateSIMReplacementMSISDNReqParsing2(dbssResp);

                if (msisdnResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = msisdnResp.message
                    };
                }

                SIMReplacementMSISDNCheckResponse customerResp =await GetCoordicatorCustomerInfo(msisdnResp.customer_id, msisdnCheckReqest.poc_msisdn_number, msisdnCheckReqest.purpose_number, msisdnCheckReqest.retailer_id);

                if (customerResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = customerResp.message
                    };
                }

                RACommonResponse simResp = await CheckSIMNumber3(new SIMNumberCheckRequest
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.SIMReplacement, null, null, msisdnResp.old_sim_type);

                if (simResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = simResp.message
                    };
                }

                return new SIMReplacementMSISDNCheckResponse()
                {
                    dbss_subscription_id = msisdnResp.dbss_subscription_id,
                    old_sim_number = msisdnResp.old_sim_number,
                    doc_id_number = "**********",//customerResp.doc_id_number,
                    dob = "**/**/****",//customerResp.dob,
                    result = true,
                    message = MessageCollection.MSISDNandSIMBothValid
                };
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.is_success = 0;
                log.msisdn = msisdnCheckReqest.mobile_number;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                }
                catch (Exception)
                {
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                }

                return new SIMReplacementMSISDNCheckResponse()
                {
                    result = false,
                    message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                };
            }

            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = apiName;

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        internal async Task<SIMReplacementMSISDNCheckResponseDataRev> ValidateCorporateMSISDNV3(CorporateMSISDNCheckRequest msisdnCheckReqest
                                                                                    , string apiName)
        {
            SIMReplacementMSISDNCheckResponseDataRev raRespModel = new SIMReplacementMSISDNCheckResponseDataRev();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
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


                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    return raRespModel = new SIMReplacementMSISDNCheckResponseDataRev()
                    {
                        isError = true,
                        message = MessageCollection.SIMReplNoDataFound
                    };
                }

                log.is_success = 1;

                CorporateSIMReplacementCheckResponseWithCustomerId msisdnResp = _dbssToRaParse.CorporateSIMReplacementMSISDNReqParsing2(dbssResp);

                if (msisdnResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponseDataRev()
                    {
                        isError = true,
                        message = msisdnResp.message
                    };
                }

                SIMReplacementMSISDNCheckResponse customerResp =await GetCoordicatorCustomerInfo(msisdnResp.customer_id, msisdnCheckReqest.poc_msisdn_number, msisdnCheckReqest.purpose_number, msisdnCheckReqest.retailer_id);

                if (customerResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponseDataRev()
                    {
                        isError = true,
                        message = customerResp.message
                    };
                }

                RACommonResponse simResp = await CheckSIMNumber3(new SIMNumberCheckRequest
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.SIMReplacement, null, null, msisdnResp.old_sim_type);

                if (simResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponseDataRev()
                    {
                        isError = true,
                        message = simResp.message
                    };
                }

                raRespModel.data = new SIMReplacementMSISDNCheckResponseRev()
                {
                    dbss_subscription_id = msisdnResp.dbss_subscription_id,
                    old_sim_number = msisdnResp.old_sim_number,
                    doc_id_number = "**********",//customerResp.doc_id_number,
                    dob = "**/**/****",//customerResp.dob
                };

                return new SIMReplacementMSISDNCheckResponseDataRev()
                {
                    data = raRespModel.data,
                    isError = true,
                    message = MessageCollection.MSISDNandSIMBothValid
                };
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.is_success = 0;
                log.msisdn = msisdnCheckReqest.mobile_number;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                }
                catch (Exception)
                {
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                }

                return new SIMReplacementMSISDNCheckResponseDataRev()
                {
                    isError = true,
                    message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                };
            }

            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = apiName;

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        internal async Task<SIMReplacementMSISDNCheckResponse> ValidateCorporateMSISDNV1(CorporateMSISDNCheckRequest msisdnCheckReqest
                                                                                          , string apiName)
        {
            SIMReplacementMSISDNCheckResponse raRespModel = new SIMReplacementMSISDNCheckResponse();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
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


                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    return raRespModel = new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = MessageCollection.SIMReplNoDataFound
                    };
                }

                log.is_success = 1;

                CorporateSIMReplacementCheckResponseWithCustomerId msisdnResp = _dbssToRaParse.CorporateSIMReplacementMSISDNReqParsing2(dbssResp);

                if (msisdnResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = msisdnResp.message
                    };
                }

                SIMReplacementMSISDNCheckResponse customerResp = await GetCoordicatorCustomerInfo(msisdnResp.customer_id, msisdnCheckReqest.poc_msisdn_number, msisdnCheckReqest.purpose_number, msisdnCheckReqest.retailer_id);

                if (customerResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = customerResp.message
                    };
                }

                RACommonResponse simResp = await CheckSIMNumber3(new SIMNumberCheckRequest
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.SIMReplacement, null, null, msisdnResp.old_sim_type);

                if (simResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = simResp.message
                    };
                }

                return new SIMReplacementMSISDNCheckResponse()
                {
                    dbss_subscription_id = msisdnResp.dbss_subscription_id,
                    old_sim_number = msisdnResp.old_sim_number,
                    doc_id_number = customerResp.doc_id_number,
                    dob = customerResp.dob,
                    result = true,
                    message = MessageCollection.MSISDNandSIMBothValid
                };
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.is_success = 0;
                log.msisdn = msisdnCheckReqest.mobile_number;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                }
                catch (Exception)
                {
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                }

                return new SIMReplacementMSISDNCheckResponse()
                {
                    result = false,
                    message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                };
            }

            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = apiName;

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }


        internal async Task<SIMReplacementMSISDNCheckResponse> ValidateCorporateMSISDNV2(CorporateMSISDNCheckRequest msisdnCheckReqest
                                                                                          , string apiName)
        {
            SIMReplacementMSISDNCheckResponse raRespModel = new SIMReplacementMSISDNCheckResponse();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
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


                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    return raRespModel = new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = MessageCollection.SIMReplNoDataFound
                    };
                }

                log.is_success = 1;

                CorporateSIMReplacementCheckResponseWithCustomerId msisdnResp = _dbssToRaParse.CorporateSIMReplacementMSISDNReqParsing2(dbssResp);

                if (msisdnResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = msisdnResp.message
                    };
                }

                SIMReplacementMSISDNCheckResponse customerResp = await GetCoordicatorCustomerInfo(msisdnResp.customer_id, msisdnCheckReqest.poc_msisdn_number, msisdnCheckReqest.purpose_number, msisdnCheckReqest.retailer_id);

                if (customerResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = customerResp.message
                    };
                }

                RACommonResponse simResp = await CheckSIMNumber4(new SIMNumberCheckRequest
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.SIMReplacement, null, null, msisdnResp.old_sim_type);

                if (simResp.result == false)
                {
                    return new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = simResp.message
                    };
                }

                return new SIMReplacementMSISDNCheckResponse()
                {
                    dbss_subscription_id = msisdnResp.dbss_subscription_id,
                    old_sim_number = msisdnResp.old_sim_number,
                    doc_id_number = customerResp.doc_id_number,
                    //doc_id_number = "**********",
                    dob = customerResp.dob,
                    //dob = "**/**/****",
                    result = true,
                    message = MessageCollection.MSISDNandSIMBothValid
                };
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.is_success = 0;
                log.msisdn = msisdnCheckReqest.mobile_number;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                }
                catch (Exception)
                {
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                }

                return new SIMReplacementMSISDNCheckResponse()
                {
                    result = false,
                    message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                };
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = apiName;

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        #endregion


        #region Get-Coordicator-Customer-Info
        public async Task<SIMReplacementMSISDNCheckResponse> GetCoordicatorCustomerInfo(string customerId, string pocMsisdnNo, string purposeNumber, string username)
        {
            SIMReplacementMSISDNCheckResponse raRespModel = new SIMReplacementMSISDNCheckResponse();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            object dbssResp = null;
            try
            {
                apiUrl = String.Format(GetAPICollection.GetCustomerInfoById, customerId);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                CorporateSIMReplacemnetCustomerInfoRootobject dbssRespModel = new CorporateSIMReplacemnetCustomerInfoRootobject();
                dbssRespModel = JsonConvert.DeserializeObject<CorporateSIMReplacemnetCustomerInfoRootobject>(dbssResp.ToString());

                if (dbssRespModel != null)
                {
                    log.is_success = 1;
                    raRespModel = _dbssToRaParse.CorporateSIMReplacementCustomerInfoReqParsing(dbssRespModel, pocMsisdnNo);
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raRespModel.result = false;
                    raRespModel.message = ex.Message;
                }
                catch (Exception)
                {
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raRespModel.result = false;
                    raRespModel.message = ex.Message;
                }
            }
            finally
            {
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = purposeNumber;
                log.user_id = username;
                log.method_name = "GetCoordicatorCustomerInfo";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return raRespModel;
        }
        #endregion


        #region SIM Number validation v2
        /// <summary>
        /// This method is used for SIM Number validation
        /// </summary>
        /// <param name="simNumberCheckReqest"></param>
        /// <returns>Success/ Failure</returns>
        protected async Task<RACommonResponse> CheckSIMNumber2(SIMNumberCheckRequest simNumberCheckReqest, int purposeOfSIMCheck, bool? isPaired, int? simCategory, string old_sim_type)
        {
            RACommonResponse raResp = new RACommonResponse();
            string apiUrl = "", txtResp = "";
            SIMValidationRequestRootobject dbssReqModel = null;
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                dbssReqModel = _raToDBssParse.ValidateSIMReqParsing2(simNumberCheckReqest);

                apiUrl = String.Format(PostAPICollection.CheckSIM);

                log.req_blob = _blJson.GetGenericJsonData(dbssReqModel);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpPostRequest(dbssReqModel, apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                if (dbssResp["data"]== null)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.NoDataFound;
                }
                raResp = _dbssToRaParse.SIMValidationParsing2(dbssResp, purposeOfSIMCheck, simCategory == null ? null : simCategory, isPaired, old_sim_type);
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                log.is_success = 0;
                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description
                                                                                    : error.error_custom_msg;
                }
                catch (Exception)
                {
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description
                                                                                    : error.error_custom_msg;
                }
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = simNumberCheckReqest.purpose_number;
                log.user_id = simNumberCheckReqest.retailer_id;
                log.method_name = "CheckSIMNumber2";
                log.msisdn = _bllLog.FormatMSISDN(simNumberCheckReqest.msisdn);

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp);
            }
            return raResp;
        }
        /// <summary>
        /// This method is used for SIM Number validation
        /// </summary>
        /// <param name="simNumberCheckReqest"></param>
        /// <returns>Success/ Failure</returns>
        public async Task<RACommonResponse> CheckSIMNumber3(SIMNumberCheckRequest simNumberCheckReqest, int purposeOfSIMCheck, bool? isPaired, int? simCategory, string old_sim_type)
        {
            RACommonResponse raResp = new RACommonResponse();
            string apiUrl = "", txtResp = "";
            SIMValidationRequestRootobject dbssReqModel = null;
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                dbssReqModel = _raToDBssParse.ValidateSIMReqParsing2(simNumberCheckReqest);

                apiUrl = String.Format(PostAPICollection.CheckSIM);

                log.req_blob = _blJson.GetGenericJsonData(dbssReqModel);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpPostRequest(dbssReqModel, apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                if (dbssResp["data"]== null)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.NoDataFound;
                }
                raResp = _dbssToRaParse.SIMValidationParsing3(dbssResp, purposeOfSIMCheck, simCategory == null ? null : simCategory, isPaired, old_sim_type);
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                log.is_success = 0;
                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description
                                                                                    : error.error_custom_msg;
                }
                catch (Exception)
                {
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description
                                                                                    : error.error_custom_msg;
                }
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = simNumberCheckReqest.purpose_number;
                log.user_id = simNumberCheckReqest.retailer_id;
                log.method_name = "CheckSIMNumber3";
                log.msisdn = _bllLog.FormatMSISDN(simNumberCheckReqest.msisdn);

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp);
            }
            return raResp;
        }

        /// <summary>
        /// This method is used for SIM Number validation
        /// </summary>
        /// <param name="simNumberCheckReqest"></param>
        /// <returns>Success/ Failure</returns>
        public async Task<RACommonResponse> CheckSIMNumber4(SIMNumberCheckRequest simNumberCheckReqest, int purposeOfSIMCheck, bool? isPaired, int? simCategory, string old_sim_type)
        {
            RACommonResponse raResp = new RACommonResponse();
            string apiUrl = "", txtResp = "";
            SIMValidationRequestRootobject dbssReqModel = null;
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                dbssReqModel = _raToDBssParse.ValidateSIMReqParsing2(simNumberCheckReqest);

                apiUrl = String.Format(PostAPICollection.CheckSIM);

                log.req_blob = _blJson.GetGenericJsonData(dbssReqModel);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpPostRequest(dbssReqModel, apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                if (dbssResp["data"] == null)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.NoDataFound;
                }

                raResp = _dbssToRaParse.SIMValidationParsing4(dbssResp, purposeOfSIMCheck, simCategory == null ? null : simCategory, isPaired, old_sim_type);
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                log.is_success = 0;
                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description
                                                                                    : error.error_custom_msg;
                }
                catch (Exception)
                {
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description
                                                                                    : error.error_custom_msg;
                }
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = simNumberCheckReqest.purpose_number;
                log.user_id = simNumberCheckReqest.retailer_id;
                log.method_name = "CheckSIMNumber4";
                log.msisdn = _bllLog.FormatMSISDN(simNumberCheckReqest.msisdn);

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp);
            }
            return raResp;
        }
        #endregion

        #region Order validation
        private bool orderValidation(RAOrderRequest model)
        {
            switch (Convert.ToInt32(model.purpose_number))
            {
                case (int)EnumPurposeNumber.NewRegistration:
                    if (string.IsNullOrEmpty(model.msisdn))
                    {
                        ModelState.AddModelError("ClientName", "Please enter your name");
                    }
                    break;

                case (int)EnumPurposeNumber.SIMReplacement:

                    break;

                case (int)EnumPurposeNumber.MNPRegistration:

                    break;

                case (int)EnumPurposeNumber.MNPEmergencyReturn:

                    break;

                case (int)EnumPurposeNumber.MNPDeRegistration:

                    break;


                case (int)EnumPurposeNumber.IndividualToCorporateTransfer:

                    break;


                case (int)EnumPurposeNumber.CorporateToIndividualTransfer:

                    break;


                case (int)EnumPurposeNumber.SIMTransfer:

                    break;


                default:

                    break;
            }
            return false;
        }
        #endregion

        #region get old SIM
        internal async Task<OldSIMNnumberResponse> GetOldSIMumber(string sIMCardsApiUrl, string username, string purposeNo)
        {
            OldSIMNnumberResponse osnResp = new OldSIMNnumberResponse();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                apiUrl = AppSettingsWrapper.ApiBaseUrl + apiUrl;
                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;


                object dbssResp = _apiReq.HttpGetRequest(apiUrl);
                txtResp = JsonConvert.SerializeObject(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                var dbssRespModel = JsonConvert.DeserializeObject<SIMNumberParsingRootobject>(dbssResp.ToString());
                log.res_time = DateTime.Now;
                log.is_success = 1;

                if (dbssRespModel == null)
                {
                    osnResp.result = false;
                    osnResp.message = MessageCollection.NoDataFound;
                    return osnResp;
                }
                osnResp = _dbssToRaParse.OldSIMNumberParsing(dbssRespModel);
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                log.is_success = 0;
                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                }
                catch (Exception)
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.is_success = 0;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                }

                osnResp.result = false;
                osnResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
            }

            finally
            {

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = purposeNo;
                log.user_id = username;
                log.method_name = "GetOldSIMumber";

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return osnResp;

        }
        #endregion


        #region Submit Order Request Binding For Log
        internal object SubmitOrderRequestBindingForLog(RAOrderRequest model)
        {
            return new
            {
                bi_token_number = model.bi_token_number,
                purpose_number = model.purpose_number,
                msisdn = model.msisdn,
                sim_category = model.sim_category,
                sim_number = model.sim_number,
                subscription_type_id = model.subscription_type_id,
                subscription_code = model.subscription_code,
                package_id = model.sim_category,
                package_code = model.package_code,
                dest_doc_type_no = model.dest_nid,
                src_nid = model.src_nid,
                dest_dob = model.dest_dob,
                src_doc_type_no = model.src_doc_type_no,
                src_dob = model.src_dob,
                platform_id = model.package_id,
                customer_name = model.customer_name,
                gender = model.gender,
                flat_number = model.flat_number,
                house_number = model.house_number,
                road_number = model.road_number,
                village = model.village,
                division_id = model.division_id,
                district_id = model.district_id,
                thana_id = model.thana_id,
                postal_code = model.postal_code,
                email = model.email,
                retailer_code = model.retailer_id,
                retailer_id = model.retailer_id,
                port_in_date = model.port_in_date,
                alt_msisdn = model.alt_msisdn,
                poc_number = model.poc_msisdn_number,
                is_urgent = model.is_urgent,
                optional1 = model.optional1,
                optional2 = model.optional2,
                optional3 = model.optional3,
                optional4 = model.optional4,
                optional5 = model.optional5,
                optional6 = model.optional6,
                note = model.note,
                sim_rep_reason_id = model.sim_rep_reason_id,
                payment_type = model.payment_type,
                is_paired = model.is_paired,
                cahnnel_id = model.channel_id,
                division_name = model.division_name,
                district_name = model.district_name,
                thana_name = model.thana_name,
                center_code = model.center_code,
                distributor_code = model.distributor_code,
                sim_replc_reason = model.sim_replc_reason,
                channel_name = model.channel_name,
                right_id = model.right_id,
                sim_replacement_type = model.sim_replacement_type,
                old_sim_number = model.old_sim_number,
                src_sim_category = model.src_sim_category,
                port_in_confirmation_code = model.port_in_confirmation_code,
                dest_ec_verifi_reqrd = model.dest_ec_verifi_reqrd,
                src_ec_verifi_reqrd = model.src_ec_verifi_reqrd,
                dest_foreign_flag = model.dest_foreign_flag,
                dbss_subscription_id = model.dbss_subscription_id,
                saf_status = model.saf_status,
                customer_id = model.customer_id
            };
        }
        internal object SubmitOrderRequestBindingForLogV2(RAOrderRequestV2 model)
        {
            return new
            {
                bi_token_number = model.bi_token_number,
                purpose_number = model.purpose_number,
                msisdn = model.msisdn,
                sim_category = model.sim_category,
                sim_number = model.sim_number,
                subscription_type_id = model.subscription_type_id,
                subscription_code = model.subscription_code,
                package_id = model.sim_category,
                package_code = model.package_code,
                dest_doc_type_no = model.dest_nid,
                src_nid = model.src_nid,
                dest_dob = model.dest_dob,
                src_doc_type_no = model.src_doc_type_no,
                src_dob = model.src_dob,
                platform_id = model.package_id,
                customer_name = model.customer_name,
                gender = model.gender,
                flat_number = model.flat_number,
                house_number = model.house_number,
                road_number = model.road_number,
                village = model.village,
                division_id = model.division_id,
                district_id = model.district_id,
                thana_id = model.thana_id,
                postal_code = model.postal_code,
                email = model.email,
                retailer_code = model.retailer_id,
                retailer_id = model.retailer_id,
                port_in_date = model.port_in_date,
                alt_msisdn = model.alt_msisdn,
                poc_number = model.poc_msisdn_number,
                is_urgent = model.is_urgent,
                optional1 = model.optional1,
                optional2 = model.optional2,
                optional3 = model.optional3,
                optional4 = model.optional4,
                optional5 = model.optional5,
                optional6 = model.optional6,
                note = model.note,
                sim_rep_reason_id = model.sim_rep_reason_id,
                payment_type = model.payment_type,
                is_paired = model.is_paired,
                cahnnel_id = model.channel_id,
                division_name = model.division_name,
                district_name = model.district_name,
                thana_name = model.thana_name,
                center_code = model.center_code,
                distributor_code = model.distributor_code,
                sim_replc_reason = model.sim_replc_reason,
                channel_name = model.channel_name,
                right_id = model.right_id,
                sim_replacement_type = model.sim_replacement_type,
                old_sim_number = model.old_sim_number,
                src_sim_category = model.src_sim_category,
                port_in_confirmation_code = model.port_in_confirmation_code,
                dest_ec_verifi_reqrd = model.dest_ec_verifi_reqrd,
                src_ec_verifi_reqrd = model.src_ec_verifi_reqrd,
                dest_foreign_flag = model.dest_foreign_flag,
                dbss_subscription_id = model.dbss_subscription_id,
                saf_status = model.saf_status,
                customer_id = model.customer_id,
                lac = model.lac,
                cid = model.cid,
                latitude = model.latitude,
                longitude = model.longitude
            };
        }
        #endregion

        #region Is DBSS Error 
        internal bool isDBSSErrorOccurred(WebException exception)
        {
            try
            {
                var error = exception.Response as HttpWebResponse;
                if(error != null)
                {
                    return error.StatusCode == HttpStatusCode.BadRequest ? false : true;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion


        #region Is DBSS 500 Error (internal server error)
        internal bool isDBSS500ErrorOccurred(WebException exception)
        {
            try
            {
                var error = exception.Response as HttpWebResponse;
                if( error != null )
                    return error.StatusCode == HttpStatusCode.InternalServerError ? true : false;

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Validate-OTP
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<OTPResponse> ValidateOTP(DBSSOTPValidationRequest model, string username)
        {
            OTPResponse OtpResp = new OTPResponse();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                apiUrl = String.Format(PatchAPICollection.VerifyOTP, model.otp);
                DBSSOTPValidationRequestRootobject vaidateOTPReqModel = _raToDBssParse.DBSSOTPValidationReqParsing(model);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl + JsonConvert.SerializeObject(vaidateOTPReqModel));
                log.req_time = DateTime.Now;

                object dbssResp = await _apiReq.HttpPatchRequest(vaidateOTPReqModel, apiUrl);

                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(txtResp);
                log.res_time = DateTime.Now;
                System.Reflection.PropertyInfo pi = dbssResp.GetType().GetProperty("Status");
                String name = (String)(pi?.GetValue(dbssResp, null));

                if (name == "WaitingForActivation")
                    throw new Exception("DBSS Error: " + name);

                var dbssRespModel = JsonConvert.DeserializeObject<DBSSOTPResponseRootobject>(dbssResp.ToString());

                if (dbssRespModel != null && dbssRespModel.data == null)
                {
                    log.is_success = 1;
                    return new OTPResponse()
                    {
                        is_otp_valid = false,
                        result = false,
                        message = MessageCollection.InvalidOTP
                    };
                }

                log.is_success = 1;
                OtpResp = _dbssToRaParse.OTPRespParsing(dbssRespModel);
                return OtpResp;
            }
            catch (WebException ex)
            {
                string resp = string.Empty;
                if(ex.Response != null)
                resp =new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                ErrorDescription error = null;

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        var respObj1 = JsonConvert.DeserializeObject<Object>(resp);
                        JObject respObj2 = (JObject)respObj1;

                        error = await _bllLog.ManageException(respObj2?["errors"] != null
                                                    && respObj2?["errors"]?.ToString() != "" ? respObj2?["errors"]?.ToString() : ex.Message, ex.HResult, "BIA");
                    }
                    catch (Exception ex2)
                    {
                        error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");
                    }
                }
                else
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    log.res_time = DateTime.Now;

                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                }

                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;

                OtpResp.is_otp_valid = false;
                OtpResp.result = false;

                var webErrorResp = ex.Response as HttpWebResponse;
                if(webErrorResp != null)
                if (webErrorResp.StatusCode == HttpStatusCode.BadRequest)
                {
                    OtpResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? MessageCollection.InvalidOTP + error.error_description
                                                                                    : MessageCollection.InvalidOTP + error.error_custom_msg;
                }
                else
                {
                    OtpResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }

                return OtpResp;
            }
            catch (Exception ex)
            {
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.is_success = 0;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    OtpResp.is_otp_valid = false;
                    OtpResp.result = false;
                    OtpResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return OtpResp;
                }
                catch (Exception ex2)
                {
                    throw ex2;
                }

            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = Convert.ToString(model.purpose);
                log.user_id = username;
                log.method_name = "ValidateOTP";
                log.msisdn = _bllLog.FormatMSISDN(model.auth_msisdn);

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        public async Task<OTPResponseRev> ValidateOTPV2(DBSSOTPValidationRequest model, string username)
        {
            OTPResponseRev OtpResp = new OTPResponseRev();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                apiUrl = String.Format(PatchAPICollection.VerifyOTP, model.otp);
                DBSSOTPValidationRequestRootobject vaidateOTPReqModel = _raToDBssParse.DBSSOTPValidationReqParsing(model);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl + JsonConvert.SerializeObject(vaidateOTPReqModel));
                log.req_time = DateTime.Now;

                object dbssResp = await _apiReq.HttpPatchRequest(vaidateOTPReqModel, apiUrl);

                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(txtResp);
                log.res_time = DateTime.Now;
                System.Reflection.PropertyInfo pi = dbssResp.GetType().GetProperty("Status");
                String name = (String)(pi?.GetValue(dbssResp, null));

                if (name == "WaitingForActivation")
                    throw new Exception("DBSS Error: " + name);

                var dbssRespModel = JsonConvert.DeserializeObject<DBSSOTPResponseRootobject>(dbssResp.ToString());

                if (dbssRespModel == null)
                {
                    return new OTPResponseRev()
                    {
                        isError = true,
                        message = MessageCollection.InvalidOTP,
                        data = new OTPRespData()
                        {
                            is_otp_valid = false
                        }
                    };
                }
                else if (dbssRespModel.data == null)
                {
                    return new OTPResponseRev()
                    {
                        isError = true,
                        message = MessageCollection.InvalidOTP,
                        data = new OTPRespData()
                        {
                            is_otp_valid = false
                        }
                    };
                }
                else
                {
                    OtpResp = _dbssToRaParse.OTPRespParsingV2(dbssRespModel);
                }

                log.is_success = 1;
                return OtpResp;
            }
            catch (WebException ex)
            {
                string resp = string.Empty;
                if(ex.Response != null)
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                ErrorDescription error = null;

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        var respObj1 = JsonConvert.DeserializeObject<Object>(resp);
                        JObject respObj2 = (JObject)respObj1;

                        error = await _bllLog.ManageException(respObj2?["errors"] != null
                                                    && respObj2?["errors"]?.ToString() != "" ? respObj2?["errors"]?.ToString() : ex.Message, ex.HResult, "BIA");
                    }
                    catch (Exception ex2)
                    {
                        error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");
                    }
                }
                else
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    log.res_time = DateTime.Now;

                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                }

                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;

                OtpResp.data = new OTPRespData()
                {
                    is_otp_valid = false
                };
                OtpResp.isError = true;

                var webErrorResp = ex.Response as HttpWebResponse;
                if (webErrorResp != null)
                if (webErrorResp.StatusCode == HttpStatusCode.BadRequest)
                {
                    OtpResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? MessageCollection.InvalidOTP + error.error_description
                                                                                    : MessageCollection.InvalidOTP + error.error_custom_msg;
                }
                else
                {
                    OtpResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }

                return OtpResp;
            }
            catch (Exception ex)
            {
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.is_success = 0;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    OtpResp.data = new OTPRespData()
                    {
                        is_otp_valid = false
                    };
                    OtpResp.isError = true;
                    OtpResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return OtpResp;
                }
                catch (Exception ex2)
                {
                    throw ex2;
                }

            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = Convert.ToString(model.purpose);
                log.user_id = username;
                log.method_name = "ValidateOTPV2";
                log.msisdn = _bllLog.FormatMSISDN(model.auth_msisdn);

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        #endregion


        #region  MSISDN validation Unpaired
        /// <summary>
        /// This method is used for MSISDN validation for unpaired
        /// </summary>
        /// <param name="imsiCheckReq">Mobile number</param>
        /// <returns>Success/ Failure</returns>

        internal async Task<GetImsiRespObj> GetImsiBySimAsync(GetImsiReq imsiCheckReq)
        {
            GetImsiRespObj imsiResp = new GetImsiRespObj();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();

            try
            {

                if (imsiCheckReq?.sim != null && imsiCheckReq.sim.Substring(0, 6) != FixedValueCollection.SIMCode)
                {
                    imsiCheckReq.sim = FixedValueCollection.SIMCode + imsiCheckReq.sim;
                }
                else if (imsiCheckReq?.sim != null && imsiCheckReq.sim.Substring(0, 6) == FixedValueCollection.SIMCode)
                {
                    imsiCheckReq.sim =  imsiCheckReq.sim;
                }

                apiUrl = String.Format(GetAPICollection.GetImsiBySim, imsiCheckReq?.sim);
                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);
                log.is_success = 1;

                if (dbssResp["data"] == null)
                {
                    log.is_success = 0;
                    imsiResp.result = false;
                    imsiResp.message = MessageCollection.NoDataFound;
                    return imsiResp;
                }

                imsiResp = await _dbssToRaParse.GetImsiRespParsingAsync(dbssResp);

                if (imsiResp.result == false)
                {
                    log.is_success = 0;
                    imsiResp.result = false;
                    imsiResp.message = imsiResp.message;
                    return imsiResp;
                }

                imsiResp.result = true;
                imsiResp.message = MessageCollection.Success;
                return imsiResp;
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                ErrorDescription error = null;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    imsiResp.result = false;
                    imsiResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                    return imsiResp;
                }
                catch (Exception)
                {
                    imsiResp.result = false;
                    imsiResp.message = ex.Message;
                    return imsiResp;
                }
            }
            finally
            {
                if(imsiCheckReq != null)
                {
                    log.msisdn = imsiCheckReq.msisdn != null ? imsiCheckReq.msisdn : "";
                    log.purpose_number = imsiCheckReq.purpose_number != null ? imsiCheckReq.purpose_number: "";
                    log.user_id = imsiCheckReq.retailer_id != null ? imsiCheckReq.retailer_id : "";
                }
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.method_name = "GetImsiBySim";
                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        #endregion


        #region bioverification process

        public async Task<BioVerifyResp> BssServiceProcess(BiomerticDataModel item)
        {
            LogModel log = new LogModel();
            BiometricPopulateModel pltApiObj = new BiometricPopulateModel();
            BioVerifyResp verifyResp = new BioVerifyResp();
            string meathodUrl = "/api/v1/biometric";
            string bssReqId = "";
            GetImsiRespObj imsiResp = new GetImsiRespObj();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BL_Json byteArrayConverter = new BL_Json();

            if (item.status == (int)EnumRAOrderStatus.BioVerificationSubmitted)
            {
                try
                {
                    if (item.purpose_number == (int)EnumPurposeNumber.NewRegistration)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateNewRegReqModel(item);
                        else
                            reqModel = pltApiObj.PopulateCorpNewRegReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBss(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }

                    else if (item.purpose_number == (int)EnumPurposeNumber.DeRegistration)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateDeRegReqModel(item);
                        else
                            reqModel = pltApiObj.PopulateCorpDeRegReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBss(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }

                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMReplacement)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateSimRepRegReqModel(item);
                        else
                        {
                            if (item.sim_replacement_type == (int)EnumSIMReplacementType.ByPOC)
                                reqModel = pltApiObj.PopulateCorpSimReplacebyPocReqModel(item);
                            else if (item.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                                reqModel = pltApiObj.PopulateCorpSimReplacebyAuthPerReqModel(item);
                            else if (item.sim_replacement_type == (int)EnumSIMReplacementType.BulkSIMReplacment)
                                reqModel = pltApiObj.PopulateCorpSimReplacebyBulkReqModel(item);
                        }

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBss(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }

                    else if (item.purpose_number == (int)EnumPurposeNumber.MNPRegistration)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateMnpRegReqModel(item);
                        else
                            reqModel = pltApiObj.PopulateCorpMnpPortInReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBss(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.MNPEmergencyReturn)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateMnpEmgRtnRegReqModel(item);
                        else
                            reqModel = pltApiObj.PopulateCorpMnpEmerReturnReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBss(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }

                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMTransfer)
                    {
                        object reqModel = new object();
                        if (item.src_ec_verification_required == 1 && String.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateSimTransferNidToNidBioReqModel(item);

                        else if (item.src_ec_verification_required == 1)
                            reqModel = pltApiObj.PopulateSimTransferBioReqModel(item);

                        else
                            reqModel = pltApiObj.PopulateSimTransferWithoutSrcBioReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBss(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.CorporateToIndividualTransfer)
                    {
                        object reqModel = new object();

                        if (item.src_ec_verification_required == 1)
                            reqModel = pltApiObj.PopulateSimTransferBioReqModel(item);
                        else if (!string.IsNullOrEmpty(item.otp_no))
                            reqModel = pltApiObj.PopulateCorpSimTransferWithOTPBioReqModel(item);
                        else
                            reqModel = pltApiObj.PopulateCorpSimTransferBioReqModel(item);
                        //object reqModel = pltObj.PopulateSimTransferBioReqModel(item);//as per siful vai instructin, use tos request model here.

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBss(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.IndividualToCorporateTransfer)
                    {
                        object reqModel = pltApiObj.PopulateSimTransferBioReqModel(item);//as per siful vai instructin, use tos request model here.

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBss(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.mnp_port_in_cancel)
                    {
                        PortInCnlRegReqModel reqModel = pltApiObj.PopulatePortCnlRegReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBss(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMCategoryMigration)
                    {
                        object reqModel = new object();
                        if (!string.IsNullOrEmpty(item.poc_number))
                        {
                            reqModel = pltApiObj.PopulateCorpNewRegReqModel(item);
                        }
                        else
                        {
                            reqModel = pltApiObj.PopulatePreToPostMigrationReqModel(item);
                        }

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBss(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;

                    }
                    return verifyResp;
                }
                catch (Exception ex)
                {
                    return verifyResp;
                }

            }
            return verifyResp;
        }

        public async Task<BioVerifyResp> BssServiceProcessV2(BiomerticDataModel item)
        {
            LogModel log = new LogModel();
            BiometricPopulateModel pltApiObj = new BiometricPopulateModel();
            BioVerifyResp verifyResp = new BioVerifyResp();
            string meathodUrl = "/api/v1/biometric";
            string bssReqId = "";
            GetImsiRespObj imsiResp = new GetImsiRespObj();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BL_Json byteArrayConverter = new BL_Json();

            if (item.status == (int)EnumRAOrderStatus.BioVerificationSubmitted)
            {
                try
                {
                    if (item.purpose_number == (int)EnumPurposeNumber.NewRegistration)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateNewRegReqModel(item);
                        else
                            reqModel = pltApiObj.PopulateCorpNewRegReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }

                    else if (item.purpose_number == (int)EnumPurposeNumber.DeRegistration)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateDeRegReqModel(item);
                        else
                            reqModel = pltApiObj.PopulateCorpDeRegReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }

                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMReplacement)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateSimRepRegReqModel(item);
                        else
                        {
                            if (item.sim_replacement_type == (int)EnumSIMReplacementType.ByPOC)
                                reqModel = pltApiObj.PopulateCorpSimReplacebyPocReqModel(item);
                            else if (item.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                                reqModel = pltApiObj.PopulateCorpSimReplacebyAuthPerReqModel(item);
                            else if (item.sim_replacement_type == (int)EnumSIMReplacementType.BulkSIMReplacment)
                                reqModel = pltApiObj.PopulateCorpSimReplacebyBulkReqModel(item);
                        }

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }

                    else if (item.purpose_number == (int)EnumPurposeNumber.MNPRegistration)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateMnpRegReqModel(item);
                        else
                            reqModel = pltApiObj.PopulateCorpMnpPortInReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.MNPEmergencyReturn)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateMnpEmgRtnRegReqModel(item);
                        else
                            reqModel = pltApiObj.PopulateCorpMnpEmerReturnReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }

                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMTransfer)
                    {
                        object reqModel = new object();
                        if (item.src_ec_verification_required == 1 && String.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateSimTransferNidToNidBioReqModel(item);

                        else if (item.src_ec_verification_required == 1)
                            reqModel = pltApiObj.PopulateSimTransferBioReqModel(item);

                        else
                            reqModel = pltApiObj.PopulateSimTransferWithoutSrcBioReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.CorporateToIndividualTransfer)
                    {
                        object reqModel = new object();

                        if (item.src_ec_verification_required == 1)
                            reqModel = pltApiObj.PopulateSimTransferBioReqModel(item);
                        else if (!string.IsNullOrEmpty(item.otp_no))
                            reqModel = pltApiObj.PopulateCorpSimTransferWithOTPBioReqModel(item);
                        else
                            reqModel = pltApiObj.PopulateCorpSimTransferBioReqModel(item);
                        //object reqModel = pltObj.PopulateSimTransferBioReqModel(item);//as per siful vai instructin, use tos request model here.

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.IndividualToCorporateTransfer)
                    {
                        object reqModel = pltApiObj.PopulateSimTransferBioReqModel(item);//as per siful vai instructin, use tos request model here.

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.mnp_port_in_cancel)
                    {
                        PortInCnlRegReqModel reqModel = pltApiObj.PopulatePortCnlRegReqModel(item);

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMCategoryMigration)
                    {
                        object reqModel = new object();
                        if (!string.IsNullOrEmpty(item.poc_number))
                        {
                            reqModel = pltApiObj.PopulateCorpNewRegReqModel(item);
                        }
                        else
                        {
                            reqModel = pltApiObj.PopulatePreToPostMigrationReqModel(item);
                        }

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;

                    }
                    return verifyResp;
                }
                catch (Exception ex)
                {
                    return verifyResp;
                }

            }
            return verifyResp;
        }

        public async Task<BioVerifyResp> BssServiceProcessStarTrek(BiomerticDataModel item, string reservationId,string userName, int isOnline)
        { 
            LogModel log = new LogModel();
            BiometricPopulateModel pltApiObj = new BiometricPopulateModel();
            BioVerifyResp verifyResp = new BioVerifyResp();
            string meathodUrl = "/api/v1/biometric";
            string bssReqId = "";
            GetImsiRespObj imsiResp = new GetImsiRespObj();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BL_Json byteArrayConverter = new BL_Json();
            RACommonResponse response = new RACommonResponse();

            if (item.status == (int)EnumRAOrderStatus.BioVerificationSubmitted)
            {
                try
                {
                    if (item.purpose_number == (int)EnumPurposeNumber.NewRegistration)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateNewRegReqModel(item);
                        else
                            reqModel = pltApiObj.PopulateCorpNewRegReqModel(item);

                        if(isOnline == 1)
                        {
                            response = await _apiCall.UnreserveMSISDNStarTrek(reservationId, userName, "", "", item.msisdn);
                            
                            if(response.result == false)
                            {
                                verifyResp.is_success = false;
                                verifyResp.err_msg = response.message;
                                return verifyResp;
                            }

                            log.req_time = DateTime.Now;
                            verifyResp = await _apiCall.BioVerificationReqToBssV3(item, reqModel, meathodUrl);
                            log.res_time = DateTime.Now;
                        }
                        else
                        {
                            log.req_time = DateTime.Now;
                            verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                            log.res_time = DateTime.Now;
                        }
                    }
                    else if (item.purpose_number == (int)EnumPurposeNumber.SIMReplacement)
                    {
                        object reqModel = new object();

                        if (string.IsNullOrEmpty(item.poc_number))
                            reqModel = pltApiObj.PopulateSimRepRegReqModel(item);
                        else
                        {
                            if (item.sim_replacement_type == (int)EnumSIMReplacementType.ByPOC)
                                reqModel = pltApiObj.PopulateCorpSimReplacebyPocReqModel(item);
                            else if (item.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                                reqModel = pltApiObj.PopulateCorpSimReplacebyAuthPerReqModel(item);
                            else if (item.sim_replacement_type == (int)EnumSIMReplacementType.BulkSIMReplacment)
                                reqModel = pltApiObj.PopulateCorpSimReplacebyBulkReqModel(item);
                        }

                        log.req_time = DateTime.Now;
                        verifyResp = await _apiCall.BioVerificationReqToBssV2(item, reqModel, meathodUrl);
                        log.res_time = DateTime.Now;
                    }                    
                    return verifyResp;
                }
                catch (Exception ex)
                {
                    verifyResp.err_msg = ex.Message;
                    return verifyResp;
                }

            }
            return verifyResp;
        }
        #endregion

        #region Get Decrypted Security Token
        internal string GetDecryptedSecurityToken(string encryptedToken)
        {
            string decriptedSecurityToken = string.Empty;
            string loginProviderId = string.Empty;
            try
            {
                decriptedSecurityToken = AESCryptography.Decrypt(encryptedToken);

                if (decriptedSecurityToken.Equals("InvalidSessionToken"))
                {
                    decriptedSecurityToken = string.Empty;
                    decriptedSecurityToken = Cryptography.Decrypt(encryptedToken, true);
                    loginProviderId = _bllCommon.GetDataFromSecurityTokenV3(decriptedSecurityToken, (int)EnumSecurityTokenPropertyIndex.LoginProvider);

                }
                else
                {
                    loginProviderId = _bllCommon.GetDataFromSecurityTokenV2(decriptedSecurityToken, (int)EnumSecurityTokenPropertyIndex.LoginProvider);
                }

                return loginProviderId;
            }
            catch (Exception)
            {
                try
                {
                    decriptedSecurityToken = Cryptography.Decrypt(encryptedToken, true);
                    loginProviderId = _bllCommon.GetDataFromSecurityTokenV3(decriptedSecurityToken, (int)EnumSecurityTokenPropertyIndex.LoginProvider);

                    return loginProviderId;
                }
                catch (Exception)
                {
                    return "Fail";
                }
            }
        }
        #endregion

        #region Cherish Number Sell
        public async Task<RACommonResponseRevamp> ValidateMSISDNVAndSIM(CherishMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();

            try
            {
                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;

                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp["data"] == null)
                {
                    log.is_success = 0;
                    raRespModel.isError = true;
                    raRespModel.message = "MSISDN: " + MessageCollection.NoDataFound;
                    return raRespModel;
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.MSISDNReqParsingCherish(dbssResp, msisdnCheckReqest.retailer_id, msisdnCheckReqest.selected_category);

                if (msisdnResp.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = msisdnResp.message;
                    return raRespModel;
                }

                var stockCheck = await _bllCommon.IsStockAvailable(msisdnResp.stock_id, Convert.ToInt32(msisdnCheckReqest.channel_id));

                if (stockCheck == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.StockIDMismatch;
                    return raRespModel;
                }


                var simResp = await CheckSIMNumber3(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = string.Empty,
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = string.Empty,
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
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
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }

                string resp = string.Empty;
                if (ex.Response != null)
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return raRespModel;
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.isError = true;
                            if (isDBSSErrorOccurred(ex))
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return raRespModel;
                        }
                        catch (Exception)
                        {
                            raRespModel.isError = true;
                            raRespModel.message = ex.Message;

                            log.error_code = error != null ? error.error_code : String.Empty;
                            log.error_source = error != null ? error.error_source : String.Empty;
                            log.message = error != null ? error.error_description : String.Empty;

                            return raRespModel;
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.error_code = error != null ? error.error_code : String.Empty;
                        log.error_source = error != null ? error.error_source : String.Empty;
                        log.message = error != null ? error.error_description : String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raRespModel.isError = true;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

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
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = apiName;

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        public async Task<RACommonResponseRevamp> ValidateMSISDNVAndSIMV2(CherishMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();

            try
            {
                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;

                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                if (dbssResp["data"] == null)
                {
                    log.is_success = 0;
                    raRespModel.isError = true;
                    raRespModel.message = "MSISDN: " + MessageCollection.NoDataFound;
                    return raRespModel;
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.MSISDNReqParsingCherish(dbssResp, msisdnCheckReqest.retailer_id, msisdnCheckReqest.selected_category);

                if (msisdnResp.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = msisdnResp.message;
                    return raRespModel;
                }

                var stockCheck = await _bllCommon.IsStockAvailable(msisdnResp.stock_id, Convert.ToInt32(msisdnCheckReqest.channel_id));

                if (stockCheck == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.StockIDMismatch;
                    return raRespModel;
                }


                var simResp = await CheckSIMNumber4(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = msisdnCheckReqest.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
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
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }

                string resp = string.Empty;
                if (ex.Response != null)
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error =await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return raRespModel;
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.isError = true;
                            if (isDBSSErrorOccurred(ex))
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return raRespModel;
                        }
                        catch (Exception)
                        {
                            raRespModel.isError = true;
                            raRespModel.message = ex.Message;

                            log.error_code = error != null ? error.error_code : String.Empty;
                            log.error_source = error != null ? error.error_source : String.Empty;
                            log.message = error != null ? error.error_description : String.Empty;

                            return raRespModel;
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (isDBSSErrorOccurred(ex))
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.error_code = error != null ? error.error_code : String.Empty;
                        log.error_source = error != null ? error.error_source : String.Empty;
                        log.message = error != null ? error.error_description : String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return raRespModel;
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raRespModel.isError = true;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

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
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidateUnpairedMSISDNV5";

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        #endregion
    }
}
