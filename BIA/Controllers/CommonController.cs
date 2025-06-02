using BIA.BLL.BLLServices;
using BIA.BLL.Utility;
using BIA.Common;
using BIA.DAL.Repositories;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.ViewModel;
using BIA.Helper;
using BIA.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Net;
using System.Reflection;
using System.Web;

namespace BIA.Controllers
{
    [Route("api/Common")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private BLLRAToDBSSParse _raToDBssParse;
        private BLLDBSSToRAParse _dbssToRaParse;
        private ApiRequest _apiReq;
        private BLLCommon _bllCommon;
        private BL_Json _blJson;
        private readonly DALBiometricRepo dataManager;
        private readonly BLLOrder _bllOrder;
        private readonly ApiManager _apiManager;
        private readonly BLLLog _bllLog;
        private readonly BaseController _bio;
        private readonly BLLDivDisThana _divDisThana;
        private readonly BLLUserAuthenticaion _bLLUserAuthenticaion;

        public CommonController(DALBiometricRepo dataManager, BLLDBSSToRAParse dbssToRaParse, BLLRAToDBSSParse raToDBssParse, ApiRequest apiReq, BL_Json blJson, BLLCommon bllCommon, BLLOrder bllOrder, ApiManager apiManager, BLLLog bllLog, BaseController bio, BLLDivDisThana divDisThana, BLLUserAuthenticaion bLLUserAuthenticaion)
        { 
            this._bllCommon = bllCommon;
            this._raToDBssParse = raToDBssParse;
            this._dbssToRaParse = dbssToRaParse;
            this._apiReq = apiReq;
            this._blJson = blJson;
            this.dataManager = dataManager;
            this._bllOrder = bllOrder;
            this._apiManager = apiManager;
            this._bllLog = bllLog;
            this._bio = bio;
            this._divDisThana = divDisThana;
            this._bLLUserAuthenticaion = bLLUserAuthenticaion;
        }

        #region Get Subscription Type
        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [HttpPost]
        [Route("GetSubscriptionTypesV1")]
        public async Task<IActionResult> GetSubscriptionTypesV1(RASubscriptionTypeReq model)
        {
            List<SubscriptionTypeReponseData> raRespData = new List<SubscriptionTypeReponseData>();
            SubscriptionTypeReponse raResp = new SubscriptionTypeReponse();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                apiUrl = String.Format(GetAPICollection.GetSubscriptionTypes, model.subscription_type, model.channel_name);

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

                    SubscriptionTypeRootData? dbssRespModel = JsonConvert.DeserializeObject<SubscriptionTypeRootData>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.data).Cast<object>().ToList();

                            raRespData = _dbssToRaParse.SubscripTypesReqParsing(result);

                            if (raRespData.Count > 0)
                            {
                                raResp.data = raRespData;
                                raResp.result = true;
                                raResp.message = MessageCollection.Success;
                            }
                            else
                            {
                                raResp.data = raRespData;
                                raResp.result = false;
                                raResp.message = MessageCollection.NoDataFound;
                            }
                        }
                        else
                        {
                            raResp.data = raRespData;
                            raResp.result = false;
                            raResp.message = "DBSS API response doesn't contains any Subscription Types data.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.result = false;
                        raResp.message = "DBSS API response doesn't contains any Subscription Types data.";
                    }
                }
                else
                {
                    raResp.data = raRespData;
                    raResp.result = false;
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
                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                }
                catch (Exception)
                {
                    raResp.data = raRespData;
                    raResp.result = false;
                    raResp.message = ex.Message;
                }

            }
            finally
            {
                log.method_name = "GetSubscriptionTypesV1";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                string rspStr = string.Empty;
                if (txtResp != null)
                {
                    rspStr = txtResp;
                }
                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, rspStr));
                //logThread.Start();
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }

        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [HttpPost]
        [Route("GetSubscriptionTypesV2")]
        public async Task<IActionResult> GetSubscriptionTypesV2(RASubscriptionTypeReq model)
        {
            List<SubscriptionTypeReponseData> raRespData = new List<SubscriptionTypeReponseData>();
            SubscriptionTypeReponse raResp = new SubscriptionTypeReponse();
            string? apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                apiUrl = String.Format(GetAPICollection.GetSubscriptionTypes, model.subscription_type, model.channel_name);

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

                    SubscriptionTypeRootData? dbssRespModel = JsonConvert.DeserializeObject<SubscriptionTypeRootData>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.data).Cast<object>().ToList();

                            raRespData = _dbssToRaParse.SubscripTypesReqParsing(result);

                            if (raRespData.Count > 0)
                            {
                                raResp.data = raRespData;
                                raResp.result = true;
                                raResp.message = MessageCollection.Success;
                            }
                            else
                            {
                                raResp.data = raRespData;
                                raResp.result = false;
                                raResp.message = MessageCollection.NoDataFound;
                            }
                        }
                        else
                        {
                            raResp.data = raRespData;
                            raResp.result = false;
                            raResp.message = "DBSS API doesn't contains any subscription types data.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.result = false;
                        raResp.message = "DBSS API doesn't contains any subscription types data.";
                    }
                }
                else
                {
                    raResp.data = raRespData;
                    raResp.result = false;
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
                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                }
                catch (Exception)
                {
                    raResp.data = raRespData;
                    raResp.result = false;
                    raResp.message = ex.Message;
                }

            }
            finally
            {
                log.method_name = "GetSubscriptionTypesV1";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                string rspStr = string.Empty;
                if (txtResp != null)
                {
                    rspStr = txtResp;
                }
                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, rspStr));
                //logThread.Start();
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }

        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [HttpPost]
        [Route("GetSubscriptionTypesPostPaid")]
        public async Task<IActionResult> GetSubscriptionTypesPostPaid(RASubscriptionTypeReq model)
        {
            List<SubscriptionTypeReponseDataRev> raRespData = new List<SubscriptionTypeReponseDataRev>();
            SubscriptionTypeReponseRev raResp = new SubscriptionTypeReponseRev();
            string? apiUrl = string.Empty, txtResp = string.Empty; 
            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                try
                {
                    secreteKey = SettingsValues.GetJWTSequrityKey();
                }
                catch
                { }
                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(model.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        if (!model.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }
                var dbssResp = await _bllCommon.GetSubscriptionTypes(model);

                if (dbssResp != null)
                {
                        if (dbssResp.data != null)
                        {
                            for(int i = 0; i < dbssResp.data.Count(); i++)
                            {
                                SubscriptionTypeReponseDataRev data = new SubscriptionTypeReponseDataRev();
                                data.subscription_id = dbssResp.data[i].subscription_id!=null? dbssResp.data[i].subscription_id.ToString():null;
                                data.subscription_name = dbssResp.data[i].subscription_name != null? dbssResp.data[i].subscription_name:null;
                                raRespData.Add(data);

                            }

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
                            raResp.message = MessageCollection.NoDataFound;
                    }
                }
                else
                {
                    raResp.data = raRespData;
                    raResp.isError = true;
                    raResp.message = MessageCollection.NoDataFound;
                }
                return Ok(raResp);
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raResp.data = null;
                    raResp.isError = true;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    raResp.isError = true;
                    raResp.message = ex.Message;
                }
                return Ok(raResp);
            }
            
        }

        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [HttpPost]
        [Route("GetSubscriptionTypesV3")]
        public async Task<IActionResult> GetSubscriptionTypesV3(RASubscriptionTypeReq model)
        {
            List<SubscriptionTypeReponseDataRev> raRespData = new List<SubscriptionTypeReponseDataRev>();
            SubscriptionTypeReponseRev raResp = new SubscriptionTypeReponseRev();
            string? apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                try
                {
                    secreteKey = SettingsValues.GetJWTSequrityKey();
                }
                catch
                { }
                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(model.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        if (!model.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                apiUrl = String.Format(GetAPICollection.GetSubscriptionTypes, model.subscription_type, model.channel_name);

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

                    SubscriptionTypeRootData? dbssRespModel = JsonConvert.DeserializeObject<SubscriptionTypeRootData>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.data).Cast<object>().ToList();

                            raRespData = _dbssToRaParse.SubscripTypesReqParsingV2(result);

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
                            raResp.message = "DBSS API doesn't contains any subscription types data.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.isError = true;
                        raResp.message = "DBSS API doesn't contains any subscription types data.";
                    }
                }
                else
                {
                    raResp.data = raRespData;
                    raResp.isError = true;
                    raResp.message = "Unable to load data from DBSS API.";
                }
                return Ok(raResp);
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
                return Ok(raResp);
            }
            finally
            {
                log.method_name = "GetSubscriptionTypesV3";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                string rspStr = string.Empty;
                if (txtResp != null)
                {
                    rspStr = txtResp;
                }
                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, rspStr));
                //logThread.Start();
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }



        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")] 
        [HttpPost]
        [Route("GetSubscriptionTypesById")]
        public async Task<IActionResult> GetSubscriptionTypesById(RASubscriptionTypeReqV2 model)
        {
            List<SubscriptionTypeByIdReponseData> raRespData = new List<SubscriptionTypeByIdReponseData>();
            SubscriptionTypeReponseById raResp = new SubscriptionTypeReponseById();
            string? apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                apiUrl = String.Format(GetAPICollection.GetSubscriptionTypesById, model.dbss_subscription_id);

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

                    SubscriptionTypeRootData? dbssRespModel = JsonConvert.DeserializeObject<SubscriptionTypeRootData>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.data).Cast<object>().ToList();

                            raRespData = _dbssToRaParse.SubscripTypesByIdReqParsing(result);

                            if (raRespData.Count > 0)
                            {
                                raResp.data = raRespData;
                                raResp.result = true;
                                raResp.message = MessageCollection.Success;
                            }
                            else
                            {
                                raResp.data = raRespData;
                                raResp.result = false;
                                raResp.message = MessageCollection.NoDataFound;
                            }
                        }
                        else
                        {
                            raResp.data = raRespData;
                            raResp.result = false;
                            raResp.message = "DBSS API doesn't contains any subscription types data.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.result = false;
                        raResp.message = "DBSS API doesn't contains any subscription types data.";
                    }
                }
                else
                {
                    raResp.data = raRespData;
                    raResp.result = false;
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
                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                }
                catch (Exception)
                {
                    raResp.data = raRespData;
                    raResp.result = false;
                    raResp.message = ex.Message;
                }

            }
            finally
            {
                log.method_name = "GetSubscriptionTypesById";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                string rspStr = string.Empty;
                if (txtResp != null)
                {
                    rspStr = txtResp;
                }
                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, rspStr));
                //logThread.Start();
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }

        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")] 
        [HttpPost]
        [Route("GetSubscriptionTypesByIdV2")]
        public async Task<IActionResult> GetSubscriptionTypesByIdV2(RASubscriptionTypeReqV2 model)
        {
            List<SubscriptionTypeByIdReponseDataRev> raRespData = new List<SubscriptionTypeByIdReponseDataRev>();
            SubscriptionTypeReponseByIdRev raResp = new SubscriptionTypeReponseByIdRev();
            string? apiUrl = string.Empty, txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
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
                        if (!model.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                apiUrl = String.Format(GetAPICollection.GetSubscriptionTypesById, model.dbss_subscription_id);

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

                    SubscriptionTypeRootData? dbssRespModel = JsonConvert.DeserializeObject<SubscriptionTypeRootData>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.data).Cast<object>().ToList();

                            raRespData = _dbssToRaParse.SubscripTypesByIdReqParsingRev(result);

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
                            raResp.message = "DBSS API doesn't contains any subscription types data.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.isError = true;
                        raResp.message = "DBSS API doesn't contains any subscription types data.";
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
                log.method_name = "GetSubscriptionTypesById";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                string rspStr = string.Empty;
                if (txtResp != null)
                {
                    rspStr = txtResp;
                }
                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, rspStr));
                //logThread.Start();
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }
        #endregion

        #region Get Package
        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [GzipCompression]
        [HttpPost]
        [Route("GetPackagesV1")]
        public async Task<IActionResult> GetPackagesV1(RAGetPackageResquest model)
        {
            //Step-0 :
            List<PackagesReponseData> raRespData = new List<PackagesReponseData>();
            PackagesResponse raResp = new PackagesResponse();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                apiUrl = String.Format(GetAPICollection.GetPackagesBySubscriptionTypeId, model.subscription_id);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.user_id = model.retailer_id;
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp != null)
                {
                    log.is_success = 1;
                    PackageRootData? dbssRespModel = JsonConvert.DeserializeObject<PackageRootData>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.included != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.included).Cast<object>().ToList();
                            raRespData = _dbssToRaParse.PackagesParsing(result);

                            if (raRespData.Count > 0)
                            {
                                raResp.data = raRespData;
                                raResp.result = true;
                                raResp.message = MessageCollection.Success;
                            }
                            else
                            {
                                raResp.data = raRespData;
                                raResp.result = false;
                                raResp.message = MessageCollection.NoDataFound;
                            }
                        }
                        else
                        {
                            raResp.data = raRespData;
                            raResp.result = false;
                            raResp.message = "Subscription type id " + model.subscription_id + " doesn't contain any packages.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.result = false;
                        raResp.message = "Subscription type id " + model.subscription_id + " doesn't contain any packages.";
                    }
                }

                return Ok(raResp);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription? error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    log.res_time = DateTime.Now;

                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.result = false;
                        raResp.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                }

                string resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);

                        if (respObj1 != null)
                        {
                            error = await _bllLog.ManageException(respObj1["errors"]["title"] != null
                            && respObj1["errors"]["title"].ToString() != "" ? respObj1["errors"]["title"].ToString() : ex.Message, ex.HResult, "BIA");


                        }
                        raResp.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return Ok(raResp);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raResp.result = false;
                            if (_bio.isDBSSErrorOccurred(ex))
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return Ok(raResp);
                        }
                        catch (Exception)
                        {
                            raResp.result = false;
                            raResp.message = ex.Message;

                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return Ok(raResp);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.result = false;
                        raResp.message = ex.Message;

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
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription? error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raResp);
                }
                catch (Exception)
                {
                    raResp.result = false;
                    raResp.message = ex.Message;

                    return Ok(raResp);
                }
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.user_id = model.retailer_id;
                log.method_name = "GetPackagesV1";

                string rspStr = string.Empty;
                if (txtResp != null)
                {
                    rspStr = txtResp;
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, rspStr));
                //logThread.Start();
                //
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailerk")]
        [GzipCompression]
        [HttpPost]
        [Route("GetPackagesV2")]
        public async Task<IActionResult> GetPackagesV2(RAGetPackageResquest model)
        {
            //Step-0 :
            List<PackagesReponseData> raRespData = new List<PackagesReponseData>();
            PackagesResponse raResp = new PackagesResponse();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                apiUrl = String.Format(GetAPICollection.GetPackagesBySubscriptionTypeId, model.subscription_id);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.user_id = model.retailer_id;
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp != null)
                {
                    log.is_success = 1;
                    PackageRootData? dbssRespModel = JsonConvert.DeserializeObject<PackageRootData>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.included != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.included).Cast<object>().ToList();
                            raRespData = _dbssToRaParse.PackagesParsing(result);

                            if (raRespData.Count > 0)
                            {
                                raResp.data = raRespData;
                                raResp.result = true;
                                raResp.message = MessageCollection.Success;
                            }
                            else
                            {
                                raResp.data = raRespData;
                                raResp.result = false;
                                raResp.message = MessageCollection.NoDataFound;
                            }
                        }
                        else
                        {
                            raResp.data = raRespData;
                            raResp.result = false;
                            raResp.message = "Subscription type id " + model.subscription_id + " doesn't contain any packages.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.result = false;
                        raResp.message = "Subscription type id " + model.subscription_id + " doesn't contain any packages.";
                    }
                }

                return Ok(raResp);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription? error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    log.res_time = DateTime.Now;

                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.result = false;
                        raResp.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                }
                string resp = string.Empty;
                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);

                        error = await _bllLog.ManageException(respObj1["errors"]["title"] != null
                                                    && respObj1["errors"]["title"].ToString() != "" ? respObj1["errors"]["title"].ToString() : ex.Message, ex.HResult, "BIA");

                        raResp.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return Ok(raResp);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raResp.result = false;
                            if (_bio.isDBSSErrorOccurred(ex))
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return Ok(raResp);
                        }
                        catch (Exception)
                        {
                            raResp.result = false;
                            raResp.message = ex.Message;

                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return Ok(raResp);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.result = false;
                        raResp.message = ex.Message;

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
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

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raResp);
                }
                catch (Exception)
                {
                    raResp.result = false;
                    raResp.message = ex.Message;

                    return Ok(raResp);
                }
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.user_id = model.retailer_id;
                log.method_name = "GetPackagesV1";
                string rspStr = string.Empty;
                if (txtResp != null)
                {
                    rspStr = txtResp;
                }
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [GzipCompression]
        [HttpPost]
        [Route("GetPackagesV3")]
        public async Task<IActionResult> GetPackagesV3(RAGetPackageResquest model)
        {
            //Step-0 :
            List<PackagesReponseDataRev> raRespData = new List<PackagesReponseDataRev>();
            PackagesResponseRev raResp = new PackagesResponseRev();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
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
                        if (!model.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                apiUrl = String.Format(GetAPICollection.GetPackagesBySubscriptionTypeId, model.subscription_id);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.user_id = model.retailer_id;
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp != null)
                {
                    log.is_success = 1;
                    PackageRootData? dbssRespModel = JsonConvert.DeserializeObject<PackageRootData>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.included != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.included).Cast<object>().ToList();
                            raRespData = _dbssToRaParse.PackagesParsingV2(result);

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
                            raResp.message = "Subscription type id " + model.subscription_id + " doesn't contain any packages.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.isError = true;
                        raResp.message = "Subscription type id " + model.subscription_id + " doesn't contain any packages.";
                    }
                }

                return Ok(raResp);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription? error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    log.res_time = DateTime.Now;

                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.isError = true;
                        raResp.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                }
                string resp = string.Empty;
                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);

                        error = await _bllLog.ManageException(respObj1["errors"]["title"] != null
                                                    && respObj1["errors"]["title"].ToString() != "" ? respObj1["errors"]["title"].ToString() : ex.Message, ex.HResult, "BIA");

                        raResp.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return Ok(raResp);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raResp.isError = true;
                            if (_bio.isDBSSErrorOccurred(ex))
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return Ok(raResp);
                        }
                        catch (Exception)
                        {
                            raResp.isError = true;
                            raResp.message = ex.Message;

                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return Ok(raResp);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.isError = true;
                        raResp.message = ex.Message;

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
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

                    raResp.isError = true;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raResp);
                }
                catch (Exception)
                {
                    raResp.isError = true;
                    raResp.message = ex.Message;
                    return Ok(raResp);
                }
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.user_id = model.retailer_id;
                log.method_name = "GetPackagesV3";
                string rspStr = string.Empty;
                if (txtResp != null)
                {
                    rspStr = txtResp;
                }
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [GzipCompression]
        [HttpPost]
        [Route("GetPackagesV4")]
        public async Task<IActionResult> GetPackagesV4(RAGetPackageResquestV3 model)
        {
            List<PackagesReponseDataRev> raRespData = new List<PackagesReponseDataRev>();
            PackagesResponseRev raResp = new PackagesResponseRev();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
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
                        if (!model.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                apiUrl = String.Format(GetAPICollection.GetPackagesBySubscriptionTypeId, model.subscription_id);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.user_id = model.retailer_id;
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp != null)
                {
                    log.is_success = 1;
                    PackageRootData? dbssRespModel = JsonConvert.DeserializeObject<PackageRootData>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.included != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.included).Cast<object>().ToList();
                            raRespData = await _dbssToRaParse.PackagesParsingV3(result,model.category_name);

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
                            raResp.message = "Subscription type id " + model.subscription_id + " doesn't contain any packages.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.isError = true;
                        raResp.message = "Subscription type id " + model.subscription_id + " doesn't contain any packages.";
                    }
                }

                return Ok(raResp);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription? error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    log.res_time = DateTime.Now;

                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.isError = true;
                        raResp.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                }
                string resp = string.Empty;
                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);

                        error = await _bllLog.ManageException(respObj1["errors"]["title"] != null
                                                    && respObj1["errors"]["title"].ToString() != "" ? respObj1["errors"]["title"].ToString() : ex.Message, ex.HResult, "BIA");

                        raResp.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return Ok(raResp);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raResp.isError = true;
                            if (_bio.isDBSSErrorOccurred(ex))
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return Ok(raResp);
                        }
                        catch (Exception)
                        {
                            raResp.isError = true;
                            raResp.message = ex.Message;

                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return Ok(raResp);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.isError = true;
                        raResp.message = ex.Message;

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
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

                    raResp.isError = true;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raResp);
                }
                catch (Exception)
                {
                    raResp.isError = true;
                    raResp.message = ex.Message;

                    return Ok(raResp);
                }
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.user_id = model.retailer_id;
                log.method_name = "GetPackagesV3";
                string rspStr = string.Empty;
                if (txtResp != null)
                {
                    rspStr = txtResp;
                }

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }




        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [GzipCompression]
        [HttpPost]
        [Route("GetPackagesPreToPostMigration")]
        public async Task<IActionResult> GetPackagesPreToPostMigration(RAGetPackageResquestV2 model)
        {
            //Step-0 :
            List<PackagesReponseData> raRespData = new List<PackagesReponseData>();
            PackagesResponse raResp = new PackagesResponse();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                apiUrl = String.Format(GetAPICollection.GetPackagesBySubscriptionTypeId, model.subscription_type_id);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.user_id = model.retailer_id;
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp != null)
                {
                    log.is_success = 1;
                    PackageRootData? dbssRespModel = JsonConvert.DeserializeObject<PackageRootData>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.included != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.included).Cast<object>().ToList();
                            raRespData = _dbssToRaParse.PackagesParsing(result);

                            if (raRespData.Count > 0)
                            {
                                raResp.data = raRespData;
                                raResp.result = true;
                                raResp.message = MessageCollection.Success;
                            }
                            else
                            {
                                raResp.data = raRespData;
                                raResp.result = false;
                                raResp.message = "packagesNotFound";  //MessageCollection.NoDataFound;
                            }
                        }
                        else
                        {
                            raResp.data = raRespData;
                            raResp.result = false;
                            raResp.message = "packagesNotFound";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.result = false;
                        raResp.message = "packagesNotFound";
                    }
                }

                return Ok(raResp);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    log.res_time = DateTime.Now;

                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.result = false;
                        raResp.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                }

                string resp = String.Empty;
                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raResp.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return Ok(raResp);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raResp.result = false;
                            if (_bio.isDBSSErrorOccurred(ex))
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return Ok(raResp);
                        }
                        catch (Exception)
                        {
                            raResp.result = false;
                            raResp.message = ex.Message;

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

                            return Ok(raResp);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.result = false;
                        raResp.message = ex.Message;

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
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

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raResp);
                }
                catch (Exception)
                {
                    raResp.result = false;
                    raResp.message = ex.Message;

                    return Ok(raResp);
                }
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.user_id = model.retailer_id;
                log.method_name = "GetPackagesPreToPostMigration";

                string rspStr = string.Empty;
                if (txtResp != null)
                {
                    rspStr = txtResp;
                }
                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, rspStr));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, rspStr);
            }
        }

        /// <summary>
        /// This API is used to Get Subscription Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [GzipCompression]
        [HttpPost]
        [Route("GetPackagesPreToPostMigrationV2")]
        public async Task<IActionResult> GetPackagesPreToPostMigrationV2(RAGetPackageResquestV2 model)
        {
            //Step-0 :
            List<PackagesReponseDataRev> raRespData = new List<PackagesReponseDataRev>();
            PackagesResponseRev raResp = new PackagesResponseRev();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
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
                        if (!model.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                apiUrl = String.Format(GetAPICollection.GetPackagesBySubscriptionTypeId, model.subscription_type_id);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.user_id = model.retailer_id;
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssResp != null)
                {
                    log.is_success = 1;
                    PackageRootData? dbssRespModel = JsonConvert.DeserializeObject<PackageRootData>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.included != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.included).Cast<object>().ToList();
                            raRespData = _dbssToRaParse.PackagesParsingV2(result);

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
                                raResp.message = "packagesNotFound";  //MessageCollection.NoDataFound;
                            }
                        }
                        else
                        {
                            raResp.data = raRespData;
                            raResp.isError = true;
                            raResp.message = "packagesNotFound";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.isError = true;
                        raResp.message = "packagesNotFound";
                    }
                }

                return Ok(raResp);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                    log.res_time = DateTime.Now;

                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.isError = true;
                        raResp.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                }

                string resp = String.Empty;
                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raResp.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;

                        return Ok(raResp);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raResp.isError = true;
                            if (_bio.isDBSSErrorOccurred(ex))
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                            }
                            else
                            {
                                raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                            }
                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return Ok(raResp);
                        }
                        catch (Exception)
                        {
                            raResp.isError = true;
                            raResp.message = ex.Message;

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

                            return Ok(raResp);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raResp.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
                    }
                    catch (Exception)
                    {
                        raResp.isError = true;
                        raResp.message = ex.Message;

                        log.error_code = error.error_code ?? String.Empty;
                        log.error_source = error.error_source ?? String.Empty;
                        log.message = error.error_description ?? String.Empty;
                        log.res_blob = _blJson.GetGenericJsonData(raResp.message);
                        log.res_time = DateTime.Now;

                        return Ok(raResp);
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

                    raResp.isError = true;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raResp);
                }
                catch (Exception)
                {
                    raResp.isError = true;
                    raResp.message = ex.Message;

                    return Ok(raResp);
                }
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.user_id = model.retailer_id;
                log.method_name = "GetPackagesPreToPostMigrationV2";

                string rspStr = string.Empty;
                if (txtResp != null)
                {
                    rspStr = txtResp;
                }
                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, rspStr));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, rspStr);
            }
        }

        #endregion


        #region  MSISDN validation Unpaired v2
        /// <summary>
        /// This API is used for MSISDN validation for unpaired MSISDN
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[GzipCompression]
        //[ResponseType(typeof(RACommonResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateUnpairedMSISDNV1")]
        public async Task<IActionResult> ValidateUnpairedMSISDNV2([FromBody] UnpairedMSISDNCheckRequest msisdnCheckReqest)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                response = await _bio.ValidateUnpairedMSISDNV2(msisdnCheckReqest, "ValidateUnpairedMSISDNV1");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// This API is used for MSISDN validation for unpaired MSISDN
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[GzipCompression]
        //[ResponseType(typeof(RACommonResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateUnpairedMSISDNV2")]
        public async Task<IActionResult> ValidateUnpairedMSISDNV3([FromBody] UnpairedMSISDNCheckRequest msisdnCheckReqest)
        {
            try
            {
                RACommonResponse response = new RACommonResponse();

                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                response = await _bio.ValidateUnpairedMSISDNV2(msisdnCheckReqest, "ValidateUnpairedMSISDNV2");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponse()
                {
                    result = false,
                    message = ex.Message

                });
            }
        }

        /// <summary>
        /// This API is used for MSISDN validation for unpaired MSISDN
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[GzipCompression]
        //[ResponseType(typeof(RACommonResponse))]
        [HttpPost]
        //[ValidateModel]
        [Route("ValidateUnpairedMSISDNV3")]
        public async Task<IActionResult> ValidateUnpairedMSISDNV4([FromBody] UnpairedMSISDNCheckRequest msisdnCheckReqest)
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
                        if (!msisdnCheckReqest.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                response = await _bio.ValidateUnpairedMSISDNV4(msisdnCheckReqest, "ValidateUnpairedMSISDNV3");

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



        /// <summary>
        /// This API is used for MSISDN validation for unpaired MSISDN
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[GzipCompression]
        //[ResponseType(typeof(RACommonResponse))]
        [HttpPost]
        //[ValidateModel]
        [Route("ValidateUnpairedMSISDNV4")]
        public async Task<IActionResult> ValidateUnpairedMSISDNV5([FromBody] UnpairedMSISDNCheckRequest msisdnCheckReqest)
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
                        if (!msisdnCheckReqest.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                response = await _bio.ValidateUnpairedMSISDNV6(msisdnCheckReqest, "ValidateUnpairedMSISDNV5");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevampV3()
                {
                    isError = true,
                    message = ex.Message,
                    data = null

                });
            }
        }



        /// <summary>
        /// This API is used for MSISDN validation for unpaired MSISDN
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[GzipCompression]
        ////eSim Start(eSim Logic)
        //[ResponseType(typeof(RACommonResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateUnpairedMSISDN_ESIM")]
        public async Task<IActionResult> ValidateUnpairedMSISDN_ESIM([FromBody] UnpairedMSISDNCheckRequest msisdnCheckReqest)
        {
            RACommonResponse rACommonResponse = new RACommonResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                rACommonResponse = await _bio.ValidateUnpairedMSISDNV3(msisdnCheckReqest, "ValidateUnpairedMSISDN_ESIM");

                return Ok(rACommonResponse);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// This API is used for MSISDN validation for unpaired MSISDN
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[GzipCompression]
        ////eSim Start(eSim Logic)
        //[ResponseType(typeof(RACommonResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateUnpairedMSISDN_ESIMV2")]
        public async Task<IActionResult> ValidateUnpairedMSISDN_ESIMV2([FromBody] UnpairedMSISDNCheckRequest msisdnCheckReqest)
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
                        if (!msisdnCheckReqest.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                rACommonResponse = await _bio.ValidateUnpairedMSISDNV5(msisdnCheckReqest, "ValidateUnpairedMSISDN_ESIMV2");

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

        #endregion


        #region Get DivDisThana
        /// <summary>
        /// This API is used for Getting DivDisThana
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        //[ResponseType(typeof(DivDisThanaResponse))]
        [GzipCompression]
        [HttpPost]
        [Route("GetDivDisThana")]
        public async Task<IActionResult> GetDivDisThana(RACommonRequest model)
        {
            DivDisThanaResponse divDisThanaRes = new DivDisThanaResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                IEnumerable<DivisionModel> divList = await _divDisThana.GetDivision();
                List<DistrictModel> disList = await _divDisThana.GetDistrict();
                List<ThanaModel> thanaList = await _divDisThana.GetThana();

                foreach (DivisionModel item in divList)
                {
                    item.DistrictModel = disList.Where(a => a.DIVISIONID == item.DIVISIONID);

                    foreach (DistrictModel item2 in item.DistrictModel)
                    {
                        item2.ThanaModel = thanaList.Where(a => a.DISTRICTID == item2.DISTRICTID);
                    }
                }
                divDisThanaRes.data = divList;
                divDisThanaRes.result = true;
                divDisThanaRes.message = MessageCollection.Success;
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    divDisThanaRes.data = null;
                    divDisThanaRes.result = false;
                    divDisThanaRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    divDisThanaRes.result = false;
                    divDisThanaRes.message = ex.Message;
                }
            }
            return Ok(divDisThanaRes);
        }

        /// <summary>
        /// This API is used for Getting DivDisThana
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        //[ResponseType(typeof(DivDisThanaResponse))]
        [GzipCompression]
        [HttpPost]
        [Route("GetDivDisThanaV2")]
        public async Task<IActionResult> GetDivDisThanaV2(RACommonRequest model)
        {
            DivDisThanaResponse divDisThanaRes = new DivDisThanaResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                IEnumerable<DivisionModel> divList = await _divDisThana.GetDivision();
                List<DistrictModel> disList = await _divDisThana.GetDistrict();
                List<ThanaModel> thanaList = await _divDisThana.GetThana();

                foreach (DivisionModel item in divList)
                {
                    item.DistrictModel = disList.Where(a => a.DIVISIONID == item.DIVISIONID);

                    foreach (DistrictModel item2 in item.DistrictModel)
                    {
                        item2.ThanaModel = thanaList.Where(a => a.DISTRICTID == item2.DISTRICTID);
                    }
                }
                divDisThanaRes.data = divList;
                divDisThanaRes.result = true;
                divDisThanaRes.message = MessageCollection.Success;
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    divDisThanaRes.data = null;
                    divDisThanaRes.result = false;
                    divDisThanaRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    divDisThanaRes.result = false;
                    divDisThanaRes.message = ex.Message;
                }
            }
            return Ok(divDisThanaRes);
        }

        /// <summary>
        /// This API is used for Getting DivDisThana
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        //[ResponseType(typeof(DivDisThanaResponse))]
        [GzipCompression]
        [HttpPost]
        [Route("GetDivDisThanaV3")]
        public async Task<IActionResult> GetDivDisThanaV3(RACommonRequest model)
        {
            DivDisThanaResponseRevamp divDisThanaRes = new DivDisThanaResponseRevamp();
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

                List<DivisionModelV2> divList = await _divDisThana.GetDivisionV2();
                List<DistrictModelV2> disList = await _divDisThana.GetDistrictV2();
                List<ThanaModelV2> thanaList = await _divDisThana.GetThanaV2();

                foreach (DivisionModelV2 item in divList)
                {
                    item.DistrictModel = disList.Where(a => a.DIVISIONID == item.DIVISIONID);

                    foreach (DistrictModelV2 item2 in item.DistrictModel)
                    {
                        item2.ThanaModel = thanaList.Where(a => a.DISTRICTID == item2.DISTRICTID);
                    }
                }
                divDisThanaRes.data = divList;
                divDisThanaRes.isError = false;
                divDisThanaRes.message = MessageCollection.Success;
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    divDisThanaRes.data = null;
                    divDisThanaRes.isError = true;
                    divDisThanaRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    divDisThanaRes.isError = true;
                    divDisThanaRes.message = ex.Message;
                }
            }
            return Ok(divDisThanaRes);
        }
        #endregion


        #region Get Status
        /// Send Order
        /// <summary>
        /// This API is used for Status.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>message</returns>
        //[ResponseType(typeof(GetStatusResponse))]
        [HttpPost]
        [Route("GetStatusV1")]
        public async Task<IActionResult> GetStatusV1([FromBody] StatusRequest model)
        {
            GetStatusResponse statusRes = new GetStatusResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                statusRes = await _bllOrder.GetStatus(model);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    statusRes.status = null;
                    statusRes.result = false;
                    statusRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                }
                catch (Exception)
                {
                    statusRes.status = null;
                    statusRes.result = false;
                    statusRes.message = ex.Message;

                }
            }
            return Ok(statusRes);
        }

        /// Send Order
        /// <summary>
        /// This API is used for Status.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>message</returns>
        //[ResponseType(typeof(GetStatusResponse))]
        [HttpPost]
        [Route("GetStatusV2")]
        public async Task<IActionResult> GetStatusV2([FromBody] StatusRequest model)
        {
            GetStatusResponse statusRes = new GetStatusResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                statusRes = await _bllOrder.GetStatus(model);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    statusRes.status = null;
                    statusRes.result = false;
                    statusRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                }
                catch (Exception)
                {
                    statusRes.status = null;
                    statusRes.result = false;
                    statusRes.message = ex.Message;

                }
            }
            return Ok(statusRes);
        }

        /// Send Order
        /// <summary>
        /// This API is used for Status.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>message</returns>
        //[ResponseType(typeof(GetStatusResponse))]
        [HttpPost]
        [Route("GetStatusV3")]
        public async Task<IActionResult> GetStatusV3([FromBody] StatusRequest model)
        {
            GetStatusResponseRevamp statusRes = new GetStatusResponseRevamp();
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

                statusRes = await _bllOrder.GetStatusV2(model);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    statusRes.data = new GetStatusResponseDataRevamp()
                    {
                        status = null
                    };
                    statusRes.isError = true;
                    statusRes.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                }
                catch (Exception)
                {
                    statusRes.data = new GetStatusResponseDataRevamp()
                    {
                        status = null
                    };
                    statusRes.isError = true;
                    statusRes.message = ex.Message;

                }
            }
            return Ok(statusRes);
        }


        #endregion


        #region Get Purpose Numbers
        /// <summary>
        /// This API is used for Getting DivDisThana
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        //[GzipCompression]
        [HttpPost]
        [Route("GetPurposeNumbers")]
        public async Task<IActionResult> GetPurposeNumbers(RAGetPurposeRequest model)
        {
            PurposeNumberReponse pnRes = new PurposeNumberReponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                pnRes = await _bllCommon.GetPurposeNumbers(model);

                return Ok(pnRes);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
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
                        result = false,
                        message = ex.Message
                    });
                }
            }

        }

        /// <summary>
        /// This API is used for Getting DivDisThana
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        //[GzipCompression]
        [HttpPost]
        [Route("GetPurposeNumbersV2")]
        public async Task<IActionResult> GetPurposeNumbersV2(RAGetPurposeRequest model)
        {
            PurposeNumberReponse pnRes = new PurposeNumberReponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                pnRes = await _bllCommon.GetPurposeNumbers(model);

                return Ok(pnRes);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

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
                        result = false,
                        message = ex.Message
                    });
                }
            }

        }

        /// <summary>
        /// This API is used for Getting DivDisThana
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        //[GzipCompression]
        [HttpPost]
        [Route("GetPurposeNumbersV3")]
        public async Task<IActionResult> GetPurposeNumbersV3(RAGetPurposeRequest model)
        {
            PurposeNumberReponseRev pnRes = new PurposeNumberReponseRev();
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

                pnRes = await _bllCommon.GetPurposeNumbersV2(model);

                return Ok(pnRes);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    });
                }
                catch (Exception)
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
        }

        #endregion


        #region Get  Rejected QC Orders 


        /// RejectedQCOrders
        /// <summary>
        /// Get rejected orders by reseller name/id.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        //[ResponseType(typeof(RejectedOrdersResponse))]
        [GzipCompression]
        [HttpPost]
        [Route("GetRejectedQCOrdersV1")]
        public async Task<IActionResult> GetRejectedQCOrdersV1(RejectedOrdersRequest model)
        {
            List<VMRejectedOrder> raRespDataList = new List<VMRejectedOrder>();
            RejectedOrdersResponse raResp = new RejectedOrdersResponse();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                string qcStatus = FixedValueCollection.QCStatusRejected;
                apiUrl = String.Format(GetAPICollection.GetRejectedQCOrders, qcStatus, model.retailer_id);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                object dbssResp = new object();
                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    log.res_blob = _blJson.GetGenericJsonData(dbssResp);
                    log.is_success = 1;

                    var dbssRespModel = JsonConvert.DeserializeObject<RejectedOrdersRootobject>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data.Count > 0)
                        {
                            for (int i = 0; i < dbssRespModel.data.Count; i++)
                            {
                                CustomerInfoResponse customerInfo = await GetCustomerInfo(dbssRespModel.data[i].relationships.usercustomer.links.related
                                                                                , model.retailer_id);
                                var rejectedOrder = await _dbssToRaParse.RejectionOrdersParsing(dbssRespModel.data[i].id
                                                                                        , dbssRespModel.data[i].relationships.usercustomer.data.id /*customer Id*/
                                                                                        , dbssRespModel.data[i].attributes
                                                                                        , customerInfo.CustomerInfo
                                                                                        , customerInfo.CustomerAddressInfo);
                                raRespDataList.Add(rejectedOrder);
                            }


                            if (raRespDataList.Count > 0)
                            {
                                raResp.data = raRespDataList;
                                raResp.result = true;
                                raResp.message = MessageCollection.Success;
                            }
                        }
                        else
                        {
                            raResp.data = null;
                            raResp.result = false;
                            raResp.message = MessageCollection.NoDataFound;
                        }
                    }
                    else
                    {
                        raResp.data = null;
                        raResp.result = false;
                        raResp.message = MessageCollection.NoDataFound;
                    }

                }
                else
                {
                    raResp.data = null;
                    raResp.result = false;
                    raResp.message = "Unable to get data from DBSS API.";
                }
            }
            catch (Exception ex)
            {
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;
                log.is_success = 0;

                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.data = null;
                    raResp.result = false;

                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                }
                catch (Exception)
                {

                    raResp.data = null;
                    raResp.result = false;
                    raResp.message = ex.Message;
                }

            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                log.method_name = "GetRejectedQCOrders";

                string resStr = string.Empty;

                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }
                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, resStr));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);

            }
            return Ok(raResp);
        }

        /// RejectedQCOrders
        /// <summary>
        /// Get rejected orders by reseller name/id.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        //[ResponseType(typeof(RejectedOrdersResponse))]
        [GzipCompression]
        [HttpPost]
        [Route("GetRejectedQCOrdersV2")]
        public async Task<IActionResult> GetRejectedQCOrdersV2(RejectedOrdersRequest model)
        {
            List<VMRejectedOrder> raRespDataList = new List<VMRejectedOrder>();
            RejectedOrdersResponse raResp = new RejectedOrdersResponse();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                string qcStatus = FixedValueCollection.QCStatusRejected;
                apiUrl = String.Format(GetAPICollection.GetRejectedQCOrders, qcStatus, model.retailer_id);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                object dbssResp = new object();
                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    log.res_blob = _blJson.GetGenericJsonData(dbssResp);
                    log.is_success = 1;

                    var dbssRespModel = JsonConvert.DeserializeObject<RejectedOrdersRootobject>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data.Count > 0)
                        {
                            for (int i = 0; i < dbssRespModel.data.Count; i++)
                            {
                                CustomerInfoResponse customerInfo = await GetCustomerInfo(dbssRespModel.data[i].relationships.usercustomer.links.related
                                                                                , model.retailer_id);
                                var rejectedOrder = await _dbssToRaParse.RejectionOrdersParsing(dbssRespModel.data[i].id
                                                                                        , dbssRespModel.data[i].relationships.usercustomer.data.id /*customer Id*/
                                                                                        , dbssRespModel.data[i].attributes
                                                                                        , customerInfo.CustomerInfo
                                                                                        , customerInfo.CustomerAddressInfo);
                                raRespDataList.Add(rejectedOrder);
                            }


                            if (raRespDataList.Count > 0)
                            {
                                raResp.data = raRespDataList;
                                raResp.result = true;
                                raResp.message = MessageCollection.Success;
                            }
                        }
                        else
                        {
                            raResp.data = null;
                            raResp.result = false;
                            raResp.message = MessageCollection.NoDataFound;
                        }
                    }
                    else
                    {
                        raResp.data = null;
                        raResp.result = false;
                        raResp.message = MessageCollection.NoDataFound;
                    }
                }
                else
                {
                    raResp.data = null;
                    raResp.result = false;
                    raResp.message = "Unable to get data from DBSS API.";
                }
            }
            catch (Exception ex)
            {
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;
                log.is_success = 0;

                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.data = null;
                    raResp.result = false;

                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                }
                catch (Exception)
                {

                    raResp.data = null;
                    raResp.result = false;
                    raResp.message = ex.Message;
                }

            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                log.method_name = "GetRejectedQCOrders";

                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, resStr));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);

            }
            return Ok(raResp);
        }

        /// RejectedQCOrders
        /// <summary>
        /// Get rejected orders by reseller name/id.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        //[ResponseType(typeof(RejectedOrdersResponse))]
        [GzipCompression]
        [HttpPost]
        [Route("GetRejectedQCOrdersV3")]
        public async Task<IActionResult> GetRejectedQCOrdersV3(RejectedOrdersRequest model)
        {
            List<VMRejectedOrder> raRespDataList = new List<VMRejectedOrder>();
            RejectedOrdersResponseRev raResp = new RejectedOrdersResponseRev();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
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
                        if (!model.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                string qcStatus = FixedValueCollection.QCStatusRejected;
                apiUrl = String.Format(GetAPICollection.GetRejectedQCOrders, qcStatus, model.retailer_id);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);

                object dbssResp = new object();
                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    log.res_blob = _blJson.GetGenericJsonData(dbssResp);
                    log.is_success = 1;

                    var dbssRespModel = JsonConvert.DeserializeObject<RejectedOrdersRootobject>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data.Count > 0)
                        {
                            for (int i = 0; i < dbssRespModel.data.Count; i++)
                            {
                                CustomerInfoResponse customerInfo = await GetCustomerInfo(dbssRespModel.data[i].relationships.usercustomer.links.related
                                                                                , model.retailer_id);
                                var rejectedOrder = await _dbssToRaParse.RejectionOrdersParsing(dbssRespModel.data[i].id
                                                                                        , dbssRespModel.data[i].relationships.usercustomer.data.id /*customer Id*/
                                                                                        , dbssRespModel.data[i].attributes
                                                                                        , customerInfo.CustomerInfo
                                                                                        , customerInfo.CustomerAddressInfo);
                                raRespDataList.Add(rejectedOrder);
                            }


                            if (raRespDataList.Count > 0)
                            {
                                raResp.data = raRespDataList;
                                raResp.isError = false;
                                raResp.message = MessageCollection.Success;
                            }
                        }
                        else
                        {
                            raResp.data = null;
                            raResp.isError = true;
                            raResp.message = MessageCollection.NoDataFound;
                        }
                    }
                    else
                    {
                        raResp.data = null;
                        raResp.isError = true;
                        raResp.message = MessageCollection.NoDataFound;
                    }
                }
                else
                {
                    raResp.data = null;
                    raResp.isError = true;
                    raResp.message = "Unable to get data from DBSS API.";
                }
            }
            catch (Exception ex)
            {
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.res_time = DateTime.Now;
                log.is_success = 0;

                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.data = null;
                    raResp.isError = true;

                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                }
                catch (Exception)
                {

                    raResp.data = null;
                    raResp.isError = true;
                    raResp.message = ex.Message;
                }

            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                log.method_name = "GetRejectedQCOrdersV3";

                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, resStr));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);

            }
            return Ok(raResp);
        }
        #endregion


        #region Get Customers Info
        private async Task<CustomerInfoResponse> GetCustomerInfo(string apiUrl, string retailerId)
        {
            CustomerInfoResponseRootobject? dbssRespModel = new CustomerInfoResponseRootobject();
            CustomerInfoResponse customerInfoResp = new CustomerInfoResponse();
            string? txtApiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                log.req_blob = _blJson.GetGenericJsonData(JsonConvert.SerializeObject(apiUrl));


                object dbssResp = new object();
                txtApiUrl = AppSettingsWrapper.ApiBaseUrl + apiUrl;
                log.req_time = DateTime.Now;

                dbssResp = await _apiReq.HttpGetRequest(AppSettingsWrapper.ApiBaseUrl + apiUrl);

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    log.is_success = 1;

                    dbssRespModel = JsonConvert.DeserializeObject<CustomerInfoResponseRootobject>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data.attributes != null
                            && dbssRespModel.data.relationships.addresses.links.related != null)
                        {
                            customerInfoResp.CustomerInfo = dbssRespModel.data.attributes;
                            customerInfoResp.CustomerAddressInfo = await GetCustomerAddress(dbssRespModel.data.relationships.addresses.links.related, retailerId);
                        }
                    }
                }
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
                catch (Exception ex2)
                {
                    throw ex2;
                }
            }
            finally
            {
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = retailerId;
                log.method_name = "GetCustomerInfo";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, resStr));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return customerInfoResp;
        }
        #endregion


        #region Get-Customer-Address
        private async Task<CustomerAddressResponseAttributes> GetCustomerAddress(string apiUrl, string retailerId)
        {
            CustomerAddressResponseRootobject? dbssRespModel = new CustomerAddressResponseRootobject();
            CustomerAddressResponseAttributes customerAddress = new CustomerAddressResponseAttributes();
            string? txtApiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                log.req_blob = _blJson.GetGenericJsonData(JsonConvert.SerializeObject(apiUrl)); ;

                object dbssResp = new object();
                txtApiUrl = AppSettingsWrapper.ApiBaseUrl + apiUrl;
                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(AppSettingsWrapper.ApiBaseUrl + apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);
                if (dbssResp != null)
                {
                    log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                    log.is_success = 1;

                    dbssRespModel = JsonConvert.DeserializeObject<CustomerAddressResponseRootobject>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data.Count < 1)
                        {
                            new Exception("No data found. API url: " + AppSettingsWrapper.ApiBaseUrl + apiUrl);
                        }
                        else
                        {
                            if (dbssRespModel != null && dbssRespModel.data != null && dbssRespModel.data.Any())
                            {
                                var firstDataItem = dbssRespModel.data.FirstOrDefault();
                                if (firstDataItem != null)
                                {
                                    customerAddress = firstDataItem.attributes;
                                }
                            }
                        }
                    }
                    else
                    {
                        new Exception("No data found. API url: " + AppSettingsWrapper.ApiBaseUrl + apiUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                log.is_success = 0;
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;
                }
                catch (Exception ex2)
                {
                    throw new Exception(ex2.Message);
                }
            }
            finally
            {

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = retailerId;
                log.method_name = "GetCustomerAddress";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }
                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, resStr));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return customerAddress;
        }
        #endregion


        #region Customer-Update
        /// RejectedQCOrders
        /// <summary>
        /// Update customer info.
        /// </summary> 
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("CustomerInfoUpdateV1")]
        public async Task<IActionResult> CustomerInfoUpdateV1(RACustomerInfoUpdateRequest model)
        {
            RACommonResponse raResp = new RACommonResponse();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                BLLRAToDBSSParse rAParse = new BLLRAToDBSSParse();

                var resRootObj = rAParse.CustomerInfoReqParsing(model);

                apiUrl = String.Format(PatchAPICollection.CustomerInfoUpdate, model.customer_id);

                log.req_blob = _blJson.GetGenericJsonData(resRootObj);
                log.req_time = DateTime.Now;

                var dbssResp = await _apiReq.HttpPatchRequest(resRootObj, apiUrl);

                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);
                txtResp = Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    log.is_success = 1;

                    var dbssRespModel = JsonConvert.DeserializeObject<CustomerUpdateRespRootobject>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var customerUpdateRAResp = _dbssToRaParse.CustomerUpdateRespParsing(dbssRespModel) ?? new RACommonResponse();

                            if (customerUpdateRAResp.result == false)
                            {
                                return Ok(new RACommonResponse()
                                {
                                    result = true,
                                    message = "Customer info updated failed!"

                                });
                            }

                            var qcStatusUpdateRAResp = await QCStatusUpdate(model.quality_control_id, model.retailer_id, model.mobile_number) ?? new RACommonResponse();

                            if (qcStatusUpdateRAResp.result == true)
                            {
                                return Ok(new RACommonResponse()
                                {
                                    result = true,
                                    message = "Customer updated successfully!"

                                });
                            }
                            else
                            {
                                return Ok(new RACommonResponse()
                                {
                                    result = false,
                                    message = MessageCollection.QCStatusUpdateFailed
                                });
                            }
                        }
                    }
                    else
                    {
                        raResp = new RACommonResponse()
                        {
                            result = false,
                            message = MessageCollection.NoDataFound
                        };
                    }
                }
                else
                {
                    raResp = new RACommonResponse()
                    {
                        result = false,
                        message = "No response got from DBSS API!"
                    };
                }

                return Ok(raResp);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;

                string resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                ErrorDescription error = null;

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    try
                    {
                        var respObj1 = JsonConvert.DeserializeObject<Object>(resp);
                        JObject respObj2 = (JObject)respObj1;

                        error = await _bllLog.ManageException(respObj2["errors"] != null
                                                    && respObj2["errors"].ToString() != "" ? respObj2["errors"].ToString() : ex.Message, ex.HResult, "BIA");
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");
                        }
                        catch (Exception)
                        {
                            raResp = new RACommonResponse()
                            {
                                result = false,
                                message = ex.Message
                            };
                            return Ok(raResp);
                        }
                    }

                }
                else
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    }
                    catch (Exception)
                    {
                        raResp = new RACommonResponse()
                        {
                            result = false,
                            message = ex.Message
                        };

                        return Ok(raResp);
                    }

                }

                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;
                raResp.result = false;

                var webErrorResp = ex.Response as HttpWebResponse;
                if (webErrorResp != null)
                {
                    if (webErrorResp.StatusCode == HttpStatusCode.BadRequest)
                    {
                        raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? MessageCollection.PleaseTryAgain + error.error_description
                                                                                        : MessageCollection.PleaseTryAgain + error.error_custom_msg;
                    }
                    else
                    {
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                    }
                }
                else
                {
                    if (_bio.isDBSSErrorOccurred(ex))
                    {
                        raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                    }
                    else
                    {
                        raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    }
                }
                return Ok(raResp);
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.msisdn = model.mobile_number;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.is_success = 0;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return Ok(raResp);
                }
                catch (Exception)
                {
                    raResp.result = false;
                    raResp.message = ex.Message;
                    return Ok(raResp);
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(model.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.BI);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                log.method_name = "CustomerInfoUpdateV1";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, resStr));
                //logThread.Start();
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        /// RejectedQCOrders
        /// <summary>
        /// Update customer info.
        /// </summary> 
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("CustomerInfoUpdateV2")]
        public async Task<IActionResult> CustomerInfoUpdateV2(RACustomerInfoUpdateRequest model)
        {
            RACommonResponse raResp = new RACommonResponse();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                BLLRAToDBSSParse rAParse = new BLLRAToDBSSParse();

                var resRootObj = rAParse.CustomerInfoReqParsing(model);

                apiUrl = String.Format(PatchAPICollection.CustomerInfoUpdate, model.customer_id);

                log.req_blob = _blJson.GetGenericJsonData(resRootObj);
                log.req_time = DateTime.Now;

                var dbssResp = await _apiReq.HttpPatchRequest(resRootObj, apiUrl);

                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);
                txtResp = Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    log.is_success = 1;

                    var dbssRespModel = JsonConvert.DeserializeObject<CustomerUpdateRespRootobject>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var customerUpdateRAResp = _dbssToRaParse.CustomerUpdateRespParsing(dbssRespModel) ?? new RACommonResponse();

                            if (customerUpdateRAResp.result == false)
                            {
                                return Ok(new RACommonResponse()
                                {
                                    result = true,
                                    message = "Customer info updated failed!"
                                });
                            }

                            var qcStatusUpdateRAResp = await QCStatusUpdate(model.quality_control_id, model.retailer_id, model.mobile_number) ?? new RACommonResponse();

                            if (qcStatusUpdateRAResp.result == true)
                            {
                                return Ok(new RACommonResponse()
                                {
                                    result = true,
                                    message = "Customer updated successfully!"

                                });
                            }
                            else
                            {
                                return Ok(new RACommonResponse()
                                {
                                    result = false,
                                    message = MessageCollection.QCStatusUpdateFailed

                                });
                            }
                        }
                    }
                    else
                    {
                        raResp = new RACommonResponse()
                        {
                            result = false,
                            message = MessageCollection.NoDataFound
                        };
                    }
                }
                else
                {
                    raResp = new RACommonResponse()
                    {
                        result = false,
                        message = "No response got from DBSS API!"
                    };
                }

                return Ok(raResp);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;

                string resp = String.Empty;
                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                ErrorDescription? error = null;

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    try
                    {
                        var respObj1 = JsonConvert.DeserializeObject<Object>(resp);
                        JObject respObj2 = (JObject)respObj1;

                        error = await _bllLog.ManageException(respObj2["errors"] != null
                                                    && respObj2["errors"].ToString() != "" ? respObj2["errors"].ToString() : ex.Message, ex.HResult, "BIA");
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");
                        }
                        catch (Exception)
                        {
                            raResp = new RACommonResponse()
                            {
                                result = false,
                                message = ex.Message
                            };
                            return Ok(raResp);
                        }
                    }

                }
                else
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    }
                    catch (Exception)
                    {
                        raResp = new RACommonResponse()
                        {
                            result = false,
                            message = ex.Message
                        };

                        return Ok(raResp);
                    }

                }

                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;
                raResp.result = false;

                var webErrorResp = ex.Response as HttpWebResponse;
                if (webErrorResp != null)
                {
                    if (webErrorResp.StatusCode == HttpStatusCode.BadRequest)
                    {
                        raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? MessageCollection.PleaseTryAgain + error.error_description
                                                                                        : MessageCollection.PleaseTryAgain + error.error_custom_msg;
                    }
                    else
                    {
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                    }
                }
                else
                {
                    if (_bio.isDBSSErrorOccurred(ex))
                    {
                        raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                    }
                    else
                    {
                        raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    }
                }
                return Ok(raResp);
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.msisdn = model.mobile_number;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.is_success = 0;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return Ok(raResp);
                }
                catch (Exception)
                {
                    raResp.result = false;
                    raResp.message = ex.Message;
                    return Ok(raResp);
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(model.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.BI);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                log.method_name = "CustomerInfoUpdateV1";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        /// RejectedQCOrders
        /// <summary>
        /// Update customer info.
        /// </summary> 
        /// <param name="model"></param>
        /// <returns>Order request token id</returns>
        [HttpPost]
        [Route("CustomerInfoUpdateV3")]
        public async Task<IActionResult> CustomerInfoUpdateV3(RACustomerInfoUpdateRequest model)
        {
            RACommonResponseRevamp raResp = new RACommonResponseRevamp();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
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
                        if (!model.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                BLLRAToDBSSParse rAParse = new BLLRAToDBSSParse();

                var resRootObj = rAParse.CustomerInfoReqParsing(model);

                apiUrl = String.Format(PatchAPICollection.CustomerInfoUpdate, model.customer_id);

                log.req_blob = _blJson.GetGenericJsonData(resRootObj);
                log.req_time = DateTime.Now;

                var dbssResp = await _apiReq.HttpPatchRequest(resRootObj, apiUrl);

                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);
                txtResp = Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    log.is_success = 1;

                    var dbssRespModel = JsonConvert.DeserializeObject<CustomerUpdateRespRootobject>(dbssResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var customerUpdateRAResp = _dbssToRaParse.CustomerUpdateRespParsing(dbssRespModel) ?? new RACommonResponse();

                            if (customerUpdateRAResp.result == false)
                            {
                                return Ok(new RACommonResponseRevamp()
                                {
                                    isError = true,
                                    message = "Customer info updated failed!",
                                    data = new Datas()
                                    {
                                        isEsim = 0,
                                        request_id = "0"
                                    }
                                });
                            }

                            var qcStatusUpdateRAResp = await QCStatusUpdate(model.quality_control_id, model.retailer_id, model.mobile_number) ?? new RACommonResponse();

                            if (qcStatusUpdateRAResp.result == true)
                            {
                                return Ok(new RACommonResponseRevamp()
                                {
                                    isError = false,
                                    message = "Customer updated successfully!",
                                    data = new Datas()
                                    {
                                        isEsim = 0,
                                        request_id = "0"
                                    }
                                });
                            }
                            else
                            {
                                return Ok(new RACommonResponseRevamp()
                                {
                                    isError = true,
                                    message = MessageCollection.QCStatusUpdateFailed,
                                    data = new Datas()
                                    {
                                        isEsim = 0,
                                        request_id = "0"
                                    }
                                });
                            }
                        }
                    }
                    else
                    {
                        raResp = new RACommonResponseRevamp()
                        {
                            isError = true,
                            message = MessageCollection.NoDataFound,
                            data = new Datas()
                            {
                                isEsim = 0,
                                request_id = "0"
                            }
                        };
                    }
                }
                else
                {
                    raResp = new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = "No response got from DBSS API!",
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    };
                }

                return Ok(raResp);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;

                string resp = String.Empty;
                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                ErrorDescription? error = null;

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    try
                    {
                        var respObj1 = JsonConvert.DeserializeObject<Object>(resp);
                        JObject respObj2 = (JObject)respObj1;

                        error = await _bllLog.ManageException(respObj2["errors"] != null
                                                    && respObj2["errors"].ToString() != "" ? respObj2["errors"].ToString() : ex.Message, ex.HResult, "BIA");
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");
                        }
                        catch (Exception)
                        {
                            raResp = new RACommonResponseRevamp()
                            {
                                isError = true,
                                message = ex.Message,
                                data = new Datas()
                                {
                                    isEsim = 0,
                                    request_id = "0"
                                }
                            };
                            return Ok(raResp);
                        }
                    }

                }
                else
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    }
                    catch (Exception)
                    {
                        raResp = new RACommonResponseRevamp()
                        {
                            isError = true,
                            message = ex.Message,
                            data = new Datas()
                            {
                                isEsim = 0,
                                request_id = "0"
                            }
                        };

                        return Ok(raResp);
                    }

                }

                log.is_success = 0;
                log.error_code = error.error_code ?? String.Empty;
                log.error_source = error.error_source ?? String.Empty;
                log.message = error.error_description ?? String.Empty;
                raResp.isError = true;

                var webErrorResp = ex.Response as HttpWebResponse;
                if (webErrorResp != null)
                {
                    if (webErrorResp.StatusCode == HttpStatusCode.BadRequest)
                    {
                        raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? MessageCollection.PleaseTryAgain + error.error_description
                                                                                        : MessageCollection.PleaseTryAgain + error.error_custom_msg;
                    }
                    else
                    {
                        if (_bio.isDBSSErrorOccurred(ex))
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                        }
                        else
                        {
                            raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        }
                    }
                }
                else
                {
                    if (_bio.isDBSSErrorOccurred(ex))
                    {
                        raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? FixedValueCollection.DBSSError + error.error_description : FixedValueCollection.DBSSError + error.error_custom_msg;
                    }
                    else
                    {
                        raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    }
                }
                return Ok(raResp);
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.msisdn = model.mobile_number;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    log.is_success = 0;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    raResp.isError = true;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    return Ok(raResp);
                }
                catch (Exception)
                {
                    raResp.isError = true;
                    raResp.message = ex.Message;
                    return Ok(raResp);
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(model.mobile_number);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.BI);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                log.method_name = "CustomerInfoUpdateV3";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        #endregion


        #region QC Status Update 
        public async Task<RACommonResponse> QCStatusUpdate(string quality_control_id, string retailer_id, string msisdn)
        {
            RACommonResponse raResp = null;
            string? apiUrl = "", txtResp = "";
            object dbssResp = new object();
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                BLLRAToDBSSParse rAParse = new BLLRAToDBSSParse();

                var reqRootObj = rAParse.QCStatusUpdateReqParsing(quality_control_id, retailer_id);

                apiUrl = String.Format(PatchAPICollection.QCStatusUpdate);

                log.req_blob = _blJson.GetGenericJsonData(reqRootObj);
                log.req_time = DateTime.Now;

                dbssResp = await _apiReq.HttpPatchRequest(reqRootObj, apiUrl);

                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                txtResp = Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    log.is_success = 1;

                    var dbssRespModel = JsonConvert.DeserializeObject<QCStatusResponseRootobject>(dbssResp.ToString());
                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            if (dbssRespModel.data.Count > 0)
                            {
                                raResp = _dbssToRaParse.QCUpdateRespParsing(dbssRespModel) ?? new RACommonResponse();
                            }
                            else
                            {
                                raResp = new RACommonResponse()
                                {
                                    result = false,
                                    message = MessageCollection.QCStatusUpdateFailed
                                };
                            }
                        }
                        else
                        {
                            raResp = new RACommonResponse()
                            {
                                result = false,
                                message = MessageCollection.QCStatusUpdateFailed
                            };
                        }
                    }
                    else
                    {
                        raResp = new RACommonResponse()
                        {
                            result = false,
                            message = MessageCollection.QCStatusUpdateFailed
                        };
                    }
                }
                else
                {
                    raResp = new RACommonResponse()
                    {
                        result = false,
                        message = MessageCollection.QCStatusUpdateFailed
                    };
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

                    raResp = new RACommonResponse()
                    {
                        result = false,
                        message = ex.Message
                    };
                }
                catch (Exception)
                {
                    raResp = new RACommonResponse()
                    {
                        result = false,
                        message = ex.Message
                    };
                }
            }
            finally
            {
                log.msisdn = msisdn;
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.BI);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = retailer_id;
                log.method_name = "QCStatusUpdate";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, resStr));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return raResp;
        }
        #endregion


        #region Get-Activity-Log-Data
        /// <summary>
        /// Get ACTIVITY LOG/ PENDING LIST/ ACTIVATION LIST by and type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[ResponseType(typeof(ActivityLogResponse))]
        [GzipCompression]
        [HttpPost]
        [Route("ActivityLogData")]
        public async Task<IActionResult> GetActivityLogData(RAOrderActivityRequest model)
        {
            ActivityLogResponse activityLogData = new ActivityLogResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                string user_id = _bllCommon.GetUserNameFromSessionToken(model.session_token);

                activityLogData = await _bllCommon.GetActivityLogData(model.activity_type_id, user_id);

                return Ok(activityLogData);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = ex.Message
                    });
                }
            }
        }

        /// <summary>
        /// Get ACTIVITY LOG/ PENDING LIST/ ACTIVATION LIST by and type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[ResponseType(typeof(ActivityLogResponse))]
        [GzipCompression]
        [HttpPost]
        [Route("ActivityLogDataV2")]
        public async Task<IActionResult> GetActivityLogDataV2(RAOrderActivityRequest model)
        {
            ActivityLogResponse activityLogData = new ActivityLogResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                string user_id = _bllCommon.GetUserNameFromSessionTokenV2(model.session_token);

                activityLogData = await _bllCommon.GetActivityLogDataV2(model.activity_type_id, user_id);

                return Ok(activityLogData);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = ex.Message
                    });
                }
            }
        }

        /// <summary>
        /// Get ACTIVITY LOG/ PENDING LIST/ ACTIVATION LIST by and type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[ResponseType(typeof(ActivityLogResponse))]
        [GzipCompression]
        [HttpPost]
        [Route("ActivityLogDataV3")]
        public async Task<IActionResult> GetActivityLogDataV3(RAOrderActivityRequest model)
        {
            ActivityLogResponseRevamp activityLogData = new ActivityLogResponseRevamp();
            string user_id = string.Empty;
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
                        user_id = security.UserName;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                activityLogData = await _bllCommon.GetActivityLogDataV3(model.activity_type_id, user_id);

                return Ok(activityLogData);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new ActivityLogResponseRevamp()
                    {
                        isError = true,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description,
                        data = new List<VMActivityLogRevamp>()
                        {

                        }
                    });
                }
                catch (Exception)
                {
                    return Ok(new ActivityLogResponseRevamp()
                    {
                        isError = true,
                        message = ex.Message,
                        data = new List<VMActivityLogRevamp>()
                        {

                        }
                    });
                }
            }
        }


        #endregion


        #region Get-Order-Info-By-TokenId

        [HttpPost]
        [ValidateModel]
        [Route("OrderInfoByTokenId")]
        public async Task<IActionResult> GetOrderInfoByTokenId(RAGetCustomerInfoByTokenNoRequest model)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                var result = await _bllOrder.GetOrderInfoByTokenNo(model.token_id);


                if (result.result == false)
                {
                    return Ok(result);
                }
                else
                {
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                return Ok(new RACommonResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }


        [HttpPost]
        [ValidateModel]
        [Route("OrderInfoByTokenIdV2")]
        public async Task<IActionResult> GetOrderInfoByTokenIdV2(RAGetCustomerInfoByTokenNoRequest model)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);


                var result = await _bllOrder.GetOrderInfoByTokenNo(model.token_id);


                if (result.result == false)
                {
                    return Ok(result);
                }
                else
                {
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                return Ok(new RACommonResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("OrderInfoByTokenIdV3")]
        public async Task<IActionResult> GetOrderInfoByTokenIdV3(RAGetCustomerInfoByTokenNoRequest model)
        {
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

                var result = await _bllOrder.GetOrderInfoByTokenNoV2(model.token_id);

                if (result.isError == true)
                {
                    return Ok(result);
                }
                else
                {
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
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
        #endregion


        #region MSISDN validation Paired 
        /// <summary>
        /// This API is used for MSISDN validation for paired MSISDN.
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(PaiedMSISDNCheckResponse))]
        [HttpPost]
        [Route("ValidatePairedMSISDNV1")]
        public async Task<IActionResult> ValidatePairedMSISDNV2([FromBody] PairedMSISDNCheckRequest msisdnCheckReqest)
        {
            PaiedMSISDNCheckResponse raRespModel = new PaiedMSISDNCheckResponse();
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

                string a = GetAPICollection.PairedMSISDNValidation;


                apiUrl = String.Format(GetAPICollection.PairedMSISDNValidation, msisdnCheckReqest.mobile_number);


                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                var dbssRespObj = JsonConvert.DeserializeObject<PairedMSISDNValidationResponseRootobject>(dbssResp.ToString());
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssRespObj.data == null)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = "MSISDN: " + MessageCollection.NoDataFound
                    });
                }

                log.is_success = 1;

                raRespModel = _dbssToRaParse.PairedMSISDNReqParsing2(dbssRespObj);

                if (raRespModel.result == false)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = raRespModel.message
                    });
                }

                #region SIM category check
                string paymentType = await GetPaymentTypeFromGetSubscriptionType(raRespModel.subscription_type_code, msisdnCheckReqest.retailer_id);
                if (GetSIMCategoryByPaymentType(paymentType) != msisdnCheckReqest.sim_category)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = String.Format(MessageCollection.SIMCategoryMismatch, msisdnCheckReqest.sim_category == (int)EnumSimCategory.Prepaid ?
                                                                                       FixedValueCollection.PaymentTypePrepaid : FixedValueCollection.PaymentTypePostpaid)
                    });
                }
                #endregion

                #region SIM Validation

                var simResp = await _bio.CheckSIMNumber3(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = raRespModel.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.NewConnection, true, msisdnCheckReqest.sim_category, "");

                if (simResp.result == false)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = simResp.message
                    });
                }
                #endregion

                if (raRespModel.result == true)
                {
                    RACommonResponse raCommon = await _bio.CheckCherishedNumber(msisdnCheckReqest, "ValidatePairedMSISDNV3");

                    if (raCommon.result == true)
                    {
                        raRespModel.result = true;
                        raRespModel.message = raCommon.message;
                    }
                    else
                    {
                        raRespModel.result = false;
                        raRespModel.message = raCommon.message;
                    }

                    return Ok(raRespModel);

                }
                //raRespModel.result = true;
                //raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                return Ok(raRespModel);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);


                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                    catch (Exception)
                    {
                        raRespModel.result = false;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                }

                string resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.result = false;
                            if (_bio.isDBSSErrorOccurred(ex))
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

                            return Ok(raRespModel);
                        }
                        catch (Exception)
                        {
                            raRespModel.result = false;
                            raRespModel.message = ex.Message;

                            log.error_code = error.error_code ?? String.Empty;
                            log.error_source = error.error_source ?? String.Empty;
                            log.message = error.error_description ?? String.Empty;

                            return Ok(raRespModel);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
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

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);

                        return Ok(raRespModel);
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);


                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raRespModel.result = false;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raRespModel);
                }
                catch (Exception)
                {
                    raRespModel.result = false;
                    raRespModel.message = ex.Message;

                    return Ok(raRespModel);
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidatePairedMSISDNV2";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }


        //[ResponseType(typeof(PaiedMSISDNCheckResponse))]
        [HttpPost]
        [Route("ValidatePairedMSISDNV2")]
        public async Task<IActionResult> ValidatePairedMSISDNV3([FromBody] PairedMSISDNCheckRequest msisdnCheckReqest)
        {
            PaiedMSISDNCheckResponse raRespModel = new PaiedMSISDNCheckResponse();
            string apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(msisdnCheckReqest.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                if (msisdnCheckReqest.mobile_number.Substring(0, 2) != FixedValueCollection.MSISDNCountryCode)
                {
                    msisdnCheckReqest.mobile_number = FixedValueCollection.MSISDNCountryCode + msisdnCheckReqest.mobile_number;
                }

                string a = GetAPICollection.PairedMSISDNValidation;


                apiUrl = String.Format(GetAPICollection.PairedMSISDNValidation, msisdnCheckReqest.mobile_number);


                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                var dbssRespObj = JsonConvert.DeserializeObject<PairedMSISDNValidationResponseRootobject>(dbssResp.ToString());
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                if (dbssRespObj == null)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = "MSISDN: " + MessageCollection.NoDataFound
                    });
                }
                if (dbssRespObj.data == null)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = "MSISDN: " + MessageCollection.NoDataFound
                    });
                }

                log.is_success = 1;

                raRespModel = _dbssToRaParse.PairedMSISDNReqParsing2(dbssRespObj);

                if (raRespModel.result == false)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = raRespModel.message
                    });
                }

                #region SIM category check
                string paymentType = await GetPaymentTypeFromGetSubscriptionType(raRespModel.subscription_type_code, msisdnCheckReqest.retailer_id);
                if (GetSIMCategoryByPaymentType(paymentType) != msisdnCheckReqest.sim_category)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = String.Format(MessageCollection.SIMCategoryMismatch, msisdnCheckReqest.sim_category == (int)EnumSimCategory.Prepaid ?
                                                                                       FixedValueCollection.PaymentTypePrepaid : FixedValueCollection.PaymentTypePostpaid)
                    });
                }
                #endregion

                #region SIM Validation

                var simResp = await _bio.CheckSIMNumber3(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = raRespModel.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.NewConnection, true, msisdnCheckReqest.sim_category, "");

                if (simResp.result == false)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = simResp.message
                    });
                }
                #endregion

                if (raRespModel.result == true)
                {
                    RACommonResponse raCommon = await _bio.CheckCherishedNumber(msisdnCheckReqest, "ValidatePairedMSISDNV3");

                    if (raCommon.result == true)
                    {
                        raRespModel.result = true;
                        raRespModel.message = raCommon.message;
                    }
                    else
                    {
                        raRespModel.result = false;
                        raRespModel.message = raCommon.message;
                    }

                    return Ok(raRespModel);
                }

                //raRespModel.result = true;
                //raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                return Ok(raRespModel);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);


                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                    catch (Exception)
                    {
                        raRespModel.result = false;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                }
                string resp = string.Empty;
                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.result = false;
                            if (_bio.isDBSSErrorOccurred(ex))
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

                            return Ok(raRespModel);
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

                            return Ok(raRespModel);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
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

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);


                        return Ok(raRespModel);
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);


                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    raRespModel.result = false;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raRespModel);
                }
                catch (Exception)
                {
                    raRespModel.result = false;
                    raRespModel.message = ex.Message;

                    return Ok(raRespModel);
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidatePairedMSISDNV2";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        #endregion


        #region Cherish MSISDN validation Paired V3
        /// <summary>
        /// This API is used for MSISDN validation for paired MSISDN.
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(PaiedMSISDNCheckResponse))]
        [HttpPost]
        [Route("ValidatePairedMSISDNV3")]
        public async Task<IActionResult> ValidatePairedMSISDNV4([FromBody] PairedMSISDNCheckRequest msisdnCheckReqest)
        {
            PaiedMSISDNCheckResponse raRespModel = new PaiedMSISDNCheckResponse();
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

                string a = GetAPICollection.PairedMSISDNValidation;


                apiUrl = String.Format(GetAPICollection.PairedMSISDNValidation, msisdnCheckReqest.mobile_number);


                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                var dbssRespObj = JsonConvert.DeserializeObject<PairedMSISDNValidationResponseRootobject>(dbssResp.ToString());
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                if (dbssRespObj == null)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = "MSISDN: " + MessageCollection.NoDataFound
                    });

                }
                if (dbssRespObj.data == null)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = "MSISDN: " + MessageCollection.NoDataFound
                    });
                }

                log.is_success = 1;

                raRespModel = _dbssToRaParse.PairedMSISDNReqParsing2(dbssRespObj);

                if (raRespModel.result == false)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = raRespModel.message
                    });
                }

                #region SIM category check
                string paymentType = await GetPaymentTypeFromGetSubscriptionType(raRespModel.subscription_type_code, msisdnCheckReqest.retailer_id);
                if (GetSIMCategoryByPaymentType(paymentType) != msisdnCheckReqest.sim_category)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = String.Format(MessageCollection.SIMCategoryMismatch, msisdnCheckReqest.sim_category == (int)EnumSimCategory.Prepaid ?
                                                                                       FixedValueCollection.PaymentTypePrepaid : FixedValueCollection.PaymentTypePostpaid)
                    });
                }
                #endregion

                #region SIM Validation

                var simResp = await _bio.CheckSIMNumber3(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = raRespModel.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.NewConnection, true, msisdnCheckReqest.sim_category, "");

                if (simResp.result == false)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = simResp.message
                    });
                }
                #endregion

                #region Cherish number check 
                if (raRespModel.result == true)
                {
                    RACommonResponse raCommon = await _bio.CheckCherishedNumber(msisdnCheckReqest, "ValidatePairedMSISDNV3");

                    if (raCommon.result == true)
                    {
                        raRespModel.result = true;
                        raRespModel.message = raCommon.message;
                    }
                    else
                    {
                        raRespModel.result = false;
                        raRespModel.message = raCommon.message;
                    }

                    return Ok(raRespModel);

                }
                #endregion

                //raRespModel.result = true;
                //raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                return Ok(raRespModel);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription? error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);


                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                    catch (Exception)
                    {
                        raRespModel.result = false;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                }

                string resp = String.Empty;

                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.result = false;
                            if (_bio.isDBSSErrorOccurred(ex))
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

                            return Ok(raRespModel);
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

                            return Ok(raRespModel);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
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

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);


                        return Ok(raRespModel);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorDescription? error = null;
                JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(ex.Message);
                log.res_blob = _blJson.GetGenericJsonData(respObj1);

                error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                            && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");


                log.res_time = DateTime.Now;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                try
                {
                    raRespModel.result = false;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raRespModel);
                }
                catch (Exception)
                {
                    raRespModel.result = false;
                    raRespModel.message = ex.Message;

                    return Ok(raRespModel);
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidatePairedMSISDNV3";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        /// <summary>
        /// This API is used for MSISDN validation for paired MSISDN.
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(PaiedMSISDNCheckResponse))]
        [HttpPost]
        [Route("ValidatePairedMSISDNV4")]
        public async Task<IActionResult> ValidatePairedMSISDNV5([FromBody] PairedMSISDNCheckRequest msisdnCheckReqest)
        {
            PaiedMSISDNCheckResponseDataRev raRespModel = new PaiedMSISDNCheckResponseDataRev();
            string? apiUrl = "", txtResp = "";
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
                        if (!msisdnCheckReqest.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
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

                string a = GetAPICollection.PairedMSISDNValidation;


                apiUrl = String.Format(GetAPICollection.PairedMSISDNValidation, msisdnCheckReqest.mobile_number);


                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                var dbssRespObj = JsonConvert.DeserializeObject<PairedMSISDNValidationResponseRootobject>(dbssResp.ToString());
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                if (dbssRespObj == null)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = "MSISDN: " + MessageCollection.NoDataFound,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    });

                }
                if (dbssRespObj.data == null)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = "MSISDN: " + MessageCollection.NoDataFound,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    });
                }

                log.is_success = 1;

                raRespModel = _dbssToRaParse.PairedMSISDNReqParsingV3(dbssRespObj);

                if (raRespModel.isError == true)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = raRespModel.message,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    });
                }

                #region SIM category check
                string paymentType = await GetPaymentTypeFromGetSubscriptionType(raRespModel.data.subscription_type_code, msisdnCheckReqest.retailer_id);
                if (GetSIMCategoryByPaymentType(paymentType) != msisdnCheckReqest.sim_category)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = String.Format(MessageCollection.SIMCategoryMismatch, msisdnCheckReqest.sim_category == (int)EnumSimCategory.Prepaid ?
                                                                                       FixedValueCollection.PaymentTypePrepaid : FixedValueCollection.PaymentTypePostpaid),
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    });
                }
                #endregion

                #region SIM Validation

                var simResp = await _bio.CheckSIMNumber3(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = raRespModel.data.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.NewConnection, true, msisdnCheckReqest.sim_category, "");

                if (simResp.result == false)
                {
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = simResp.message,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    });
                }
                #endregion

                #region Cherish number check 
                if (raRespModel.isError == false)
                {
                    RACommonResponse raCommon = await _bio.CheckCherishedNumber(msisdnCheckReqest, "ValidatePairedMSISDNV3");

                    if (raCommon.result == true)
                    {
                        raRespModel.isError = false;
                        raRespModel.message = raCommon.message;
                        raRespModel.data = new PaiedMSISDNCheckResponseRev()
                        {
                            sim_number = raRespModel.data.sim_number,
                            subscription_type_code = raRespModel.data.subscription_type_code,
                            imsi = raRespModel.data.imsi
                        };
                    }
                    else
                    {
                        raRespModel.isError = true;
                        raRespModel.message = raCommon.message;
                        raRespModel.data = null;
                    }

                    return Ok(raRespModel);
                }
                #endregion

                //raRespModel.result = true;
                //raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                return Ok(raRespModel);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription? error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);


                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
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
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                }

                string resp = String.Empty;

                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.isError = true;
                            if (_bio.isDBSSErrorOccurred(ex))
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

                            return Ok(raRespModel);
                        }
                        catch (Exception)
                        {
                            raRespModel.isError = true;
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

                            return Ok(raRespModel);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
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

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);


                        return Ok(raRespModel);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorDescription? error = null;
                JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(ex.Message);

                error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                            && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");


                log.res_time = DateTime.Now;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);


                try
                {
                    raRespModel.isError = true;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raRespModel);
                }
                catch (Exception)
                {
                    raRespModel.isError = true;
                    raRespModel.message = ex.Message;

                    return Ok(raRespModel);
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidatePairedMSISDNV4";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        /// <summary>
        /// This API is used for MSISDN validation for paired MSISDN.
        /// </summary>
        /// <param name="msisdnCheckReqest">Mobile number</param>
        /// <returns>Success/ Failure</returns>
        //[Authorize]
        //[ResponseType(typeof(PaiedMSISDNCheckResponse))]
        [HttpPost]
        [Route("ValidatePairedMSISDNV5")]
        public async Task<IActionResult> ValidatePairedMSISDNV6([FromBody] PairedMSISDNCheckRequest msisdnCheckReqest)
        {
            PaiedMSISDNCheckResponseDataRevV1 raRespModel = new PaiedMSISDNCheckResponseDataRevV1();
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            string cherish_category_config = string.Empty;
            string category_config = String.Empty;
            string[] cofigValue = null;
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
                        if (!msisdnCheckReqest.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
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

                string a = GetAPICollection.PairedMSISDNValidation;


                apiUrl = String.Format(GetAPICollection.PairedMSISDNValidation, msisdnCheckReqest.mobile_number);


                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                var dbssRespObj = JsonConvert.DeserializeObject<PairedMSISDNValidationResponseRootobject>(dbssResp.ToString());
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                if (dbssRespObj == null)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = "MSISDN: " + MessageCollection.NoDataFound,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    });

                }
                if (dbssRespObj.data == null)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = "MSISDN: " + MessageCollection.NoDataFound,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    });
                }

                log.is_success = 1;

                raRespModel = _dbssToRaParse.PairedMSISDNReqParsingV4(dbssRespObj);

                if (raRespModel.isError == true)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = raRespModel.message,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    });
                }
                else
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
                    cherish_category_config = configuration.GetSection("AppSettings:cherish_categories").Value;
                    if (category_config.Contains(","))
                    {
                        cofigValue = cherish_category_config.Split(',');
                    }
                    else
                    {
                        cofigValue = cherish_category_config.Split(' ');
                    }

                    if (cofigValue.Any(x => x == raRespModel.data.number_category))
                    {
                        var category = cofigValue.Where(x => x.Equals(raRespModel.data.number_category)).FirstOrDefault();
                        if (category != null)
                        {
                            var catInfo = await _bllCommon.GetDesiredCategoryMessage(category,msisdnCheckReqest.channel_name);
                            raRespModel.data = new PaiedMSISDNCheckResponseRevV1()
                            {
                                sim_number = raRespModel.data.sim_number,
                                subscription_type_code = raRespModel.data.subscription_type_code,
                                imsi = raRespModel.data.imsi,
                                message =catInfo!=null? catInfo.message: "No amount is configured for " + category + " category",
                                isDesiredCategory = catInfo != null ? true :false
                            };
                        }
                    }
                    else
                    {
                        RACommonResponse raCommon = await _bio.CheckCherishedNumber(msisdnCheckReqest, "ValidatePairedMSISDNV3");

                        if (raCommon.result == true)
                        {
                            raRespModel.isError = false;
                            raRespModel.message = raCommon.message;
                            raRespModel.data = new PaiedMSISDNCheckResponseRevV1()
                            {
                                sim_number = raRespModel.data.sim_number,
                                subscription_type_code = raRespModel.data.subscription_type_code,
                                imsi = raRespModel.data.imsi,
                                message = "",
                                isDesiredCategory = false
                            };
                        }
                        else
                        {
                            raRespModel.isError = true;
                            raRespModel.message = raCommon.message;
                            raRespModel.data = null;
                            return Ok(raRespModel);
                        }
                    }
                }

                #region SIM category check
                string paymentType = await GetPaymentTypeFromGetSubscriptionType(raRespModel.data.subscription_type_code, msisdnCheckReqest.retailer_id);
                if (GetSIMCategoryByPaymentType(paymentType) != msisdnCheckReqest.sim_category)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = String.Format(MessageCollection.SIMCategoryMismatch, msisdnCheckReqest.sim_category == (int)EnumSimCategory.Prepaid ?
                                                                                       FixedValueCollection.PaymentTypePrepaid : FixedValueCollection.PaymentTypePostpaid),
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    });
                }
                #endregion

                #region SIM Validation

                var simResp = await _bio.CheckSIMNumber3(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = raRespModel.data.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.NewConnection, true, msisdnCheckReqest.sim_category, "");

                if (simResp.result == false)
                {
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = simResp.message,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = "0"
                        }
                    });
                }
                #endregion                
                return Ok(raRespModel);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription? error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);


                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
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
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                }

                string resp = String.Empty;

                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.isError = true;
                            if (_bio.isDBSSErrorOccurred(ex))
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

                            return Ok(raRespModel);
                        }
                        catch (Exception)
                        {
                            raRespModel.isError = true;
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

                            return Ok(raRespModel);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
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

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);


                        return Ok(raRespModel);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorDescription? error = null;
                JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(ex.Message);

                error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                            && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");


                log.res_time = DateTime.Now;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);


                try
                {
                    raRespModel.isError = true;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raRespModel);
                }
                catch (Exception)
                {
                    raRespModel.isError = true;
                    raRespModel.message = ex.Message;

                    return Ok(raRespModel);
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidatePairedMSISDNV4";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }
        #endregion
        //[ResponseType(typeof(PaiedMSISDNCheckResponse))]
        [HttpPost]
        [Route("ValidatePairedMSISDN_ESIM")]
        public async Task<IActionResult> ValidatePairedMSISDN_ESIM([FromBody] PairedMSISDNCheckRequest msisdnCheckReqest)
        {
            PaiedMSISDNCheckResponse raRespModel = new PaiedMSISDNCheckResponse();
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

                string a = GetAPICollection.PairedMSISDNValidation;


                apiUrl = String.Format(GetAPICollection.PairedMSISDNValidation, msisdnCheckReqest.mobile_number);


                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                var dbssRespObj = JsonConvert.DeserializeObject<PairedMSISDNValidationResponseRootobject>(dbssResp.ToString());
                txtResp = Convert.ToString(dbssResp);

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);


                if (dbssRespObj == null)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = "MSISDN: " + MessageCollection.NoDataFound
                    });
                }
                if (dbssRespObj.data == null)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = "MSISDN: " + MessageCollection.NoDataFound
                    });
                }

                log.is_success = 1;

                raRespModel = _dbssToRaParse.PairedMSISDNReqParsing2(dbssRespObj);

                if (raRespModel.result == false)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = raRespModel.message
                    });
                }

                #region SIM category check
                string paymentType = await GetPaymentTypeFromGetSubscriptionType(raRespModel.subscription_type_code, msisdnCheckReqest.retailer_id);
                if (GetSIMCategoryByPaymentType(paymentType) != msisdnCheckReqest.sim_category)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = String.Format(MessageCollection.SIMCategoryMismatch, msisdnCheckReqest.sim_category == (int)EnumSimCategory.Prepaid ?
                                                                                       FixedValueCollection.PaymentTypePrepaid : FixedValueCollection.PaymentTypePostpaid)
                    });
                }
                #endregion

                #region SIM Validation

                var simResp = await _bio.CheckSIMNumber4(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = raRespModel.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.NewConnection, true, msisdnCheckReqest.sim_category, "");

                if (simResp.result == false)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = simResp.message
                    });
                }
                #endregion

                if (raRespModel.result == true)
                {
                    RACommonResponse raCommon = await _bio.CheckCherishedNumber(msisdnCheckReqest, "ValidatePairedMSISDN_ESIM");

                    if (raCommon.result == true)
                    {
                        raRespModel.result = true;
                        raRespModel.message = raCommon.message;
                    }
                    else
                    {
                        raRespModel.result = false;
                        raRespModel.message = raCommon.message;
                    }

                    return Ok(raRespModel);

                }
                //raRespModel.result = true;
                //raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                return Ok(raRespModel);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);


                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                    catch (Exception)
                    {
                        raRespModel.result = false;
                        raRespModel.message = ex.Message;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                }

                string resp = String.Empty;

                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.result = false;
                            if (_bio.isDBSSErrorOccurred(ex))
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

                            return Ok(raRespModel);
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
                            return Ok(raRespModel);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.result = false;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
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

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);


                        return Ok(raRespModel);
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription? error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);


                try
                {
                    JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(ex.Message);
                    log.res_blob = _blJson.GetGenericJsonData(respObj1);

                    error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");


                    raRespModel.result = false;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raRespModel);
                }
                catch (Exception)
                {
                    raRespModel.result = false;
                    raRespModel.message = ex.Message;

                    return Ok(raRespModel);
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidatePairedMSISDN_ESIM";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        //[ResponseType(typeof(PaiedMSISDNCheckResponse))]
        [HttpPost]
        [Route("ValidatePairedMSISDN_ESIMV2")]
        public async Task<IActionResult> ValidatePairedMSISDN_ESIMV2([FromBody] PairedMSISDNCheckRequest msisdnCheckReqest)
        {
            PaiedMSISDNCheckResponseDataRev raRespModel = new PaiedMSISDNCheckResponseDataRev();
            string? apiUrl = "", txtResp = "";
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
                        if (!msisdnCheckReqest.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
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

                string a = GetAPICollection.PairedMSISDNValidation;


                apiUrl = String.Format(GetAPICollection.PairedMSISDNValidation, msisdnCheckReqest.mobile_number);


                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                log.res_blob = _blJson.GetGenericJsonData(dbssResp);
                var dbssRespObj = JsonConvert.DeserializeObject<PairedMSISDNValidationResponseRootobject>(dbssResp.ToString());
                txtResp = Convert.ToString(dbssResp);

                if (dbssRespObj == null)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = "MSISDN: " + MessageCollection.NoDataFound,
                        data = new Datas()
                        {
                            isEsim = 1,
                            request_id = "0"
                        }
                    });
                }
                if (dbssRespObj.data == null)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = "MSISDN: " + MessageCollection.NoDataFound,
                        data = new Datas()
                        {
                            isEsim = 1,
                            request_id = "0"
                        }
                    });
                }

                log.is_success = 1;

                raRespModel = _dbssToRaParse.PairedMSISDNReqParsingV3(dbssRespObj);

                if (raRespModel.isError == true)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = raRespModel.message,
                        data = new Datas()
                        {
                            isEsim = 1,
                            request_id = "0"
                        }
                    });
                }

                #region SIM category check
                string paymentType = await GetPaymentTypeFromGetSubscriptionType(raRespModel.data.subscription_type_code, msisdnCheckReqest.retailer_id);
                if (GetSIMCategoryByPaymentType(paymentType) != msisdnCheckReqest.sim_category)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = String.Format(MessageCollection.SIMCategoryMismatch, msisdnCheckReqest.sim_category == (int)EnumSimCategory.Prepaid ?
                                                                                       FixedValueCollection.PaymentTypePrepaid : FixedValueCollection.PaymentTypePostpaid),
                        data = new Datas()
                        {
                            isEsim = 1,
                            request_id = "0"
                        }
                    });
                }
                #endregion

                #region SIM Validation

                var simResp = await _bio.CheckSIMNumber4(new SIMNumberCheckRequest()
                {
                    center_code = String.IsNullOrEmpty(msisdnCheckReqest.center_code) ? "" : msisdnCheckReqest.center_code,
                    distributor_code = "",
                    channel_name = msisdnCheckReqest.channel_name,
                    session_token = msisdnCheckReqest.session_token,
                    sim_number = raRespModel.data.sim_number,
                    retailer_id = msisdnCheckReqest.retailer_id,
                    product_code = "",
                    inventory_id = msisdnCheckReqest.inventory_id,
                    msisdn = msisdnCheckReqest.mobile_number,
                    purpose_number = msisdnCheckReqest.purpose_number
                }, (int)EnumPurposeOfSIMCheck.NewConnection, true, msisdnCheckReqest.sim_category, "");

                if (simResp.result == false)
                {
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = simResp.message,
                        data = new Datas()
                        {
                            isEsim = 1,
                            request_id = "0"
                        }
                    });
                }
                #endregion

                if (raRespModel.isError == false)
                {
                    RACommonResponse raCommon = await _bio.CheckCherishedNumber(msisdnCheckReqest, "ValidatePairedMSISDN_ESIM");

                    if (raCommon.result == true)
                    {
                        raRespModel.isError = false;
                        raRespModel.message = raCommon.message;
                        raRespModel.data = new PaiedMSISDNCheckResponseRev()
                        {
                            sim_number = raRespModel.data.sim_number,
                            subscription_type_code = raRespModel.data.subscription_type_code,
                            imsi = raRespModel.data.imsi
                        };
                    }
                    else
                    {
                        raRespModel.isError = true;
                        raRespModel.message = raCommon.message;
                        raRespModel.data = new PaiedMSISDNCheckResponseRev()
                        {
                            sim_number = raRespModel.data.sim_number,
                            subscription_type_code = raRespModel.data.subscription_type_code,
                            imsi = raRespModel.data.imsi
                        };
                    }

                    return Ok(raRespModel);

                }
                //raRespModel.result = true;
                //raRespModel.message = MessageCollection.MSISDNandSIMBothValid;
                return Ok(raRespModel);
            }
            catch (WebException ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription error = null;
                log.is_success = 0;

                if (_bio.isDBSS500ErrorOccurred(ex))
                {
                    log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
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
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
                        raRespModel.message = ex.Message;
                        raRespModel.data = null;

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);
                        log.res_time = DateTime.Now;

                        return Ok(raRespModel);
                    }
                }

                string resp = String.Empty;

                if (ex.Response != null)
                {
                    resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    log.res_blob = _blJson.GetGenericJsonData(resp);
                    log.res_time = DateTime.Now;

                    try
                    {
                        JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(resp);
                        log.res_blob = _blJson.GetGenericJsonData(respObj1);

                        error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                    && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            error = await _bllLog.ManageException(ex2.Message, ex2.HResult, "BIA");

                            raRespModel.isError = true;
                            if (_bio.isDBSSErrorOccurred(ex))
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

                            return Ok(raRespModel);
                        }
                        catch (Exception)
                        {
                            raRespModel.isError = true;
                            raRespModel.message = ex.Message;
                            raRespModel.data = null;

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
                            return Ok(raRespModel);
                        }
                    }
                }
                else
                {
                    try
                    {
                        error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                        raRespModel.isError = true;
                        if (_bio.isDBSSErrorOccurred(ex))
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

                        return Ok(raRespModel);
                    }
                    catch (Exception)
                    {
                        raRespModel.isError = true;
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

                        log.res_blob = _blJson.GetGenericJsonData(raRespModel.message);


                        return Ok(raRespModel);
                    }
                }
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                ErrorDescription? error = null;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);

                try
                {
                    JObject respObj1 = (JObject)JsonConvert.DeserializeObject<Object>(ex.Message);
                    log.res_blob = _blJson.GetGenericJsonData(respObj1);

                    error = await _bllLog.ManageException(respObj1?["errors"]?["title"] != null
                                                && respObj1?["errors"]?["title"]?.ToString() != "" ? respObj1?["errors"]?["title"]?.ToString() : ex.Message, ex.HResult, "BIA");


                    raRespModel.isError = true;

                    raRespModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;


                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(raRespModel);
                }
                catch (Exception)
                {
                    raRespModel.isError = true;
                    raRespModel.message = ex.Message;

                    return Ok(raRespModel);
                }
            }
            finally
            {
                log.msisdn = _bllLog.FormatMSISDN(msisdnCheckReqest.mobile_number);

                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);

                log.purpose_number = msisdnCheckReqest.purpose_number;
                log.user_id = msisdnCheckReqest.retailer_id;
                log.method_name = "ValidatePairedMSISDN_ESIMV2";
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, resStr));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
        }

        #region Get Payment Type (Prepaid/Postpaid) from get subscription type 

        public async Task<string> GetPaymentTypeFromGetSubscriptionType(string subscription_type_code, string retailerName)
        {
            string paymentType = "";
            string? apiUrl = "", txtResp = "";
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                apiUrl = String.Format(GetAPICollection.GetPaymentTypeFromGetSubscriptionType, subscription_type_code);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                var dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                if (!dbssResp.HasValues)
                {
                    return paymentType;
                }

                log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                log.is_success = 1;

                paymentType = _dbssToRaParse.PaymentTypeFromSubscripTypeReqParsing(dbssResp);

                if (String.IsNullOrEmpty(paymentType)) throw new Exception(MessageCollection.DataNotFound);

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

                throw new Exception(ex.Message.ToString());

            }
            finally
            {
                log.method_name = "GetPaymentTypeFromGetSubscriptionType";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = retailerName;
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, resStr));
                //logThread.Start();
                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return paymentType;
        }
        #endregion


        #region Get-SIM-Category-By-Payment-Type
        private int GetSIMCategoryByPaymentType(string paymentType)
        {
            return paymentType == "prepaid" ? (int)EnumSimCategory.Prepaid : (int)EnumSimCategory.Postpaid;
        }
        #endregion


        #region Check Security Token Valid or Not
        /// <summary>
        /// This API is used for Check Security Token
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckSecurityToken")]
        public async Task<IActionResult> CheckSecurityTokenV1(RACommonRequest model)
        {
            try
            {
                bool result = await _apiManager.ValidUserBySecurityToken(model.session_token);
                return Ok(new RACommonResponse()
                {
                    result = result,
                    message = result == false ? MessageCollection.InvalidSecurityToken : MessageCollection.ValidAccessToken
                });
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = ex.Message
                    });
                }
            }
        }

        /// <summary>
        /// This API is used for Check Security Token
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckSecurityTokenV2")]
        public async Task<IActionResult> CheckSecurityTokenV2(RACommonRequest model)
        {
            try
            {
                bool result = await _apiManager.ValidUserBySecurityTokenV2(model.session_token);
                return Ok(new RACommonResponse()
                {
                    result = result,
                    message = result == false ? MessageCollection.InvalidSecurityToken : MessageCollection.ValidAccessToken
                });
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = ex.Message
                    });
                }
            }
        }

        /// <summary>
        /// This API is used for Check Security Token
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckSecurityTokenV3")]
        public async Task<IActionResult> CheckSecurityTokenV3(RACommonRequest model)
        {
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


                return Ok(new RACommonResponseRevamp
                {
                    isError = false,
                    message = security == null ? "" : security.Message //result == false ? MessageCollection.InvalidSecurityToken : MessageCollection.ValidAccessToken
                });
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

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

        #endregion

        #region Unpaired MSISDN and SIM serial
        /// <summary>
        /// This API is used to Get Unpaired MSISDN List Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [HttpPost]
        [Route("GetUnpairedMSISDNList")]
        public async Task<IActionResult> GetUnpairedMSISDNList(UnpairedMSISDNListReqModel model)
        {
            List<ReponseData> raRespData = new List<ReponseData>();
            UnpairedMSISDNData raResp = new UnpairedMSISDNData();
            string apiUrl = string.Empty;
            string? txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                if (String.IsNullOrEmpty(model.msisdn))
                {
                    //GetUnpairedMSISDNSearchDefaultValue
                    model.msisdn = await _bllCommon.GetUnpairedMSISDNSearchDefaultValue(model);

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

                apiUrl = String.Format(UnpairedMSISDNList.GetUnpairedMSISDNList, 1, 10, model.msisdn, stockIdValue);

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
                            var result = ((IEnumerable)dbssRespModel.data).Cast<object>().ToList();

                            raRespData = _dbssToRaParse.UnpairedMSISDNListDataParsing(result);

                            if (raRespData.Count > 0)
                            {
                                raResp.data = raRespData;
                                raResp.result = true;
                                raResp.message = MessageCollection.Success;
                            }
                            else
                            {
                                raResp.data = raRespData;
                                raResp.result = false;
                                raResp.message = MessageCollection.NoDataFound;
                            }
                        }
                        else
                        {
                            raResp.data = raRespData;
                            raResp.result = false;
                            raResp.message = "DBSS API doesn't contains any Unpaired MSISDN list.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.result = false;
                        raResp.message = "DBSS API doesn't contains any Unpaired MSISDN list.";
                    }
                }
                else
                {
                    raResp.data = raRespData;
                    raResp.result = false;
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
                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    raResp.data = raRespData;
                    raResp.result = false;
                    raResp.message = ex.Message;
                }
            }
            finally
            {
                log.method_name = "GetUnpairedMSISDNList";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }

        /// <summary>
        /// This API is used to Get Unpaired MSISDN List Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")] 
        [HttpPost]
        [Route("GetUnpairedMSISDNListV2")]
        public async Task<IActionResult> GetUnpairedMSISDNListV2(UnpairedMSISDNListReqModel model)
        {
            List<ReponseDataRev> raRespData = new List<ReponseDataRev>();
            UnpairedMSISDNDataRev raResp = new UnpairedMSISDNDataRev();
            string apiUrl = string.Empty;
            string? txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
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
                        if (!model.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                if (String.IsNullOrEmpty(model.msisdn))
                {
                    //GetUnpairedMSISDNSearchDefaultValue
                    model.msisdn = await _bllCommon.GetUnpairedMSISDNSearchDefaultValue(model);

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

                apiUrl = String.Format(UnpairedMSISDNList.GetUnpairedMSISDNList, 1, 10, model.msisdn, stockIdValue);

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
                            var result = ((IEnumerable)dbssRespModel.data).Cast<object>().ToList();

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
                log.method_name = "GetUnpairedMSISDNListV2";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }

        [HttpPost]
        [Route("GetCherishedMSISDNList")]
        public async Task<IActionResult> GetCherishedMSISDNList(UnpairedMSISDNListReqModelV2 model)
        {
            List<ReponseDataRev> raRespData = new List<ReponseDataRev>();
            UnpairedMSISDNDataRev raResp = new UnpairedMSISDNDataRev();
            string apiUrl = string.Empty;
            string? txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
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
                        if (!model.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                if (String.IsNullOrEmpty(model.msisdn))
                {
                    //GetUnpairedMSISDNSearchDefaultValue
                    model.msisdn = await _bllCommon.GetUnpairedMSISDNSearchDefaultValueCherished(model);

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

                apiUrl = String.Format(UnpairedMSISDNList.GetUnpairedMSISDNListCherished, 1, 10, model.msisdn, stockIdValue, model.Selected_category);

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
                            var result = ((IEnumerable)dbssRespModel.data).Cast<object>().ToList();

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
                log.method_name = "GetUnpairedMSISDNListV2";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }


        /// <summary>
        /// This API is used to Get Unpaired SIM from DMS Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [HttpPost]
        [Route("GetUnpairedSIMlist")]
        public async Task<IActionResult> GetUnpairedSIMlist(UnpairedSIMsearchReqModel model)
        {
            List<SIMReponseData> raRespData = new List<SIMReponseData>();
            UnpairedSIMData raResp = new UnpairedSIMData();
            string apiUrl = string.Empty;
            string? txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse dMSParse = new BLLRAToDBSSParse();

            string userName = string.Empty;
            string password = string.Empty;
            string product_code_prepaid = string.Empty;
            string product_code_postpaid = string.Empty;
            string product_category_prepaid = string.Empty;
            string product_category_postpaid = string.Empty;
            string sim_s = string.Empty;
            string product_category_simReplacment = string.Empty;

            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);



                try
                {
                    string secreteKey = string.Empty;
                    string loginProviderId = string.Empty;

                    secreteKey = SettingsValues.GetJWTSequrityKey();

                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                    product_code_prepaid = configuration.GetSection("AppSettings:product_code_prepaid").Value;
                    product_code_postpaid = configuration.GetSection("AppSettings:product_code_Postpaid").Value;
                    product_category_prepaid = configuration.GetSection("AppSettings:product_category_prepaid").Value;
                    product_category_postpaid = configuration.GetSection("AppSettings:product_category_postpaid").Value;
                    product_category_simReplacment = configuration.GetSection("AppSettings:product_category_simReplacment").Value;

                    try
                    {
                        if (model.sim_serial.Length < 4)
                        {
                            string msg = "sim_serial must be last 4 digits!";
                            raResp.result = false;
                            raResp.message = msg;
                            return Ok(raResp);
                        }
                        else if (model.sim_serial.Length > 4)
                        {
                            sim_s = model.sim_serial.Substring(model.sim_serial.Length - Math.Min(4, model.sim_serial.Length));
                            model.sim_serial = sim_s;
                        }
                    }
                    catch (Exception)
                    {
                        string keyNotFound = "sim_serial is Mandatory!";
                        raResp.result = false;
                        raResp.message = keyNotFound;
                        return Ok(raResp);
                    }

                    model.user_name = SettingsValues.GetDMSUserName();
                    model.password = SettingsValues.GetDMSPassword();
                    model.product_code_prepaid = product_code_prepaid;
                    model.product_code_postpaid = product_code_postpaid;
                    model.product_category_prepaid = product_category_prepaid;
                    model.product_category_postpaid = product_category_postpaid;
                    model.product_category_simReplacement = product_category_simReplacment;
                }
                catch (Exception)
                {
                    string keyNotFound = "Key not found in Web.config!";
                    raResp.result = false;
                    raResp.message = keyNotFound;
                    return Ok(raResp);
                }

                apiUrl = String.Format(UnpairedMSISDNList.CheckUnpairedSIM);
                UnpairedSIMreqRootModel reqValue = dMSParse.UnpairedSIMReqModelParse(model);
                log.req_blob = _blJson.GetGenericJsonData(reqValue);
                log.req_time = DateTime.Now;

                JObject dmsResp = (JObject)await _apiReq.HttpPostRequestSIMSerial(reqValue, apiUrl);

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dmsResp);
                log.res_blob = _blJson.GetGenericJsonData(dmsResp);

                if (dmsResp != null)
                {
                    log.res_blob = _blJson.GetGenericJsonData(dmsResp);

                    log.is_success = 1;

                    UnpairedSIMRespRootData? dbssRespModel = JsonConvert.DeserializeObject<UnpairedSIMRespRootData>(dmsResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.data).Cast<object>().ToList();

                            raRespData = _dbssToRaParse.UnpairedSIMListDataParsing(result);

                            if (raRespData.Count > 0)
                            {
                                raResp.data = raRespData;
                                raResp.result = true;
                                raResp.message = MessageCollection.Success;
                            }
                            else
                            {
                                raResp.data = raRespData;
                                raResp.result = false;
                                raResp.message = MessageCollection.NoDataFound;
                            }
                        }
                        else
                        {
                            raResp.data = raRespData;
                            raResp.result = false;
                            raResp.message = "DMS API doesn't return any SIM.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.result = false;
                        raResp.message = "DMS API doesn't return any SIM.";
                    }
                }
                else
                {
                    raResp.data = raRespData;
                    raResp.result = false;
                    raResp.message = "Unable to load data from DMS API.";
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
                    raResp.result = false;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;

                }
                catch (Exception)
                {
                    raResp.data = raRespData;
                    raResp.result = false;
                    raResp.message = ex.Message;
                }
            }
            finally
            {
                log.method_name = "GetUnpairedSIMlist";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_code;
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, resStr));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }

        /// <summary>
        /// This API is used to Get Unpaired SIM from DMS Type.
        /// </summary>
        /// <param name=""></param>
        /// <returns>Subscription Type List / Failure</returns>
        //[Authorize(Roles = "Retailer")]
        [HttpPost]
        [Route("GetUnpairedSIMlistV2")]
        public async Task<IActionResult> GetUnpairedSIMlistV2(UnpairedSIMsearchReqModel model)
        {
            List<SIMReponseDataRev> raRespData = new List<SIMReponseDataRev>();
            UnpairedSIMDataRev raResp = new UnpairedSIMDataRev();
            string apiUrl = string.Empty;
            string? txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            BLLRAToDBSSParse dMSParse = new BLLRAToDBSSParse();

            string userName = string.Empty;
            string password = string.Empty;
            string product_code_prepaid = string.Empty;
            string product_code_postpaid = string.Empty;
            string product_category_prepaid = string.Empty;
            string product_category_postpaid = string.Empty;
            string sim_s = string.Empty;
            string product_category_simReplacment = string.Empty;
            string product_code_simReplacment = string.Empty;
            string product_code_StarTrekPrepaid = string.Empty;
            string product_code_StarTrekPrepaid_esim = string.Empty;
            string product_category_StarTrekPrepaid = string.Empty;
            string product_category_StarTrekPrepaid_esim = string.Empty;

            try 
            {
                try
                {
                    string secreteKey = string.Empty;
                    string loginProviderId = string.Empty;
                    secreteKey = SettingsValues.GetJWTSequrityKey();
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                    try { product_code_prepaid = configuration.GetSection("AppSettings:product_code_prepaid").Value; }
                    catch (Exception) { throw new Exception("product_code_prepaid is not found in appsettings.json"); }

                    try { product_code_postpaid = configuration.GetSection("AppSettings:product_code_Postpaid").Value; }
                    catch (Exception) { throw new Exception("product_code_Postpaid is not found in appsettings.json"); }

                    try { product_category_prepaid = configuration.GetSection("AppSettings:product_category_prepaid").Value; }
                    catch (Exception) { throw new Exception("product_category_prepaid is not found in appsettings.json"); }

                    try { product_category_postpaid = configuration.GetSection("AppSettings:product_category_postpaid").Value; }
                    catch (Exception) { throw new Exception("product_category_postpaid is not found in appsettings.json"); }

                    try { product_category_simReplacment = configuration.GetSection("AppSettings:product_category_simReplacment").Value; }
                    catch (Exception) { throw new Exception("product_category_simReplacment is not found in appsettings.json"); }

                    try { product_code_simReplacment = configuration.GetSection("AppSettings:product_code_simReplacment").Value; }
                    catch (Exception) { throw new Exception("product_code_simReplacment is not found in appsettings.json"); }

                    try { product_code_StarTrekPrepaid = configuration.GetSection("AppSettings:p_code_starTrek_prepaid").Value; }
                    catch (Exception) { throw new Exception("p_code_starTrek_prepaid is not found in appsettings.json"); }

                    try { product_code_StarTrekPrepaid_esim = configuration.GetSection("AppSettings:p_code_starTrek_prepaid_esim").Value; }
                    catch (Exception) { throw new Exception("p_code_starTrek_prepaid_esim is not found in appsettings.json"); }

                    try { product_category_StarTrekPrepaid = configuration.GetSection("AppSettings:product_category_StarTrekPrepaid").Value; }
                    catch (Exception) { throw new Exception("product_category_StarTrekPrepaid is not found in appsettings.json"); }

                    try { product_category_StarTrekPrepaid_esim = configuration.GetSection("AppSettings:product_category_StarTrekPrepaid_esim").Value; }
                    catch (Exception) { throw new Exception("product_category_StarTrekPrepaid_esim is not found in appsettings.json"); }

                    ValidTokenResponse security = new ValidTokenResponse();
                    TokenValidationService token = new TokenValidationService(secreteKey);

                    security = token.ValidateToken(model.session_token);

                    if (security != null)
                    {
                        if (security.IsVallid == true)
                        {
                            if (!String.IsNullOrEmpty(model.retailer_code))
                            {
                                string username = model.retailer_code.Substring(1);
                                //if (!username.Equals(security.UserName))
                                //{
                                //    throw new Exception(SettingsValues.GetSessionMessage());
                                //}
                                loginProviderId = security.LoginProviderId;
                            }
                        }
                        else
                        {
                            throw new Exception(security.Message);
                        }
                    }

                    try
                    {
                        if (model.sim_serial.Length < 4)
                        {
                            string msg = "sim_serial must be last 4 digits!";
                            raResp.isError = true;
                            raResp.message = msg;
                            return Ok(raResp);
                        }
                        else if (model.sim_serial.Length > 4)
                        {
                            sim_s = model.sim_serial.Substring(model.sim_serial.Length - Math.Min(4, model.sim_serial.Length));
                            model.sim_serial = sim_s;
                        }
                    }
                    catch (Exception)
                    {
                        string keyNotFound = "sim_serial is Mandatory!";
                        raResp.isError = true;
                        raResp.message = keyNotFound;
                        return Ok(raResp);
                    }
                    try { model.user_name = SettingsValues.GetDMSUserName(); }
                    catch (Exception) { throw new Exception("userName is not found in appsettings.json"); }

                    try { model.password = SettingsValues.GetDMSPassword(); }
                    catch (Exception) { throw new Exception("dms_pas is not found in appsettings.json"); }

                    model.product_code_prepaid = product_code_prepaid;
                    model.product_code_postpaid = product_code_postpaid;
                    model.product_category_prepaid = product_category_prepaid;
                    model.product_category_postpaid = product_category_postpaid;
                    model.product_category_simReplacement = product_category_simReplacment;
                    model.product_code_simReplacement = product_code_simReplacment;
                    model.product_code_StarTrekPrepaid = product_code_StarTrekPrepaid;
                    model.product_code_StarTrekEsim = product_code_StarTrekPrepaid_esim;
                    model.product_category_StarTrekPrepaid = product_category_StarTrekPrepaid;
                    model.product_category_StarTrekEsim = product_category_StarTrekPrepaid_esim;
                }
                catch (Exception ex)
                {
                    string keyNotFound = ex.Message;
                    raResp.isError = true;
                    raResp.message = keyNotFound;
                    return Ok(raResp);
                }

                apiUrl = String.Format(UnpairedMSISDNList.CheckUnpairedSIM);
                UnpairedSIMreqRootModel reqValue = dMSParse.UnpairedSIMReqModelParse(model);
                log.req_blob = _blJson.GetGenericJsonData(reqValue);
                log.req_time = DateTime.Now;

                JObject dmsResp = (JObject)await _apiReq.HttpPostRequestSIMSerial(reqValue, apiUrl);

                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dmsResp);
                log.res_blob = _blJson.GetGenericJsonData(dmsResp);

                if (dmsResp != null)
                {
                    log.res_blob = _blJson.GetGenericJsonData(dmsResp);

                    log.is_success = 1;

                    UnpairedSIMRespRootData? dbssRespModel = JsonConvert.DeserializeObject<UnpairedSIMRespRootData>(dmsResp.ToString());

                    if (dbssRespModel != null)
                    {
                        if (dbssRespModel.data != null)
                        {
                            var result = ((IEnumerable)dbssRespModel.data).Cast<object>().ToList();

                            raRespData = _dbssToRaParse.UnpairedSIMListDataParsingV2(result);

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
                            raResp.message = "DMS API doesn't return any SIM.";
                        }
                    }
                    else
                    {
                        raResp.data = raRespData;
                        raResp.isError = true;
                        raResp.message = "DMS API doesn't return any SIM.";
                    }
                }
                else
                {
                    raResp.data = raRespData;
                    raResp.isError = true;
                    raResp.message = "Unable to load data from DMS API.";
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
                log.method_name = "GetUnpairedSIMlistV2";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_code;
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, resStr));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }

        #endregion
        #region Get Channel Wise Payment Method
        /// <summary>
        /// This API is used for Getting DivDisThana
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        //[GzipCompression]
        [HttpPost]
        [Route("GetPaymentMethod")]
        public async Task<IActionResult> GetPaymentMethod(RAGetPaymentMehtodRequest model)
        {
            ChannelWiseResponse cwRes = new ChannelWiseResponse();
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                cwRes = await _bllCommon.GetPaymentMethod(model);

                return Ok(cwRes);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = ex.Message
                    });
                }
            }
        }

        /// <summary>
        /// This API is used for Getting DivDisThana
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        //[GzipCompression]
        [HttpPost]
        [Route("GetPaymentMethodV2")]
        public async Task<IActionResult> GetPaymentMethodV2(RAGetPaymentMehtodRequest model)
        {
            ChannelWiseResponseRev cwRes = new ChannelWiseResponseRev();
            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;
                string user_name = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(model.session_token);
                 
                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;
                        user_name = security.UserName;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                cwRes = await _bllCommon.GetPaymentMethodV2(model, user_name);

                return Ok(cwRes);
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
        #endregion
        #region get paired MSISDN
        //[Authorize(Roles = "Retailer")]  
        [HttpPost]
        [Route("GetPairedMSISDN")]
        public async Task<IActionResult> GetPairedMSISDN(PairedMSISDNReqModel model)
        {
            List<ReponseDataRev> raRespData = new List<ReponseDataRev>();
            PairedMSISDNDataRev raResp = new PairedMSISDNDataRev();
            string apiUrl = string.Empty;
            string? txtResp = string.Empty;
            BIAToDBSSLog log = new BIAToDBSSLog();
            JObject dbssResp = new JObject();
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

                if (model.sim_serial.Substring(0, FixedValueCollection.SIMCode.Length) != FixedValueCollection.SIMCode)
                {
                    model.sim_serial = FixedValueCollection.SIMCode + model.sim_serial;
                }

                apiUrl = String.Format(PairedMSISDN.PairedMSISDNURL, model.sim_serial);

                log.req_blob = _blJson.GetGenericJsonData(apiUrl);
                log.req_time = DateTime.Now;
                dbssResp = await _apiReq.HttpGetRequest(apiUrl);
                log.res_time = DateTime.Now;
                txtResp = Convert.ToString(dbssResp);

                if (dbssResp != null)
                {
                    log.res_blob = _blJson.GetGenericJsonData(dbssResp);

                    log.is_success = 1;
                    PairedMSISDNRootData dbssRespModel = JsonConvert.DeserializeObject<PairedMSISDNRootData>(dbssResp.ToString());

                    if (!dbssResp["data"].HasValues)
                    {
                        log.is_success = 0;
                        raResp.isError = true;
                        raResp.message = "MSISDN: " + MessageCollection.NoDataFound;
                        return Ok(raResp);
                    }
                    raResp = _dbssToRaParse.PairedMSISDNSearchParsing(dbssResp);

                    if (raResp.data != null)
                    {
                        return Ok(new PairedMSISDNDataRev()
                        {
                            isError = false,
                            message = "MSISDN found",
                            data = new ReponseDataRev()
                            {
                                msisdn = raResp.data.msisdn
                            }
                        });
                    }
                    else
                    {
                        return Ok(new PairedMSISDNDataRev()
                        {
                            isError = true,
                            message = "MSISDN not found",
                            data = new ReponseDataRev()
                            {
                                msisdn = ""
                            }
                        });
                    }
                }
                else
                {
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

                    raResp.isError = true;
                    raResp.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    raResp.isError = true;
                    raResp.message = ex.Message;
                }
                return Ok(raResp);
            }
            finally
            {
                log.method_name = "GetPairedMSISDN";
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.user_id = model.retailer_id;
                string resStr = string.Empty;
                if (txtResp != null)
                {
                    resStr = txtResp.ToString();
                }

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(log, apiUrl, txtResp));
                //logThread.Start();

                await _bllLog.RAToDBSSLog(log, apiUrl, txtResp);
            }
            return Ok(raResp);
        }
        #endregion
        #region App information Update from Retailer
        [HttpPost]
        [Route("UpdateAppVersionFromRetailerApp")]
        public async Task<IActionResult> UpdateAppVersionFromRetailerApp(AppInfoUpdateReqModel model)
        {
            RACommonResponseRevamp cwRes = new RACommonResponseRevamp();
            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                try
                {
                    secreteKey = SettingsValues.GetJWTSequrityKey();
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
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                await _bllCommon.AppInfoUpdate(model, loginProviderId);

                return Ok(new RACommonResponseRevamp()
                {
                    isError = false,
                    message = "Successfully Updated!"
                });
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
        [Route("UpdateAppVersionFromRetailerAppV2")]
        public async Task<IActionResult> UpdateAppVersionFromRetailerAppV2(AppInfoUpdateReqModel model)
        {
            RACommonResponseRevamp cwRes = new RACommonResponseRevamp();
            string secreteKey = string.Empty;
            APPVersionRespModel respModel = new APPVersionRespModel();

            secreteKey = SettingsValues.GetJWTSequrityKey();

            TokenService token = new TokenService(secreteKey);
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
                        message = "Invalid Security Token"
                    });
                }
                try
                {
                   await _bllCommon.AppInfoUpdate(model, loginProviderId);
                }
                catch (Exception ex)
                {
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = ex.Message
                    });
                }

                ResellerLoginUserInfoResponse resellerLogin = new ResellerLoginUserInfoResponse()
                {
                    user_name = model.user_name,
                    center_code = model.center_code,
                    channel_name = model.channel_name,
                    distributor_code = model.distributor_code
                };

                return Ok(new RACommonResponseRetailLoginUpdateToken()
                {
                    isError = false,
                    message = "Successfully Updated!",
                    data = new SessionForRetailToBiometric()
                    {
                        session_token = token.GenerateTokenV2(resellerLogin, loginProviderId)
                    }
                });
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
        [Route("UpdateAppVersionFromRetailerAppV3")]
        public async Task<IActionResult> UpdateAppVersionFromRetailerAppV3(AppInfoUpdateReqModel model)
        {
            RACommonResponseRevamp cwRes = new RACommonResponseRevamp();
            string secreteKey = string.Empty;
            APPVersionRespModel respModel = new APPVersionRespModel();

            secreteKey = SettingsValues.GetJWTSequrityKey();

            TokenService token = new TokenService(secreteKey);
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
                        message = "Invalid Security Token"
                    });
                }
                try
                {
                    respModel = await _bLLUserAuthenticaion.GetAppVersion();

                    if (Convert.ToInt32(model.app_version_code) < respModel.app_version)
                    {
                        return Ok(new RACommonResponseRevamp
                        {
                            isError = true,
                            message = $"Update version is available. Please update {respModel.app_url} Version!"
                        });
                    }
                    else
                    {
                       await _bllCommon.AppInfoUpdate(model, loginProviderId);
                    }
                }
                catch (Exception ex)
                {
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = ex.Message
                    });
                }

                ResellerLoginUserInfoResponse resellerLogin = new ResellerLoginUserInfoResponse()
                {
                    user_name = model.user_name,
                    center_code = model.center_code,
                    channel_name = model.channel_name,
                    distributor_code = model.distributor_code
                };

                return Ok(new RACommonResponseRetailLoginUpdateToken()
                {
                    isError = false,
                    message = "Successfully Updated!",
                    data = new SessionForRetailToBiometric()
                    {
                        session_token = token.GenerateTokenV2(resellerLogin, loginProviderId)
                    }
                });
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
        [Route("GetBTSSiteId")]
        public async Task<IActionResult> GetBTSSiteId(SiteIdRequestModel model)
        {
            RACommonResponseRevamp cwRes = new RACommonResponseRevamp();
            BTSCode bTSCode = new BTSCode();
            SiteIdResponseModel bts_response = new SiteIdResponseModel();
            string secreteKey = string.Empty;

            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

                string loginProviderId = string.Empty;

                try
                {
                    secreteKey = SettingsValues.GetJWTSequrityKey();
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
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                int isBTSshow = 0;

                isBTSshow = SettingsValues.GetBTSCodeShowingOrNot();

                if (isBTSshow != 0)
                {
                    bTSCode = await _bllCommon.GetBTSCode(model);

                    if (!String.IsNullOrEmpty(bTSCode.bts_code))
                    {
                        bts_response = new SiteIdResponseModel()
                        {
                            isError = false,
                            message = "BTS_ID Found!",
                            data = new BTSCode()
                            {
                                bts_code = bTSCode.bts_code
                            }
                        };
                    }
                    else
                    {
                        bts_response = new SiteIdResponseModel()
                        {
                            isError = false,
                            message = "BTS_ID Not Found!",
                            data = new BTSCode()
                            {
                                bts_code = "---"
                            }
                        };
                    }
                }
                else
                {
                    bts_response = new SiteIdResponseModel()
                    {
                        isError = false,
                        message = "BTS_ID Not Found!",
                        data = new BTSCode()
                        {
                            bts_code = "---"
                        }
                    };
                }

                return Ok(bts_response);
            }
            catch (Exception ex)
            {
                bts_response = new SiteIdResponseModel()
                {
                    isError = true,
                    message = "BTS_ID Not Found!",
                    data = new BTSCode()
                    {
                        bts_code = "--"
                    }
                };

                return Ok(bts_response);

            }
        }

        [HttpPost]
        [Route("GetRestrictedAddress")]
        public async Task<IActionResult> GetRestrictedAddress(RACommonRequest model)
        {
            string secreteKey = string.Empty;
            BlackListedWordModel blackListed = new BlackListedWordModel();

            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

                string loginProviderId = string.Empty;

                try
                {
                    secreteKey = SettingsValues.GetJWTSequrityKey();
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
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                blackListed = await _bllCommon.GetBlackListedWordForAddress();
                blackListed.message = "Success!";
                blackListed.isError = false;

                return Ok(blackListed);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString().Contains("expired"))
                {
                    blackListed.message = "The session token is expired!";
                }
                else
                {
                    blackListed.message = ex.Message.ToString();
                }
                blackListed.isError = true;
                return Ok(blackListed);
            }
        }

        [HttpPost]
        [Route("GetRestrictedName")]
        public async Task<IActionResult> GetRestrictedName(RACommonRequest model)
        {
            string secreteKey = string.Empty;
            BlackListedWordModel blackListed = new BlackListedWordModel();

            try
            {
                ValidTokenResponse security = new ValidTokenResponse();

                string loginProviderId = string.Empty;

                try
                {
                    secreteKey = SettingsValues.GetJWTSequrityKey();
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
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                blackListed = await _bllCommon.GetBlackListedWordForName();
                blackListed.message = "Success!";
                blackListed.isError = false;

                return Ok(blackListed);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString().Contains("expired"))
                {
                    blackListed.message = "The session token is expired!";
                }
                else
                {
                    blackListed.message = ex.Message.ToString();
                }
                blackListed.isError = true;
                return Ok(blackListed);
            }
        }

        [HttpPost]
        [Route("GetScannerInfo")]
        public async Task<IActionResult> GetScannerInfo(ScannerInfoReqModel model)
        {
            string secreteKey = string.Empty;
            ScannerInfoRespModel scannerInfo = new ScannerInfoRespModel();

            try
            {
                scannerInfo = await _bllCommon.GetScannerInfo(model);

                return Ok(scannerInfo);
            }
            catch (Exception ex)
            {
                scannerInfo.isError = false;
                scannerInfo.message = ex.Message;
                scannerInfo.data = new ScannerData()
                {
                    is_bl_scanner = "No"
                };
                return Ok(scannerInfo);
            }
        }

        #endregion

        #region Cherish Number Sell
        [HttpPost]
        [Route("categoryDropdown")]
        public async Task<IActionResult> CherishCategoryDropdown(CherishCategoryReqModel model)
        {
            CherishCategoryListResModel categoryData = new CherishCategoryListResModel();
            string user_id = string.Empty;
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
                        user_id = security.UserName;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                categoryData = await _bllCommon.GetCherishCategoyListData(model.channel_name);

                return Ok(categoryData);
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new ActivityLogResponseRevamp()
                    {
                        isError = true,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description,
                        data = null
                    });
                }
                catch (Exception)
                {
                    return Ok(new ActivityLogResponseRevamp()
                    {
                        isError = true,
                        message = ex.Message,
                        data = null
                    });
                }
            }
        }

        [HttpPost]
        //[ValidateModel]
        [Route("ValidateMSISDNANDSIM")]
        public async Task<IActionResult> ValidateMSISDNANDSIM([FromBody] CherishMSISDNCheckRequest msisdnCheckReqest)
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
                        if (!msisdnCheckReqest.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                response = await _bio.ValidateMSISDNVAndSIM(msisdnCheckReqest, "ValidateMSISDNANDSIM");

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
        [Route("ValidateMSISDNANDSIM_ESIM")]
        public async Task<IActionResult> ValidateMSISDNANDSIM_ESIM([FromBody] CherishMSISDNCheckRequest msisdnCheckReqest)
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
                        if (!msisdnCheckReqest.retailer_id.Equals(security.UserName))
                        {
                            throw new Exception(SettingsValues.GetSessionMessage());
                        }
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                rACommonResponse = await _bio.ValidateMSISDNVAndSIMV2(msisdnCheckReqest, "ValidateUnpairedMSISDN_ESIMV2");

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

        #endregion
    }
}
