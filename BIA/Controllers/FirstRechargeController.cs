using BIA.BLL.BLLServices;
using BIA.BLL.Utility;
using BIA.Common;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.JWET;
using BIA.JWT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Configuration;
using System.Net;
using System.Text;

namespace BIA.Controllers
{
    [Route("api/FirstRecharge")]
    [ApiController]
    public class FirstRechargeController : ControllerBase
    {
        private readonly BLLFirstRecharge _frManager;
        private readonly BaseController _bio;
        private readonly BLLLog _bllLog;
        private readonly BLLFTRRestriction _bLLFTR;
        private readonly FTR_Restrict _ftr_restrict;
        private readonly BL_Json _blJson;
        private readonly BLLCommon _bLLCommon;
        private readonly IConfiguration _configuration;

        public FirstRechargeController(BLLFirstRecharge frManager, BaseController bio, BLLLog bllLog, BLLFTRRestriction bLLFTR, FTR_Restrict ftr_restrict, BL_Json blJson, BLLCommon bLLCommon, IConfiguration configuration)
        {
            _frManager = frManager;
            _bio = bio;
            _bllLog = bllLog;
            _bLLFTR = bLLFTR;
            _ftr_restrict = ftr_restrict;
            _blJson = blJson;
            _bLLCommon = bLLCommon;
            _configuration = configuration;
        }         

