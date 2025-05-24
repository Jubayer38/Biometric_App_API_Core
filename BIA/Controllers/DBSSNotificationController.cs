using BIA.BLL.BLLServices;
using BIA.Common;
using BIA.Entity.Collections;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.ViewModel;
using BIA.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Reflection;

namespace BIA.Controllers
{
    [Route("api/DBSSNotification")]
    [ApiController]
    public class DBSSNotificationController : ControllerBase
    {
        private readonly BL_Json _blJson;
        private readonly BLLLog _bllLog;
        private readonly BLLCommon _bllCommon;
        private readonly BLLDBSSNotification bllDbssNotify;
        private readonly OrderScheduler _orderScheduler;
        private readonly BiometricApiCall _apiCall;
        private readonly ApiManager _apiManager;
        public DBSSNotificationController(BL_Json blJson, BLLLog bllLog,BLLCommon bllCommon, BLLDBSSNotification _bLLDBSSNotification, OrderScheduler orderScheduler, BiometricApiCall apiCall, ApiManager apiManager)
        {
            _blJson = blJson;
            _bllLog = bllLog;
            _bllCommon = bllCommon;
            bllDbssNotify = _bLLDBSSNotification;
            _orderScheduler = orderScheduler;
            _apiCall = apiCall;
            _apiManager = apiManager;
        }

        #region DBSS Bio Status Update From DBSS v2
        /// <summary>
        /// Biometric status update by bss request id.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[ResponseType(typeof(BIFinishNotiResponse))]
        [HttpPost]
        [Route("BioStatusUpdateFromDBSS")]
        public async Task<IActionResult> BioStatusUpdateFromDBSSV2([FromBody] BIAFinishNotiRequest request)
        {
            BIFinishNotiResponse response = null;
            BIAToDBSSLog logObj = new BIAToDBSSLog();
            OrderDataModel orderData = new OrderDataModel();
            string? txtResp = "", txtReq = "";
            DBSSNotificationResponse dBSSNotificationResponse = new DBSSNotificationResponse();
            try
            {
                logObj.req_time = DateTime.Now;
                string req_string = JsonConvert.SerializeObject(request);
                JObject parsedObj = JObject.Parse(req_string);
                logObj.req_blob = _blJson.GetGenericJsonData(parsedObj.ToString());

                #region model validation
                if (String.IsNullOrEmpty(request.session_token))
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = "'session_token' field is required."
                    });
                }

