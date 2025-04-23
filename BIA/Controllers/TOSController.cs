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
using static BIA.Common.ModelValidation;

namespace BIA.Controllers
{
    [Route("api/TOS")]
    [ApiController]
    public class TOSController : ControllerBase
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

        public TOSController(ApiRequest apiReq, BL_Json blJson, BLLOrder orderManager, BLLLog bllLog, BiometricApiCall apiCall, BaseController bio, GeoFencingValidation geo, BLLDBSSToRAParse dbssToRaParse, ApiManager apiManager)
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
        }

        #region Individual TOS MSISDN validation NEW
        //======================================New [14-05-2020] =====================================
        /// <summary>
        /// This API is used for source customer's MSISDN validation for TOS NID to NID
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[ResponseType(typeof(TosNidToNidMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForTosNidToNid")]
        public async Task<IActionResult> ValidateMSISDNForTosNidToNidV1([FromBody] TosNidToNidMsisdnCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = string.Empty, txtResp = string.Empty;
            string loanCheckApiUrl = string.Empty;
            string debtCheckApi = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            TOSLoanStatusResponse loanStatusResponse = new TOSLoanStatusResponse();
            TOSDebtStatusResponse debtStatusResponse = new TOSDebtStatusResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);


                string srcMsisdn = msisdnCheckReqest.mobile_number;

                if (srcMsisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    srcMsisdn = FixedValueCollection.MSISDNCountryCode + srcMsisdn;
                }

                apiUrl = String.Format(GetAPICollection.GetSubscriptionByMSISDNIncludingSimCardsPayerCustomerOwnerCustomerUserCustomer, srcMsisdn);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = MessageCollection.SIMReplNoDataFound,
                    });
                }

                TosNidToNidMSISDNCheckResponse msisdnValidationResp = _dbssToRaParse.TosNidToNidMSISDNReqParsingV1(dbssResp);

                if (msisdnValidationResp.result == false)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = FixedValueCollection.MSISDNError + msisdnValidationResp.message
                    });
                }
                else
                {
                    try
                    {
                        debtCheckApi = String.Format(GetAPICollection.DebtCheckApi, msisdnValidationResp.dbss_subscription_id);
                        JObject dbssRespForDebt = await GetDebtStatusForTOS(debtCheckApi, msisdnCheckReqest);
                        debtStatusResponse = _dbssToRaParse.TOSDebtStatusCheckParse(dbssRespForDebt);

                        if (debtStatusResponse.result == true)
                        {
                            msisdnValidationResp.result = debtStatusResponse.result;
                            msisdnValidationResp.message = debtStatusResponse.message;
                            return Ok(msisdnValidationResp);
                        }
                        else
                        {
                            loanCheckApiUrl = String.Format(GetAPICollection.GetLoanStatusForTOS, msisdnValidationResp.dbss_subscription_id);
                            JObject dbssRespForLoan = await GetLoanStatusForTOS(loanCheckApiUrl, msisdnCheckReqest);
                            loanStatusResponse = _dbssToRaParse.TosNiDtoNIDLoanStatusCheckParsing(dbssRespForLoan);

                            msisdnValidationResp.result = loanStatusResponse.result;
                            msisdnValidationResp.message = loanStatusResponse.message;
                        }

                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                msisdnValidationResp.dob = "**/**/****";
                msisdnValidationResp.doc_id_number = "**********";

                return Ok(msisdnValidationResp);
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

                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = true,
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
                log.method_name = "ValidateMSISDNForTosNidToNidV1";
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        /// <summary>
        /// This API is used for source customer's MSISDN validation for TOS NID to NID
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[ResponseType(typeof(TosNidToNidMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForTosNidToNidV2")]
        public async Task<IActionResult> ValidateMSISDNForTosNidToNidV2([FromBody] TosNidToNidMsisdnCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = string.Empty, txtResp = string.Empty;
            string loanCheckApiUrl = string.Empty;
            string debtCheckApi = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            TOSLoanStatusResponse loanStatusResponse = new TOSLoanStatusResponse();
            TOSDebtStatusResponse debtStatusResponse = new TOSDebtStatusResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                string srcMsisdn = msisdnCheckReqest.mobile_number;

                if (srcMsisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    srcMsisdn = FixedValueCollection.MSISDNCountryCode + srcMsisdn;
                }

                apiUrl = String.Format(GetAPICollection.GetSubscriptionByMSISDNIncludingSimCardsPayerCustomerOwnerCustomerUserCustomer, srcMsisdn);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = MessageCollection.SIMReplNoDataFound,
                    });
                }

                TosNidToNidMSISDNCheckResponse msisdnValidationResp = _dbssToRaParse.TosNidToNidMSISDNReqParsingV1(dbssResp);

                if (msisdnValidationResp.result == false)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = FixedValueCollection.MSISDNError + msisdnValidationResp.message
                    });
                }
                else
                {
                    try
                    {
                        debtCheckApi = String.Format(GetAPICollection.DebtCheckApi, msisdnValidationResp.dbss_subscription_id);
                        JObject dbssRespForDebt = await GetDebtStatusForTOS(debtCheckApi, msisdnCheckReqest);
                        debtStatusResponse = _dbssToRaParse.TOSDebtStatusCheckParse(dbssRespForDebt);

                        if (debtStatusResponse.result == true)
                        {
                            msisdnValidationResp.result = debtStatusResponse.result;
                            msisdnValidationResp.message = debtStatusResponse.message;
                            return Ok(msisdnValidationResp);
                        }
                        else
                        {
                            loanCheckApiUrl = String.Format(GetAPICollection.GetLoanStatusForTOS, msisdnValidationResp.dbss_subscription_id);
                            JObject dbssRespForLoan = await GetLoanStatusForTOS(loanCheckApiUrl, msisdnCheckReqest);
                            loanStatusResponse = _dbssToRaParse.TosNiDtoNIDLoanStatusCheckParsing(dbssRespForLoan);

                            msisdnValidationResp.result = loanStatusResponse.result;
                            msisdnValidationResp.message = loanStatusResponse.message;
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                msisdnValidationResp.dob = "**/**/****";
                msisdnValidationResp.doc_id_number = "**********";

                return Ok(msisdnValidationResp);
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

                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = true,
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
                log.method_name = "ValidateMSISDNForTosNidToNidV2";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        /// <summary>
        /// This API is used for source customer's MSISDN validation for TOS NID to NID
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[ResponseType(typeof(TosNidToNidMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForTosNidToNidV3")]
        public async Task<IActionResult> ValidateMSISDNForTosNidToNidV3([FromBody] TosNidToNidMsisdnCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = string.Empty, txtResp = string.Empty;
            string loanCheckApiUrl = string.Empty;
            string debtCheckApi = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            TOSLoanStatusResponse loanStatusResponse = new TOSLoanStatusResponse();
            TOSDebtStatusResponse debtStatusResponse = new TOSDebtStatusResponse();
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

                string srcMsisdn = msisdnCheckReqest.mobile_number;

                if (srcMsisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    srcMsisdn = FixedValueCollection.MSISDNCountryCode + srcMsisdn;
                }

                apiUrl = String.Format(GetAPICollection.GetSubscriptionByMSISDNIncludingSimCardsPayerCustomerOwnerCustomerUserCustomer, srcMsisdn);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = MessageCollection.SIMReplNoDataFound,
                    });
                }

                TosNidToNidMSISDNCheckResponse msisdnValidationResp = _dbssToRaParse.TosNidToNidMSISDNReqParsingV1(dbssResp);
                msisdnValidationResp.dob = "**/**/****";
                msisdnValidationResp.doc_id_number = "**********";
                if (msisdnValidationResp.result == false)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = FixedValueCollection.MSISDNError + msisdnValidationResp.message
                    });
                }
                else
                {
                    try
                    {
                        debtCheckApi = String.Format(GetAPICollection.DebtCheckApi, msisdnValidationResp.dbss_subscription_id);
                        JObject dbssRespForDebt = await GetDebtStatusForTOS(debtCheckApi, msisdnCheckReqest);
                        debtStatusResponse = _dbssToRaParse.TOSDebtStatusCheckParse(dbssRespForDebt);

                        if (debtStatusResponse.result == true)
                        {
                            msisdnValidationResp.message = debtStatusResponse.message;
                            return Ok(new TosNidToNidMSISDNCheckResponseRevamp()
                            {
                                isError = true,
                                message = msisdnValidationResp.message,
                                data = msisdnValidationResp
                            });
                        }
                        else
                        {
                            loanCheckApiUrl = String.Format(GetAPICollection.GetLoanStatusForTOS, msisdnValidationResp.dbss_subscription_id);
                            JObject dbssRespForLoan = await GetLoanStatusForTOS(loanCheckApiUrl, msisdnCheckReqest);
                            loanStatusResponse = _dbssToRaParse.TosNiDtoNIDLoanStatusCheckParsing(dbssRespForLoan);

                            if (loanStatusResponse.result == false)
                            {
                                return Ok(new TosNidToNidMSISDNCheckResponseRevamp()
                                {
                                    isError = true,
                                    message = loanStatusResponse.message,
                                    data = msisdnValidationResp
                                });
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                return Ok(new TosNidToNidMSISDNCheckResponseRevamp()
                {
                    isError = false,
                    message = msisdnValidationResp.message,
                    data = msisdnValidationResp
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

                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
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
                log.method_name = "ValidateMSISDNForTosNidToNidV2";
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        #endregion

        public async Task<NidDobInfoResponse> GetNidDobForTOS(TosNidToNidMsisdnCheckRequest msisdnCheckReqest)
        {
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            string? apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                string srcMsisdn = msisdnCheckReqest.mobile_number;

                if (srcMsisdn.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    srcMsisdn = FixedValueCollection.MSISDNCountryCode + srcMsisdn;
                }

                apiUrl = String.Format(GetAPICollection.GetSubscriptionByMSISDNIncludingSimCardsPayerCustomerOwnerCustomerUserCustomer, srcMsisdn);

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

                TosNidToNidMSISDNCheckResponse msisdnValidationResp = _dbssToRaParse.TosNidToNidMSISDNReqParsingV1(dbssResp);

                if (msisdnValidationResp.result == false)
                {
                    nidDobInfo.result = false;
                    nidDobInfo.message = FixedValueCollection.MSISDNError + msisdnValidationResp.message;
                    return nidDobInfo;
                }

                nidDobInfo.result = true;
                nidDobInfo.src_nid = msisdnValidationResp.doc_id_number;
                nidDobInfo.src_dob = msisdnValidationResp.dob;
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
                log.method_name = "GetNidDobForTOS";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        public async Task<NidDobInfoResponse> GetNidDobForCorporateTOS(CorporateMSISDNCheckRequest msisdnCheckReqest)
        {
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            string? apiUrl = string.Empty, txtResp = string.Empty;
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
                    nidDobInfo.result = false;
                    nidDobInfo.message = MessageCollection.SIMReplNoDataFound;
                    return nidDobInfo;
                }

                log.is_success = 1;

                CorporateSIMReplacementCheckResponseWithCustomerId msisdnResp = _dbssToRaParse.CorporateSIMReplacementMSISDNReqParsing2(dbssResp);

                if (msisdnResp.result == false)
                {
                    nidDobInfo.result = false;
                    nidDobInfo.message = msisdnResp.message;
                    return nidDobInfo;
                }

                SIMReplacementMSISDNCheckResponse customerResp = await _bio.GetCoordicatorCustomerInfo(msisdnResp.customer_id, msisdnCheckReqest.poc_msisdn_number, msisdnCheckReqest.purpose_number, msisdnCheckReqest.retailer_id);

                if (customerResp.result == false)
                {
                    nidDobInfo.result = false;
                    nidDobInfo.message = customerResp.message;
                    return nidDobInfo;
                }

                nidDobInfo.result = true;
                nidDobInfo.src_nid = customerResp.doc_id_number;
                nidDobInfo.src_dob = customerResp.dob;
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
                log.method_name = "GetNidDobForCorporateTOS";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }


        #region Corporate TOS MSISDN validation by POC
        /// <summary>
        /// This API is used for MSISDN validation 
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateTOSByPOCV1")]
        public async Task<IActionResult> ValidateMSISDNForCorporateTOSByPOCV1([FromBody] CorporateMSISDNCheckRequest msisdnCheckReqest)
        {
            SIMReplacementMSISDNCheckResponse response = new SIMReplacementMSISDNCheckResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                response = await _bio.ValidateCorporateMSISDN(msisdnCheckReqest, "ValidateMSISDNForCorporateTOSByPOCV1");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new SIMReplacementMSISDNCheckResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }
        /// <summary>
        /// This API is used for MSISDN validation 
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateTOSByPOCV2")]
        public async Task<IActionResult> ValidateMSISDNForCorporateTOSByPOCV2([FromBody] CorporateMSISDNCheckRequest msisdnCheckReqest)
        {
            SIMReplacementMSISDNCheckResponse response = new SIMReplacementMSISDNCheckResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                response = await _bio.ValidateCorporateMSISDN(msisdnCheckReqest, "ValidateMSISDNForCorporateTOSByPOCV2");

                if (response.result == false)
                {
                    return Ok(new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = response.message
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new SIMReplacementMSISDNCheckResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// This API is used for MSISDN validation 
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateTOSByPOCV3")]
        public async Task<IActionResult> ValidateMSISDNForCorporateTOSByPOCV3([FromBody] CorporateMSISDNCheckRequest msisdnCheckReqest)
        {
            SIMReplacementMSISDNCheckResponseDataRev response = new SIMReplacementMSISDNCheckResponseDataRev();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                response = await _bio.ValidateCorporateMSISDNV3(msisdnCheckReqest, "ValidateMSISDNForCorporateTOSByPOCV2");

                if (response.isError == true)
                {
                    return Ok(new SIMReplacementMSISDNCheckResponseDataRev()
                    {
                        isError = true,
                        message = response.message
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new SIMReplacementMSISDNCheckResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }

        #endregion


        #region TOS NID to NID submit Order API
        /// Send Order
        /// <summary>
        /// This API is used for SimReplacement submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("TosNidToNidSubmitOrder")]
        public async Task<IActionResult> TosNidToNidSubmitOrderV1([FromBody] RAOrderRequest model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            TosNidToNidMsisdnCheckRequest tosNidToNid = new TosNidToNidMsisdnCheckRequest();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new WebException(MessageCollection.InvalidSecurityToken);
                //Order Model Validation New

                #region Get NID DOB
                tosNidToNid.mobile_number = model.msisdn;
                tosNidToNid.purpose_number = model.purpose_number;
                tosNidToNid.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForTOS(tosNidToNid);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_nid = nidDobInfo.src_nid;
                    model.src_dob = nidDobInfo.src_dob;
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
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = validateResponse.message
                    });
                }
                //=== Ordedr model validation ===
                BLLRAReqModelValidation bLLRAReqModelValidation = new BLLRAReqModelValidation();
                bLLRAReqModelValidation.ValidateOrderReq(model);

                #region Check if submitted order is already in process or not.
                var orderValidationResult = await _orderManager.ValidateOrder(new VMValidateOrder
                {
                    msisdn = model.msisdn,
                    purpose_number = Convert.ToInt16(model.purpose_number),
                    is_corporate = 0,
                    retailer_id = model.retailer_id,
                    dest_dob = model.dest_dob
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
                #endregion
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


                        var imsiResp = await _bio.GetImsiBySimAsync(new GetImsiReq
                        {
                            purpose_number = model.purpose_number,
                            retailer_id = model.retailer_id,
                            sim = model.old_sim_number,
                            msisdn = model.msisdn
                        });

                        if (imsiResp.result == false)
                        {
                            model.status = (int)EnumRAOrderStatus.Failed;
                            orderRes.request_id = "0";
                            orderRes.is_success = false;
                            orderRes.message = imsiResp.message;
                            model.err_msg = orderRes.message;
                            return Ok(orderRes);
                        }
                        else
                        {
                            model.dest_imsi = imsiResp.imsi;//[Note: here IMSI is being sent as SIM number as per business requirement]
                        }

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
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.is_success = false;
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
                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.request_id;
                log.method_name = "TosNidToNidSubmitOrderV1";
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
        [Route("TosNidToNidSubmitOrderV2")]
        public async Task<IActionResult> TosNidToNidSubmitOrderV2([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            TosNidToNidMsisdnCheckRequest tosNidToNid = new TosNidToNidMsisdnCheckRequest();
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

                #region Get NID DOB
                tosNidToNid.mobile_number = model.msisdn;
                tosNidToNid.purpose_number = model.purpose_number;
                tosNidToNid.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForTOS(tosNidToNid);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_nid = nidDobInfo.src_nid;
                    model.src_dob = nidDobInfo.src_dob;
                }
                #endregion

                //Order Model Validation New
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
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = validateResponse.message
                    });
                }
                //=== Ordedr model validation ===
                BLLRAReqModelValidation bLLRAReqModelValidation = new BLLRAReqModelValidation();
                bLLRAReqModelValidation.ValidateOrderReqV2(model);

                #region Check if submitted order is already in process or not.
                var orderValidationResult = await _orderManager.ValidateOrder(new VMValidateOrder
                {
                    msisdn = model.msisdn,
                    purpose_number = Convert.ToInt16(model.purpose_number),
                    is_corporate = 0,
                    retailer_id = model.retailer_id,
                    dest_dob = model.dest_dob
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
                #endregion
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

                        #region Get IMSI
                        var imsiResp = await _bio.GetImsiBySimAsync(new GetImsiReq
                        {
                            purpose_number = model.purpose_number,
                            retailer_id = model.retailer_id,
                            sim = model.old_sim_number,
                            msisdn = model.msisdn
                        });

                        if (imsiResp.result == false)
                        {
                            model.status = (int)EnumRAOrderStatus.Failed;
                            orderRes.request_id = "0";
                            orderRes.is_success = false;
                            orderRes.message = imsiResp.message;
                            model.err_msg = orderRes.message;
                            return Ok(orderRes);
                        }
                        else
                        {
                            model.dest_imsi = imsiResp.imsi;//[Note: here IMSI is being sent as SIM number as per business requirement]
                        }
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
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.is_success = false;
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
                        user_name = model.retailer_id,
                        msidn = model.msisdn,
                        dest_imsi = model.dest_imsi,
                        status = model.status,
                        bss_reqId = model.bss_reqId,
                        error_id = model.error_id,
                        err_msg = model.err_msg,
                    });
                }
                log.res_time = DateTime.Now;
                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.request_id;
                log.method_name = "TosNidToNidSubmitOrderV2";
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
        [Route("TosNidToNidSubmitOrderV3")]
        public async Task<IActionResult> TosNidToNidSubmitOrderV3([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            TosNidToNidMsisdnCheckRequest tosNidToNid = new TosNidToNidMsisdnCheckRequest();
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

                #region Get NID DOB
                tosNidToNid.mobile_number = model.msisdn;
                tosNidToNid.purpose_number = model.purpose_number;
                tosNidToNid.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForTOS(tosNidToNid);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_nid = nidDobInfo.src_nid;
                    model.src_dob = nidDobInfo.src_dob;
                }
                #endregion

                //Order Model Validation_New
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
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = validateResponse.message
                    });
                }
                //=== Ordedr model validation ===
                BLLRAReqModelValidation bLLRAReqModelValidation = new BLLRAReqModelValidation();
                bLLRAReqModelValidation.ValidateOrderReqV2(model);


                #region Check if submitted order is already in process or not.
                var orderValidationResult = await _orderManager.ValidateOrder(new VMValidateOrder
                {
                    msisdn = model.msisdn,
                    purpose_number = Convert.ToInt16(model.purpose_number),
                    is_corporate = 0,
                    retailer_id = model.retailer_id,
                    dest_dob = model.dest_dob
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
                #endregion
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

                        #region Get IMSI
                        var imsiResp = await _bio.GetImsiBySimAsync(new GetImsiReq
                        {
                            purpose_number = model.purpose_number,
                            retailer_id = model.retailer_id,
                            sim = model.old_sim_number,
                            msisdn = model.msisdn
                        });

                        if (imsiResp.result == false)
                        {
                            model.status = (int)EnumRAOrderStatus.Failed;
                            orderRes.request_id = "0";
                            orderRes.is_success = false;
                            orderRes.message = imsiResp.message;
                            model.err_msg = orderRes.message;
                            return Ok(orderRes);
                        }
                        else
                        {
                            model.dest_imsi = imsiResp.imsi;//[Note: here IMSI is being sent as SIM number as per business requirement]
                        }
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
                log.res_time = DateTime.Now;
                ErrorDescription error;
                log.is_success = 0;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    orderRes.request_id = null;
                    orderRes.is_success = false;
                    orderRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.is_success = false;
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
                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.request_id;
                log.method_name = "TosNidToNidSubmitOrderV3";
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
        [Route("TosNidToNidSubmitOrderV4")]
        public async Task<IActionResult> TosNidToNidSubmitOrderV4([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            TosNidToNidMsisdnCheckRequest tosNidToNid = new TosNidToNidMsisdnCheckRequest();
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

                #region Get NID DOB
                tosNidToNid.mobile_number = model.msisdn;
                tosNidToNid.purpose_number = model.purpose_number;
                tosNidToNid.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForTOS(tosNidToNid);

                if (nidDobInfo.result == false)
                {
                    orderRes.data = new DataRes()
                    {
                        request_id = "0",
                        isEsim = 0
                    };
                    orderRes.isError = true;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_nid = nidDobInfo.src_nid;
                    model.src_dob = nidDobInfo.src_dob;
                }
                #endregion

                //Order Model Validation_New
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
                //=== Ordedr model validation ===
                BLLRAReqModelValidation bLLRAReqModelValidation = new BLLRAReqModelValidation();
                bLLRAReqModelValidation.ValidateOrderReqV2(model);


                #region Check if submitted order is already in process or not.
                var orderValidationResult = await _orderManager.ValidateOrder(new VMValidateOrder
                {
                    msisdn = model.msisdn,
                    purpose_number = Convert.ToInt16(model.purpose_number),
                    is_corporate = 0,
                    retailer_id = model.retailer_id,
                    dest_dob = model.dest_dob
                });

                if (orderValidationResult.result == false)
                {
                    orderRes.data = new DataRes()
                    {
                        request_id = "0",
                        isEsim = 0
                    };
                    orderRes.isError = true;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                #endregion
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;

                //if (model.bi_token_number == null || model.bi_token_number == 0)
                //{
                orderRes = await _orderManager.SubmitOrderV7(model, loginProviderId);
                if (orderRes.isError)
                {
                    return Ok(new SendOrderResponseRev()
                    {
                        isError = true,
                        message = orderRes.message
                    });
                }
                model.bi_token_number = orderRes.data != null ? Convert.ToDouble(orderRes.data.request_id) : 0;
                // }

                //if (model.bi_token_number != null && model.bi_token_number !=0)
                //{
                try
                {
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

                    #region Get IMSI
                    var imsiResp = await _bio.GetImsiBySimAsync(new GetImsiReq
                    {
                        purpose_number = model.purpose_number,
                        retailer_id = model.retailer_id,
                        sim = model.old_sim_number,
                        msisdn = model.msisdn
                    });

                    if (imsiResp.result == false)
                    {
                        model.status = (int)EnumRAOrderStatus.Failed;
                        orderRes.data = new DataRes()
                        {
                            request_id = "0",
                            isEsim = 0
                        };
                        orderRes.isError = true;
                        orderRes.message = imsiResp.message;
                        model.err_msg = orderRes.message;
                        return Ok(orderRes);
                    }
                    else
                    {
                        model.dest_imsi = imsiResp.imsi;//[Note: here IMSI is being sent as SIM number as per business requirement]
                    }
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

                    orderRes.data = new DataRes()
                    {
                        request_id = "",
                        isEsim = 0
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
                log.method_name = "TosNidToNidSubmitOrderV4";
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

        #region Corporate TOS submit Order API
        /// Send Order
        /// <summary>
        /// This API is used for SimReplacement submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("CorporateTOSSubmitOrderV1")]
        public async Task<IActionResult> CorporateTOSSubmitOrderV1([FromBody] RAOrderRequest model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            CorporateMSISDNCheckRequest msisdnCheckReqest = new CorporateMSISDNCheckRequest();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new WebException(MessageCollection.InvalidSecurityToken);

                #region Get NID DOB for TOS
                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.purpose_number = model.purpose_number;
                msisdnCheckReqest.poc_msisdn_number = model.poc_msisdn_number;
                msisdnCheckReqest.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForCorporateTOS(msisdnCheckReqest);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = nidDobInfo.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_dob = nidDobInfo.src_dob;
                    model.src_nid = nidDobInfo.src_nid;
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
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = validateResponse.message
                    });
                }
                #region Check if submitted order is already in process or not.
                var orderValidationResult = await _orderManager.ValidateOrder(new VMValidateOrder
                {
                    msisdn = model.msisdn,
                    sim_number = model.sim_number,
                    purpose_number = Convert.ToInt32(model.purpose_number),
                    is_corporate = 1,
                    retailer_id = model.retailer_id,
                    dest_dob = DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat)
                });
                if (orderValidationResult.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderValidationResult.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                #endregion
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

                        var pardedData = _orderManager.SubmitOrderDataPurse(model);
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
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.is_success = false;
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
                log.req_blob = _blJson.GetGenericJsonData(Convert.ToString(_bio.SubmitOrderRequestBindingForLog(model)));

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
                log.bi_token_number = model.bi_token_number.ToString();
                log.method_name = "CorporateTOSSubmitOrderV1";
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
        [Route("CorporateTOSSubmitOrderV2")]
        public async Task<IActionResult> CorporateTOSSubmitOrderV2([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            CorporateMSISDNCheckRequest msisdnCheckReqest = new CorporateMSISDNCheckRequest();
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

                #region Get NID DOB for TOS
                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.purpose_number = model.purpose_number;
                msisdnCheckReqest.poc_msisdn_number = model.poc_msisdn_number;
                msisdnCheckReqest.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForCorporateTOS(msisdnCheckReqest);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = nidDobInfo.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_dob = nidDobInfo.src_dob;
                    model.src_nid = nidDobInfo.src_nid;
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
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = validateResponse.message
                    });
                }
                //===== Check if submitted order is already in process or not.=====
                #region Check if submitted order is already in process or not.
                var orderValidationResult = await _orderManager.ValidateOrder(new VMValidateOrder
                {
                    msisdn = model.msisdn,
                    sim_number = model.sim_number,
                    purpose_number = Convert.ToInt32(model.purpose_number),
                    is_corporate = 1,
                    retailer_id = model.retailer_id,
                    dest_dob = DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat)
                });
                if (orderValidationResult.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderValidationResult.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                #endregion
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

                        var pardedData = await _orderManager.SubmitOrderDataPurseV2(model);
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
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.is_success = false;
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
                log.req_blob = _blJson.GetGenericJsonData(Convert.ToString(_bio.SubmitOrderRequestBindingForLogV2(model)));

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
                log.bi_token_number = model.bi_token_number.ToString();
                log.method_name = "CorporateTOSSubmitOrderV2";
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
        [Route("CorporateTOSSubmitOrderV3")]
        public async Task<IActionResult> CorporateTOSSubmitOrderV3([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            CorporateMSISDNCheckRequest msisdnCheckReqest = new CorporateMSISDNCheckRequest();
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

                #region Get NID DOB for TOS
                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.purpose_number = model.purpose_number;
                msisdnCheckReqest.poc_msisdn_number = model.poc_msisdn_number;
                msisdnCheckReqest.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForCorporateTOS(msisdnCheckReqest);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = nidDobInfo.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_dob = nidDobInfo.src_dob;
                    model.src_nid = nidDobInfo.src_nid;
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
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = validateResponse.message
                    });
                }
                //===== Check if submitted order is already in process or not.=====
                #region Check if submitted order is already in process or not.
                var orderValidationResult = await _orderManager.ValidateOrder(new VMValidateOrder
                {
                    msisdn = model.msisdn,
                    sim_number = model.sim_number,
                    purpose_number = Convert.ToInt32(model.purpose_number),
                    is_corporate = 1,
                    retailer_id = model.retailer_id,
                    dest_dob = DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat)
                });
                if (orderValidationResult.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderValidationResult.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                #endregion
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

                        var pardedData = await _orderManager.SubmitOrderDataPurseV2(model);
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
                    model.err_msg = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
                catch (Exception)
                {
                    orderRes.is_success = false;
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
                log.req_blob = _blJson.GetGenericJsonData(Convert.ToString(_bio.SubmitOrderRequestBindingForLogV2(model)));

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
                log.bi_token_number = model.bi_token_number.ToString();
                log.method_name = "CorporateTOSSubmitOrderV3";
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
        [Route("CorporateTOSSubmitOrderV4")]
        public async Task<IActionResult> CorporateTOSSubmitOrderV4([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            CorporateMSISDNCheckRequest msisdnCheckReqest = new CorporateMSISDNCheckRequest();
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

                #region Get NID DOB for TOS
                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.purpose_number = model.purpose_number;
                msisdnCheckReqest.poc_msisdn_number = model.poc_msisdn_number;
                msisdnCheckReqest.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForCorporateTOS(msisdnCheckReqest);

                if (nidDobInfo.result == false)
                {
                    orderRes.data = new DataRes()
                    {
                        request_id = "",
                        isEsim = 0
                    };
                    orderRes.isError = true;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = nidDobInfo.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_dob = nidDobInfo.src_dob;
                    model.src_nid = nidDobInfo.src_nid;
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
                //===== Check if submitted order is already in process or not.=====
                #region Check if submitted order is already in process or not.
                var orderValidationResult = await _orderManager.ValidateOrder(new VMValidateOrder
                {
                    msisdn = model.msisdn,
                    sim_number = model.sim_number,
                    purpose_number = Convert.ToInt32(model.purpose_number),
                    is_corporate = 1,
                    retailer_id = model.retailer_id,
                    dest_dob = DateTime.Parse(model.dest_dob).ToString(StringFormatCollection.DBSSDOBFormat)
                });
                if (orderValidationResult.result == false)
                {
                    orderRes.data = new DataRes()
                    {
                        request_id = "",
                        isEsim = 0
                    };
                    orderRes.isError = true;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderValidationResult.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                #endregion
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;

                if (model.bi_token_number == null || model.bi_token_number == 0)
                {
                    orderRes = await _orderManager.SubmitOrderV7(model, loginProviderId);
                    if (orderRes.isError)
                    {
                        return Ok(new SendOrderResponseRev()
                        {
                            isError = true,
                            message = orderRes.message
                        });
                    }
                    model.bi_token_number = orderRes.data != null ? Convert.ToDouble(orderRes.data.request_id) : 0;
                }

                if (model.bi_token_number != null && model.bi_token_number != 0)
                {
                    try
                    {

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
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                }
            }
            finally
            {
                log.purpose_number = model.purpose_number;
                log.msisdn = _bllLog.FormatMSISDN(model.msisdn);
                log.req_time = DateTime.Now;
                log.req_blob = _blJson.GetGenericJsonData(Convert.ToString(_bio.SubmitOrderRequestBindingForLogV2(model)));

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
                    log.bi_token_number = model.bi_token_number.ToString();
                }

                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.method_name = "CorporateTOSSubmitOrderV3";
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

        #region Special TOS submit Order API
        /// Send Order
        /// <summary>
        /// This API is used for Special TOS submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("SpecialTOSSubmitOrder")]
        public async Task<IActionResult> SpecialTOSSubmitOrderV1([FromBody] RAOrderRequest model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            TosNidToNidMsisdnCheckRequest tosNidToNid = new TosNidToNidMsisdnCheckRequest();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new WebException(MessageCollection.InvalidSecurityToken);

                #region Get NID DOB
                tosNidToNid.mobile_number = model.msisdn;
                tosNidToNid.purpose_number = model.purpose_number;
                tosNidToNid.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForTOS(tosNidToNid);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_nid = nidDobInfo.src_nid;
                    model.src_dob = nidDobInfo.src_dob;
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
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = validateResponse.message
                    });
                }

                //==== Check if submitted order is already in process or not.=====
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
                    model.err_msg = orderValidationResult.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
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
                        log.req_time = DateTime.Now;
                        log.req_blob = _blJson.GetGenericJsonData(Convert.ToString(_bio.SubmitOrderRequestBindingForLog(model)));

                        #region bio verification

                        var pardedData = _orderManager.SubmitOrderDataPurse(model);
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
                if (orderRes != null)
                {
                    log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                }
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = model.bi_token_number.ToString();
                log.method_name = "SpecialTOSSubmitOrderV1";
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
        /// This API is used for Special TOS submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("SpecialTOSSubmitOrderV2")]
        public async Task<IActionResult> SpecialTOSSubmitOrderV2([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            TosNidToNidMsisdnCheckRequest tosNidToNid = new TosNidToNidMsisdnCheckRequest();
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

                #region Get NID DOB
                tosNidToNid.mobile_number = model.msisdn;
                tosNidToNid.purpose_number = model.purpose_number;
                tosNidToNid.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForTOS(tosNidToNid);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_nid = nidDobInfo.src_nid;
                    model.src_dob = nidDobInfo.src_dob;
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
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = validateResponse.message
                    });
                }
                //==== Check if submitted order is already in process or not.=====
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
                    model.err_msg = orderValidationResult.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
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
                        log.req_time = DateTime.Now;
                        log.req_blob = _blJson.GetGenericJsonData(Convert.ToString(_bio.SubmitOrderRequestBindingForLogV2(model)));


                        #region bio verification

                        var pardedData = await _orderManager.SubmitOrderDataPurseV2(model);
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
                log.bi_token_number = model.bi_token_number.ToString();
                log.method_name = "SpecialTOSSubmitOrderV2";
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
        /// This API is used for Special TOS submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("SpecialTOSSubmitOrderV3")]
        public async Task<IActionResult> SpecialTOSSubmitOrderV3([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            TosNidToNidMsisdnCheckRequest tosNidToNid = new TosNidToNidMsisdnCheckRequest();
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

                #region Get NID DOB
                tosNidToNid.mobile_number = model.msisdn;
                tosNidToNid.purpose_number = model.purpose_number;
                tosNidToNid.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForTOS(tosNidToNid);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_nid = nidDobInfo.src_nid;
                    model.src_dob = nidDobInfo.src_dob;
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
                    return Ok(new SendOrderResponse()
                    {
                        is_success = false,
                        message = validateResponse.message
                    });
                }
                //==== Check if submitted order is already in process or not.=====
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
                    model.err_msg = orderValidationResult.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
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
                        log.req_time = DateTime.Now;
                        log.req_blob = _blJson.GetGenericJsonData(Convert.ToString(_bio.SubmitOrderRequestBindingForLogV2(model)));
                        #region bio verification

                        var pardedData = await _orderManager.SubmitOrderDataPurseV2(model);
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
                log.bi_token_number = model.bi_token_number.ToString();
                log.method_name = "SpecialTOSSubmitOrderV3";
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
        /// This API is used for Special TOS submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("SpecialTOSSubmitOrderV4")]
        public async Task<IActionResult> SpecialTOSSubmitOrderV4([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            ModelValidation modelValidation = new ModelValidation();
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            TosNidToNidMsisdnCheckRequest tosNidToNid = new TosNidToNidMsisdnCheckRequest();
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

                #region Get NID DOB
                tosNidToNid.mobile_number = model.msisdn;
                tosNidToNid.purpose_number = model.purpose_number;
                tosNidToNid.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForTOS(tosNidToNid);

                if (nidDobInfo.result == false)
                {
                    orderRes.data = new DataRes()
                    {
                        request_id = ""
                    };
                    orderRes.isError = true;
                    orderRes.message = nidDobInfo.message;
                    model.err_msg = orderRes.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    model.src_nid = nidDobInfo.src_nid;
                    model.src_dob = nidDobInfo.src_dob;
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
                //==== Check if submitted order is already in process or not.=====
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
                        request_id = ""
                    };
                    orderRes.isError = true;
                    orderRes.message = orderValidationResult.message;
                    model.err_msg = orderValidationResult.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                if (model.bi_token_number == null || model.bi_token_number == 0)
                {
                    orderRes = await _orderManager.SubmitOrderV7(model, loginProviderId);
                    if (orderRes.isError)
                    {
                        return Ok(new SendOrderResponseRev()
                        {
                            isError = true,
                            message = orderRes.message
                        });
                    }
                    model.bi_token_number = orderRes.data != null ? Convert.ToDouble(orderRes.data.request_id) : 0;
                }
                if (model.bi_token_number != null && model.bi_token_number != 0)
                {
                    try
                    {
                        #endregion
                        log.req_time = DateTime.Now;
                        log.req_blob = _blJson.GetGenericJsonData(Convert.ToString(_bio.SubmitOrderRequestBindingForLogV2(model)));



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
                    log.bi_token_number = model.bi_token_number.ToString();
                }
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.method_name = "SpecialTOSSubmitOrderV3";
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
        public async Task<JObject> GetLoanStatusForTOS(string apiUrl, TosNidToNidMsisdnCheckRequest msisdnCheckRequest)
        {
            string? txtResp = string.Empty;
            string loanCheckApiUrl = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            JObject dbssResp = null;
            try
            {

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                return dbssResp;
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

                    return dbssResp;
                }
                catch (Exception) { return dbssResp; }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckRequest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckRequest.purpose_number;
                log.user_id = msisdnCheckRequest.retailer_id;
                log.method_name = "GetLoanStatusForTOS";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        public async Task<JObject> GetDebtStatusForTOS(string apiUrl, TosNidToNidMsisdnCheckRequest msisdnCheckRequest)
        {
            string? txtResp = string.Empty;
            string loanCheckApiUrl = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            JObject dbssResp = null;
            try
            {

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                return dbssResp;
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

                    return dbssResp;
                }
                catch (Exception) { return dbssResp; }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckRequest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckRequest.purpose_number;
                log.user_id = msisdnCheckRequest.retailer_id;
                log.method_name = "GetDebtStatusForTOS";

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
            if (!String.IsNullOrEmpty(order.src_dob))
                resp.src_dob = order.src_dob;
            if (order.src_sim_category != null)
                resp.src_sim_category = (int)order.src_sim_category;

            resp.dest_left_thumb = order.dest_left_thumb;
            resp.dest_left_index = order.dest_left_index;
            resp.dest_right_thumb = order.dest_right_thumb;
            resp.dest_right_index = order.dest_right_index;
            resp.src_left_index = order.src_left_index;
            resp.src_left_thumb = order.src_left_thumb;
            resp.src_right_index = order.src_right_index;
            resp.src_right_thumb = order.src_right_thumb;

            if (order.src_doc_type_no != null)
                resp.src_doc_type_no = order.src_doc_type_no.ToString();
            if (order.src_ec_verifi_reqrd != null)
                resp.src_ec_verification_required = (int)order.src_ec_verifi_reqrd;
            if (!String.IsNullOrEmpty(order.src_nid))
                resp.src_doc_id = order.src_nid;

            return resp;

        }
    }
}
