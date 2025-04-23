using BIA.BLL.BLLServices;
using BIA.Entity.Collections;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.Interfaces;
using BIA.Entity.ResponseEntity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.Utility
{
    public class OrderApiCall
    {
        private readonly BllBiometricBssService errorUpdate;
        private readonly BllOrderBssService _bllOrderBssService;
        private readonly BllHandleException _manageExecption;
        private readonly BLLLog _bllLog;

        public OrderApiCall(BllBiometricBssService _errorUpdate, BllOrderBssService bllOrderBssService, BllHandleException manageExecption, BLLLog bllLog)
        {
            errorUpdate = _errorUpdate;
            _bllOrderBssService = bllOrderBssService;
            _manageExecption = manageExecption;
            _bllLog = bllLog;
        }
        public async Task PatchOrderRequestToBss(OrderDataModel item, object reqModel, string meathodUrl)
        {
            BSS_Json byteArrayConverter = new BSS_Json();
            ApiCall genericApiCall = new ApiCall();
            LogModel log = new LogModel();
            object orderResponse = null;
            log.status = item.status;
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;
            try
            {
                log.req_time = DateTime.Now;
                log.req_string = JsonConvert.SerializeObject(reqModel);
                log.req_blob = byteArrayConverter.GetGenericJsonData(reqModel);
                try
                { orderResponse = await genericApiCall.HttpPatchRequest(reqModel, meathodUrl); }
                catch (Exception ex)
                { throw new Exception("DBSS: Order-" + ex.Message); }
                //log.res_time = DateTime.Now;
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(orderResponse);
                log.res_blob = byteArrayConverter.GetGenericJsonData(orderResponse);

                OrderResModelPatch? response = new OrderResModelPatch();
                try
                {
                    if (orderResponse.ToString() != null)
                    {
                        response = JsonConvert.DeserializeObject<OrderResModelPatch>(orderResponse.ToString());
                    }
                }
                catch (Exception ex)
                { throw new Exception("DBSS: Order Api Response Parsing Error."); }


                if (response == null)
                    throw new Exception("DBSS: Order Api Response is not valid.");
                else if (response.data == null)
                    throw new Exception("DBSS: Order Api Response is not valid.");
                else if (response.data.FirstOrDefault()?.id == null)
                    throw new Exception("DBSS: Order Api Response is not valid.");

                _bllOrderBssService.UpdateBioDbForOrderReq(item.bi_token_number, response.data.FirstOrDefault()?.id);
                log.is_success = 1;
            }
            catch (Exception ex)
            {
                ErrorDescription error = null;
                try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "DBSS Service"); }
                catch { }
                //log.res_time = DateTime.Now;
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(orderResponse != null ? orderResponse.ToString() : ex.Message).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(orderResponse != null ? orderResponse : ex.Message);
                log.message = error != null ? error.error_description : null;
                log.error_code = error != null ? error.error_code : null;
                log.error_source = error != null ? error.error_source : "BIA OM Request";
                log.is_success = 0;
                //BIRequset Table Update with Status 35 and Error id and description for Order failure
                item.status = (int)StatusNo.order_request_fail;
                item.error_id = error != null ? error.error_id : 0;
                item.error_description = "DBSS: Order-" + ex.Message;
                await errorUpdate.UpdateStatusandErrorMessage(item.bi_token_number, item.status, item.error_id, item.error_description);
            }

            finally
            {
                log.bss_request_id = item.bss_request_id;
                log.bi_token_number = item.bi_token_number;
                log.msisdn = item.msisdn;
                log.user_id = item.user_id;
                log.integration_point_from = (int)IntegrationPoint.bss_service;
                log.integration_point_to = (int)IntegrationPoint.bss;
                log.method_name = "PatchOrderRequestToBss_New";
                log.purpose_number = item.purpose_number.ToString();
                await _bllLog.BALogInsert(log);
            }
        }

        public async Task PostOrderRequestToBss(OrderDataModel item, object reqModel, string meathodUrl)
        {
            BSS_Json byteArrayConverter = new BSS_Json();
            ApiCall genericApiCall = new ApiCall();
            LogModel log = new LogModel();
            object orderResponse = null;
            log.status = item.status;


            try
            {
                log.req_time = DateTime.Now;
                log.req_string = JsonConvert.SerializeObject(reqModel);
                log.req_blob = byteArrayConverter.GetGenericJsonData(reqModel);
                try
                {
                    DateTime reqTime = DateTime.Now;
                    log.req_time = reqTime;

                    orderResponse = await genericApiCall.HttpPostRequestOrderDBSS(reqModel, meathodUrl);

                    DateTime resTime = DateTime.Now;
                    log.res_time = resTime;
                }
                catch (Exception ex)
                { throw new Exception("DBSS: Order-" + ex.Message); }

                //log.res_time = DateTime.Now;


                log.res_string = JsonConvert.SerializeObject(orderResponse);
                log.res_blob = byteArrayConverter.GetGenericJsonData(orderResponse);

                string confirmationCode = string.Empty;
                try
                {
                    JObject dbssRespObj = JObject.Parse(log.res_string);

                    if (dbssRespObj != null && dbssRespObj.ContainsKey("data"))
                    {
                        if (dbssRespObj["data"] != null)
                        {
                            confirmationCode = (string)dbssRespObj["data"]["attributes"]["confirmation-code"];
                        }
                    }

                }
                catch (Exception ex)
                { throw new Exception("DBSS: Order Api Response Parsing Error."); }


                if (confirmationCode == null)
                    throw new Exception("DBSS: Order Api Response is not valid.");

                _bllOrderBssService.UpdateBioDbForOrderReq(item.bi_token_number, confirmationCode);
                log.is_success = 1;
            }
            catch (Exception ex)
            {
                ErrorDescription error = null;
                try { error = await _manageExecption.ManageException(ex.Message, ex.HResult, "DBSS Service"); }
                catch { }
                //log.res_time = DateTime.Now;
                log.res_time = DateTime.Now; ;
                log.res_string = JsonConvert.SerializeObject(orderResponse != null ? orderResponse.ToString() : ex.Message).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(orderResponse != null ? orderResponse : ex.Message);
                log.message = error != null ? error.error_description : null;
                log.error_code = error != null ? error.error_code : null;
                log.error_source = error != null ? error.error_source : "DBSS Service";
                log.is_success = 0;
                //BIRequset Table Update with Status 35 and Error id and description for Order failure
                item.status = (int)StatusNo.order_request_fail;
                item.error_id = error != null ? error.error_id : 0;
                item.error_description = "DBSS: Order-" + ex.Message;
                await errorUpdate.UpdateStatusandErrorMessage(item.bi_token_number, item.status, item.error_id, item.error_description);
            }

            finally
            {
                log.bss_request_id = item.bss_request_id;
                log.bi_token_number = item.bi_token_number;
                log.msisdn = item.msisdn;
                log.user_id = item.user_id;
                log.integration_point_from = (int)IntegrationPoint.bss_service;
                log.integration_point_to = (int)IntegrationPoint.bss;
                log.method_name = "PostOrderRequestToBss_New";
                log.purpose_number = item.purpose_number.ToString();
                await _bllLog.BALogInsert(log);
            }
        }

        public async Task<string> PostCreatCustomerRequestToBss(OrderDataModel item, object reqModel, int createCustomerMaxRetry)
        {
            string ownerCustomerId = "";

            for (int i = 0; i < createCustomerMaxRetry; i++)
            {
                BSS_Json byteArrayConverter = new BSS_Json();
                ApiCall genericApiCall = new ApiCall();
                LogModel log = new LogModel();
                object orderResponse = null;
                log.status = item.status;
                DateTime reqTime = DateTime.Now;
                DateTime resTime = DateTime.Now;
                try
                {
                    log.req_string = JsonConvert.SerializeObject(reqModel);
                    log.req_blob = byteArrayConverter.GetGenericJsonData(reqModel);


                    try
                    {
                        log.req_time = DateTime.Now;

                        orderResponse = await genericApiCall.HttpPostRequest(reqModel, "/api/v1/customers");

                        log.res_time = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("DBSS: Order create customer-" + ex.Message);
                    }

                    log.res_string = JsonConvert.SerializeObject(orderResponse);
                    log.res_blob = byteArrayConverter.GetGenericJsonData(orderResponse);

                    try
                    {
                        JObject dbssRespObj = JObject.Parse(orderResponse.ToString());
                        if (dbssRespObj.ContainsKey("data"))
                            ownerCustomerId = (string)dbssRespObj["data"]["id"];
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("DBSS: Create Customer Api Response Parsing Error.");
                    }

                    if (ownerCustomerId == null)
                    {
                        throw new Exception("DBSS: Create Customer Api Response is not valid.");
                    }

                    await _bllOrderBssService.UpdateBioDbForCreateCustomerReq(item.bi_token_number, ownerCustomerId);
                    log.is_success = 1;

                    i = createCustomerMaxRetry;
                }
                catch (Exception ex)
                {
                    log.req_time = reqTime;
                    log.res_time = resTime;

                    ErrorDescription error = null;

                    try
                    {
                        error = await _manageExecption.ManageException(ex.Message.Substring(0, Math.Min(ex.Message.Length, 400)), ex.HResult, "DBSS Service");
                    }
                    catch
                    {
                    }

                    log.res_string = JsonConvert.SerializeObject(orderResponse != null ? orderResponse.ToString() : ex.Message).ToString();
                    log.res_blob = byteArrayConverter.GetGenericJsonData(orderResponse != null ? orderResponse : ex.Message);
                    log.message = error != null ? error.error_description : null;
                    log.error_code = error != null ? error.error_code : null;
                    log.error_source = error != null ? error.error_source : "DBSS Service";
                    log.is_success = 0;
                    item.status = (int)StatusNo.order_request_fail;
                    item.error_id = error != null ? error.error_id : 0;

                    try
                    {
                        item.error_description = "DBSS: Order-" + ex.Message.Substring(0, Math.Min(ex.Message.Length, 900));
                    }
                    catch
                    {
                    }

                    if (i == createCustomerMaxRetry - 1)
                    {
                        await errorUpdate.UpdateStatusandErrorMessage(item.bi_token_number, item.status, item.error_id, item.error_description);
                    }
                }
                finally
                {
                    log.bss_request_id = item.bss_request_id;
                    log.bi_token_number = item.bi_token_number;
                    log.msisdn = item.msisdn;
                    log.user_id = item.user_id;
                    log.integration_point_from = (int)IntegrationPoint.bss_service;
                    log.integration_point_to = (int)IntegrationPoint.bss;
                    log.method_name = "PostCreatCustomerRequestToBss_New";
                    log.purpose_number = item.purpose_number.ToString();
                    await _bllLog.BALogInsert(log);
                }
            }

            return ownerCustomerId;
        }

    }
}