                if (!await _apiManager.ValidUserBySecurityTokenForDBSS(request.session_token))
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = MessageCollection.InvalidSecurityToken
                    });
                }

                if (String.IsNullOrEmpty(request.bio_request_id))
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = "'bio_request_id' field is required."
                    });
                }

                if (request.is_Success == null)
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = "'is_Success' field is required. Only 0 or 1 is acceptable. 1 for success and 0 for failure."
                    });
                }

                if (request.is_Success != 0 && request.is_Success != 1)
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = "Only 0 or 1 is acceptable for 'is_Success' field. 1 for success and 0 for failure."
                    });
                }

                if (request.is_Success == 0 && String.IsNullOrEmpty(request.description))
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = "'description' field is required when the value of 'is_Success' is 0."
                    });
                }

                #endregion

                txtReq = Convert.ToString(request);

                #region Custom Error Msg 
                if (request.is_Success == 0)
                {
                    decimal errorId = GetErrorIdFromErrorDescription(request.description);

                    if (errorId > 0)
                    {
                        VMErrorMsg error = await bllDbssNotify.GetCustomErrorMsg(errorId);
                        request.description = error.error_msg;
                        request.error_code = error.error_code;
                    }
                    else
                    {
                        ErrorDescription error = await _bllLog.ManageException(request.description, 0, "BIA");
                        request.description = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        request.error_code = error.error_code;
                    }
                }
                #endregion

                dBSSNotificationResponse = await bllDbssNotify.VarificationFinishNotification(request);

                if (dBSSNotificationResponse.result == true && String.IsNullOrEmpty(dBSSNotificationResponse.error_description) && String.IsNullOrEmpty(dBSSNotificationResponse.poc_number)
                    || (dBSSNotificationResponse.poc_number != null && dBSSNotificationResponse.purpose_number == 6 && dBSSNotificationResponse.sim_replacement_type != 3))
                {
                    orderData = OMRequestSubmitOrderDataParse(dBSSNotificationResponse);

                    await _orderScheduler.BssServiceProcess(orderData);  //OM request after successfully DBSS Notification.

                }
                if (dBSSNotificationResponse.result == true && dBSSNotificationResponse.is_unreservation_needed == true)
                {
                    var unreservationResult = await _apiCall.UnreserveMSISDNV2(dBSSNotificationResponse.msisdn_reservation_id, request.session_token,
                                                              request.bio_request_id, dBSSNotificationResponse.bi_token_number, dBSSNotificationResponse.msisdn);
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = true,
                        message = unreservationResult.message
                    });
                }
                else if (dBSSNotificationResponse.result == false)
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = dBSSNotificationResponse.message
                    });
                }
                else
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = true,
                        message = dBSSNotificationResponse.message
                    });

                }
            }
            catch (Exception ex)
            {
                txtResp = Convert.ToString(response);
                logObj.res_blob = _blJson.GetGenericJsonData(response);
                logObj.res_time = DateTime.Now;

                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    logObj.is_success = 0;
                    logObj.error_code = error.error_code ?? String.Empty;
                    logObj.error_source = error.error_source ?? String.Empty;
                    logObj.message = error.error_description ?? String.Empty;

                    response = new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    };
                    logObj.res_blob = _blJson.GetGenericJsonData(response);
                    return Ok(response);

                }
                catch (Exception)
                {
                    response = new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = ex.Message
                    };
                    logObj.res_blob = _blJson.GetGenericJsonData(response);
                    return Ok(response);
                }
            }
            finally
            {
                logObj.msisdn = dBSSNotificationResponse.msisdn;
                logObj.bi_token_number = dBSSNotificationResponse.bi_token_number;
                logObj.dbss_request_id = request.bio_request_id;
                logObj.method_name = "BioStatusUpdateFromDBSS_From_BIA";
                logObj.error_source = "BIA";
                logObj.user_id = _bllCommon.GetUserNameFromSessionToken(request.session_token);// 500 error occures for invalid security token format.
                logObj.message = dBSSNotificationResponse.message;
                logObj.is_success = dBSSNotificationResponse.result ? 1 : 0;
                logObj.remarks = "BioStatusUpdateFromDBSS (BI Finish Notification)";
                logObj.res_blob = _blJson.GetGenericJsonData(response);
                logObj.res_time = DateTime.Now;
                logObj.integration_point_from = Convert.ToDecimal(IntegrationPoints.BSS);
                logObj.integration_point_to = Convert.ToDecimal(IntegrationPoints.RA);
                txtResp = Convert.ToString(response);

                await _bllLog.RAToDBSSLog(logObj, txtReq, txtResp);
            }
        }

        /// <summary>
        /// Biometric status update by bss request id. This method is created for AES encryption.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        ///[ResponseType(typeof(BIFinishNotiResponse))]
        ///

        [HttpPost]
        [Route("BioStatusUpdateFromDBSSV2")]
        public async Task<IActionResult> BioStatusUpdateFromDBSSV3([FromBody] BIAFinishNotiRequest request)
        {
            BIFinishNotiResponse response = null;
            BIAToDBSSLog logObj = new BIAToDBSSLog();
            OrderDataModel orderData = new OrderDataModel();
            string txtResp = "", txtReq = "";
            DBSSNotificationResponse dBSSNotificationResponse = new DBSSNotificationResponse();
            try
            {
                logObj.req_time = DateTime.Now;
                string req_string =JsonConvert.SerializeObject(request);
                JObject parsedObj = JObject.Parse(req_string);
                logObj.req_blob = _blJson.GetGenericJsonData(parsedObj.ToString());

                #region model validation
                if (String.IsNullOrEmpty(request.session_token))
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = "'session_token' field is required."
                    });
                }
                //=====here security token valid or not should be checked, otherwise for 
                //invalid security token 500 error will occure!=======
                //if (!ApiManager.ValidUserBySecurityTokenForDBSSV2(request.session_token))
                //{
                //    return Ok(new BIFinishNotiResponse()
                //    {
                //        is_success = false,
                //        message = MessageCollection.InvalidSecurityToken
                //    });
                //}

                if (!await _apiManager.ValidUserBySecurityTokenForDBSSV2(request.session_token))
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = MessageCollection.InvalidSecurityToken
                    });
                }

                if (String.IsNullOrEmpty(request.bio_request_id))
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = "'bio_request_id' field is required."
                    });
                }

                if (request.is_Success == null)
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = "'is_Success' field is required. Only 0 or 1 is acceptable. 1 for success and 0 for failure."
                    });
                }

                if (request.is_Success != 0 && request.is_Success != 1)
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = "Only 0 or 1 is acceptable for 'is_Success' field. 1 for success and 0 for failure."
                    });
                }

                if (request.is_Success == 0 && String.IsNullOrEmpty(request.description))
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = "'description' field is required when the value of 'is_Success' is 0."
                    });
                }

                #endregion

                txtReq = Convert.ToString(request);

                #region Custom Error Msg 
                if (request.is_Success == 0)
                {
                    decimal errorId = GetErrorIdFromErrorDescription(request.description);

                    if (errorId > 0)
                    {
                        VMErrorMsg error = await bllDbssNotify.GetCustomErrorMsg(errorId);
                        request.description = error.error_msg;
                        request.error_code = error.error_code;
                    }
                    else
                    {
                        ErrorDescription error = await _bllLog.ManageException(request.description, 0, "BIA");
                        request.description = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                        request.error_code = error.error_code;
                    }
                }
                #endregion

                dBSSNotificationResponse = await bllDbssNotify.VarificationFinishNotification(request);

                if (dBSSNotificationResponse.result == true && String.IsNullOrEmpty(dBSSNotificationResponse.error_description) && String.IsNullOrEmpty(dBSSNotificationResponse.poc_number)
                    || (dBSSNotificationResponse.poc_number != null && dBSSNotificationResponse.purpose_number == 6 && dBSSNotificationResponse.sim_replacement_type != 3))
                {
                    orderData = OMRequestSubmitOrderDataParse(dBSSNotificationResponse);

                    //new System.Threading.Tasks.Task(async () =>
                    //{
                       await _orderScheduler.BssServiceProcess(orderData);  //OM request after successfully DBSS Notification.

                    //}).Start();
                }

                if (dBSSNotificationResponse.result == true && dBSSNotificationResponse.is_unreservation_needed == true)
                {
                    var unreservationResult = await _apiCall.UnreserveMSISDNV2(dBSSNotificationResponse.msisdn_reservation_id, request.session_token,
                                                              request.bio_request_id, dBSSNotificationResponse.bi_token_number, dBSSNotificationResponse.msisdn);
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = true,
                        message = unreservationResult.message
                    });
                }
                else if (dBSSNotificationResponse.result == false)
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = dBSSNotificationResponse.message
                    });
                }
                else
                {
                    return Ok(new BIFinishNotiResponse()
                    {
                        is_success = true,
                        message = dBSSNotificationResponse.message
                    });

                }
            }
            catch (Exception ex)
            {
                txtResp = Convert.ToString(response);
                logObj.res_blob = _blJson.GetGenericJsonData(response);
                logObj.res_time = DateTime.Now;

                try
                {
                    ErrorDescription error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    logObj.is_success = 0;
                    logObj.error_code = error.error_code ?? String.Empty;
                    logObj.error_source = error.error_source ?? String.Empty;
                    logObj.message = error.error_description ?? String.Empty;

                    response = new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg
                    };
                    logObj.res_blob = _blJson.GetGenericJsonData(response);
                    return Ok(response);

                }
                catch (Exception)
                {
                    response = new BIFinishNotiResponse()
                    {
                        is_success = false,
                        message = ex.Message
                    };
                    logObj.res_blob = _blJson.GetGenericJsonData(response);
                    return Ok(response);
                }
            }
            finally
            {
                logObj.msisdn = dBSSNotificationResponse.msisdn;
                logObj.bi_token_number = dBSSNotificationResponse.bi_token_number;
                logObj.dbss_request_id = request.bio_request_id;
                logObj.method_name = "BioStatusUpdateFromDBSS_From_BIA_V2";
                logObj.error_source = "BIA";
                logObj.user_id = _bllCommon.GetUserNameFromSessionTokenV2(request.session_token);// 500 error occures for invalid security token format.
                logObj.message = dBSSNotificationResponse.message;
                logObj.is_success = dBSSNotificationResponse.result ? 1 : 0;
                logObj.remarks = "BioStatusUpdateFromDBSS (BI Finish Notification)";
                logObj.res_blob = _blJson.GetGenericJsonData(response);
                logObj.res_time = DateTime.Now;
                logObj.integration_point_from = Convert.ToDecimal(IntegrationPoints.BSS);
                logObj.integration_point_to = Convert.ToDecimal(IntegrationPoints.RA);
                txtResp = Convert.ToString(response);

                await _bllLog.RAToDBSSLog(logObj, txtReq, txtResp);

                //Thread logThread = new Thread(() => _bllLog.RAToDBSSLog(logObj, txtReq, txtResp));
                //logThread.Start();
            }
        }

        private OrderDataModel OMRequestSubmitOrderDataParse(DBSSNotificationResponse responseModel)
        {
            OrderDataModel orderData = new OrderDataModel();

            orderData.bi_token_number = responseModel.bi_token_number;
            orderData.bss_request_id = responseModel.bss_request_id;
            orderData.purpose_number = responseModel.purpose_number;
            orderData.msisdn = responseModel.msisdn;
            orderData.sim_category = responseModel.sim_category;
            orderData.sim_number = responseModel.sim_number;
            orderData.subscription_code = responseModel.subscription_code;
            orderData.package_code = responseModel.package_code;
            orderData.dest_doc_type_no = responseModel.dest_doc_type_no;
            orderData.dest_doc_id = responseModel.dest_doc_id;
            orderData.dest_dob = responseModel.dest_dob;
            orderData.customer_name = responseModel.customer_name;
            orderData.gender = responseModel.gender;
            orderData.flat_number = responseModel.flat_number;
            orderData.house_number = responseModel.house_number;
            orderData.road_number = responseModel.road_number;
            orderData.village = responseModel.village;
            orderData.division_Name = responseModel.division_Name;
            orderData.district_Name = responseModel.district_Name;
            orderData.thana_Name = responseModel.thana_Name;
            orderData.postal_code = responseModel.postal_code;
            orderData.user_id = responseModel.user_id;
            orderData.port_in_date = responseModel.port_in_date;
            orderData.alt_msisdn = responseModel.alt_msisdn;
            orderData.status = responseModel.status;
            orderData.error_id = responseModel.error_id;
            orderData.error_description = responseModel.error_description;
            orderData.create_date = responseModel.create_date;
            orderData.dest_id_type_exp_time = responseModel.dest_id_type_exp_time;
            orderData.confirmation_code = responseModel.confirmation_code;
            orderData.email = responseModel.email;
            orderData.salesman_code = responseModel.salesman_code;
            orderData.channel_name = responseModel.channel_name;
            orderData.center_or_distributor_code = responseModel.center_or_distributor_code;
            orderData.sim_replace_reason = responseModel.sim_replace_reason;
            orderData.is_paired = responseModel.is_paired;
            orderData.dbss_subscription_id = responseModel.dbss_subscription_id;
            orderData.old_sim_number = responseModel.old_sim_number;
            orderData.sim_replacement_type = responseModel.sim_replacement_type;
            orderData.src_sim_category = responseModel.src_sim_category;
            orderData.port_in_confirmation_code = responseModel.port_in_confirmation_code;
            orderData.payment_type = responseModel.payment_type;
            orderData.poc_number = responseModel.poc_number;

            return orderData;
        }
        #endregion

        #region Get-Error-Id-From-Error-Description
        private decimal GetErrorIdFromErrorDescription(string errorDescription)
        {
            decimal errorId = 0;
            try
            {
                if (!String.IsNullOrEmpty(errorDescription)
                    && errorDescription.Length > 1
                    && errorDescription.Contains('_')
                    )
                {
                    string[] splitedData = errorDescription.Split('_');
                    if (splitedData.Count() > 0)
                    {
                        if (String.IsNullOrEmpty(splitedData.FirstOrDefault()))
                        {
                            return errorId;
                        }
                        if (!IsDigitsOnly(splitedData.FirstOrDefault()))
                        {
                            return errorId;
                        }
                        return errorId = splitedData.Length > 0 ? Convert.ToDecimal(splitedData[0]) : 0;
                    }
                    else
                    {
                        return errorId;
                    }
                }
                else
                {
                    return errorId;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }
}