        [HttpPost]
        [Route("SubmitFirstRecharge")]
        public async Task<IActionResult> SubmitFirstRecharge([FromBody] RechargeRequestModel model)
        {
            string apiUrl = RetailerAPI.RechargeAPI;
            ErrorDescription error = new ErrorDescription();
            RechargeResponseModel? apiResponse = new RechargeResponseModel();
            BL_Json _blJson = new BL_Json();
            BIAToDBSSLog log = new BIAToDBSSLog();
            RechargeResponseModel rechargeResponse = new RechargeResponseModel();
            string responseContent = String.Empty;
            RechargeReqModel reqModel = new RechargeReqModel();
            BLLRAToDBSSParse dBSSParse = new BLLRAToDBSSParse();
            JWETToken jWETToken = new JWETToken();
            ValidTokenResponse security = new ValidTokenResponse();
            FTRDBUpdateModel fTRDBUpdateModel = new FTRDBUpdateModel();
            FTRAirResponseModel fTRAir = new FTRAirResponseModel();
            int restrictAllowFTR = 1;
            try
            {
                string loginProviderId = string.Empty;
                string loginProviderIdRet = string.Empty;
                int addMinutes = 0;
                int substractMinutes = 0;
                string secreteKey = string.Empty; 
                string channelName = string.Empty;
                string userName = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();
                try
                {
                    loginProviderIdRet = _configuration.GetSection("AppSettings:JWETLoginProvider").Value;
                    addMinutes = Convert.ToInt32(_configuration.GetSection("AppSettings:addMinutesForJWET").Value);
                    substractMinutes = Convert.ToInt32(_configuration.GetSection("AppSettings:substarctMinutesForJWET").Value);
                }
                catch
                { }

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(model.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        if(model.userId != null)
                        {
                            if (!model.userId.Equals(security.UserName))
                            {
                                throw new Exception(SettingsValues.GetSessionMessage());
                            }
                        }
                        
                        loginProviderId = security.LoginProviderId;
                        channelName = security.ChannelName;
                        userName = security.UserName;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                double balance = 0;
                int isFtrFeatureOn = 0;
                
                isFtrFeatureOn = Convert.ToInt32(_configuration.GetSection("AppSettings:isFtrFeatureOn").Value);

                if (isFtrFeatureOn == 1)
                {
                    RetailerBalanceRespModel balanceAmount = await _ftr_restrict.CheckEVBalance(model.retailerCode, model.subscriberNo, model.userPin, model.bi_token_number,model.subscriberNo);

                    try
                    {
                        if (balanceAmount.Balance != null && balanceAmount.Balance == "0")
                        {
                            return Ok(new RechargeResponseModel()
                            {
                                isError = true,
                                message = balanceAmount.message
                            });
                        }
                        else if (!String.IsNullOrEmpty(balanceAmount.message) && balanceAmount.message.Contains("Invalid"))
                        {
                            return Ok(new RechargeResponseModel()
                            {
                                isError = true,
                                message = balanceAmount.message
                            });
                        }
                        else
                        {
                            balance = Convert.ToDouble(balanceAmount.Balance);
                        }
                    }
                    catch
                    {
                        balance = 0;
                    }
                    if (balance >= Convert.ToDouble(model.amount))
                    {
                        fTRAir = await _ftr_restrict.FTRRestrictionRequsetToAIR(model, channelName, userName);
                        if (fTRAir != null)
                        {
                            if (fTRAir.responseCode == 0)
                            {
                                await Task.Delay(500);
                                string retailer_code = string.Empty;
                                if (model.retailerCode != null && model.retailerCode.Substring(0, 1) != "R")
                                {
                                    retailer_code = "R" + model.retailerCode;
                                    model.userId = model.retailerCode;
                                }
                                else
                                {
                                    retailer_code = model.retailerCode;
                                    model.userId = model.retailerCode.Substring(0, 1);
                                }
                                if (String.IsNullOrEmpty(model.deviceId))
                                {
                                    model.deviceId = "BL-Smartpos-app";
                                }

                                model.sessionToken = jWETToken.GenerateJWETToken(model.subscriberNo, retailer_code, model.deviceId, loginProviderIdRet, model.userId, 0);

                                log.req_time = DateTime.Now;

                                reqModel = dBSSParse.RechargeReqPargeModel(model);

                                log.req_blob = _blJson.GetGenericJsonData(reqModel);

                                string jsonData = JsonConvert.SerializeObject(reqModel);

                                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                                using (HttpClient client = new HttpClient())
                                {
                                    HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                                    responseContent = response.Content.ReadAsStringAsync().Result;
                                }

                                apiResponse = JsonConvert.DeserializeObject<RechargeResponseModel>(responseContent);

                                if (apiResponse != null)
                                {
                                    if (apiResponse.isError == true && apiResponse.message.Contains("Invalid session token"))
                                    {
                                        model.sessionToken = jWETToken.GenerateJWETToken(model.subscriberNo, retailer_code, model.deviceId, loginProviderIdRet, model.userId, substractMinutes);

                                        content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                                        using (HttpClient client = new HttpClient())
                                        {
                                            HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                                            responseContent = response.Content.ReadAsStringAsync().Result;
                                        }

                                        apiResponse = JsonConvert.DeserializeObject<RechargeResponseModel>(responseContent);

                                        if (apiResponse != null)
                                        {
                                            if (apiResponse.isError == true && apiResponse.message.Contains("Invalid session token"))
                                            {
                                                model.sessionToken = jWETToken.GenerateJWETToken(model.subscriberNo, retailer_code, model.deviceId, loginProviderIdRet, model.userId, addMinutes);

                                                content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                                                using (HttpClient client = new HttpClient())
                                                {
                                                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                                                    responseContent = await response.Content.ReadAsStringAsync();
                                                }

                                                apiResponse = JsonConvert.DeserializeObject<RechargeResponseModel>(responseContent);

                                                if (apiResponse != null && apiResponse.isError == false)
                                                {
                                                    #region Update_Bi_Request_Raise_Complaint_Flag
                                                    _frManager.UpdateOrderFirstRechargeStatus(model.bi_token_number);

                                                    #endregion
                                                    return Ok(new RechargeResponseModel()
                                                    {
                                                        isError = false,
                                                        message = apiResponse.message
                                                    });
                                                }
                                                else if (apiResponse != null && apiResponse.isError == true && apiResponse.message.Contains("Invalid session token"))
                                                {
                                                    return Ok(new RechargeResponseModel()
                                                    {
                                                        isError = true,
                                                        message = "Retailer App API: " + apiResponse.message
                                                    });
                                                }
                                            }
                                            if (apiResponse != null && apiResponse.isError == true)
                                            {
                                                return Ok(new RechargeResponseModel()
                                                {
                                                    isError = true,
                                                    message = apiResponse.message
                                                });
                                            }
                                            if (apiResponse != null && apiResponse.isError == false)
                                            {
                                                _frManager.UpdateOrderFirstRechargeStatus(model.bi_token_number);
                                                return Ok(new RechargeResponseModel()
                                                {
                                                    isError = false,
                                                    message = apiResponse.message
                                                });
                                            }
                                        }
                                        if (apiResponse != null && apiResponse.isError == false)
                                        {
                                            #region Update_Bi_Request_Raise_Complaint_Flag
                                            _frManager.UpdateOrderFirstRechargeStatus(model.bi_token_number);

                                            #endregion
                                            return Ok(new RechargeResponseModel()
                                            {
                                                isError = false,
                                                message = apiResponse.message
                                            });
                                        }
                                    }
                                    if (apiResponse != null && apiResponse.isError == true)
                                    {
                                        return Ok(new RechargeResponseModel()
                                        {
                                            isError = true,
                                            message = apiResponse != null ? apiResponse.message : " "
                                        });
                                    }
                                }
                                if (apiResponse != null && apiResponse.isError == false)
                                {
                                    #region Update_Bi_Request_Raise_Complaint_Flag
                                    _frManager.UpdateOrderFirstRechargeStatus(model.bi_token_number);
                                    #endregion
                                    return Ok(new RechargeResponseModel()
                                    {
                                        isError = false,
                                        message = apiResponse.message
                                    });
                                }
                                else if (apiResponse != null && apiResponse.isError == true)
                                {
                                    return Ok(new RechargeResponseModel()
                                    {
                                        isError = true,
                                        message = apiResponse.message ?? "apiResponse.message not found"
                                    });
                                }
                                else
                                {
                                    return Ok(new RechargeResponseModel()
                                    {
                                        isError = false,
                                        message = "Invalid API Response"
                                    });
                                }

                            }
                            else if (fTRAir.responseCode == 102)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". Subscriber not found"
                                });

                            }
                            else if (fTRAir.responseCode == 136)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". Date adjustment error"
                                });

                            }
                            else if (fTRAir.responseCode == 104)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". Temporary blocked"
                                });

                            }
                            else if (fTRAir.responseCode == 165)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". Offer not found"
                                });

                            }
                            else if (fTRAir.responseCode == 260)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". Capability not available"
                                });

                            }
                            else if (fTRAir.responseCode == 247)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". Product not found"
                                });

                            }
                            else if (fTRAir.responseCode == 238)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". Not allowed to create a provider account offer without providing a Provider ID."
                                });

                            }
                            else if (fTRAir.responseCode == 237)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". Not allowed to add a Provider ID to another offer type than provider account offer."
                                });

                            }
                            else if (fTRAir.responseCode == 230)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". Not allowed to convert to other type of lifetime(1)"
                                });

                            }
                            else if (fTRAir.responseCode == 227)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". Invalid PAM Period Relative Dates Expiry PAM Period Indicator"
                                });

                            }
                            else if (fTRAir.responseCode == 225)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". The offer start date can not be changed because the offer is already active.(PC:08204)"
                                });

                            }
                            else if (fTRAir.responseCode == 224)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". The old offer date provided in the request did not match the current date.(PC:08204)"
                                });

                            }
                            else if (fTRAir.responseCode == 223)
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = "Air responseCode: " + fTRAir.responseCode + ". Service failed because new offer date provided in the request was incorrect.(PC:08204)"
                                });

                            }
                            else
                            {
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = true,
                                    message = fTRAir != null ? "Air responseCode: " + fTRAir.responseCode + ". " + fTRAir.Message : apiResponse.message
                                });
                            }
                        }
                        else
                        {
                            return Ok(new RechargeResponseModel()
                            {
                                isError = true,
                                message = "FTR Restriction response is not valid!"
                            });
                        }                      

                    }
                    else
                    {
                        return Ok(new RechargeResponseModel()
                        {
                            isError = true,
                            message = "You have not sufficient balance to recharge!"
                        });
                    }
                }
                else
                {
                    string retailer_code = string.Empty;
                    if (model.retailerCode != null && model.retailerCode.Substring(0, 1) != "R")
                    {
                        retailer_code = "R" + model.retailerCode;
                        model.userId = model.retailerCode;
                    }
                    else
                    {
                        retailer_code = model.retailerCode;
                        model.userId = model.retailerCode.Substring(0, 1);
                    }
                    if (String.IsNullOrEmpty(model.deviceId))
                    {
                        model.deviceId = "BL-Smartpos-app";
                    }

                    model.sessionToken = jWETToken.GenerateJWETToken(model.subscriberNo, retailer_code, model.deviceId, loginProviderIdRet, model.userId, 0);


                    reqModel = dBSSParse.RechargeReqPargeModel(model);

                    log.req_blob = _blJson.GetGenericJsonData(reqModel);

                    string jsonData = JsonConvert.SerializeObject(reqModel);

                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                        responseContent = response.Content.ReadAsStringAsync().Result;
                    }

                    apiResponse = JsonConvert.DeserializeObject<RechargeResponseModel>(responseContent);

                    if (apiResponse != null)
                    {
                        if (apiResponse.isError == true && apiResponse.message.Contains("Invalid session token"))
                        {
                            model.sessionToken = jWETToken.GenerateJWETToken(model.subscriberNo, retailer_code, model.deviceId, loginProviderIdRet, model.userId, substractMinutes);

                            content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                            using (HttpClient client = new HttpClient())
                            {
                                HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                                responseContent = response.Content.ReadAsStringAsync().Result;
                            }

                            apiResponse = JsonConvert.DeserializeObject<RechargeResponseModel>(responseContent);

                            if (apiResponse != null)
                            {
                                if (apiResponse.isError == true && apiResponse.message.Contains("Invalid session token"))
                                {
                                    model.sessionToken = jWETToken.GenerateJWETToken(model.subscriberNo, retailer_code, model.deviceId, loginProviderIdRet, model.userId, addMinutes);

                                    content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                                    using (HttpClient client = new HttpClient())
                                    {
                                        HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                                        responseContent = response.Content.ReadAsStringAsync().Result;
                                    }

                                    apiResponse = JsonConvert.DeserializeObject<RechargeResponseModel>(responseContent);

                                    if (apiResponse != null && apiResponse.isError == false)
                                    {
                                        #region Update_Bi_Request_Raise_Complaint_Flag
                                        _frManager.UpdateOrderFirstRechargeStatus(model.bi_token_number);

                                        #endregion
                                        return Ok(new RechargeResponseModel()
                                        {
                                            isError = false,
                                            message = apiResponse.message
                                        });
                                    }
                                    else if (apiResponse != null && apiResponse.isError == true && apiResponse.message.Contains("Invalid session token"))
                                    {
                                        return Ok(new RechargeResponseModel()
                                        {
                                            isError = true,
                                            message = apiResponse.message
                                        });
                                    }
                                }
                                if (apiResponse != null && apiResponse.isError == true)
                                {
                                    return Ok(new RechargeResponseModel()
                                    {
                                        isError = true,
                                        message = apiResponse.message
                                    });
                                }
                                if (apiResponse != null && apiResponse.isError == false)
                                {
                                    _frManager.UpdateOrderFirstRechargeStatus(model.bi_token_number);
                                    return Ok(new RechargeResponseModel()
                                    {
                                        isError = false,
                                        message = apiResponse.message
                                    });
                                }
                            }
                            if (apiResponse != null && apiResponse.isError == false)
                            {
                                #region Update_Bi_Request_Raise_Complaint_Flag
                                _frManager.UpdateOrderFirstRechargeStatus(model.bi_token_number);

                                #endregion
                                return Ok(new RechargeResponseModel()
                                {
                                    isError = false,
                                    message = apiResponse.message
                                });
                            }
                        }
                        if (apiResponse != null && apiResponse.isError == true)
                        {
                            return Ok(new RechargeResponseModel()
                            {
                                isError = true,
                                message = apiResponse.message
                            });
                        }
                        if (apiResponse != null && apiResponse.isError == false)
                        {
                            #region Update_Bi_Request_Raise_Complaint_Flag
                            _frManager.UpdateOrderFirstRechargeStatus(model.bi_token_number);

                            #endregion
                            return Ok(new RechargeResponseModel()
                            {
                                isError = false,
                                message = apiResponse.message
                            });
                        }
                        else
                        {
                            return Ok(new RechargeResponseModel()
                            {
                                isError = false,
                                message = apiResponse?.message ?? "apiResponse.message not found"
                            });
                        }
                    }
                    else
                    {
                        return Ok(new RechargeResponseModel()
                        {
                            isError = true,
                            message = "Invalid API Response (Retailer API)"
                        });
                    }
                }

            }
            catch (WebException ex)
            {
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        rechargeResponse.isError = true;

                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            rechargeResponse.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            rechargeResponse.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        log.res_blob = _blJson.GetGenericJsonData(rechargeResponse);
                        log.res_time = DateTime.Now;

                        return Ok(rechargeResponse);
                    }
                    catch (Exception)
                    {
                        rechargeResponse.isError = true;
                        rechargeResponse.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(rechargeResponse);
                        log.res_time = DateTime.Now;

                        return Ok(rechargeResponse);
                    }
                }
                throw new Exception(ex.Message);
            }
            catch (Exception ex2)
            {
                log.res_time = DateTime.Now;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex2.Message);

                try
                {
                    error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                    rechargeResponse.isError = true;

                    rechargeResponse.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                    return Ok(rechargeResponse);
                }
                catch (Exception)
                {
                    rechargeResponse.isError = true;
                    rechargeResponse.message = ex2.Message;

                    return Ok(rechargeResponse);
                }
            }
            finally
            {
                log.res_time = DateTime.Now;
                log.message = rechargeResponse.message;
                log.msisdn = _bllLog.FormatMSISDN(model.subscriberNo);
                log.res_blob = _blJson.GetGenericJsonData(responseContent);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailerCode;
                log.method_name = "FirstRecharge";

                await _bllLog.RAToDBSSLog(log, apiUrl, responseContent);
            }
        }
        [HttpPost]
        [Route("GetRechargeAmount")]
        public async Task<IActionResult> GetRechargeAmount(RechargeAmountReqModel model)
        {
            RechargeAmountData amountData = new RechargeAmountData();
            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;
                string userName = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(model.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;
                        userName = security.UserName;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                amountData = await _bLLCommon.GetRechargeAmount(model, userName);

                return Ok(amountData);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = ex.Message
                    });
                }
            }
        }

        [HttpPost]
        [Route("GetRechargeAmountV2")]
        public async Task<IActionResult> GetRechargeAmountV2(RechargeAmountReqModelRev model)
        {
            RechargeAmountData amountData = new RechargeAmountData();
            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;
                string userName = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(model.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;
                        userName = security.UserName;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                amountData = await _bLLCommon.GetRechargeAmountV2(model, userName);

                return Ok(amountData);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = ex.Message
                    });
                }
            }
        }
    }
}
