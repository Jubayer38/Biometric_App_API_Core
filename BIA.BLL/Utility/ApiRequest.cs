using BIA.Entity.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.Utility
{
    public class ApiRequest
    {
        //private static readonly HttpClientHandler handler = new HttpClientHandler
        //{
        //    SslProtocols = SslProtocols.Tls12,
        //    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        //};

        //private static readonly HttpClient httpClient = new HttpClient(handler)
        //{
        //    Timeout = TimeSpan.FromSeconds(30)
        //};

        //static ApiRequest() 
        //{
        //    httpClient.DefaultRequestHeaders.Accept.Clear();
        //    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));
        //}

        public async Task<JObject> HttpPostRequest(object obj, string apiUrl)
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(obj);
                var data = new StringContent(jsonString, Encoding.UTF8, "application/vnd.api+json");

                using var httpClient = new HttpClient();

                var response = await httpClient.PostAsync(apiUrl, data);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    var resObj = JsonConvert.DeserializeObject<JObject>(responseString);

                    if (resObj != null)
                    {
                        return resObj;
                    }

                    return new JObject(); // Return an empty JObject if the response is empty.                  
                }
                else
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    string message = string.Empty;

                    if (resp != null && resp.Length > 0 && resp.Length < 1000)
                    {
                        message = resp;
                        throw new Exception("DBSS Error: " + message);
                    }
                    else
                    {
                        try
                        {
                            message = response.ToString();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        throw new Exception("DBSS Error: " + message);
                    }

                }
            }
            catch (WebException ex)
            {
                throw new Exception(isDBSSErrorOccurred(ex) ? FixedValueCollection.DBSSError + ex.Message.ToString() : ex.Message.ToString());

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> HttpPostRequestSIMSerial(Object obj, string apiUrl)
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(obj);
                var data = new StringContent(jsonString, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();

                var response = await httpClient.PostAsync(apiUrl, data);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var resObj = JsonConvert.DeserializeObject<object>(responseString);
                    if (resObj != null) { return resObj; }

                    return new object();
                }
                else
                {
                    var resp = await response.Content.ReadAsStringAsync();

                    string message = string.Empty;

                    if (resp != null && resp.Length > 0 && resp.Length < 1000)
                    {
                        message = resp;
                        throw new Exception("DBSS Error: " + message);
                    }
                    else
                    {
                        try
                        {
                            message = response.ToString();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        throw new Exception("DBSS Error: " + message);
                    }
                }
            }
            catch (WebException ex)
            {
                throw new Exception(isDBSSErrorOccurred(ex) ? FixedValueCollection.DMSError + ex.Message.ToString() : ex.Message.ToString());

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<object> HttpPatchRequest(object obj, string apiUrl)
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(obj);
                var data = new StringContent(jsonString, Encoding.UTF8, "application/vnd.api+json");

                using var httpClient = new HttpClient();

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl);
                request.Content = data;

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var resObj = JsonConvert.DeserializeObject<object>(responseString);

                    if (resObj != null) { return resObj; }

                    return new object();
                }
                else
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    string message = string.Empty;

                    if (resp != null && resp.Length > 0 && resp.Length < 1000)
                    {
                        message = resp;
                        throw new Exception("DBSS Error: " + message);
                    }
                    else
                    {
                        try
                        {
                            message = response.ToString();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        throw new Exception("DBSS Error: " + message);
                    }
                }
            }
            catch (WebException ex)
            {
                throw new Exception(isDBSSErrorOccurred(ex) ? FixedValueCollection.DBSSError + ex.Message.ToString() : ex.Message.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// Delete Request
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="apiUrl"></param>
        /// <returns></returns>        
        public async Task<object> HttpDeleteRequest(object obj, string apiUrl)
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(obj);
                var data = new StringContent(jsonString, Encoding.UTF8, "application/vnd.api+json");

                using var httpClient = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Delete, apiUrl);
                request.Content = data;

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var resObj = JsonConvert.DeserializeObject<object>(responseString);

                    if (resObj != null) { return resObj; }

                    return new object();

                }
                else
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    string message = string.Empty;

                    if (resp != null && resp.Length > 0 && resp.Length < 1000)
                    {
                        message = resp;
                        throw new Exception("DBSS Error: " + message);
                    }
                    else
                    {
                        try
                        {
                            message = response.ToString();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        throw new Exception("DBSS Error: " + message);
                    }
                }
            }
            catch (WebException ex)
            {
                throw new Exception(isDBSSErrorOccurred(ex) ? FixedValueCollection.DBSSError + ex.Message.ToString() : ex.Message.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }



        /// <summary>
        /// API Get Request 
        /// </summary>
        /// <param name="apiUrl"></param>
        /// <returns></returns>     
        public async Task<JObject> HttpGetRequest(string apiUrl)
        {
            string message = string.Empty;
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var resObj = JsonConvert.DeserializeObject<JObject>(responseString);

                    if (resObj != null)
                    {
                        return resObj;
                    }

                    return new JObject(); // Return an empty JObject if the response is empty.
                }
                else
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    

                    if (resp != null && resp.Length > 0 && resp.Length < 1000)
                    {
                        message = resp;
                        throw new Exception("DBSS Error: " + message);
                    }
                    else
                    {
                        //message = response.ReasonPhrase.ToString();
                        //string respMessage = string.Empty;
                        try
                        {
                            message = response.ToString();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        throw new Exception("DBSS Error: " + message);
                    }
                }
            }
            catch (Exception ex)
            {
                if (String.IsNullOrEmpty(message))
                {
                    message = ex.Message.ToString();
                }
                throw new Exception(message);
            }
        }


        /// <summary>
        /// API Get Request for MNP Port in (only used for MNP port-in)
        /// </summary>
        /// <param name="apiUrl"></param>
        /// <returns></returns>       
        public async Task<object> HttpGetRequestForMNPPortIn(string apiUrl)
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var resObj = JsonConvert.DeserializeObject<object>(responseString);

                    if (resObj != null) { return resObj; }

                    return new object();
                }
                else
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    string message = string.Empty;     
                    
                    if (resp != null && resp.Length > 0 && resp.Length < 1000)
                    {
                        message = resp;
                        throw new Exception(message);
                    }
                    else
                    {
                        try
                        {
                            message = response.ToString();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        throw new Exception(message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private bool isDBSSErrorOccurred(WebException exception)
        {
            try
            {
                var error = exception.Response as HttpWebResponse;
                if (error != null)
                {
                    return error.StatusCode == HttpStatusCode.BadRequest ? false : true;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
