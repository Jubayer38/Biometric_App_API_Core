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
using static BIA.Entity.RequestEntity.CherishMSISDNCheckRequest;

namespace BIA.Controllers
{
    [Route("api/SimReplacement")]
    [ApiController]
    public class SimReplacementController : ControllerBase
    {
        private readonly BLLRAToDBSSParse _raToDBssParse;
        private readonly BLLDBSSToRAParse _dbssToRaParse;
        private readonly ApiRequest _apiReq;
        private readonly BL_Json _blJson;
        private readonly BLLCommon _bllCommon;
        private readonly BaseController _bio;
        private readonly ApiManager _apiManager;
        private readonly BLLOrder _orderManager;
        private readonly GeoFencingValidation _geo;
        private readonly BLLSIMReplacement _simReplacementManager; 
        private readonly BLLLog _bllLog;
        private readonly IConfiguration _configuration;


        public SimReplacementController(BLLRAToDBSSParse raToDBssParse, BLLDBSSToRAParse dbssToRaParse, ApiRequest apiReq, BL_Json blJson, BLLCommon bllCommon, BaseController bio, ApiManager apiManager, BLLOrder orderManager, GeoFencingValidation geo, BLLSIMReplacement simReplacementManager, BLLLog bllLog, IConfiguration configuration)
        {
            _raToDBssParse = raToDBssParse;
            _dbssToRaParse = dbssToRaParse;
            _apiReq = apiReq;
            _blJson = blJson;
            _bllCommon = bllCommon;
            _bio = bio;
            _apiManager = apiManager;
            _orderManager = orderManager;
            _geo = geo;
            _simReplacementManager = simReplacementManager;
            _bllLog = bllLog;
            _configuration = configuration;
        }

        #region Get SIM Replacement Reasons
        /// Send Order
        /// <summary>
        /// Get SIM replacement reasons.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>message</returns>
        //[ResponseType(typeof(SIMReplacementReasonsResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("GetSIMReplacementReasonsV1")]
        public async Task<IActionResult> GetSIMReplacementReasonsV1([FromBody] RACommonRequest model)
        {
            List<SIMReplacementReasonModel> reasons = new List<SIMReplacementReasonModel>();
            SIMReplacementReasonsResponse reasonsResp = new SIMReplacementReasonsResponse();
            BIAToDBSSLog log = new BIAToDBSSLog();

            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                reasons = await _simReplacementManager.GetSIMReplacementReasons();
                if (reasons.Count > 0)
                {
                    reasonsResp.data = reasons;
                    reasonsResp.result = true;
                    reasonsResp.message = MessageCollection.Success;
                }
                else
                {
                    reasonsResp.data = null;
                    reasonsResp.result = false;
                    reasonsResp.message = MessageCollection.NoDataFound;
                }
            }
            catch (Exception ex)
            {
                ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;

                reasonsResp.data = null;
                reasonsResp.result = false;
                reasonsResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
            }
            return Ok(reasonsResp);
        }

