using BIA.BLL.BLLServices;
using BIA.BLL.Utility;
using BIA.Common;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.Interfaces;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Helper;
using BIA.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Collections;


namespace BIA.Controllers
{
    [Route("api/StarTrekCommon")]
    [ApiController]
    public class StarTrekCommonController : ControllerBase
    {
        private readonly BaseController _bio;
        private readonly eShopAPICall _eShopAPI;
        private readonly BLLDBSSToRAParse _dbssToRaParse;
        private readonly BLLLog _bllLog;
        private readonly BLLCommon _bllCommon;
        private readonly IConfiguration _configuration;

        public StarTrekCommonController(BaseController bio, eShopAPICall eShopAPI, BLLDBSSToRAParse dbssToRaParse, BLLLog bllLog, BLLCommon bllCommon, IConfiguration configuration)
        {
            _bio = bio;
            _eShopAPI = eShopAPI;
            _dbssToRaParse = dbssToRaParse;
            _bllLog = bllLog;
            _bllCommon = bllCommon;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("validate-msisdn")]
        public async Task<IActionResult> ValidateUnpairedMSISDN([FromBody] UnpairedMSISDNCheckRequest msisdnCheckReqest)
        {
            try
            {
                RACommonResponseRevamp response = new RACommonResponseRevamp();

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

                if (SettingsValues.GetRyzeAllowOrNot() == 1)
                {
                    response = await ValidateUnpairedMSISDNSTartTrek(msisdnCheckReqest, "ValidateUnpairedMSISDNSTartTrek");
                }
                else
                {
                    response = await ValidateUnpairedMSISDNSTartTrekV2(msisdnCheckReqest, "ValidateUnpairedMSISDNSTartTrekV2");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        isEsim = 0,
                        request_id = "0"
                    }

                });
            }
        }

        [HttpPost]
        [Route("validate-msisdnv2")]
        public async Task<IActionResult> ValidateUnpairedMSISDNV2([FromBody] UnpairedMSISDNCheckRequest msisdnCheckReqest)
        {
            try
            {
                RACommonResponseRevampV3 response = new RACommonResponseRevampV3();

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

                if (SettingsValues.GetRyzeAllowOrNot() == 1)
                {
                    response = await ValidateUnpairedMSISDNSTartTrekV4(msisdnCheckReqest, "ValidateUnpairedMSISDNSTartTrekV4");
                }
                else
                {
                    response = await ValidateUnpairedMSISDNSTartTrekV3(msisdnCheckReqest, "ValidateUnpairedMSISDNSTartTrekV3");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        isEsim = 0,
                        request_id = "0"
                    }

                });
            }
        }

        [HttpPost]
        [Route("validate-msisdn-online")]
        public async Task<IActionResult> ValidateUnpairedMSISDNOnline([FromBody] UnpairedMSISDNCheckRequestOnline msisdnCheckReqest)
        {
            try
            {
                RACommonResponseRevamp response = new RACommonResponseRevamp();
                eShopOrderResponseModel responseModel = new eShopOrderResponseModel();
                string reservation_id = string.Empty;
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

                eShopOrderValidationReqModel eShopOrder = new eShopOrderValidationReqModel()
                {
                    orderId = msisdnCheckReqest.order_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    retailer_id = msisdnCheckReqest.retailer_id
                };

                responseModel = await _eShopAPI.OrderValidation(eShopOrder);

                if (responseModel != null)
                {
                    if (responseModel.data != null)
                    {
                        if (responseModel.data.is_reserved == false)
                        {
                            throw new Exception(responseModel.message);
                        }
                        else if (String.IsNullOrEmpty(responseModel.data.reservation_id))
                        {
                            throw new Exception("The reservation_id field is empty!");
                        }
                        else if (!String.IsNullOrEmpty(responseModel.data.reservation_id) && responseModel.data.status_code == 200 && responseModel.data.is_reserved == true)
                        {
                            reservation_id = responseModel.data.reservation_id;
                        }
                        else
                        {
                            throw new Exception("Invalid eShop API response!");
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid eShop API response!");
                    }
                }
                else
                {
                    throw new Exception("Invalid eShop API response!");
                }

                response = await ValidateUnpairedMSISDNSTartTrekOnline(msisdnCheckReqest, reservation_id, "ValidateUnpairedMSISDNOnline");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        isEsim = 0,
                        request_id = "0"
                    }
                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("Validate-msisdn-esim")]
        public async Task<IActionResult> ValidateUnpairedMSISDN_ESIM([FromBody] UnpairedMSISDNCheckRequest msisdnCheckReqest)
        {
            RACommonResponseRevamp rACommonResponse = new RACommonResponseRevamp();
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

                if (SettingsValues.GetRyzeAllowOrNot() == 1)
                {
                    rACommonResponse = await ValidateUnpairedMSISDNESIM(msisdnCheckReqest, "ValidateUnpairedMSISDNESIM");
                }
                else
                {
                    rACommonResponse = await ValidateUnpairedMSISDNESIMV2(msisdnCheckReqest, "ValidateUnpairedMSISDNESIMV2");
                }

                return Ok(rACommonResponse);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        request_id = "0",
                        isEsim = 1
                    }
                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("Validate-msisdn-esim-online")]
        public async Task<IActionResult> ValidateUnpairedMSISDN_ESIM_Online([FromBody] UnpairedMSISDNCheckRequestOnline msisdnCheckReqest)
        {
            RACommonResponseRevamp rACommonResponse = new RACommonResponseRevamp();
            eShopOrderResponseModel responseModel = new eShopOrderResponseModel();
            string reservation_id = string.Empty;
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

                eShopOrderValidationReqModel eShopOrder = new eShopOrderValidationReqModel()
                {
                    orderId = msisdnCheckReqest.order_id,
                    msisdn = msisdnCheckReqest.mobile_number
                };

                responseModel = await _eShopAPI.OrderValidation(eShopOrder);

                if (responseModel != null)
                {
                    if (String.IsNullOrEmpty(responseModel.data.reservation_id))
                    {
                        throw new Exception("MSISDN isn't reserved in eShop yet!");
                    }
                    else if (String.IsNullOrEmpty(responseModel.data.reservation_id) && responseModel.data.status_code == 200)
                    {
                        reservation_id = responseModel.data.reservation_id;
                    }
                    else
                    {
                        throw new Exception("Invalid eShop API response!");
                    }
                }
                else
                {
                    throw new Exception("Invalid eShop API response!");
                }

                rACommonResponse = await ValidateUnpairedMSISDNESIM_Online(msisdnCheckReqest, reservation_id, "ValidateUnpairedMSISDNESIM");

                return Ok(rACommonResponse);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        request_id = "0",
                        isEsim = 1
                    }
                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("validate-sim-replacement")]
        public async Task<IActionResult> STarTrekValidateSIMReplacement([FromBody] IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            IndividualSIMReplacementMSISDNCheckResponseRevamp response = new IndividualSIMReplacementMSISDNCheckResponseRevamp();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
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

                if (SettingsValues.GetRyzeAllowOrNot() == 1)
                {
                    response = await STarTrekValidateSIMForReplacement(msisdnCheckReqest);
                }
                else
                {
                    response = await STarTrekValidateSIMForReplacementV2(msisdnCheckReqest);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        isEsim = 0,
                        request_id = "0"
                    }
                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("validate-sim-replacement-online")]
        public async Task<IActionResult> STarTrekValidateSIMReplacementOnline([FromBody] IndividualSIMReplsMSISDNCheckRequestOnline msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            IndividualSIMReplacementMSISDNCheckResponseRevamp response = new IndividualSIMReplacementMSISDNCheckResponseRevamp();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
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

                var msisdnResp = StarTrekSIMReplacementParsing(dbssResp);

                if (msisdnResp.result == false)
                {
                    return Ok(new IndividualSIMReplacementMSISDNCheckResponseRevamp()
                    {
                        isError = true,
                        message = FixedValueCollection.MSISDNError + msisdnResp.message
                    });
                }

                var simResp = await CheckSIMNumberReplacement(new SIMNumberCheckRequest()
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
                log.method_name = "STarTrekValidateSIMReplacement";
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("validate-sim-replacement-esim")]
        public async Task<IActionResult> StarTrekValidateSIMReplacement_ESIM([FromBody] IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
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

                if (SettingsValues.GetRyzeAllowOrNot() == 1)
                {
                    response = await StarTrekValidateSIMForReplacement_ESIM(msisdnCheckReqest);
                }
                else
                {
                    response = await StarTrekValidateSIMForReplacement_ESIMV2(msisdnCheckReqest);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        isEsim = 0,
                        request_id = "0"
                    }
                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("validate-sim-replacement-esim-online")]
        public async Task<IActionResult> StarTrekValidateSIMReplacement_ESIMOnline([FromBody] IndividualSIMReplsMSISDNCheckRequestOnline msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
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

                var msisdnResp = StarTrekSIMReplacementParsing(dbssResp);

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
                log.method_name = "StarTrekValidateSIMReplacement_ESIM";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        [HttpPost]
        [Route("cyn-online")]
        public async Task<IActionResult> GetUnpairedMSISDNLOnline(UnpairedMSISDNListReqModel model)
        {
            List<ReponseDataRev> raRespData = new List<ReponseDataRev>();
            UnpairedMSISDNDataRev raResp = new UnpairedMSISDNDataRev();
            string apiUrl = string.Empty;
            string? txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

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

                if (String.IsNullOrEmpty(model.msisdn))
                {
                    model.msisdn = await _bllCommon.GetUnpairedMSISDNSearchDefaultValueV2(model);

                    if (String.IsNullOrEmpty(model.msisdn))
                    {
                        return Ok(raResp);
                    }
                    if (model.msisdn.Substring(0, 4) != FixedValueCollection.MSISDNFixedValue)
                    {
                        model.msisdn = FixedValueCollection.MSISDNFixedValue + model.msisdn;
                    }
                    if (model.msisdn.Substring(0, 1) == "0")
                    {
                        model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                    }
                }
                else
                {
                    if (model.msisdn.Substring(0, 4) != FixedValueCollection.MSISDNFixedValue)
                    {
                        model.msisdn = FixedValueCollection.MSISDNFixedValue + model.msisdn;
                    }
                    if (model.msisdn.Substring(0, 1) == "0")
                    {
                        model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                    }
                }

                string channelIdFromConfig = string.Empty;
                string[] arrChannelId = null;
                string stockIdFromConfig = string.Empty;
                string[] arrStockId = null;
                string channelId = string.Empty;
                int arrIndexChannel = 0;
                string stockIdValue = string.Empty;
                string stockIdByDefault = string.Empty;
                try
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                    channelIdFromConfig = configuration.GetSection("AppSettings:ChannelId").Value;
                    stockIdFromConfig = configuration.GetSection("AppSettings:ChannelStockId").Value;
                    stockIdByDefault = configuration.GetSection("AppSettings:ChannelStockIdDefault").Value;
                }
                catch { }

                if (channelIdFromConfig.Contains(","))
                {
                    arrChannelId = channelIdFromConfig.Split(',');
                }
                else
                {
                    arrChannelId = channelIdFromConfig.Split(' ');
                }

                if (stockIdFromConfig.Contains(","))
                {
                    arrStockId = stockIdFromConfig.Split(',');
                }
                else
                {
                    arrStockId = stockIdFromConfig.Split(' ');
                }

                channelId = await _dbssToRaParse.GetStockResponses(model.channel_name);

                if (arrChannelId.Contains(channelId))
                {
                    arrIndexChannel = Array.IndexOf(arrChannelId, channelId);
                    stockIdValue = arrStockId[arrIndexChannel];
                }
                else
                {
                    stockIdValue = stockIdByDefault;
                }

                apiUrl = String.Format(UnpairedMSISDNList.GetCYNListOnline, 1, 10, model.msisdn);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                    log.is_success = 1;
                    //var dataBss = JsonConvert.DeserializeObject(dbssResp.ToString());
                    UnpairedMSISDNRootData? dbssRespModel = JsonConvert.DeserializeObject<UnpairedMSISDNRootData>(dbssResp.ToString());
                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var result = ((IEnumerable<object>)dbssRespModel.data).ToList();

                            raRespData = _dbssToRaParse.UnpairedMSISDNListDataParsingV2(result);

                            if (raRespData.Count > 0)
                            {
                                raResp.data = raRespData;
                                raResp.isError = false;
                                raResp.message = MessageCollection.Success;
                            }
                            else
                            {
                                raResp.data = raRespData;
                                raResp.isError = true;
                                raResp.message = MessageCollection.NoDataFound;
                            }
                        }
                        else
                        {
                            raResp.data = raRespData;
                            raResp.isError = true;
                            raResp.message = "DBSS API doesn't contains any Unpaired MSISDN list.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.isError = true;
                        raResp.message = "DBSS API doesn't contains any Unpaired MSISDN list.";
                    }
                }
                else
                {
                    raResp.data = raRespData;
                    raResp.isError = true;
                    raResp.message = "Unable to load data from DBSS API.";
                }
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

                    raResp.data = raRespData;
                    raResp.isError = true;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    raResp.data = raRespData;
                    raResp.isError = true;
                    raResp.message = ex.Message;
                }
            }
            finally
            {
                log.method_name = "GetUnpairedMSISDNLOnline";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }

        [HttpPost]
        [Route("cyn-physical")]
        public async Task<IActionResult> GetUnpairedMSISDNPhysical(UnpairedMSISDNListReqModel model)
        {
            List<ReponseDataRev> raRespData = new List<ReponseDataRev>();
            UnpairedMSISDNDataRev raResp = new UnpairedMSISDNDataRev();
            string apiUrl = string.Empty;
            string? txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

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

                if (String.IsNullOrEmpty(model.msisdn))
                {
                    model.msisdn = await _bllCommon.GetUnpairedMSISDNSearchDefaultValueV2(model);

                    if (String.IsNullOrEmpty(model.msisdn))
                    {
                        return Ok(raResp);
                    }
                    if (model.msisdn.Substring(0, 4) != FixedValueCollection.MSISDNFixedValue)
                    {
                        model.msisdn = FixedValueCollection.MSISDNFixedValue + model.msisdn;
                    }
                    if (model.msisdn.Substring(0, 1) == "0")
                    {
                        model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                    }
                }
                else
                {
                    if (model.msisdn.Substring(0, 4) != FixedValueCollection.MSISDNFixedValue)
                    {
                        model.msisdn = FixedValueCollection.MSISDNFixedValue + model.msisdn;
                    }
                    if (model.msisdn.Substring(0, 1) == "0")
                    {
                        model.msisdn = FixedValueCollection.MSISDNCountryCode + model.msisdn;
                    }
                }

                string channelIdFromConfig = string.Empty;
                string[] arrChannelId = null;
                string stockIdFromConfig = string.Empty;
                string[] arrStockId = null;
                string channelId = string.Empty;
                int arrIndexChannel = 0;
                string stockIdValue = string.Empty;
                string stockIdByDefault = string.Empty;
                try
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                    channelIdFromConfig = configuration.GetSection("AppSettings:ChannelId").Value;
                    stockIdFromConfig = configuration.GetSection("AppSettings:ChannelStockId").Value;
                    stockIdByDefault = configuration.GetSection("AppSettings:ChannelStockIdDefault").Value;
                }
                catch { }

                if (channelIdFromConfig.Contains(","))
                {
                    arrChannelId = channelIdFromConfig.Split(',');
                }
                else
                {
                    arrChannelId = channelIdFromConfig.Split(' ');
                }

                if (stockIdFromConfig.Contains(","))
                {
                    arrStockId = stockIdFromConfig.Split(',');
                }
                else
                {
                    arrStockId = stockIdFromConfig.Split(' ');
                }

                channelId = await _dbssToRaParse.GetStockResponses(model.channel_name);

                if (arrChannelId.Contains(channelId))
                {
                    arrIndexChannel = Array.IndexOf(arrChannelId, channelId);
                    stockIdValue = arrStockId[arrIndexChannel];
                }
                else
                {
                    stockIdValue = stockIdByDefault;
                }

                if (SettingsValues.GetRyzeAllowOrNot() == 1)
                {
                    apiUrl = String.Format(UnpairedMSISDNList.GetCYNListPhysical, 1, 10, model.msisdn, stockIdValue);
                }
                else
                {
                    apiUrl = String.Format(UnpairedMSISDNList.GetCYNListPhysicalStock16, 1, 10, model.msisdn, stockIdValue);
                }

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                    log.is_success = 1;
                    //var dataBss = JsonConvert.DeserializeObject(dbssResp.ToString());
                    UnpairedMSISDNRootData? dbssRespModel = JsonConvert.DeserializeObject<UnpairedMSISDNRootData>(dbssResp.ToString());
                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var result = ((IEnumerable<object>)dbssRespModel.data).ToList();

                            raRespData = _dbssToRaParse.UnpairedMSISDNListDataParsingV2(result);

                            if (raRespData.Count > 0)
                            {
                                raResp.data = raRespData;
                                raResp.isError = false;
                                raResp.message = MessageCollection.Success;
                            }
                            else
                            {
                                raResp.data = raRespData;
                                raResp.isError = true;
                                raResp.message = MessageCollection.NoDataFound;
                            }
                        }
                        else
                        {
                            raResp.data = raRespData;
                            raResp.isError = true;
                            raResp.message = "DBSS API doesn't contains any Unpaired MSISDN list.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.isError = true;
                        raResp.message = "DBSS API doesn't contains any Unpaired MSISDN list.";
                    }
                }
                else
                {
                    raResp.data = raRespData;
                    raResp.isError = true;
                    raResp.message = "Unable to load data from DBSS API.";
                }
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

                    raResp.data = raRespData;
                    raResp.isError = true;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    raResp.data = raRespData;
                    raResp.isError = true;
                    raResp.message = ex.Message;
                }
            }
            finally
            {
                log.method_name = "GetUnpairedMSISDNPhysical";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }

        [HttpPost]
        [Route("validate-msisdn-mnp")]
        public async Task<IActionResult> StarTrekValidateMSISDNMNP([FromBody] UnpairedMSISDNCheckRequest msisdnCheckReqest)
        {
            try
            {
                RACommonResponseRevamp response = new RACommonResponseRevamp();

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

                if (SettingsValues.GetRyzeAllowOrNot() == 1)
                {
                    response = await ValidateUnpairedMSISDNMNPSTartTrek(msisdnCheckReqest, "ValidateUnpairedMSISDNMNPSTartTrek");
                }
                else
                {
                    response = await ValidateUnpairedMSISDNMNPSTartTrekV2(msisdnCheckReqest, "ValidateUnpairedMSISDNMNPSTartTrekV2");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        isEsim = 0,
                        request_id = "0"
                    }

                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("Validate-msisdn-mnp-esim")]
        public async Task<IActionResult> StarTrekValidateMSISDNMNP_ESIM([FromBody] UnpairedMSISDNCheckRequest msisdnCheckReqest)
        {
            RACommonResponseRevamp rACommonResponse = new RACommonResponseRevamp();
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

                if (SettingsValues.GetRyzeAllowOrNot() == 1)
                {
                    rACommonResponse = await ValidateUnpairedMSISDNMNPSTartTrekesim(msisdnCheckReqest, "ValidateUnpairedMSISDNMNPSTartTrekesim");
                }
                else
                {
                    rACommonResponse = await ValidateUnpairedMSISDNMNPSTartTrekesimV2(msisdnCheckReqest, "ValidateUnpairedMSISDNMNPSTartTrekesimV2");
                }

                return Ok(rACommonResponse);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        request_id = "0",
                        isEsim = 1
                    }
                });
            }
        }

        [HttpPost]
        [Route("validate-msisdn-online-resubmit")]
        public async Task<IActionResult> ValidateUnpairedMSISDNOnlineV2([FromBody] UnpairedMSISDNCheckRequestOnline msisdnCheckReqest)
        {
            try
            {
                RACommonResponseRevamp response = new RACommonResponseRevamp();
                eShopOrderResponseModel responseModel = new eShopOrderResponseModel();
                string reservation_id = string.Empty;
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

                eShopOrderValidationReqModel eShopOrder = new eShopOrderValidationReqModel()
                {
                    orderId = msisdnCheckReqest.order_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    retailer_id = msisdnCheckReqest.retailer_id
                };

                responseModel = await _eShopAPI.OrderValidation(eShopOrder);

                if (responseModel != null)
                {
                    if (responseModel.data != null)
                    {
                        if (responseModel.data.is_reserved == false)
                        {
                            throw new Exception(responseModel.message);
                        }
                        else if (String.IsNullOrEmpty(responseModel.data.reservation_id))
                        {
                            throw new Exception("The reservation_id field is empty!");
                        }
                        else if (!String.IsNullOrEmpty(responseModel.data.reservation_id) && responseModel.data.status_code == 200 && responseModel.data.is_reserved == true)
                        {
                            reservation_id = responseModel.data.reservation_id;
                        }
                        else
                        {
                            throw new Exception("Invalid eShop API response!");
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid eShop API response!");
                    }
                }
                else
                {
                    throw new Exception("Invalid eShop API response!");
                }

                response = await ValidateUnpairedMSISDNSTartTrekOnlineV2(msisdnCheckReqest, reservation_id, "ValidateUnpairedMSISDNOnlineV2");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        isEsim = 0,
                        request_id = "0"
                    }
                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("Validate-msisdn-esim-online-resubmit")]
        public async Task<IActionResult> ValidateUnpairedMSISDN_ESIM_OnlineV2([FromBody] UnpairedMSISDNCheckRequestOnline msisdnCheckReqest)
        {
            RACommonResponseRevamp rACommonResponse = new RACommonResponseRevamp();
            eShopOrderResponseModel responseModel = new eShopOrderResponseModel();
            string reservation_id = string.Empty;
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

                eShopOrderValidationReqModel eShopOrder = new eShopOrderValidationReqModel()
                {
                    orderId = msisdnCheckReqest.order_id,
                    msisdn = msisdnCheckReqest.mobile_number
                };

                responseModel = await _eShopAPI.OrderValidation(eShopOrder);

                if (responseModel != null)
                {
                    if (String.IsNullOrEmpty(responseModel.data.reservation_id))
                    {
                        throw new Exception("MSISDN isn't reserved in eShop yet!");
                    }
                    else if (String.IsNullOrEmpty(responseModel.data.reservation_id) && responseModel.data.status_code == 200)
                    {
                        reservation_id = responseModel.data.reservation_id;
                    }
                    else
                    {
                        throw new Exception("Invalid eShop API response!");
                    }
                }
                else
                {
                    throw new Exception("Invalid eShop API response!");
                }

                rACommonResponse = await ValidateUnpairedMSISDNESIM_OnlineV2(msisdnCheckReqest, reservation_id, "ValidateUnpairedMSISDNESIM");

                return Ok(rACommonResponse);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        request_id = "0",
                        isEsim = 1
                    }
                });
            }
        }

        public async Task<RACommonResponse> CheckSIMNumberReplacement(SIMNumberCheckRequest simNumberCheckReqest, int purposeOfSIMCheck, bool? isPaired, int? simCategory, string old_sim_type)
        {
            RACommonResponse raResp = new RACommonResponse();
            string apiUrl = "", txtResp = "";
            SIMValidationRequestRootobject dbssReqModel = null;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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
                raResp = SIMValidationParsingSIMReplacement(dbssResp, purposeOfSIMCheck, simCategory == null ? null : simCategory, isPaired, old_sim_type);
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
                log.method_name = "CheckSIMNumberReplacement";
                log.msisdn = _bllLog.FormatMSISDN(simNumberCheckReqest.msisdn);

                await _bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp);
            }
            return raResp;
        }
        public async Task<RACommonResponse> CheckSIMNumberReplacementV2(SIMNumberCheckRequest simNumberCheckReqest, int purposeOfSIMCheck, bool? isPaired, int? simCategory, string old_sim_type)
        {
            RACommonResponse raResp = new RACommonResponse();
            string apiUrl = "", txtResp = "";
            SIMValidationRequestRootobject dbssReqModel = null;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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
                raResp = SIMValidationParsingSIMReplacementV2(dbssResp, purposeOfSIMCheck, simCategory == null ? null : simCategory, isPaired, old_sim_type);
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
                log.method_name = "CheckSIMNumberReplacementV2";
                log.msisdn = _bllLog.FormatMSISDN(simNumberCheckReqest.msisdn);

                await _bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp);
            }
            return raResp;
        }
        public RACommonResponse SIMValidationParsingSIMReplacement(JObject dbssResp, int purposeOfSIMCheck, int? simCategory, bool? isPired, string oldSimType)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (dbssResp?["data"]?["status"] == null
                    && dbssResp?["data"]?["logical_inventory_status"] == null
                    && dbssResp?["data"]?["physical_inventory_status"] == null
                    && String.IsNullOrEmpty(dbssResp?["data"]?["status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["logical_inventory_status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["physical_inventory_status"]?.ToString()))
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESimStarTrek.ToLower() /*ryz-esim*/)
                {
                    {
                        response.result = false;
                        response.message = "This is not Physical SIM.";
                        return response;
                    }
                }
                else if (dbssResp?["data"]?["status"]?.ToString().ToLower() == "failed")
                {
                    response.result = false;

                    response.message = MessageCollection.SIMIsNotInInventory;
                    if (dbssResp != null && dbssResp.ContainsKey("data") && dbssResp["data"] != null && (dbssResp["data"] is JObject dataObj && dataObj.ContainsKey("error_message")))
                    {
                        var errorMessage = dbssResp["data"]["error_message"]?.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            response.message = errorMessage;
                        }
                    }

                    return response;
                }

                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == "used")
                {
                    response.result = false;
                    response.message = MessageCollection.SIMIsUsed;
                    return response;
                }                //----------------SIMReplacement--------------
                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.SIMReplacement
                    && !String.IsNullOrEmpty(oldSimType))
                {
                    if (oldSimType.ToLower() == FixedValueCollection.SIMTypeUSIM /*"usim"*/)
                    {
                        if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaidStarTrek.ToLower() /*"ryze-prepaid"*/)
                        {
                            response.result = true;
                            response.message = MessageCollection.SIMValid;
                            return response;
                        }
                        else
                        {
                            response.result = false;
                            response.message = MessageCollection.NotASwapSIMStarTrek;
                            return response;
                        }
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMTypeIsNotSIMOrUSIM;
                        return response;
                    }
                }
                else
                {
                    response.result = false;
                    response.message = MessageCollection.InvalidAttempt + " while checking SIM!";
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public RACommonResponse SIMValidationParsingSIMReplacementV2(JObject dbssResp, int purposeOfSIMCheck, int? simCategory, bool? isPired, string oldSimType)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (dbssResp?["data"]?["status"] == null
                    && dbssResp?["data"]?["logical_inventory_status"] == null
                    && dbssResp?["data"]?["physical_inventory_status"] == null
                    && String.IsNullOrEmpty(dbssResp?["data"]?["status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["logical_inventory_status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["physical_inventory_status"]?.ToString()))
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESimStarTrek.ToLower() /*ryz-esim*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESim.ToLower() /*E-SIM*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeE_SIM_SWAP.ToLower()/*e_sim_swap*/)
                {
                    response.result = false;
                    response.message = "This is not Physical SIM.";
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePostpaid.ToLower()
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaidStarTrek.ToLower() /*ryz-prepaid*/)
                {
                    response.result = false;
                    response.message = "Incorrect Product!";
                    return response;
                }

                else if (dbssResp?["data"]?["status"]?.ToString().ToLower() == "failed")
                {
                    response.result = false;
                    
                    response.message = MessageCollection.SIMIsNotInInventory;
                    if (dbssResp != null && dbssResp.ContainsKey("data") && dbssResp["data"] != null && (dbssResp["data"] is JObject dataObj && dataObj.ContainsKey("error_message")))
                    {
                        var errorMessage = dbssResp["data"]["error_message"]?.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            response.message = errorMessage;
                        }
                    }

                    return response;
                }

                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == "used")
                {
                    response.result = false;
                    response.message = MessageCollection.SIMIsUsed;
                    return response;
                }                //----------------SIMReplacement--------------
                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.SIMReplacement
                    && !String.IsNullOrEmpty(oldSimType))
                {
                    if (oldSimType.ToLower() == FixedValueCollection.SIMTypeUSIM /*"usim"*/)
                    {
                        if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PhycalInventorySIMTypeSIM_SWAP.ToLower() /*"sim_swap"*/)
                        {
                            response.result = true;
                            response.message = MessageCollection.SIMValid;
                            return response;
                        }
                        else
                        {
                            response.result = false;
                            response.message = MessageCollection.NotASwapSIMStarTrek;
                            return response;
                        }
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMTypeIsNotUSIM;
                        return response;
                    }
                }
                else
                {
                    response.result = false;
                    response.message = MessageCollection.InvalidAttempt + " while checking SIM!";
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IndividualSIMReplacementMSISDNCheckResponseRevamp> STarTrekValidateSIMForReplacement(IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            IndividualSIMReplacementMSISDNCheckResponseRevamp response = new IndividualSIMReplacementMSISDNCheckResponseRevamp();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
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

                JObject dbssResp = new JObject();
                try
                {
                    dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Not Found"))
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
                    response.isError = true;
                    response.message = MessageCollection.SIMReplNoDataFound;
                    return response;
                }

                log.is_success = 1;

                var msisdnResp = StarTrekSIMReplacementParsing(dbssResp);

                if (msisdnResp.result == false)
                {
                    response.isError = true;
                    response.message = FixedValueCollection.MSISDNError + msisdnResp.message;
                    return response;
                }

                var simResp = await CheckSIMNumberReplacement(new SIMNumberCheckRequest()
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
                    response.isError = true;
                    response.message = simResp.message;
                    return response;
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
                response.isError = false;
                response.message = resp.message;
                response.data = resp;
                return response;
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

                    response.isError = true;
                    response.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return response;
                }
                catch (Exception)
                {
                    response.isError = true;
                    response.message = ex.Message;
                    return response;
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "STarTrekValidateSIMForReplacement";
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public async Task<IndividualSIMReplacementMSISDNCheckResponseRevamp> STarTrekValidateSIMForReplacementV2(IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            IndividualSIMReplacementMSISDNCheckResponseRevamp response = new IndividualSIMReplacementMSISDNCheckResponseRevamp();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
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

                JObject dbssResp = new JObject();
                try
                {
                    dbssResp = await _apiReq.HttpGetRequest(apiUrl);

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Not Found"))
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
                    response.isError = true;
                    response.message = MessageCollection.SIMReplNoDataFound;
                    return response;
                }

                log.is_success = 1;

                var msisdnResp = StarTrekSIMReplacementParsing(dbssResp);

                if (msisdnResp.result == false)
                {
                    response.isError = true;
                    response.message = FixedValueCollection.MSISDNError + msisdnResp.message;
                    return response;
                }

                var simResp = await CheckSIMNumberReplacementV2(new SIMNumberCheckRequest()
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
                    response.isError = true;
                    response.message = simResp.message;
                    return response;
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
                response.isError = false;
                response.message = resp.message;
                response.data = resp;
                return response;
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

                    response.isError = true;
                    response.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return response;
                }
                catch (Exception)
                {
                    response.isError = true;
                    response.message = ex.Message;
                    return response;
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "STarTrekValidateSIMForReplacementV2";
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public async Task<IndividualSIMReplacementMSISDNCheckResponseRevamp> StarTrekValidateSIMForReplacement_ESIM([FromBody] IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
            IndividualSIMReplacementMSISDNCheckResponseRevamp response = new IndividualSIMReplacementMSISDNCheckResponseRevamp();
            try
            {

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
                    response.isError = true;
                    response.message = MessageCollection.SIMReplNoDataFound;
                    return response;
                }

                log.is_success = 1;

                var msisdnResp = StarTrekSIMReplacementParsing(dbssResp);

                if (msisdnResp.result == false)
                {
                    response.isError = true;
                    response.message = FixedValueCollection.MSISDNError + msisdnResp.message;
                    return response;
                }

                var simResp = await CheckSIMNumber(new SIMNumberCheckRequest()
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
                    response.isError = true;
                    response.message = simResp.message;
                    return response;
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
                response.isError = false;
                response.message = resp.message;
                response.data = resp;
                return response;
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

                    response.isError = true;
                    response.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return response;
                }
                catch (Exception)
                {
                    response.isError = true;
                    response.message = ex.Message;
                    return response;
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "StarTrekValidateSIMReplacement_ESIM";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public async Task<IndividualSIMReplacementMSISDNCheckResponseRevamp> StarTrekValidateSIMForReplacement_ESIMV2([FromBody] IndividualSIMReplsMSISDNCheckRequest msisdnCheckReqest)
        {
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
            IndividualSIMReplacementMSISDNCheckResponseRevamp response = new IndividualSIMReplacementMSISDNCheckResponseRevamp();
            try
            {
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
                    response.isError = true;
                    response.message = MessageCollection.SIMReplNoDataFound;
                    return response;
                }

                log.is_success = 1;

                var msisdnResp = StarTrekSIMReplacementParsing(dbssResp);

                if (msisdnResp.result == false)
                {
                    response.isError = true;
                    response.message = FixedValueCollection.MSISDNError + msisdnResp.message;
                    return response;
                }

                var simResp = await CheckSIMNumberV3(new SIMNumberCheckRequest()
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
                    response.isError = true;
                    response.message = simResp.message;
                    return response;
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
                response.isError = false;
                response.message = resp.message;
                response.data = resp;
                return response;
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

                    response.isError = true;
                    response.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return response;
                }
                catch (Exception)
                {
                    response.isError = true;
                    response.message = ex.Message;
                    return response;
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "StarTrekValidateSIMForReplacement_ESIMV2";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public IndividualSIMReplacementMSISDNCheckResponse StarTrekSIMReplacementParsing(JObject dbssRespObj)
        {
            IndividualSIMReplacementMSISDNCheckResponse raResp = new IndividualSIMReplacementMSISDNCheckResponse();
            try
            {
                if (!dbssRespObj["data"].HasValues
                    || dbssRespObj["data"].Count() <= 0)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.SIMReplNoDataFound;
                    return raResp;
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"]))
                {
                    raResp.result = false;
                    raResp.message = "Msisdn status not found!";
                    return raResp;
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] == "terminated")
                {
                    raResp.result = false;
                    raResp.message = "Msisdn is not valid for SIM replacemnt!";
                    return raResp;
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] != "active"
                     && (string)dbssRespObj["data"]["attributes"]["status"] != "idle")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNStatusNotActiveOrIdle;
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }

                if (!dbssRespObj["included"].HasValues
                    || (dbssRespObj["included"].Count() != 2
                    && dbssRespObj["included"].Count() != 3))
                {
                    raResp.result = false;
                    raResp.message = "Data not found in include field!";
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }

                if (dbssRespObj["data"]["id"] == null)
                {
                    raResp.result = false;
                    raResp.message = "Subscription ID field empty!";
                    return raResp;
                }
                if (dbssRespObj["included"][0]["attributes"] == null
                    || dbssRespObj["included"][1]["attributes"] == null)
                {
                    raResp.result = false;
                    raResp.message = "Data not found in include field!";
                    return raResp;
                }
                if (String.IsNullOrEmpty((string)dbssRespObj["included"][1]["attributes"]["icc"]))
                {
                    raResp.result = false;
                    raResp.message = "Old SIM number not found!";
                    return raResp;
                }
                if (String.IsNullOrEmpty((string)dbssRespObj["included"][1]["attributes"]["sim-type"]))
                {
                    raResp.result = false;
                    raResp.message = "sim-type not found!";
                    return raResp;
                }

                if (dbssRespObj["included"][0]["attributes"]["is-company"] == null)
                {
                    raResp.result = false;
                    raResp.message = "Company information not found!";
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }

                if (dbssRespObj["included"][0]["attributes"]["id-document-type"] == null
                     || String.IsNullOrEmpty((string)dbssRespObj["included"][0]["attributes"]["id-document-type"]))
                {
                    raResp.result = false;
                    raResp.message = "id-document-type not found!";
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }

                string idDocumentType = (string)dbssRespObj["included"][0]["attributes"]["id-document-type"];

                if (idDocumentType != "national_id"
                    && idDocumentType != "smart_national_id")
                {
                    raResp.result = false;
                    raResp.message = "Customer is not registered with National ID!";
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }
                else if ((bool)dbssRespObj["included"][0]["attributes"]["is-company"] == true)
                {
                    raResp.result = false;
                    raResp.message = "This MSISDN is not eligible for individual SIM replacement.";
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }
                else
                {
                    raResp.saf_status = true;//[Has_SAF] By deafult this value is true. At this moment we are not checking saf status because DBSS API   
                    raResp.customer_id = String.Empty;
                    raResp.dob = (string)dbssRespObj["included"][0]["attributes"]["date-of-birth"];
                    raResp.doc_id_number = (string)dbssRespObj["included"][0]["attributes"]["id-document-number"];
                    raResp.dbss_subscription_id = (int)dbssRespObj["data"]["id"];
                    raResp.old_sim_number = (string)dbssRespObj["included"][1]["attributes"]["icc"];
                    raResp.old_sim_type = (string)dbssRespObj["included"][1]["attributes"]["sim-type"];
                    raResp.result = true;
                    raResp.message = MessageCollection.MSISDNValid;
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNSTartTrekOnline(UnpairedMSISDNCheckRequestOnline msisdnCheckReqest, string reservation_id, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = UnpairedMSISDNResPargingOnline(dbssResp, msisdnCheckReqest.retailer_id);

                if (msisdnResp.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = msisdnResp.message;
                    return raRespModel;
                }
                else
                {
                    if (!msisdnResp.reservation_id.Equals(reservation_id))
                    {
                        raRespModel.isError = true;
                        raRespModel.message = "The reservation id is not matched with DBSS!";
                        return raRespModel;
                    }
                }

                var stockCheck = await _bllCommon.IsStockAvailable(msisdnResp.stock_id, Convert.ToInt32(msisdnCheckReqest.channel_id));

                if (stockCheck == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.StockIDMismatch;
                    return raRespModel;
                }


                var simResp = await StarTrekCheckSIMNumber(new SIMNumberCheckRequest()
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

                Datas datas = new Datas();
                datas.isEsim = 0;
                datas.request_id = "Test";
                datas.reservation_id = reservation_id;

                raRespModel.isError = false;
                raRespModel.data = new Datas()
                {
                    reservation_id = reservation_id
                };
                raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                return raRespModel;
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
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNSTartTrek(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = UnpairedMSISDNReqParsing(dbssResp, msisdnCheckReqest.retailer_id);

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


                var simResp = await StarTrekCheckSIMNumber(new SIMNumberCheckRequest()
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
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNSTartTrekV2(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = UnpairedMSISDNReqParsingV2(dbssResp, msisdnCheckReqest.retailer_id);

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

                var simResp = await StarTrekCheckSIMNumberV2(new SIMNumberCheckRequest()
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

        public async Task<RACommonResponseRevampV3> ValidateUnpairedMSISDNSTartTrekV3(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevampV3 raRespModel = new RACommonResponseRevampV3();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = await UnpairedMSISDNReqParsingV3(dbssResp, msisdnCheckReqest.retailer_id, msisdnCheckReqest.channel_name);

                if (msisdnResp.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = msisdnResp.message;
                    raRespModel.data = new DesiredCategoryData()
                    {
                        message = msisdnResp.data_message,
                        isDesiredCategory = msisdnResp.isDesiredCategory,
                        category = msisdnResp.category_name
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
                        message = msisdnResp.data_message,
                        isDesiredCategory = msisdnResp.isDesiredCategory,
                        category = msisdnResp.category_name
                    };
                    return raRespModel;
                }

                var simResp = await StarTrekCheckSIMNumberV2(new SIMNumberCheckRequest()
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
                        message = msisdnResp.data_message,
                        isDesiredCategory = msisdnResp.isDesiredCategory,
                        category = msisdnResp.category_name
                    };
                    return raRespModel;
                }

                raRespModel.isError = false;
                raRespModel.data = new DesiredCategoryData()
                {
                    message = msisdnResp.data_message,
                    isDesiredCategory = msisdnResp.isDesiredCategory,
                    category = msisdnResp.category_name
                };
                raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                return raRespModel;
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
        public async Task<RACommonResponseRevampV3> ValidateUnpairedMSISDNSTartTrekV4(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevampV3 raRespModel = new RACommonResponseRevampV3();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = UnpairedMSISDNReqParsing(dbssResp, msisdnCheckReqest.retailer_id);

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


                var simResp = await StarTrekCheckSIMNumber(new SIMNumberCheckRequest()
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
        
        public UnpairedMSISDNStartrekCheckResponse UnpairedMSISDNReqParsing(JObject dbssRespObj, string retailer_id)
        {
            UnpairedMSISDNStartrekCheckResponse raResp = new UnpairedMSISDNStartrekCheckResponse();
            try
            {
                string status = String.Empty;
                string reserved_for = String.Empty;
                int stockId = 0;
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (!String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                            && !String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["stock"]))
                        {
                            status = (string)dbssRespObj["data"]["attributes"]["status"];
                            stockId = (int)dbssRespObj["data"]["attributes"]["stock"];
                            reserved_for = (string)dbssRespObj["data"]["attributes"]["reserved-for"];
                        }
                    }
                }
                if (stockId != 33)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StockIDMismatch;
                    return raResp;
                }
                if (!String.IsNullOrEmpty(reserved_for))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StarTrekNotEligible;
                    return raResp;
                }
                if (String.IsNullOrEmpty(reserved_for) && status == "available")
                {
                    raResp = ValidateCherishedNumer(dbssRespObj, retailer_id);
                    raResp.stock_id = stockId;
                    raResp.reservation_id = reserved_for;
                    return raResp;
                }
                else if (status == "in_use")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInUse;
                    return raResp;
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "MSISDN is invalid.";
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public UnpairedMSISDNStartrekCheckResponse UnpairedMSISDNReqParsingV2(JObject dbssRespObj, string retailer_id)
        {
            UnpairedMSISDNStartrekCheckResponse raResp = new UnpairedMSISDNStartrekCheckResponse();
            try
            {
                string status = String.Empty;
                string reserved_for = String.Empty;
                int stockId = 0;
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (!String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                            && !String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["stock"]))
                        {
                            status = (string)dbssRespObj["data"]["attributes"]["status"];
                            stockId = (int)dbssRespObj["data"]["attributes"]["stock"];
                            reserved_for = (string)dbssRespObj["data"]["attributes"]["reserved-for"];
                        }
                    }
                }
                if (stockId != 33)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StockIDMismatch;
                    return raResp;
                }
                if (!String.IsNullOrEmpty(reserved_for))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StarTrekNotEligible;
                    return raResp;
                }
                if (String.IsNullOrEmpty(reserved_for) && status == "available")
                {
                    raResp = ValidateCherishedNumer(dbssRespObj, retailer_id);
                    raResp.stock_id = stockId;
                    raResp.reservation_id = reserved_for;
                    return raResp;
                }
                else if (status == "in_use")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInUse;
                    return raResp;
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "MSISDN is invalid.";
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public UnpairedMSISDNStartrekCheckResponse UnpairedMSISDNReqParsingV4(JObject dbssRespObj, string retailer_id, string selectedCategory)
        {
            UnpairedMSISDNStartrekCheckResponse raResp = new UnpairedMSISDNStartrekCheckResponse();
            try
            {
                string status = String.Empty;
                string reserved_for = String.Empty;
                int stockId = 0;
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (!String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                            && !String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["stock"]))
                        {
                            status = (string)dbssRespObj["data"]["attributes"]["status"];
                            stockId = (int)dbssRespObj["data"]["attributes"]["stock"];
                            reserved_for = (string)dbssRespObj["data"]["attributes"]["reserved-for"];
                            number_category = (string)dbssRespObj["data"]["attributes"]["number-category"];
                        }
                    }
                }
                if (selectedCategory.ToLower() != number_category.ToLower())
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.CherishCategoryMismatch;
                    return raResp;
                }
                if (stockId == 33)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StockIDMismatch;
                    return raResp;
                }
                if (!String.IsNullOrEmpty(reserved_for))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StarTrekNotEligible;
                    return raResp;
                }
                if (String.IsNullOrEmpty(reserved_for) && status == "available")
                {
                    raResp.result = true;
                    raResp.stock_id = stockId;
                    raResp.reservation_id = reserved_for;
                    return raResp;
                }
                else if (status == "in_use")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInUse;
                    return raResp;
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "MSISDN is invalid.";
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<UnpairedMSISDNStartrekCheckResponseV2> UnpairedMSISDNReqParsingV3(JObject dbssRespObj, string retailer_id, string channel_name)
        {
            UnpairedMSISDNStartrekCheckResponseV2 raResp = new UnpairedMSISDNStartrekCheckResponseV2();
            try
            {
                string status = String.Empty;
                string reserved_for = String.Empty;
                int stockId = 0;
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;
                string cherish_category_config = string.Empty;

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (!String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                            && !String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["stock"]))
                        {
                            status = (string)dbssRespObj["data"]["attributes"]["status"];
                            stockId = (int)dbssRespObj["data"]["attributes"]["stock"];
                            reserved_for = (string)dbssRespObj["data"]["attributes"]["reserved-for"];
                            number_category = (string)dbssRespObj["data"]["attributes"]["number-category"];
                        }
                    }
                }
                if (stockId == 33)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StockIDMismatch;
                    return raResp;
                }
                if (!String.IsNullOrEmpty(reserved_for))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StarTrekNotEligible;
                    return raResp;
                }
                if (String.IsNullOrEmpty(reserved_for) && status == "available")
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
                    cherish_category_config = configuration.GetSection("AppSettings:cherish_categories").Value;
                    if (cherish_category_config.Contains(","))
                    {
                        cofigValue = cherish_category_config.Split(',');
                    }
                    else
                    {
                        cofigValue = cherish_category_config.Split(' ');
                    }

                    if (cofigValue.Any(x => x == number_category))
                    {
                        var category = cofigValue.Where(x => x.Equals(number_category)).FirstOrDefault();
                        if (category != null)
                        {
                            var catInfo = await _bllCommon.GetDesiredCategoryMessage(category, channel_name);
                            if (catInfo != null)
                            {
                                raResp.data_message = catInfo.message;
                                raResp.message = MessageCollection.MSISDNValid;
                                raResp.category_name = catInfo.name;
                                raResp.isDesiredCategory = true;
                                raResp.result = true;
                            }
                            else
                            {
                                raResp.data_message = "No amount is configured for " + category + " category";
                                raResp.category_name = category;
                                raResp.isDesiredCategory = false;
                                raResp.result = false;
                                raResp.message = "No amount is configured for " + category + " category";
                            }
                        }
                        
                    }
                    else
                    {
                        raResp = ValidateCherishedNumerV2(dbssRespObj, retailer_id);
                    }
                    raResp.stock_id = stockId;
                    raResp.reservation_id = reserved_for;
                    return raResp;
                }
                else if (status == "in_use")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInUse;
                    return raResp;
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "MSISDN is invalid.";
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<RACommonResponseRevampResp2> ValidateUnpairedMSISDNSTartTrekTestOnline(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevampResp2 raRespModel = new RACommonResponseRevampResp2();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
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

                var msisdnResp = UnpairedMSISDNResPargingOnline(dbssResp, msisdnCheckReqest.retailer_id);

                if (msisdnResp.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = msisdnResp.message;
                    return raRespModel;
                }
                raRespModel.reservationId = msisdnResp.reservation_id;

                return raRespModel;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public UnpairedMSISDNStartrekCheckResponse UnpairedMSISDNResPargingOnline(JObject dbssRespObj, string retailer_id)
        {
            UnpairedMSISDNStartrekCheckResponse raResp = new UnpairedMSISDNStartrekCheckResponse();
            try
            {
                string status = String.Empty;
                string reserved_for = String.Empty;
                int stockId = 0;
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (!String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                            && !String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["stock"]))
                        {
                            status = (string)dbssRespObj["data"]["attributes"]["status"];
                            stockId = (int)dbssRespObj["data"]["attributes"]["stock"];
                            reserved_for = (string)dbssRespObj["data"]["attributes"]["reserved-for"];
                        }
                    }
                }
                if (stockId != 33)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StockIDMismatch;
                    return raResp;
                }
                if (String.IsNullOrEmpty(reserved_for))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StarTrekNotEligibleOnline;
                    return raResp;
                }
                if (!String.IsNullOrEmpty(reserved_for) && status == "available")
                {
                    raResp = ValidateCherishedNumer(dbssRespObj, retailer_id);
                    raResp.stock_id = stockId;
                    raResp.reservation_id = reserved_for;
                    return raResp;
                }
                else if (status == "in_use")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInUse;
                    return raResp;
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "MSISDN is invalid.";
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public UnpairedMSISDNStartrekCheckResponse ValidateCherishedNumer(JObject dbssRespObj, string retailer_id)
        {

            UnpairedMSISDNStartrekCheckResponse raResp = new UnpairedMSISDNStartrekCheckResponse();

            string status = String.Empty;
            int stockId = 0;
            string retailer_code = String.Empty;
            string number_category = String.Empty;
            string category_config = String.Empty;
            string[] cofigValue = null;

            try
            {
                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        try
                        {
                            category_config = _configuration.GetSection("AppSettings:number_category").Value;

                        }
                        catch (Exception) { throw new Exception("Key not found in appsettings"); }

                        if (category_config.Contains(","))
                        {
                            cofigValue = category_config.Split(',');
                        }
                        else
                        {
                            cofigValue = category_config.Split(' ');
                        }

                        if (dbssRespObj["data"]["attributes"]["number-category"] != null)
                        {
                            retailer_code = dbssRespObj["data"]["attributes"]["salesman-id"].ToString();
                            number_category = dbssRespObj["data"]["attributes"]["number-category"].ToString();

                            if (!String.IsNullOrEmpty(retailer_code))
                            {
                                if (retailer_code.Length < 6)
                                {
                                    char pad = '0';
                                    retailer_code = retailer_code.PadLeft(6, pad);
                                }
                            }

                            if (!String.IsNullOrEmpty(retailer_code) && !String.IsNullOrEmpty(number_category) && cofigValue.Any(x => x != number_category)) // from Web.config 
                            {
                                if (retailer_id.Equals(retailer_code))
                                {
                                    raResp.result = true;
                                    raResp.message = MessageCollection.ValidCherishedNumber;
                                }
                                else
                                {
                                    raResp.result = false;
                                    raResp.message = MessageCollection.InvalidCherishedNumber;
                                }
                            }
                            else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                            {
                                raResp.result = true;
                                raResp.message = MessageCollection.ValidCherishedNumber;
                            }
                            else if (!String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                            {
                                raResp.result = true;
                                raResp.message = MessageCollection.ValidCherishedNumber; ;
                            }
                            else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x != number_category))
                            {
                                raResp.result = false;
                                raResp.message = "MSISDN not tagged with this Retailer (ID: " + retailer_id + ")";
                            }
                            else
                            {
                                raResp.result = false;
                                raResp.message = "MSISDN is not Valid.";
                            }
                        }
                        else
                        {
                            raResp.result = false;
                            raResp.message = "Invalid MSISDN Category!";
                        }
                    }
                    else
                    {
                        raResp.result = false;
                        raResp.message = "No Data found!";
                    }
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "No Data found!";
                }

                return raResp;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public UnpairedMSISDNStartrekCheckResponseV2 ValidateCherishedNumerV2(JObject dbssRespObj, string retailer_id)
        {

            UnpairedMSISDNStartrekCheckResponseV2 raResp = new UnpairedMSISDNStartrekCheckResponseV2();

            string status = String.Empty;
            int stockId = 0;
            string retailer_code = String.Empty;
            string number_category = String.Empty;
            string category_config = String.Empty;
            string[] cofigValue = null;

            try
            {
                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        try
                        {
                            category_config = _configuration.GetSection("AppSettings:number_category").Value;

                        }
                        catch (Exception) { throw new Exception("Key not found in appsettings"); }

                        if (category_config.Contains(","))
                        {
                            cofigValue = category_config.Split(',');
                        }
                        else
                        {
                            cofigValue = category_config.Split(' ');
                        }

                        if (dbssRespObj["data"]["attributes"]["number-category"] != null)
                        {
                            retailer_code = dbssRespObj["data"]["attributes"]["salesman-id"].ToString();
                            number_category = dbssRespObj["data"]["attributes"]["number-category"].ToString();

                            if (!String.IsNullOrEmpty(retailer_code))
                            {
                                if (retailer_code.Length < 6)
                                {
                                    char pad = '0';
                                    retailer_code = retailer_code.PadLeft(6, pad);
                                }
                            }

                            if (!String.IsNullOrEmpty(retailer_code) && !String.IsNullOrEmpty(number_category) && cofigValue.Any(x => x != number_category)) // from Web.config 
                            {
                                if (retailer_id.Equals(retailer_code))
                                {
                                    raResp.result = true;
                                    raResp.message = MessageCollection.ValidCherishedNumber;
                                }
                                else
                                {
                                    raResp.result = false;
                                    raResp.message = MessageCollection.InvalidCherishedNumber;
                                }
                            }
                            else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                            {
                                raResp.result = true;
                                raResp.message = MessageCollection.ValidCherishedNumber;
                            }
                            else if (!String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                            {
                                raResp.result = true;
                                raResp.message = MessageCollection.ValidCherishedNumber; ;
                            }
                            else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x != number_category))
                            {
                                raResp.result = false;
                                raResp.message = "MSISDN not tagged with this Retailer (ID: " + retailer_id + ")";
                            }
                            else
                            {
                                raResp.result = false;
                                raResp.message = "MSISDN is not Valid.";
                            }
                        }
                        else
                        {
                            raResp.result = false;
                            raResp.message = "Invalid MSISDN Category!";
                        }
                    }
                    else
                    {
                        raResp.result = false;
                        raResp.message = "No Data found!";
                    }
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "No Data found!";
                }

                return raResp;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<RACommonResponse> StarTrekCheckSIMNumber(SIMNumberCheckRequest simNumberCheckReqest, int purposeOfSIMCheck, bool? isPaired, int? simCategory, string old_sim_type)
        {
            RACommonResponse raResp = new RACommonResponse();
            string apiUrl = "", txtResp = "";
            SIMValidationRequestRootobject dbssReqModel = null;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
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
                raResp = SIMValidationParsing(dbssResp, purposeOfSIMCheck, simCategory == null ? null : simCategory, isPaired, old_sim_type);
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
                log.method_name = "StarTrekCheckSIMNumber";
                log.msisdn = _bllLog.FormatMSISDN(simNumberCheckReqest.msisdn);

                await _bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp);
            }
            return raResp;
        }
        public async Task<RACommonResponse> StarTrekCheckSIMNumberV2(SIMNumberCheckRequest simNumberCheckReqest, int purposeOfSIMCheck, bool? isPaired, int? simCategory, string old_sim_type)
        {
            RACommonResponse raResp = new RACommonResponse();
            string apiUrl = "", txtResp = "";
            SIMValidationRequestRootobject dbssReqModel = null;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
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
                raResp = SIMValidationParsingV2(dbssResp, purposeOfSIMCheck, simCategory == null ? null : simCategory, isPaired, old_sim_type);
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
                log.method_name = "StarTrekCheckSIMNumberV2";
                log.msisdn = _bllLog.FormatMSISDN(simNumberCheckReqest.msisdn);

                await _bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp);
            }
            return raResp;
        }
        public RACommonResponse SIMValidationParsing(JObject dbssResp, int purposeOfSIMCheck, int? simCategory, bool? isPired, string oldSimType)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (dbssResp?["data"]?["status"] == null
                    && dbssResp?["data"]?["logical_inventory_status"] == null
                    && dbssResp?["data"]?["physical_inventory_status"] == null
                    && String.IsNullOrEmpty(dbssResp?["data"]?["status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["logical_inventory_status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["physical_inventory_status"]?.ToString()))
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePostpaid.ToLower()
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaid.ToLower()) /*ryz-esim*/
                {
                    response.result = false;
                    response.message = "Incorrect Product!";
                    return response;
                }
                else if (dbssResp?["data"]?["status"]?.ToString().ToLower() == "failed")
                {
                    response.result = false;

                    response.message = MessageCollection.SIMIsNotInInventory;
                    if (dbssResp != null && dbssResp.ContainsKey("data") && dbssResp["data"] != null && (dbssResp["data"] is JObject dataObj && dataObj.ContainsKey("error_message")))
                    {
                        var errorMessage = dbssResp["data"]["error_message"]?.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            response.message = errorMessage;
                        }
                    }

                    return response;
                }
                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == "used")
                {
                    response.result = false;
                    response.message = MessageCollection.SIMIsUsed;
                    return response;
                }

                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection)
                {
                    if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower()/*"unpaired"*/
                        && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaidStarTrek.ToLower() /*"ryz-prepaid"*/)
                    {
                        response.result = true;
                        response.message = MessageCollection.SIMValid;
                        return response;
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMInvalid;
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return response;
        }
        public RACommonResponse SIMValidationParsingV2(JObject dbssResp, int purposeOfSIMCheck, int? simCategory, bool? isPired, string oldSimType)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (dbssResp?["data"]?["status"] == null
                    && dbssResp?["data"]?["logical_inventory_status"] == null
                    && dbssResp?["data"]?["physical_inventory_status"] == null
                    && String.IsNullOrEmpty(dbssResp?["data"]?["status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["logical_inventory_status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["physical_inventory_status"]?.ToString()))
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESimStarTrek.ToLower() /*ryz-esim*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESim.ToLower() /*E-SIM*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeE_SIM_SWAP.ToLower() /*E_SIM_SWAP*/)
                {
                    response.result = false;
                    response.message = "This is not physical SIM!";
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePostpaid.ToLower()
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PhycalInventorySIMTypeSIM_SWAP.ToLower()) /*SIM_SWAP*/
                {
                    response.result = false;
                    response.message = "Incorrect Product!";
                    return response;
                }
                else if (dbssResp?["data"]?["status"]?.ToString().ToLower() == "failed")
                {
                    response.result = false;

                    response.message = MessageCollection.SIMIsNotInInventory;
                    if (dbssResp != null && dbssResp.ContainsKey("data") && dbssResp["data"] != null && (dbssResp["data"] is JObject dataObj && dataObj.ContainsKey("error_message")))
                    {
                        var errorMessage = dbssResp["data"]["error_message"]?.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            response.message = errorMessage;
                        }
                    }

                    return response;
                }
                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == "used")
                {
                    response.result = false;
                    response.message = MessageCollection.SIMIsUsed;
                    return response;
                }

                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection)
                {
                    if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower()/*"unpaired"*/
                        && (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaid.ToLower() /*"prepaid"*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaidStarTrek.ToLower() /*ryz-prepaid*/))
                    {
                        response.result = true;
                        response.message = MessageCollection.SIMValid;
                        return response;
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMInvalid;
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return response;
        }
        public RACommonResponse SIMValidationParsingESIM(JObject dbssResp, int purposeOfSIMCheck, int? simCategory, bool? isPired, string oldSimType)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (dbssResp?["data"]?["status"] == null
                    && dbssResp?["data"]?["logical_inventory_status"] == null
                    && dbssResp?["data"]?["physical_inventory_status"] == null
                    && String.IsNullOrEmpty(dbssResp?["data"]?["status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["logical_inventory_status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["physical_inventory_status"]?.ToString()))
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaidStarTrek.ToLower()) /*ryz-prepaid*/
                {
                    response.result = false;
                    response.message = "Incorrect Product!";
                    return response;
                }
                else if (dbssResp?["data"]?["status"]?.ToString().ToLower() == "failed")
                {
                    response.result = false;
                    
                    response.message = MessageCollection.SIMIsNotInInventory;
                    if (dbssResp != null && dbssResp.ContainsKey("data") && dbssResp["data"] != null && (dbssResp["data"] is JObject dataObj && dataObj.ContainsKey("error_message")))
                    {
                        var errorMessage = dbssResp["data"]["error_message"]?.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            response.message = errorMessage;
                        }
                    }

                    return response;
                }
                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == "used")
                {
                    response.result = false;
                    response.message = MessageCollection.SIMIsUsed;
                    return response;
                }
                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower()/*"unpaired"*/
                    && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESimStarTrek.ToLower() /*"ryz-esim"*/)
                {
                    response.result = true;
                    response.message = MessageCollection.SIMValid;
                    return response;
                }
                else
                {
                    response.result = false;
                    response.message = MessageCollection.SIMInvalid;
                    return response;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return response;
        }
        public RACommonResponse SIMValidationParsingESIMV2(JObject dbssResp, int purposeOfSIMCheck, int? simCategory, bool? isPired, string oldSimType)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (dbssResp?["data"]?["status"] == null
                    && dbssResp?["data"]?["logical_inventory_status"] == null
                    && dbssResp?["data"]?["physical_inventory_status"] == null
                    && String.IsNullOrEmpty(dbssResp?["data"]?["status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["logical_inventory_status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["physical_inventory_status"]?.ToString()))
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePostpaid.ToLower() /*postpaid*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaidStarTrek.ToLower() /*ryz-prepaid*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaid.ToLower() /*Prepaid*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PhycalInventorySIMTypeSIM_SWAP.ToLower()) /*SIM_SWAP*/
                {
                    response.result = false;
                    response.message = "This is not eSIM!";
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESimStarTrek.ToLower() /*ryz-esim*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeE_SIM_SWAP.ToLower() /*E_SIM_SWAP*/)
                {
                    response.result = false;
                    response.message = "Incorrect Product!";
                    return response;
                }
                else if (dbssResp?["data"]?["status"]?.ToString().ToLower() == "failed")
                {
                    response.result = false;

                    response.message = MessageCollection.SIMIsNotInInventory;
                    if (dbssResp != null && dbssResp.ContainsKey("data") && dbssResp["data"] != null && (dbssResp["data"] is JObject dataObj && dataObj.ContainsKey("error_message")))
                    {
                        var errorMessage = dbssResp["data"]["error_message"]?.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            response.message = errorMessage;
                        }
                    }

                    return response;
                }
                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == "used")
                {
                    response.result = false;
                    response.message = MessageCollection.SIMIsUsed;
                    return response;
                }
                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower()/*"unpaired"*/
                    && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESim.ToLower() /*"esim"*/)
                {
                    response.result = true;
                    response.message = MessageCollection.SIMValid;
                    return response;
                }
                else
                {
                    response.result = false;
                    response.message = MessageCollection.SIMInvalid;
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
        public RACommonResponse SIMValidationParsingESIMV3(JObject dbssResp, int purposeOfSIMCheck, int? simCategory, bool? isPired, string oldSimType)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (dbssResp?["data"]?["status"] == null
                    && dbssResp?["data"]?["logical_inventory_status"] == null
                    && dbssResp?["data"]?["physical_inventory_status"] == null
                    && String.IsNullOrEmpty(dbssResp?["data"]?["status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["logical_inventory_status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["physical_inventory_status"]?.ToString()))
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaidStarTrek.ToLower()/*ryz-prepaid*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PhycalInventorySIMTypeSIM_SWAP.ToLower() /*sim_swap*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaid.ToLower()/*Prepaid*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePostpaid.ToLower()/*Postpaid*/)
                {
                    response.result = false;
                    response.message = "This is not eSIM!";
                    return response;
                }

                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESimStarTrek.ToLower() /*ryze_esim*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESim.ToLower()/*e-sim*/)
                {
                    response.result = false;
                    response.message = "Incorrect Product!";
                    return response;
                }
                else if (dbssResp?["data"]?["status"]?.ToString().ToLower() == "failed")
                {
                    response.result = false;

                    response.message = MessageCollection.SIMIsNotInInventory;
                    if (dbssResp != null && dbssResp.ContainsKey("data") && dbssResp["data"] != null && (dbssResp["data"] is JObject dataObj && dataObj.ContainsKey("error_message")))
                    {
                        var errorMessage = dbssResp["data"]["error_message"]?.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            response.message = errorMessage;
                        }
                    }

                    return response;
                }
                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == "used")
                {
                    response.result = false;
                    response.message = MessageCollection.SIMIsUsed;
                    return response;
                }
                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower()/*"unpaired"*/
                    && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeE_SIM_SWAP.ToLower() /*"e_sim_swap"*/)
                {
                    response.result = true;
                    response.message = MessageCollection.SIMValid;
                    return response;
                }
                else
                {
                    response.result = false;
                    response.message = MessageCollection.SIMInvalid;
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNESIM(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = UnpairedMSISDNReqParsing(dbssResp, msisdnCheckReqest.retailer_id);

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

                var simResp = await CheckSIMNumber(new SIMNumberCheckRequest()
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
                log.method_name = "StartTrekValidateUnpairedMSISDN_ESIM";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNESIMV2(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = UnpairedMSISDNReqParsingV2(dbssResp, msisdnCheckReqest.retailer_id);

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

                var simResp = await CheckSIMNumberV2(new SIMNumberCheckRequest()
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
                log.method_name = "StartTrekValidateUnpairedMSISDNESIMV2";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNESIMV3(CherishMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = UnpairedMSISDNReqParsingV4(dbssResp, msisdnCheckReqest.retailer_id, msisdnCheckReqest.selected_category);

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

                var simResp = await CheckSIMNumberV2(new SIMNumberCheckRequest()
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
                log.method_name = "StartTrekValidateUnpairedMSISDNESIMV2";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        public async Task<RACommonResponse> CheckSIMNumber(SIMNumberCheckRequest simNumberCheckReqest, int purposeOfSIMCheck, bool? isPaired, int? simCategory, string old_sim_type)
        {
            RACommonResponse raResp = new RACommonResponse();
            string apiUrl = "", txtResp = "";
            SIMValidationRequestRootobject dbssReqModel = null;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
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

                raResp = SIMValidationParsingESIM(dbssResp, purposeOfSIMCheck, simCategory == null ? null : simCategory, isPaired, old_sim_type);
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
                log.method_name = "CheckSIMNumberStarTrek";
                log.msisdn = _bllLog.FormatMSISDN(simNumberCheckReqest.msisdn);

                await _bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp);
            }
            return raResp;
        }
        public async Task<RACommonResponse> CheckSIMNumberV2(SIMNumberCheckRequest simNumberCheckReqest, int purposeOfSIMCheck, bool? isPaired, int? simCategory, string old_sim_type)
        {
            RACommonResponse raResp = new RACommonResponse();
            string apiUrl = "", txtResp = "";
            SIMValidationRequestRootobject dbssReqModel = null;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
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

                raResp = SIMValidationParsingESIMV2(dbssResp, purposeOfSIMCheck, simCategory == null ? null : simCategory, isPaired, old_sim_type);
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
                log.method_name = "CheckSIMNumberStarTrekV2";
                log.msisdn = _bllLog.FormatMSISDN(simNumberCheckReqest.msisdn);

                await _bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp);
            }
            return raResp;
        }
        public async Task<RACommonResponse> CheckSIMNumberV3(SIMNumberCheckRequest simNumberCheckReqest, int purposeOfSIMCheck, bool? isPaired, int? simCategory, string old_sim_type)
        {
            RACommonResponse raResp = new RACommonResponse();
            string apiUrl = "", txtResp = "";
            SIMValidationRequestRootobject dbssReqModel = null;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();
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

                raResp = SIMValidationParsingESIMV3(dbssResp, purposeOfSIMCheck, simCategory == null ? null : simCategory, isPaired, old_sim_type);
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
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : error.error_custom_msg;
                }
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = simNumberCheckReqest.purpose_number;
                log.user_id = simNumberCheckReqest.retailer_id;
                log.method_name = "CheckSIMNumberStarTrekV3";
                log.msisdn = _bllLog.FormatMSISDN(simNumberCheckReqest.msisdn);

                await _bllLog.RAToDBSSLog(log, apiUrl + "//Request Body: " + Convert.ToString(dbssReqModel), txtResp);
            }
            return raResp;
        }
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNESIM_Online(UnpairedMSISDNCheckRequestOnline msisdnCheckReqest, string reservation_id, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = UnpairedMSISDNResPargingOnline(dbssResp, msisdnCheckReqest.retailer_id);

                if (msisdnResp.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = msisdnResp.message;
                    return raRespModel;
                }
                else
                {
                    if (!msisdnResp.reservation_id.Equals(reservation_id))
                    {
                        raRespModel.isError = true;
                        raRespModel.message = "The reservation id is not matched with DBSS!";
                        return raRespModel;
                    }
                }

                var stockCheck = await _bllCommon.IsStockAvailable(msisdnResp.stock_id, Convert.ToInt32(msisdnCheckReqest.channel_id));

                if (stockCheck == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.StockIDMismatch;
                    return raRespModel;
                }

                var simResp = await CheckSIMNumber(new SIMNumberCheckRequest()
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
                raRespModel.data.reservation_id = msisdnResp.reservation_id;
                raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                return raRespModel;
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
                log.method_name = "StartTrekValidateUnpairedMSISDN_ESIM";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNMNPSTartTrek(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            JObject dbssResp = new JObject();
            BL_Json _blJson = new BL_Json();
            ApiRequest _apiReq = new ApiRequest();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            try
            {
                var dbssReqModel = _raToDBssParse.ValidateMSISDNReqParsing(msisdnCheckReqest);

                if (dbssReqModel.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    dbssReqModel = FixedValueCollection.MSISDNCountryCode + dbssReqModel;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, dbssReqModel);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

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
                            var simResp = await StarTrekCheckSIMNumber(new SIMNumberCheckRequest()
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

                var msisdnResp2 = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);

                if (msisdnResp2.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.MSISDNAlreadyExists;
                    return raRespModel;
                }

                var simResp2 = await StarTrekCheckSIMNumber(new SIMNumberCheckRequest()
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
                JObject jsonObject = JObject.Parse(ex.Message);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(ex.Message);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                string statusValue = jsonObject?["errors"]?["status"]?.ToString();
                string title = jsonObject?["errors"]?["title"]?.ToString();

                if (!String.IsNullOrEmpty(statusValue) && (statusValue == "7001" || title == "Msisdn Not Found"))
                {
                    var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);
                    log.is_success = 1;
                    var simResp = await StarTrekCheckSIMNumber(new SIMNumberCheckRequest()
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
                else
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
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;//userName
                log.method_name = "ValidateUnpairedMSISDNMNPSTartTrek";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNMNPSTartTrekV2(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            JObject dbssResp = new JObject();
            BL_Json _blJson = new BL_Json();
            ApiRequest _apiReq = new ApiRequest();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            try
            {
                var dbssReqModel = _raToDBssParse.ValidateMSISDNReqParsing(msisdnCheckReqest);

                if (dbssReqModel.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    dbssReqModel = FixedValueCollection.MSISDNCountryCode + dbssReqModel;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, dbssReqModel);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

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
                            log.is_success = 1;
                            var simResp = await StarTrekCheckSIMNumberV2(new SIMNumberCheckRequest()
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

                var msisdnResp2 = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);

                if (msisdnResp2.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.MSISDNAlreadyExists;
                    return raRespModel;
                }

                var simResp2 = await StarTrekCheckSIMNumberV2(new SIMNumberCheckRequest()
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
                JObject jsonObject = JObject.Parse(ex.Message);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(ex.Message);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                string statusValue = jsonObject?["errors"]?["status"]?.ToString();
                string title = jsonObject?["errors"]?["title"]?.ToString();

                if (!String.IsNullOrEmpty(statusValue) && (statusValue == "7001" || title == "Msisdn Not Found"))
                {
                    var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);
                    log.is_success = 1;
                    var simResp = await StarTrekCheckSIMNumberV2(new SIMNumberCheckRequest()
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
                else
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
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;//userName
                log.method_name = "ValidateUnpairedMSISDNMNPSTartTrekV2";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNMNPSTartTrekesim(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            JObject dbssResp = new JObject();
            BL_Json _blJson = new BL_Json();
            ApiRequest _apiReq = new ApiRequest();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            try
            {
                var dbssReqModel = _raToDBssParse.ValidateMSISDNReqParsing(msisdnCheckReqest);

                if (dbssReqModel.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    dbssReqModel = FixedValueCollection.MSISDNCountryCode + dbssReqModel;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, dbssReqModel);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

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
                            var simResp = await CheckSIMNumber(new SIMNumberCheckRequest()
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

                var msisdnResp2 = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);

                if (msisdnResp2.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.MSISDNAlreadyExists;
                    return raRespModel;
                }

                var simResp2 = await CheckSIMNumber(new SIMNumberCheckRequest()
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
                JObject jsonObject = JObject.Parse(ex.Message);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(ex.Message);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                string statusValue = jsonObject?["errors"]?["status"]?.ToString();
                string title = jsonObject?["errors"]?["title"]?.ToString();

                if (!String.IsNullOrEmpty(statusValue) && (statusValue == "7001" || title == "Msisdn Not Found"))
                {
                    var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);
                    log.is_success = 1;
                    var simResp = await CheckSIMNumber(new SIMNumberCheckRequest()
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
                else
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
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;//userName
                log.method_name = "ValidateUnpairedMSISDNMNPSTartTrekesim";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);

            }
        }
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNMNPSTartTrekesimV2(UnpairedMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            JObject dbssResp = new JObject();
            BL_Json _blJson = new BL_Json();
            ApiRequest _apiReq = new ApiRequest();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            try
            {
                var dbssReqModel = _raToDBssParse.ValidateMSISDNReqParsing(msisdnCheckReqest);

                if (dbssReqModel.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    dbssReqModel = FixedValueCollection.MSISDNCountryCode + dbssReqModel;
                }

                apiUrl = String.Format(GetAPICollection.UnpairedMSISDNValidation, dbssReqModel);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

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
                            log.is_success = 1;
                            var simResp = await CheckSIMNumberV2(new SIMNumberCheckRequest()
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

                var msisdnResp2 = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);

                if (msisdnResp2.result == false)
                {
                    raRespModel.isError = true;
                    raRespModel.message = MessageCollection.MSISDNAlreadyExists;
                    return raRespModel;
                }

                var simResp2 = await CheckSIMNumberV2(new SIMNumberCheckRequest()
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
                JObject jsonObject = JObject.Parse(ex.Message);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(ex.Message);
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                string statusValue = jsonObject?["errors"]?["status"]?.ToString();
                string title = jsonObject?["errors"]?["title"]?.ToString();

                if (!String.IsNullOrEmpty(statusValue) && (statusValue == "7001" || title == "Msisdn Not Found"))
                {
                    var msisdnResp = _dbssToRaParse.UnpairedMSISDNReqParsingForMNPProtIn(dbssResp);
                    log.is_success = 1;
                    var simResp = await CheckSIMNumberV2(new SIMNumberCheckRequest()
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
                else
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
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;//userName
                log.method_name = "ValidateUnpairedMSISDNMNPSTartTrekesimV2";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNSTartTrekOnlineV2(UnpairedMSISDNCheckRequestOnline msisdnCheckReqest, string reservation_id, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = UnpairedMSISDNResPargingOnline(dbssResp, msisdnCheckReqest.retailer_id);

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


                var simResp = await StarTrekCheckSIMNumber(new SIMNumberCheckRequest()
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

                Datas datas = new Datas();
                datas.isEsim = 0;
                datas.request_id = "Test";
                datas.reservation_id = reservation_id;

                raRespModel.isError = false;
                raRespModel.data = new Datas()
                {
                    reservation_id = reservation_id
                };
                raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                return raRespModel;
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
        public async Task<RACommonResponseRevamp> ValidateUnpairedMSISDNESIM_OnlineV2(UnpairedMSISDNCheckRequestOnline msisdnCheckReqest, string reservation_id, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = UnpairedMSISDNResPargingOnline(dbssResp, msisdnCheckReqest.retailer_id);

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

                var simResp = await CheckSIMNumber(new SIMNumberCheckRequest()
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
                raRespModel.data.reservation_id = msisdnResp.reservation_id;
                raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                return raRespModel;
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
                log.method_name = "ValidateUnpairedMSISDNESIM_OnlineV2";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        #region Cherish Number Sell
        [HttpPost]
        [Route("validate-msisdn-cherish")]
        public async Task<IActionResult> ValidateChrishMSISDN([FromBody] CherishMSISDNCheckRequest msisdnCheckReqest)
        {
            try
            {
                RACommonResponseRevamp response = new RACommonResponseRevamp();

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

                if (SettingsValues.GetRyzeAllowOrNot() == 1)
                {
                    response = await ValidateMSISDNSTartTrekCherish(msisdnCheckReqest, "ValidateMSISDNSTartTrekCherish");
                }
                else
                {
                    response = await ValidateMSISDNSTartTrekCherishV2(msisdnCheckReqest, "ValidateMSISDNSTartTrekCherishV2");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        isEsim = 0,
                        request_id = "0"
                    }

                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("Validate-msisdn-cherish-esim")]
        public async Task<IActionResult> ValidateChrishMSISDN_ESIM([FromBody] CherishMSISDNCheckRequest msisdnCheckReqest)
        {
            RACommonResponseRevamp rACommonResponse = new RACommonResponseRevamp();
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

                if (SettingsValues.GetRyzeAllowOrNot() == 1)
                {
                    rACommonResponse = await ValidateCherishMSISDNESIM(msisdnCheckReqest, "ValidateUnpairedMSISDNESIM");
                }
                else
                {
                    rACommonResponse = await ValidateUnpairedMSISDNESIMV3(msisdnCheckReqest, "ValidateUnpairedMSISDNESIMV2");
                }

                return Ok(rACommonResponse);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message,
                    data = new Datas()
                    {
                        request_id = "0",
                        isEsim = 1
                    }
                });
            }
        }

        public async Task<RACommonResponseRevamp> ValidateMSISDNSTartTrekCherish(CherishMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = CherishMSISDNReqParsing(dbssResp, msisdnCheckReqest.retailer_id,msisdnCheckReqest.selected_category);

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


                var simResp = await StarTrekCheckSIMNumber(new SIMNumberCheckRequest()
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

        public async Task<RACommonResponseRevamp> ValidateMSISDNSTartTrekCherishV2(CherishMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = CherishMSISDNReqParsingV2(dbssResp, msisdnCheckReqest.retailer_id,msisdnCheckReqest.selected_category);

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

                var simResp = await StarTrekCheckSIMNumberV2(new SIMNumberCheckRequest()
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
        public async Task<RACommonResponseRevamp> ValidateCherishMSISDNESIM(CherishMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = CherishMSISDNReqParsing(dbssResp, msisdnCheckReqest.retailer_id,msisdnCheckReqest.selected_category);

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

                var simResp = await CheckSIMNumber(new SIMNumberCheckRequest()
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
                log.method_name = "ValidateCherishMSISDNESIM";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public async Task<RACommonResponseRevamp> ValidateCherishMSISDNESIMV2(CherishMSISDNCheckRequest msisdnCheckReqest, string apiName)
        {
            RACommonResponseRevamp raRespModel = new RACommonResponseRevamp();
            JObject dbssResp = null;
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse _raToDBssParse = new BLLRAToDBSSParse();
            ApiRequest _apiReq = new ApiRequest();
            BL_Json _blJson = new BL_Json();

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

                var msisdnResp = CherishMSISDNReqParsingV2(dbssResp, msisdnCheckReqest.retailer_id, msisdnCheckReqest.selected_category);

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

                var simResp = await CheckSIMNumberV2(new SIMNumberCheckRequest()
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
                log.method_name = "ValidateCherishMSISDNESIMV2";

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        public UnpairedMSISDNStartrekCheckResponse CherishMSISDNReqParsing(JObject dbssRespObj, string retailer_id, string selectedCategory)
        {
            UnpairedMSISDNStartrekCheckResponse raResp = new UnpairedMSISDNStartrekCheckResponse();
            try
            {
                string status = String.Empty;
                string reserved_for = String.Empty;
                int stockId = 0;
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (!String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                            && !String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["stock"]))
                        {
                            status = (string)dbssRespObj["data"]["attributes"]["status"];
                            stockId = (int)dbssRespObj["data"]["attributes"]["stock"];
                            reserved_for = (string)dbssRespObj["data"]["attributes"]["reserved-for"];
                            number_category = (string)dbssRespObj["data"]["attributes"]["number-category"];
                        }
                    }
                }
                if (selectedCategory.ToLower() != number_category.ToLower())
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.CherishCategoryMismatch;
                    return raResp;
                }
                if (stockId != 33)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StockIDMismatch;
                    return raResp;
                }
                if (!String.IsNullOrEmpty(reserved_for))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StarTrekNotEligible;
                    return raResp;
                }
                if (String.IsNullOrEmpty(reserved_for) && status == "available")
                {
                    //raResp = ValidateCherishedNumer(dbssRespObj, retailer_id);
                    raResp.stock_id = stockId;
                    raResp.reservation_id = reserved_for;
                    return raResp;
                }
                else if (status == "in_use")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInUse;
                    return raResp;
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "MSISDN is invalid.";
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public UnpairedMSISDNStartrekCheckResponse CherishMSISDNReqParsingV2(JObject dbssRespObj, string retailer_id, string selectedCategory)
        {
            UnpairedMSISDNStartrekCheckResponse raResp = new UnpairedMSISDNStartrekCheckResponse();
            try
            {
                string status = String.Empty;
                string reserved_for = String.Empty;
                int stockId = 0;
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (!String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                            && !String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["stock"]))
                        {
                            status = (string)dbssRespObj["data"]["attributes"]["status"];
                            stockId = (int)dbssRespObj["data"]["attributes"]["stock"];
                            reserved_for = (string)dbssRespObj["data"]["attributes"]["reserved-for"];
                            number_category = (string)dbssRespObj["data"]["attributes"]["number-category"];
                        }
                    }
                }
                if (selectedCategory.ToLower() != number_category.ToLower())
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.CherishCategoryMismatch;
                    return raResp;
                }
                if (stockId == 33)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StockIDMismatch;
                    return raResp;
                }
                if (!String.IsNullOrEmpty(reserved_for))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StarTrekNotEligible;
                    return raResp;
                }
                if (String.IsNullOrEmpty(reserved_for) && status == "available")
                {
                    raResp.result = true;
                    raResp.stock_id = stockId;
                    raResp.reservation_id = reserved_for;
                    return raResp;
                }
                else if (status == "in_use")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInUse;
                    return raResp;
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "MSISDN is invalid.";
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion
    }
}
