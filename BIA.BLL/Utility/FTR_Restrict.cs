using BIA.BLL.BLLServices;
using BIA.Entity.Collections;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.PopulateModel;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BIA.BLL.Utility
{
    public class FTR_Restrict
    {
        private readonly BLLFTRRestriction _bLLFTRRestriction;
        private readonly BllHandleException _manageExecption;
        private readonly BLLLog _bLLLog; 
        private readonly BLLCommon _bLLCommon;
        public FTR_Restrict(BLLFTRRestriction bLLFTRRestriction, BllHandleException manageExecption, BLLLog bLLLog, BLLCommon bLLCommon)
        {
            _bLLFTRRestriction = bLLFTRRestriction;
            _manageExecption = manageExecption;
            _bLLLog = bLLLog;
            _bLLCommon = bLLCommon;
        }
        public async Task<RetailerBalanceRespModel> CheckEVBalance(string userName, string TransactionMsisdn, string ev_pin, string bi_token_no, string subscriberNo)
        {
            LogModel log = new LogModel();
            string eTopUPValue = string.Empty;
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;
            BSS_Json byteArrayConverter = new BSS_Json();
            string responseContent = string.Empty;
            RetailerBalanceRespModel respModel = new RetailerBalanceRespModel();

            try
            {
                log.msisdn = subscriberNo;
                string msisdn = await _bLLFTRRestriction.GetRetailerItopUpNumber(userName);                
                
                if (msisdn.Substring(0, 3) == "880")
                {
                    msisdn = msisdn.Substring(3);
                }
                else if (msisdn.Substring(0, 1) == "0")
                {
                    msisdn = msisdn.Substring(1);
                }

                string apiUrl = SettingsValues.GetEV_API_URL();

                string requestBody = SettingsValues.GetEV_API_RequestBody();

                requestBody = string.Format(requestBody, msisdn, ev_pin, msisdn);
                log.req_time = DateTime.Now;
                log.req_blob = byteArrayConverter.GetGenericJsonData(requestBody);
                using (HttpClient client = new HttpClient())
                {
                    var urlBuilder = new UriBuilder(apiUrl);

                    urlBuilder.Query = SettingsValues.GetEV_API_QueryString();

                    var request = new HttpRequestMessage(HttpMethod.Post, urlBuilder.Uri)
                    {
                        Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "text/plain")
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        responseContent = await response.Content.ReadAsStringAsync();

                        string[] keyValuePairs = responseContent.Split('&');

                        var dictionary = new System.Collections.Generic.Dictionary<string, string>();

                        foreach (string pair in keyValuePairs)
                        {
                            string[] parts = pair.Split('=');

                            dictionary.Add(parts[0], parts[1]);
                        }
                        var jsonResponse = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
                        JObject jsonObject = JObject.Parse(jsonResponse);

                        string message = (string)jsonObject["MESSAGE"];

                        if (message != null && message.Contains("eTopUP"))
                        {
                            string pattern = @"eTopUP:(\d+)";

                            Match match = Regex.Match(message, pattern);

                            if (match.Success)
                            {
                                respModel.Balance = match.Groups[1].Value;
                                log.is_success = 1;
                            }
                            else
                            {
                                respModel.Balance = "0";
                                log.is_success = 1;
                            }
                        }
                        else
                        {
                            respModel.Balance = "0";
                            respModel.message = message;
                            log.is_success = 1;
                        }
                    }
                    resTime = DateTime.Now;
                }
                return respModel;
            }
            catch (Exception ex)
            {
                respModel.Balance = "0";
                respModel.message = ex.Message.ToString();
                ErrorDescription error = null;
                error = await _manageExecption.ManageException(ex.Message, ex.HResult, "BIA");
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_blob = byteArrayConverter.GetGenericJsonData(responseContent != null ? responseContent : ex.Message);
                log.message = error != null ? error.error_description : null;
                log.error_code = error != null ? error.error_code : null;
                log.error_source = error != null ? error.error_source : "BIA";
                log.is_success = 0;
                throw;
            }
            finally
            {
                log.bi_token_number = bi_token_no;
                log.msisdn = TransactionMsisdn;
                log.user_id = userName;
                log.res_time = resTime;
                log.res_blob = byteArrayConverter.GetGenericJsonData(responseContent);
                log.integration_point_from = (int)IntegrationPoint.bss_service;
                log.integration_point_to = (int)IntegrationPoint.bss;
                log.method_name = "CheckEVBalance";
                await _bLLLog.BALogInsert(log);
            }
        }

        public async Task<FTRDBUpdateModel> FTRRestrictionRequsetToDbss(RechargeRequestModel item)
        {
            bool result;
            LogModel log = new LogModel();
            object status_response = null;
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;
            FTR_RestrictionPopulateModel populateModel = new FTR_RestrictionPopulateModel();
            ApiCall genericApiCall = new ApiCall();
            BSS_Json byteArrayConverter = new BSS_Json();
            FTRDBUpdateModel model = new FTRDBUpdateModel();

            try
            {
                string meathodUrl = populateModel.GetMethodUrl(item.subscriberNo);
                FTRRestrictionReqModel _item = populateModel.PopulateFTRRestrictionReq();
                log.req_time = DateTime.Now;
                log.req_string = meathodUrl;
                log.req_blob = byteArrayConverter.GetGenericJsonData(_item);
                try
                {
                    status_response = await genericApiCall.HttpPostRequest(_item, meathodUrl);
                }
                catch (Exception ex)
                {
                    model.is_ftr_restricted = 0;
                    model.bi_token_no = (long)Convert.ToDouble(item.bi_token_number);
                    model.ftr_message = ex.Message.ToString();
                    _bLLFTRRestriction.FTR_UPdateData(model);

                    throw new Exception("DBSS: FTR Restriction-" + ex.Message);
                }
                log.req_time = reqTime;
                log.res_time = resTime;

                FTRRestrictionResponseModel response = new FTRRestrictionResponseModel();

                try
                {
                    response = JsonConvert.DeserializeObject<FTRRestrictionResponseModel>(status_response.ToString());
                }
                catch (Exception ex)
                {
                    model.is_ftr_restricted = 0;
                    model.bi_token_no = (long)Convert.ToDouble(item.bi_token_number);
                    model.ftr_message = ex.Message.ToString();
                    _bLLFTRRestriction.FTR_UPdateData(model);
                    throw new Exception("DBSS: FTR Restriction-" + ex.Message);
                }

                if (response.data != null)
                {
                    if (response.data != null && response.data[0].attributes.status == "scheduled")
                    {
                        model.is_ftr_restricted = 1;
                        model.bi_token_no = (long)Convert.ToDouble(item.bi_token_number);
                        model.ftr_message = "";
                        _bLLFTRRestriction.FTR_UPdateData(model);
                    }
                    result = true;
                }
                else
                {
                    model.is_ftr_restricted = 0;
                    model.bi_token_no = (long)Convert.ToDouble(item.bi_token_number);
                    model.ftr_message = response.ToString();
                    _bLLFTRRestriction.FTR_UPdateData(model);
                }

                log.res_string = JsonConvert.SerializeObject(status_response).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(status_response);
                log.is_success = 1;
            }
            catch (Exception ex)
            {
                model.is_ftr_restricted = 0;
                ErrorDescription error = null;
                error = await _manageExecption.ManageException(ex.Message, ex.HResult, "BIA");
                log.req_time = reqTime;
                log.res_time = resTime;
                log.res_string = JsonConvert.SerializeObject(status_response).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(status_response != null ? status_response : ex.Message);
                log.message = error != null ? error.error_description : null;
                log.error_code = error != null ? error.error_code : null;
                log.error_source = error != null ? error.error_source : "BIA";
                log.is_success = 0;
                result = false;
            }
            finally
            {
                log.bi_token_number = item.bi_token_number.ToString();
                log.msisdn = item.subscriberNo;
                log.user_id = item.retailerCode;
                log.integration_point_from = (int)IntegrationPoint.bss_service;
                log.integration_point_to = (int)IntegrationPoint.bss;
                log.method_name = "FTRRestrictionRequsetToDbss";
                await _bLLLog.BALogInsert(log);
            }
            return model;
        }

        public async Task<FTRAirResponseModel> FTRRestrictionRequsetToAIR(RechargeRequestModel item, string channelName, string userName)
        {
            LogModel log = new LogModel();
            object status_response = null;
            DateTime reqTime = DateTime.Now;
            DateTime resTime = DateTime.Now;
            FTR_RestrictionPopulateModel populateModel = new FTR_RestrictionPopulateModel();
            ApiCall genericApiCall = new ApiCall();
            BSS_Json byteArrayConverter = new BSS_Json();
            FTRDBUpdateModel model = new FTRDBUpdateModel();
            XDocument respXML = new XDocument();
            XmlToByteConverter xmlToByte = new XmlToByteConverter();
            FTRAirResponseModel fTRAir = new FTRAirResponseModel();
            FTROfferIdRespModel fTROfferId = new FTROfferIdRespModel();
            int responseCode = 1;
            try
            {
                if (item.subscriberNo.Substring(0, 2) == "01")
                {
                    item.subscriberNo = "88" + item.subscriberNo;
                }
                else if (item.subscriberNo.Substring(0, 2) != "88")
                {
                    item.subscriberNo = "88"+item.subscriberNo;
                }
                else if (item.subscriberNo.Substring(0, 3) != "880") 
                {
                    item.subscriberNo = "880" + item.subscriberNo;
                }

                fTROfferId = await _bLLCommon.GetOfferIdforFTR(channelName, userName, item.bi_token_number);

                UpdateOfferRequest request = new UpdateOfferRequest();
                request.OriginNodeType = SettingsValues.GetoriginNodeType();
                request.OriginHostName = Dns.GetHostName();
                request.OriginTransactionID = item.bi_token_number.ToString();
                request.OriginTimeStamp = DateTime.Now;
                DateTime expDate = request.OriginTimeStamp.AddMinutes(SettingsValues.GetFTRExpairyDate());
                request.expiryDateTime = expDate;
                request.SubscriberNumberNAI = SettingsValues.GetsubscriberNumberNAI();
                request.NegotiatedCapabilities = SettingsValues.GetnegotiatedCapabilities();
                request.SubscriberNumber = item.subscriberNo;

                request.OfferID = Convert.ToInt32(fTROfferId.offer_id);

                request.OfferType = 2;                
                log.req_time = DateTime.Now;
                #region Request String             
                var xml = new XDocument(
                    new XElement("methodCall",
                        new XElement("methodName", "UpdateOffer"),
                        new XElement("params",
                            new XElement("param",
                                new XElement("value",
                                    new XElement("struct",
                                        new XElement("member",
                                            new XElement("name", "originNodeType"),
                                            new XElement("value",
                                                new XElement("string", request.OriginNodeType)
                                            )
                                        ),
                                        new XElement("member",
                                            new XElement("name", "originHostName"),
                                            new XElement("value",
                                                new XElement("string", request.OriginHostName)
                                            )
                                        ),
                                        new XElement("member",
                                            new XElement("name", "originTransactionID"),
                                            new XElement("value",
                                                new XElement("string", request.OriginTransactionID)
                                            )
                                        ),
                                        new XElement("member",
                                            new XElement("name", "originTimeStamp"),
                                            new XElement("value",
                                                new XElement("dateTime.iso8601", request.OriginTimeStamp.ToString("yyyyMMddTHH:mm:ss+0600"))
                                            )
                                        ),
                                        new XElement("member",
                                            new XElement("name", "subscriberNumberNAI"),
                                            new XElement("value",
                                                new XElement("int", request.SubscriberNumberNAI)
                                            )
                                        ),
                                        new XElement("member",
                                            new XElement("name", "negotiatedCapabilities"),
                                            new XElement("value",
                                                new XElement("array",
                                                    new XElement("data",
                                                        new XElement("value",
                                                            new XElement("int", request.NegotiatedCapabilities)
                                                        )
                                                    )
                                                )
                                            )
                                        ),
                                        new XElement("member",
                                            new XElement("name", "subscriberNumber"),
                                            new XElement("value",
                                                new XElement("string", request.SubscriberNumber)
                                            )
                                        ),
                                        new XElement("member",
                                            new XElement("name", "offerID"),
                                            new XElement("value",
                                                new XElement("int", request.OfferID)
                                            )
                                        ),
                                        new XElement("member",
                                            new XElement("name", "offerType"),
                                            new XElement("value",
                                                new XElement("int", request.OfferType)
                                            )
                                        ),
                                        new XElement("member",
                                            new XElement("name", "expiryDateTime"),
                                            new XElement("value",
                                                new XElement("dateTime.iso8601", request.expiryDateTime.ToString("yyyyMMddTHH:mm:ss+0600"))
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
                #endregion

                var airUrl = SettingsValues.GetAirBaseUrl();
                //var username = SettingsValues.GetAirUserName();
                //var password = SettingsValues.GetAirPassword();
                var base64AuthToken = SettingsValues.GetAirAuthToken();

                using (var client = new HttpClient())
                {
                    //var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64AuthToken);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Ugw Server/5.0/1.0");
                    client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                    var content = new StringContent(xml.ToString(), Encoding.UTF8, "text/xml");

                    XDocument xmlDoc = XDocument.Parse(xml.ToString());
                    log.req_time = DateTime.Now;
                    log.req_blob = xmlToByte.ConvertXmlToByteArray(xml.ToString());

                    var response = await client.PostAsync(airUrl, content);

                    try
                    {
                        var responseString = await response.Content.ReadAsStringAsync();

                        try
                        {
                            respXML = XDocument.Parse(responseString);

                            responseCode = (int)respXML
                                .Descendants("member")
                                .FirstOrDefault(m => m.Element("name")?.Value == "responseCode")
                                ?.Element("value")
                                ?.Element("i4");

                            if (responseCode == 0)
                            {
                                fTRAir.responseCode = responseCode;
                                fTRAir.Message = "Restricted!";
                                model.is_ftr_restricted = 1;
                                model.bi_token_no = (long)Convert.ToDouble(item.bi_token_number);
                                model.ftr_message = "Restricted!";
                                _bLLFTRRestriction.FTR_UPdateData(model);
                            }
                            else
                            {
                                fTRAir.responseCode = responseCode;
                                fTRAir.Message = responseString.ToString().Substring(1, 200);
                                model.is_ftr_restricted = 0;
                                model.bi_token_no = (long)Convert.ToDouble(item.bi_token_number);
                                model.ftr_message = responseString.ToString().Substring(1, 200);
                                _bLLFTRRestriction.FTR_UPdateData(model);
                            }
                            log.res_blob = xmlToByte.ConvertXmlToByteArray(responseString);
                        }
                        catch (Exception ex)
                        {
                            fTRAir.responseCode = responseCode;
                            fTRAir.Message = responseString.ToString().Substring(1, 200);
                            model.is_ftr_restricted = 0;
                            model.bi_token_no = (long)Convert.ToDouble(item.bi_token_number);
                            model.ftr_message = responseString.ToString().Substring(1, 200);
                            log.res_blob = byteArrayConverter.GetGenericJsonData(responseString);
                            _bLLFTRRestriction.FTR_UPdateData(model);
                        }

                        log.is_success = 1;
                    }
                    catch (Exception ex)
                    {
                        fTRAir.responseCode = responseCode;
                        fTRAir.Message = ex.Message.ToString().Substring(1, 200);
                        log.res_blob = byteArrayConverter.GetGenericJsonData(ex.Message.ToString());
                        model.is_ftr_restricted = 0;
                        model.bi_token_no = (long)Convert.ToDouble(item.bi_token_number);
                        model.ftr_message = ex.Message.ToString().Substring(1, 200);
                        log.res_time = DateTime.Now;
                        _bLLFTRRestriction.FTR_UPdateData(model);
                        throw new Exception(ex.Message.ToString());
                    }

                    log.res_time = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                fTRAir.responseCode = responseCode;
                fTRAir.Message = ex.Message.ToString().Substring(1, 200);
                model.is_ftr_restricted = 0;
                ErrorDescription error = null;
                error =await _manageExecption.ManageException(ex.Message, ex.HResult, "BIA");
                //log.req_time = reqTime;
                log.res_time = DateTime.Now;
                log.res_string = JsonConvert.SerializeObject(respXML).ToString();
                log.res_blob = byteArrayConverter.GetGenericJsonData(ex.Message.ToString());
                log.message = error != null ? error.error_description : null;
                log.error_code = error != null ? error.error_code : null;
                log.error_source = error != null ? error.error_source : "BIA";
                log.is_success = 0;
                model.bi_token_no = (long)Convert.ToDouble(item.bi_token_number);
                model.ftr_message = ex.Message.ToString().Substring(1, 200);
                _bLLFTRRestriction.FTR_UPdateData(model);
                throw new Exception(ex.Message.ToString());
            }
            finally
            {
                log.bi_token_number = item.bi_token_number.ToString();
                log.msisdn = item.subscriberNo;
                log.user_id = item.retailerCode;
                log.res_time = DateTime.Now;
                log.integration_point_from = (int)IntegrationPoint.bss_service;
                log.integration_point_to = (int)IntegrationPoint.bss;
                log.method_name = "FTRRestrictionRequsetToAIR";
                await _bLLLog.BALogInsert(log);
            }

            return fTRAir;
        }

    }
}