        /// Send Order
        /// <summary>
        /// Get SIM replacement reasons.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>message</returns>
        //[ResponseType(typeof(SIMReplacementReasonsResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("GetSIMReplacementReasonsV2")]
        public async Task<IActionResult> GetSIMReplacementReasonsV2([FromBody] RACommonRequest model)
        {
            List<SIMReplacementReasonModel> reasons = new List<SIMReplacementReasonModel>();
            SIMReplacementReasonsResponse reasonsResp = new SIMReplacementReasonsResponse();
            BIAToDBSSLog log = new BIAToDBSSLog();

            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                reasons = await _simReplacementManager.GetSIMReplacementReasons();
                if (reasons.Count > 0)
                {
                    reasonsResp.data = reasons;
                    reasonsResp.result = true;
                    reasonsResp.message = MessageCollection.Success;
                }
                else
                {
                    reasonsResp.data = null;
                    reasonsResp.result = false;
                    reasonsResp.message = MessageCollection.NoDataFound;
                }
            }
            catch (Exception ex)
            {
                ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;

                reasonsResp.data = null;
                reasonsResp.result = false;
                reasonsResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
            }
            return Ok(reasonsResp);
        }


        /// Send Order
        /// <summary>
        /// Get SIM replacement reasons.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>message</returns>
        //[ResponseType(typeof(SIMReplacementReasonsResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("GetSIMReplacementReasonsV3")]
        public async Task<IActionResult> GetSIMReplacementReasonsV3([FromBody] RACommonRequest model)
        {
            List<SIMReplacementReasonModel> reasons = new List<SIMReplacementReasonModel>();
            SIMReplacementReasonsResponseRevamp reasonsResp = new SIMReplacementReasonsResponseRevamp();
            BIAToDBSSLog log = new BIAToDBSSLog();
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();

            try
            {
                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(model.session_token);

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

                reasons = await _simReplacementManager.GetSIMReplacementReasons();
                if (reasons.Count > 0)
                {
                    reasonsResp.data = reasons;
                    reasonsResp.isError = false;
                    reasonsResp.message = MessageCollection.Success;
                }
                else
                {
                    reasonsResp.data = null;
                    reasonsResp.isError = true;
                    reasonsResp.message = MessageCollection.NoDataFound;
                }
            }
            catch (Exception ex)
            {
                ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;

                reasonsResp.data = null;
                reasonsResp.isError = true;
                reasonsResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
            }
            return Ok(reasonsResp);
        }
        #endregion

        public async Task<NidDobInfoResponse> GetNidDob(IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            BIAToDBSSLog log = new BIAToDBSSLog();
            string apiUrl = string.Empty;
            string? txtResp = string.Empty;
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

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        public async Task<NidDobInfoResponse> GetNidDobForCorporate(CorporateMSISDNCheckRequest msisdnCheckReqest)
        {
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            BIAToDBSSLog log = new BIAToDBSSLog();
            string apiUrl = string.Empty;
            string? txtResp = string.Empty;
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
                nidDobInfo.dest_nid = customerResp.doc_id_number;
                nidDobInfo.dest_dob = customerResp.dob;
                nidDobInfo.result = true;
                nidDobInfo.message = "";
                log.res_blob = _blJson.GetGenericJsonData(nidDobInfo);
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

        #region Individual SIM Replacement MSISDN validation  
        /// <summary>
        /// This API is used for MSISDN validation for paired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(IndividualSIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForIndividualSIMReplacementV1")]
        public async Task<IActionResult> ValidateMSISDNForIndividualSIMReplacementV1([FromBody] IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

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
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = MessageCollection.SIMReplNoDataFound,
                    });
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.IndividualSIMReplacementMSISDNReqParsingV3(dbssResp);

                if (msisdnResp.result == false)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = FixedValueCollection.MSISDNError + msisdnResp.message
                    });
                }

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
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.SIMReplacement, null, null, msisdnResp.old_sim_type);

                if (simResp.result == false)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = simResp.message
                    });
                }

                var resp = new IndividualSIMReplacementMSISDNCheckResponse()
                {
                    dbss_subscription_id = msisdnResp.dbss_subscription_id,
                    old_sim_number = msisdnResp.old_sim_number,
                    doc_id_number = "**********",
                    dob = "**/**/****",
                    result = true,
                    message = MessageCollection.MSISDNandSIMBothValid,
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

                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = true,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
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
                log.method_name = "ValidateMSISDNForIndividualSIMReplacementV1";

                //Thread logThread = new Thread(() => bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        /// <summary>
        /// This API is used for MSISDN validation for paired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(IndividualSIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForIndividualSIMReplacementV2")]
        public async Task<IActionResult> ValidateMSISDNForIndividualSIMReplacementV2([FromBody] IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

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
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = MessageCollection.SIMReplNoDataFound,
                    });
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.IndividualSIMReplacementMSISDNReqParsingV3(dbssResp);

                if (msisdnResp.result == false)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = FixedValueCollection.MSISDNError + msisdnResp.message
                    });
                }

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
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.SIMReplacement, null, null, msisdnResp.old_sim_type);

                if (simResp.result == false)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = simResp.message
                    });
                }

                var resp = new IndividualSIMReplacementMSISDNCheckResponse()
                {
                    dbss_subscription_id = msisdnResp.dbss_subscription_id,
                    old_sim_number = msisdnResp.old_sim_number,
                    doc_id_number = "**********",
                    dob = "**/**/****",
                    result = true,
                    message = MessageCollection.MSISDNandSIMBothValid,
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

                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = true,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
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
                log.method_name = "ValidateMSISDNForIndividualSIMReplacementV2";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        /// <summary>
        /// This API is used for MSISDN validation for paired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(IndividualSIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForIndividualSIMReplacementV3")]
        public async Task<IActionResult> ValidateMSISDNForIndividualSIMReplacementV3([FromBody] IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            IndividualSIMReplacementMSISDNCheckResponseRevamp response = new IndividualSIMReplacementMSISDNCheckResponseRevamp();

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

                apiUrl = String.Format(GetAPICollection.GetSubscriptionByMSISDNIncludingOwnerCustomerUserCustomerSimCardInfo, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = new JObject();
                try
                {
                    dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                }
                catch (Exception ex)
                {
                    if(ex.Message.Contains("Not Found"))
                    {
                        throw new Exception("Invalid MSISDN input for SIM Replacement.");
                    }
                    else
                    {
                        throw new Exception(ex.Message.ToString());
                    }                        
                }

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = MessageCollection.SIMReplNoDataFound,
                    });
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.IndividualSIMReplacementMSISDNReqParsingV3(dbssResp);

                if (msisdnResp.result == false)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = FixedValueCollection.MSISDNError + msisdnResp.message
                    });
                }

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
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.SIMReplacement, null, null, msisdnResp.old_sim_type);

                if (simResp.result == false)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = simResp.message
                    });
                }

                var resp = new IndividualSIMReplacementMSISDNCheckResponse()
                {
                    dbss_subscription_id = msisdnResp.dbss_subscription_id,
                    old_sim_number = msisdnResp.old_sim_number,
                    doc_id_number = "**********",
                    dob = "**/**/****",
                    result = true,
                    message = MessageCollection.MSISDNandSIMBothValid,
                    saf_status = msisdnResp.saf_status,
                    customer_id = msisdnResp.customer_id
                };
                return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
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

                    return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
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
                log.method_name = "ValidateMSISDNForIndividualSIMReplacementV3";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        /// <summary>
        /// This API is used for MSISDN validation for paired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(IndividualSIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForIndividualReplacement_ESIM")]
        public async Task<IActionResult> ValidateMSISDNForIndividualESIMReplacement([FromBody] IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

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
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = MessageCollection.SIMReplNoDataFound,
                    });
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.IndividualSIMReplacementMSISDNReqParsingV3(dbssResp);

                if (msisdnResp.result == false)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = FixedValueCollection.MSISDNError + msisdnResp.message
                    });
                }

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
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.SIMReplacement, null, null, msisdnResp.old_sim_type);

                if (simResp.result == false)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = simResp.message
                    });
                }

                var resp = new IndividualSIMReplacementMSISDNCheckResponse()
                {
                    dbss_subscription_id = msisdnResp.dbss_subscription_id,
                    old_sim_number = msisdnResp.old_sim_number,
                    doc_id_number = "**********",
                    dob = "**/**/****",
                    result = true,
                    message = MessageCollection.MSISDNandSIMBothValid,
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

                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
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
                log.method_name = "ValidateMSISDNForIndividualReplacement_ESIM";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }


        /// <summary>
        /// This API is used for MSISDN validation for paired
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(IndividualSIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForIndividualReplacement_ESIMV2")]
        public async Task<IActionResult> ValidateMSISDNForIndividualESIMReplacementV2([FromBody] IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
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

                apiUrl = String.Format(GetAPICollection.GetSubscriptionByMSISDNIncludingOwnerCustomerUserCustomerSimCardInfo, msisdnCheckReqest.mobile_number);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;

                JObject dbssResp = new JObject();
                try
                {
                    dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Not Found"))
                    {
                        throw new Exception("Invalid MSISDN input for E-SIM Replacement.");
                    }
                    else
                    {
                        throw new Exception(ex.Message.ToString());
                    }
                }

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp["data"] == null || dbssResp["included"] == null)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = MessageCollection.SIMReplNoDataFound,
                    });
                }

                log.is_success = 1;

                var msisdnResp = _dbssToRaParse.IndividualSIMReplacementMSISDNReqParsingV3(dbssResp);

                if (msisdnResp.result == false)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = FixedValueCollection.MSISDNError + msisdnResp.message
                    });
                }

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
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.SIMReplacement, null, null, msisdnResp.old_sim_type);

                if (simResp.result == false)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = simResp.message
                    });
                }

                var resp = new IndividualSIMReplacementMSISDNCheckResponse()
                {
                    dbss_subscription_id = msisdnResp.dbss_subscription_id,
                    old_sim_number = msisdnResp.old_sim_number,
                    doc_id_number = "**********",
                    dob = "**/**/****",
                    result = true,
                    message = MessageCollection.MSISDNandSIMBothValid,
                    saf_status = msisdnResp.saf_status,
                    customer_id = msisdnResp.customer_id
                };
                return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
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

                    return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });                   
                }
                catch (Exception)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
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
                log.method_name = "ValidateMSISDNForIndividualReplacement_ESIM";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        #endregion

        #region Corporate SIM Replacement MSISDN validation by POC
        /// <summary>
        /// This API is used for MSISDN validation for B2B SIM replacement by POC.  
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(SIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateSIMReplacementByPOCV1")]
        public async Task<IActionResult> ValidateMSISDNForCorporateSIMReplacementByPOCV1([FromBody] CorporateMSISDNCheckRequest msisdnCheckReqest)
        {
            SIMReplacementMSISDNCheckResponse sIMReplacementMSISDNCheckResponse = new SIMReplacementMSISDNCheckResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                sIMReplacementMSISDNCheckResponse = await _bio.ValidateCorporateMSISDN(msisdnCheckReqest, "ValidateMSISDNForCorporateSIMReplacementByPOCV1");

                return Ok(sIMReplacementMSISDNCheckResponse);
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
        /// This API is used for MSISDN validation for B2B SIM replacement by POC.  
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(SIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateSIMReplacementByPOCV2")]
        public async Task<IActionResult> ValidateMSISDNForCorporateSIMReplacementByPOCV2([FromBody] CorporateMSISDNCheckRequest msisdnCheckReqest)
        {
            SIMReplacementMSISDNCheckResponse checkResponse = new SIMReplacementMSISDNCheckResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                checkResponse = await _bio.ValidateCorporateMSISDNV1(msisdnCheckReqest, "ValidateMSISDNForCorporateSIMReplacementByPOCV1");

                return Ok(checkResponse);
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
        /// This API is used for MSISDN validation for B2B SIM replacement by POC.  
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(SIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateSIMReplacementByPOCV3")]
        public async Task<IActionResult> ValidateMSISDNForCorporateSIMReplacementByPOCV3([FromBody] CorporateMSISDNCheckRequest msisdnCheckReqest)
        {
            SIMReplacementMSISDNCheckResponse checkResponse = new SIMReplacementMSISDNCheckResponse();
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

                checkResponse = await _bio.ValidateCorporateMSISDNV1(msisdnCheckReqest, "ValidateMSISDNForCorporateSIMReplacementByPOCV1");

                if(checkResponse.result == true)
                {
                    return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = checkResponse.result == true ? false : true,
                        message = checkResponse.message,
                        data = checkResponse
                    });
                }
                else
                {
                    return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = checkResponse.message,
                        data = checkResponse
                    });
                }                
            }
            catch (Exception ex)
            {
                return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                {
                    isError = true,
                    message = ex.Message
                });
            }
        }
        /// <summary>
        /// This API is used for MSISDN validation for B2B SIM replacement by POC.  
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //eSim(New Logic)
        //[ResponseType(typeof(SIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateReplacementByPOC_ESIM")]
        public async Task<IActionResult> ValidateMSISDNForCorporateE_SIMReplacementByPOC([FromBody] CorporateMSISDNCheckRequest msisdnCheckReqest)
        {
            SIMReplacementMSISDNCheckResponse checkResponse = new SIMReplacementMSISDNCheckResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                checkResponse = await _bio.ValidateCorporateMSISDNV2(msisdnCheckReqest, "ValidateMSISDNForCorporateReplacementByPOC_ESIM");

                return Ok(checkResponse);
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
        /// This API is used for MSISDN validation for B2B SIM replacement by POC.  
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //eSim(New Logic)
        //[ResponseType(typeof(SIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateReplacementByPOC_ESIMV2")]
        public async Task<IActionResult> ValidateMSISDNForCorporateE_SIMReplacementByPOCV2([FromBody] CorporateMSISDNCheckRequest msisdnCheckReqest)
        {
            SIMReplacementMSISDNCheckResponse checkResponse = new SIMReplacementMSISDNCheckResponse();
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

                checkResponse = await _bio.ValidateCorporateMSISDNV2(msisdnCheckReqest, "ValidateMSISDNForCorporateReplacementByPOC_ESIM");

                return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                {
                    isError = checkResponse.result == true ? false:true,
                    message = checkResponse.message,
                    data = checkResponse
                });
            }
            catch (Exception ex)
            {
                return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                {
                    isError = true,
                    message = ex.Message
                });
            }
        }

        #endregion

        #region Corporate SIM Replacement MSISDN validation BY Auth Person 
        /// <summary>
        /// This API is used for MSISDN validation B2B by auth person. 
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(SIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateSIMReplacementByAuthPersonV1")]
        public async Task<IActionResult> ValidateMSISDNForCorporateSIMReplacementByAuthPersonV1([FromBody] CorporateMSISDNCheckWithOTPRequest msisdnCheckReqest)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                #region OTP validation
                OTPResponse otpResp = await _bio.ValidateOTP(new DBSSOTPValidationRequest()
                {
                    otp = msisdnCheckReqest.otp,
                    poc_msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.poc_msisdn_number),
                    auth_msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number),
                    purpose = Convert.ToInt16(EnumPurposeForDBSSOTP.SIMReplByAuth)
                }, msisdnCheckReqest.retailer_id);

                if (otpResp.is_otp_valid == false)
                {
                    return Ok(new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = otpResp.message
                    });
                }
                #endregion

                return Ok(await _bio.ValidateCorporateMSISDN(new CorporateMSISDNCheckRequest
                {
                    poc_msisdn_number = msisdnCheckReqest.poc_msisdn_number,
                    mobile_number = msisdnCheckReqest.mobile_number,
                    sim_number = msisdnCheckReqest.sim_number,
                    channel_name = msisdnCheckReqest.channel_name,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    lan = msisdnCheckReqest.lan,
                    purpose_number = msisdnCheckReqest.purpose_number,
                    session_token = msisdnCheckReqest.session_token
                }, "ValidateMSISDNForCorporateSIMReplacementByAuthPersonV1"));
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
        /// This API is used for MSISDN validation B2B by auth person. 
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(SIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateSIMReplacementByAuthPersonV2")]
        public async Task<IActionResult> ValidateMSISDNForCorporateSIMReplacementByAuthPersonV2([FromBody] CorporateMSISDNCheckWithOTPRequest msisdnCheckReqest)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                #region OTP validation
                OTPResponse otpResp = await _bio.ValidateOTP(new DBSSOTPValidationRequest()
                {
                    otp = msisdnCheckReqest.otp,
                    poc_msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.poc_msisdn_number),
                    auth_msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number),
                    purpose = Convert.ToInt16(EnumPurposeForDBSSOTP.SIMReplByAuth)
                }, msisdnCheckReqest.retailer_id);

                if (otpResp.is_otp_valid == false)
                {
                    return Ok(new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = otpResp.message
                    });
                }
                #endregion

                return Ok(await _bio.ValidateCorporateMSISDNV1(new CorporateMSISDNCheckRequest
                {
                    poc_msisdn_number = msisdnCheckReqest.poc_msisdn_number,
                    mobile_number = msisdnCheckReqest.mobile_number,
                    sim_number = msisdnCheckReqest.sim_number,
                    channel_name = msisdnCheckReqest.channel_name,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    lan = msisdnCheckReqest.lan,
                    purpose_number = msisdnCheckReqest.purpose_number,
                    session_token = msisdnCheckReqest.session_token
                }, "ValidateMSISDNForCorporateSIMReplacementByAuthPersonV2"));
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
        /// This API is used for MSISDN validation B2B by auth person. 
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(SIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateSIMReplacementByAuthPersonV3")]
        public async Task<IActionResult> ValidateMSISDNForCorporateSIMReplacementByAuthPersonV3([FromBody] CorporateMSISDNCheckWithOTPRequest msisdnCheckReqest)
        {
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

                #region OTP validation
                OTPResponseRev otpResp = await _bio.ValidateOTPV2(new DBSSOTPValidationRequest()
                {
                    otp = msisdnCheckReqest.otp,
                    poc_msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.poc_msisdn_number),
                    auth_msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number),
                    purpose = Convert.ToInt16(EnumPurposeForDBSSOTP.SIMReplByAuth)
                }, msisdnCheckReqest.retailer_id);

                if (otpResp.data != null && otpResp.data.is_otp_valid == false)
                {
                    return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = otpResp.message
                    });
                }
                #endregion
                var response = await _bio.ValidateCorporateMSISDNV1(new CorporateMSISDNCheckRequest
                {
                    poc_msisdn_number = msisdnCheckReqest.poc_msisdn_number,
                    mobile_number = msisdnCheckReqest.mobile_number,
                    sim_number = msisdnCheckReqest.sim_number,
                    channel_name = msisdnCheckReqest.channel_name,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    lan = msisdnCheckReqest.lan,
                    purpose_number = msisdnCheckReqest.purpose_number,
                    session_token = msisdnCheckReqest.session_token
                }, "ValidateMSISDNForCorporateSIMReplacementByAuthPersonV3");

                if(response.result == true)
                {
                    return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = false,
                        message = response.message,
                        data = response
                    });
                }
                else
                {
                    return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = response.message,
                        data = response
                    });
                }                
            }
            catch (Exception ex)
            {
                return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                {
                    isError = true,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// This API is used for MSISDN validation B2B by auth person. 
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //eSim(Existing Logic)
        //[ResponseType(typeof(SIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateReplacementByAuthPerson_ESIM")]
        public async Task<IActionResult> ValidateMSISDNForCorporateE_SIMReplacementByAuthPerson([FromBody] CorporateMSISDNCheckWithOTPRequest msisdnCheckReqest)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                #region OTP validation
                OTPResponse otpResp = await _bio.ValidateOTP(new DBSSOTPValidationRequest()
                {
                    otp = msisdnCheckReqest.otp,
                    poc_msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.poc_msisdn_number),
                    auth_msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number),
                    purpose = Convert.ToInt16(EnumPurposeForDBSSOTP.SIMReplByAuth)
                }, msisdnCheckReqest.retailer_id);

                if (otpResp.is_otp_valid == false)
                {
                    return Ok(new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = otpResp.message
                    });
                }


                #endregion

                return Ok(await _bio.ValidateCorporateMSISDNV2(new CorporateMSISDNCheckRequest
                {
                    poc_msisdn_number = msisdnCheckReqest.poc_msisdn_number,
                    mobile_number = msisdnCheckReqest.mobile_number,
                    sim_number = msisdnCheckReqest.sim_number,
                    channel_name = msisdnCheckReqest.channel_name,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    lan = msisdnCheckReqest.lan,
                    purpose_number = msisdnCheckReqest.purpose_number,
                    session_token = msisdnCheckReqest.session_token
                }, "ValidateMSISDNForCorporateReplacementByAuthPerson_ESIM"));
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
        /// This API is used for MSISDN validation B2B by auth person. 
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //eSim(Existing Logic)
        //[ResponseType(typeof(SIMReplacementMSISDNCheckResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateMSISDNForCorporateReplacementByAuthPerson_ESIMV2")]
        public async Task<IActionResult> ValidateMSISDNForCorporateE_SIMReplacementByAuthPersonV2([FromBody] CorporateMSISDNCheckWithOTPRequest msisdnCheckReqest)
        {
            ValidTokenResponse security = new ValidTokenResponse();
            try
            {
                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                try
                {
                    secreteKey = SettingsValues.GetJWTSequrityKey();
                }
                catch
                { }

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

                #region OTP validation
                OTPResponseRev otpResp = await _bio.ValidateOTPV2(new DBSSOTPValidationRequest()
                {
                    otp = msisdnCheckReqest.otp,
                    poc_msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.poc_msisdn_number),
                    auth_msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number),
                    purpose = Convert.ToInt16(EnumPurposeForDBSSOTP.SIMReplByAuth)
                }, msisdnCheckReqest.retailer_id);

                if (otpResp.data != null && otpResp.data.is_otp_valid == false)
                {
                    return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = otpResp.message
                    });
                }


                #endregion
                var response = await _bio.ValidateCorporateMSISDNV2(new CorporateMSISDNCheckRequest
                {
                    poc_msisdn_number = msisdnCheckReqest.poc_msisdn_number,
                    mobile_number = msisdnCheckReqest.mobile_number,
                    sim_number = msisdnCheckReqest.sim_number,
                    channel_name = msisdnCheckReqest.channel_name,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    lan = msisdnCheckReqest.lan,
                    purpose_number = msisdnCheckReqest.purpose_number,
                    session_token = msisdnCheckReqest.session_token
                }, "ValidateMSISDNForCorporateReplacementByAuthPerson_ESIM");

                if(response.result == true)
                {
                    return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = false,
                        message = response.message,
                        data = response
                    });
                }
                else
                {
                    return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = response.message,
                        data = response
                    });
                }                
            }
            catch (Exception ex)
            {
                return Ok(new SIMReplacementMSISDNCheckResponseRevamp()
                {
                    isError = true,
                    message = ex.Message
                });
            }
        }
        #endregion 

        #region Individual SimReplacement submit Order API
        /// Send Order
        /// <summary>
        /// This API is used for SimReplacement submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        //[ResponseType(typeof(SendOrderResponse))]
        [HttpPost]
        [Route("IndividualSIMReplacementSubmitOrderV1")]
        public async Task<IActionResult> IndividualSIMReplacementSubmitOrderV1([FromBody] RAOrderRequest model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse dobInfoResponse = new NidDobInfoResponse();
            IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest = new IndividualSIMReplsMSISDNCheckRequest();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new WebException(MessageCollection.InvalidSecurityToken);

                #region Get_Data_from_Validation

                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.purpose_number = model.purpose_number;
                msisdnCheckReqest.retailer_id = model.retailer_id;

                dobInfoResponse = await GetNidDob(msisdnCheckReqest);

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
                            orderRes.request_id = "0";
                            orderRes.is_success = false;
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
                    log.res_time = DateTime.Now;
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
                log.method_name = "IndividualSIMReplacementSubmitOrderV1";
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
        [Route("IndividualSIMReplacementSubmitOrderV2")]
        public async Task<IActionResult> IndividualSIMReplacementSubmitOrderV2([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse dobInfoResponse = new NidDobInfoResponse();
            IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest = new IndividualSIMReplsMSISDNCheckRequest();

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

                #region Get_Data_from_Validation

                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.purpose_number = model.purpose_number;
                msisdnCheckReqest.retailer_id = model.retailer_id;

                dobInfoResponse = await GetNidDob(msisdnCheckReqest);

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
                            orderRes.request_id = "0";
                            orderRes.is_success = false;
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
                    log.res_time = DateTime.Now;
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
                log.method_name = "IndividualSIMReplacementSubmitOrderV2";
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
        [Route("IndividualSIMReplacementSubmitOrderV3")]
        public async Task<IActionResult> IndividualSIMReplacementSubmitOrderV3([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse dobInfoResponse = new NidDobInfoResponse();
            IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest = new IndividualSIMReplsMSISDNCheckRequest();

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

                #region Get_Data_from_Validation

                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.purpose_number = model.purpose_number;
                msisdnCheckReqest.retailer_id = model.retailer_id;

                dobInfoResponse = await GetNidDob(msisdnCheckReqest);

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
                            orderRes.request_id = "0";
                            orderRes.is_success = false;
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
                    log.res_time = DateTime.Now;
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
                log.method_name = "IndividualSIMReplacementSubmitOrderV3";
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
        [Route("IndividualSIMReplacementSubmitOrderV4")]
        public async Task<IActionResult> IndividualSIMReplacementSubmitOrderV4([FromBody] RAOrderRequestV2 model)
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
                log.method_name = "IndividualSIMReplacementSubmitOrderV3";
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
        [Route("IndividualSIMReplacementSubmitOrder_ESIM")]
        public async Task<IActionResult> IndividualSIMReplacementSubmitOrder_ESIM([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse dobInfoResponse = new NidDobInfoResponse();
            IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest = new IndividualSIMReplsMSISDNCheckRequest();

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

                #region Get_Data_from_Validation

                msisdnCheckReqest.mobile_number = model.msisdn;
                msisdnCheckReqest.purpose_number = model.purpose_number;
                msisdnCheckReqest.retailer_id = model.retailer_id;

                dobInfoResponse = await GetNidDob(msisdnCheckReqest);

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
                #endregion
                #region Insert_Order
                model.status = (int)EnumRAOrderStatus.RequestSubmitted;
                model.order_booking_flag = 800;
                model.is_esim = 1;
                orderRes = await _orderManager.SubmitOrderV6(model, loginProviderId);

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
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
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
                    log.res_time = DateTime.Now;
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
                log.method_name = "IndividualSIMReplacementSubmitOrder_ESIM";
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
        [Route("IndividualSIMReplacementSubmitOrder_ESIMV2")]
        public async Task<IActionResult> IndividualSIMReplacementSubmitOrder_ESIMV2([FromBody] RAOrderRequestV2 model)
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
                if(orderRes != null)
                {
                    log.is_success = orderRes.data != null && orderRes.data.request_id.Length > 1 ? 1 : 0;
                    log.bi_token_number = orderRes.data != null ? orderRes.data.request_id : "";
                }
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.method_name = "IndividualSIMReplacementSubmitOrder_ESIM";
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

        //#region Corporate SimReplacement submit Order API
        /// Send Order
        /// <summary>
        /// This API is used for SimReplacement submit order.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        //[ResponseType(typeof(SendOrderResponse))]
        [HttpPost]
        [Route("CorporateSIMReplacementSubmitOrderV1")]
        public async Task<IActionResult> CorporateSIMReplacementSubmitOrderV1([FromBody] RAOrderRequest model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            CorporateMSISDNCheckRequest checkRequest = new CorporateMSISDNCheckRequest();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new WebException(MessageCollection.InvalidSecurityToken);

                #region Get_NID_DOB
                checkRequest.mobile_number = model.msisdn;
                checkRequest.poc_msisdn_number = model.poc_msisdn_number;
                checkRequest.purpose_number = model.purpose_number;
                checkRequest.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForCorporate(checkRequest);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    if (model.sim_replacement_type != null && model.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                    {
                        model.src_nid = nidDobInfo.dest_nid;
                        model.src_dob = nidDobInfo.dest_dob;
                    }
                    else
                    {
                        model.dest_nid = nidDobInfo.dest_nid;
                        model.dest_dob = nidDobInfo.dest_dob;
                    }
                }
                #endregion

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
                            orderRes.request_id = "0";
                            orderRes.is_success = false;
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
                log.res_time = DateTime.Now;
                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.request_id;
                log.method_name = "CorporateSIMReplacementSubmitOrderV1";
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
        [Route("CorporateSIMReplacementSubmitOrderV2")]
        public async Task<IActionResult> CorporateSIMReplacementSubmitOrderV2([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            CorporateMSISDNCheckRequest checkRequest = new CorporateMSISDNCheckRequest();
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

                #region Get_NID_DOB
                checkRequest.mobile_number = model.msisdn;
                checkRequest.poc_msisdn_number = model.poc_msisdn_number;
                checkRequest.purpose_number = model.purpose_number;
                checkRequest.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForCorporate(checkRequest);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    if (model.sim_replacement_type != null && model.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                    {
                        model.src_nid = nidDobInfo.dest_nid;
                        model.src_dob = nidDobInfo.dest_dob;
                    }
                    else
                    {
                        model.dest_nid = nidDobInfo.dest_nid;
                        model.dest_dob = nidDobInfo.dest_dob;
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

                #region Check if submitted order is already in process or not.
                var orderValidationResult =await _orderManager.ValidateOrder(new VMValidateOrder
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
                            model.status = 150;
                            orderRes.request_id = "0";
                            orderRes.is_success = false;
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
                log.res_time = DateTime.Now;
                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.request_id;
                log.method_name = "CorporateSIMReplacementSubmitOrderV2";
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
        [Route("CorporateSIMReplacementSubmitOrderV3")]
        public async Task<IActionResult> CorporateSIMReplacementSubmitOrderV3([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            CorporateMSISDNCheckRequest checkRequest = new CorporateMSISDNCheckRequest();
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

                #region Get_NID_DOB
                checkRequest.mobile_number = model.msisdn;
                checkRequest.poc_msisdn_number = model.poc_msisdn_number;
                checkRequest.purpose_number = model.purpose_number;
                checkRequest.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForCorporate(checkRequest);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    if (model.sim_replacement_type != null && model.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                    {
                        model.src_nid = nidDobInfo.dest_nid;
                        model.src_dob = nidDobInfo.dest_dob;
                    }
                    else
                    {
                        model.dest_nid = nidDobInfo.dest_nid;
                        model.dest_dob = nidDobInfo.dest_dob;
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
                            orderRes.request_id = "0";
                            orderRes.is_success = false;
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
                log.res_time = DateTime.Now;
                log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.request_id;
                log.method_name = "CorporateSIMReplacementSubmitOrderV3";
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
        [Route("CorporateSIMReplacementSubmitOrderV4")]
        public async Task<IActionResult> CorporateSIMReplacementSubmitOrderV4([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            CorporateMSISDNCheckRequest checkRequest = new CorporateMSISDNCheckRequest();
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

                #region Get_NID_DOB
                checkRequest.mobile_number = model.msisdn;
                checkRequest.poc_msisdn_number = model.poc_msisdn_number;
                checkRequest.purpose_number = model.purpose_number;
                checkRequest.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForCorporate(checkRequest);

                if (nidDobInfo.result == false)
                {
                    //orderRes.request_id = "0";
                    orderRes.isError = true;
                    orderRes.message = nidDobInfo.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    if (model.sim_replacement_type != null && model.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                    {
                        model.src_nid = nidDobInfo.dest_nid;
                        model.src_dob = nidDobInfo.dest_dob;
                    }
                    else
                    {
                        model.dest_nid = nidDobInfo.dest_nid;
                        model.dest_dob = nidDobInfo.dest_dob;
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
                    //orderRes.request_id = "0";
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
                orderRes = await _orderManager.SubmitOrderV7(model, loginProviderId);

                if (orderRes.isError)
                {
                    return Ok(new SendOrderResponseRev()
                    {
                        isError = true,
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
                            orderRes.isError = false;
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
                        orderRes.isError = false;
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
                        orderRes.data.request_id = verifyResp.bss_req_id;
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
                log.method_name = "CorporateSIMReplacementSubmitOrderV3";
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
        [Route("CorporateReplacementSubmitOrder_ESIM")]
        public async Task<IActionResult> CorporateReplacementSubmitOrder_ESIM([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponse orderRes = new SendOrderResponse();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            CorporateMSISDNCheckRequest checkRequest = new CorporateMSISDNCheckRequest();
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

                #region Get_NID_DOB
                checkRequest.mobile_number = model.msisdn;
                checkRequest.poc_msisdn_number = model.poc_msisdn_number;
                checkRequest.purpose_number = model.purpose_number;
                checkRequest.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForCorporate(checkRequest);

                if (nidDobInfo.result == false)
                {
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
                    orderRes.message = nidDobInfo.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    if (model.sim_replacement_type != null && model.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                    {
                        model.src_nid = nidDobInfo.dest_nid;
                        model.src_dob = nidDobInfo.dest_dob;
                    }
                    else
                    {
                        model.dest_nid = nidDobInfo.dest_nid;
                        model.dest_dob = nidDobInfo.dest_dob;
                    }
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
                orderRes = await _orderManager.SubmitOrderV6(model, loginProviderId);

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
                    orderRes.request_id = "0";
                    orderRes.is_success = false;
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
                //=====Order submission=====

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
                        dest_imsi = model.dest_imsi,
                        msidn = model.msisdn,
                        status = model.status,
                        bss_reqId = model.bss_reqId,
                        error_id = model.error_id,
                        err_msg = model.err_msg,
                    });
                }
                log.res_time = DateTime.Now;
                try
                {
                    log.is_success = orderRes.request_id.Length > 1 ? 1 : 0;
                }
                catch
                {}
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.request_id;
                log.method_name = "CorporateReplacementSubmitOrder_ESIM";
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
        [Route("CorporateReplacementSubmitOrder_ESIMV2")]
        public async Task<IActionResult> CorporateReplacementSubmitOrder_ESIMV2([FromBody] RAOrderRequestV2 model)
        {
            SendOrderResponseRev orderRes = new SendOrderResponseRev();
            SendOrderResponse2 response2 = new SendOrderResponse2();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BioVerifyResp verifyResp = null;
            NidDobInfoResponse nidDobInfo = new NidDobInfoResponse();
            CorporateMSISDNCheckRequest checkRequest = new CorporateMSISDNCheckRequest();
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


                #region Get_NID_DOB
                checkRequest.mobile_number = model.msisdn;
                checkRequest.poc_msisdn_number = model.poc_msisdn_number;
                checkRequest.purpose_number = model.purpose_number;
                checkRequest.retailer_id = model.retailer_id;

                nidDobInfo = await GetNidDobForCorporate(checkRequest);

                if (nidDobInfo.result == false)
                {
                    //orderRes.request_id = "0";
                    orderRes.isError = true;
                    orderRes.message = nidDobInfo.message;
                    log.is_success = 0;
                    log.res_time = DateTime.Now;
                    log.res_blob = _blJson.GetGenericJsonData(orderRes);
                    return Ok(orderRes);
                }
                else
                {
                    if (model.sim_replacement_type != null && model.sim_replacement_type == (int)EnumSIMReplacementType.ByAuthPerson)
                    {
                        model.src_nid = nidDobInfo.dest_nid;
                        model.src_dob = nidDobInfo.dest_dob;
                    }
                    else
                    {
                        model.dest_nid = nidDobInfo.dest_nid;
                        model.dest_dob = nidDobInfo.dest_dob;
                    }
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
                    // orderRes.request_id = "0";
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
                        isError = false,
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
                    //orderRes.request_id = "0";
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
                //=====Order submission=====

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
                        orderRes.data.request_id = verifyResp.bss_req_id;
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
                    orderRes.isError = false;
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
                        dest_imsi = model.dest_imsi,
                        msidn = model.msisdn,
                        status = model.status,
                        bss_reqId = model.bss_reqId,
                        error_id = model.error_id,
                        err_msg = model.err_msg,
                    });
                }
                log.res_time = DateTime.Now;
                try
                {
                    log.is_success = orderRes.data.request_id.Length > 1 ? 1 : 0;
                }
                catch
                {
                }
                log.res_blob = _blJson.GetGenericJsonData(orderRes);
                log.bi_token_number = orderRes.data.request_id;
                log.method_name = "CorporateReplacementSubmitOrder_ESIM";
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
#endregion