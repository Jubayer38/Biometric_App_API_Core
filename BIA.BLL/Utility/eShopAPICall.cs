using BIA.BLL.BLLServices;
using BIA.Entity.Collections;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.Interfaces;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.Utility 
{
    public class eShopAPICall
    {
        private readonly BLLLog _bLLLog;
        public eShopAPICall(BLLLog bLLLog)
        {
            _bLLLog = bLLLog;
        }
        public async Task<eShopOrderResponseModel> OrderValidation(eShopOrderValidationReqModel model)
        {
            eShopOrderResponseModel responseModel = new eShopOrderResponseModel();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BL_Json _blJson = new BL_Json();
            try
            {
                var Baseurl = SettingsValues.GeteShopBaseUrl();
                var methodUrl = "api/v1/bio/order-activation";
                var authorizationHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes(SettingsValues.GeteShopCredential()));

                if(model.msisdn.Substring(0, 2) == "01")
                {
                    model.msisdn = "88"+model.msisdn;
                }               

                var requestData = new
                {
                    order_id = model.orderId,
                    msisdn = model.msisdn,
                };

                var url = Baseurl + methodUrl;
                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                log.req_blob = _blJson.GetGenericJsonData(json);

                using var client = new HttpClient();

                client.DefaultRequestHeaders.Add("Authorization", $"Basic {authorizationHeader}");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    log.res_blob = _blJson.GetGenericJsonData(responseBody);

                    try
                    {
                        responseModel = JsonConvert.DeserializeObject<eShopOrderResponseModel>(responseBody);

                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message.ToString());
                    }
                }
                else 
                {
                    var resp = await response.Content.ReadAsStringAsync();

                    log.res_blob = _blJson.GetGenericJsonData(resp);

                    if (resp != null && resp.Length > 0 && resp.Length < 1000)
                    {
                        responseModel.message = resp;
                        throw new Exception("eShop Error: " + responseModel.message);
                    }
                    else
                    {
                        try
                        {
                            responseModel.message = response.ToString();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        throw new Exception("eShop Error: " + responseModel.message);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("eShop Error: " + ex.Message.ToString());
            }
            finally
            {
                log.msisdn = _bLLLog.FormatMSISDN(model.msisdn);
                log.integration_point_from = Convert.ToDecimal(IntegrationPoints.RA);
                log.integration_point_to = Convert.ToDecimal(IntegrationPoints.BSS);
                log.purpose_number = "2";
                log.user_id = model.retailer_id;
                log.method_name = "eShopOrderValidation";

                await _bLLLog.RAToDBSSLog(log, "", "");
            }
            return responseModel;
        }
    }
}
