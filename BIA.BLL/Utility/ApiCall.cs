using BIA.Entity.Collections;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;

namespace BIA.BLL.Utility
{
    public class ApiCall
    {
        public static string baseUrl = String.Empty;
        public ApiCall()
        {
            baseUrl = SettingsValues.GetDbssBaseUrl();
        }

        //private static readonly HttpClientHandler handler = new HttpClientHandler
        //{
        //    SslProtocols = SslProtocols.Tls12,
        //    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        //};

        //private static readonly HttpClient httpClient = new HttpClient(handler)
        //{
        //    Timeout = TimeSpan.FromSeconds(30)
        //};

        //static ApiCall()
        //{
        //    httpClient.DefaultRequestHeaders.Accept.Clear();
        //    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));
        //}

        public ApiCall(string _baseURL)
        {
            baseUrl = SettingsValues.GetDbssBaseUrlWithParam(_baseURL);
        }      

        public async Task<object> HttpPostRequest(object obj, string methodUrl)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var uri = new Uri(baseUrl + methodUrl);
                var jsonString = JsonConvert.SerializeObject(obj);
                var data = new StringContent(jsonString, Encoding.UTF8, "application/vnd.api+json");

                var response = await httpClient.PostAsync(uri, data);

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
                    throw new Exception(resp);
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException webException && webException.Response != null)
                {
                    using (var responseStream = webException.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var resp = reader.ReadToEnd();
                            var resObj = JsonConvert.DeserializeObject<Object>(resp);
                            throw new Exception(resObj?.ToString());
                            
                        }
                    }
                }
                else
                {
                    throw new Exception(ex.Message); // If it's not a WebException, rethrow the original exception
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> HttpPostRequestCDT(object obj, string methodUrl)
        {
            try 
            {
                HttpClient httpClient = new HttpClient();
                var uri = new Uri(baseUrl + methodUrl);
                var jsonString = JsonConvert.SerializeObject(obj);
                var data = new StringContent(jsonString, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(uri, data);

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
                    throw new Exception(resp);
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException webException && webException.Response != null)
                {
                    using (var responseStream = webException.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var resp = reader.ReadToEnd();
                            var resObj = JsonConvert.DeserializeObject<Object>(resp);
                            throw new Exception(resObj?.ToString());

                        }
                    }
                }
                else
                {
                    throw new Exception(ex.Message); // If it's not a WebException, rethrow the original exception
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<object> HttpPostRequestOrderDBSS(object obj, string methodUrl)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var uri = new Uri(baseUrl + methodUrl);
                var jsonString = JsonConvert.SerializeObject(obj);
                var data = new StringContent(jsonString, Encoding.UTF8, "application/vnd.api+json");

                var response = await httpClient.PostAsync(uri, data);

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
                    throw new Exception(resp);
                }
            }
            catch (HttpRequestException ex)
            { 
                if (ex.InnerException is WebException webException && webException.Response != null)
                {
                    using (var responseStream = webException.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var resp = reader.ReadToEnd();
                            var resObj = JsonConvert.DeserializeObject<Object>(resp);
                            throw new Exception(resObj?.ToString());

                        }
                    }
                }
                else
                {
                    throw new Exception(ex.Message); // If it's not a WebException, rethrow the original exception
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<object> HttpPatchRequest(object obj, string methodUrl)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var uri = new Uri(baseUrl + methodUrl);
                var jsonString = JsonConvert.SerializeObject(obj);
                var data = new StringContent(jsonString, Encoding.UTF8, "application/vnd.api+json");

                var response = await httpClient.PatchAsync(uri, data);

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
                    throw new Exception(resp);
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException webException && webException.Response != null)
                {
                    using (var responseStream = webException.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var resp = reader.ReadToEnd();
                            var resObj = JsonConvert.DeserializeObject<Object>(resp);
                            throw new Exception(resObj?.ToString());

                        }
                    }
                }
                else
                {
                    throw new Exception(ex.Message); // If it's not a WebException, rethrow the original exception
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public Object HttpGetRequest(string apiUrl)
        {

            try
            {
                HttpClient httpClient = new HttpClient();
                var uri = new Uri(baseUrl + apiUrl);

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));

                var response = httpClient.GetAsync(uri).Result; // Blocking call

                if (response.IsSuccessStatusCode)
                {
                    var responseString = response.Content.ReadAsStringAsync().Result; // Blocking call
                    var resObj = JsonConvert.DeserializeObject<object>(responseString);
                    if (resObj != null) { return resObj; }

                    return new object();
                }
                else
                {
                    var resp = response.Content.ReadAsStringAsync().Result; // Blocking call
                    throw new Exception(resp);
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException webException && webException.Response != null)
                {
                    using (var responseStream = webException.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var resp = reader.ReadToEnd();
                            var resObj = JsonConvert.DeserializeObject<Object>(resp);
                            throw new Exception(resObj?.ToString());

                        }
                    }
                }
                else
                {
                    throw new Exception(ex.Message); // If it's not a WebException, rethrow the original exception
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public object HttpGetRequest(string apiUrl, out DateTime reqTime, out DateTime resTime)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                reqTime = DateTime.Now;
                var uri = new Uri(baseUrl + apiUrl);

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));

                reqTime = DateTime.Now;

                var response = httpClient.GetAsync(uri).Result; // Blocking call

                resTime = DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    var responseString = response.Content.ReadAsStringAsync().Result; // Blocking call
                    var resObj = JsonConvert.DeserializeObject<object>(responseString);
                    if (resObj != null) { return resObj; }

                    return new object();
                }
                else
                {
                    var resp = response.Content.ReadAsStringAsync().Result; // Blocking call
                    throw new Exception(resp);
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException webException && webException.Response != null)
                {
                    using (var responseStream = webException.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var resp = reader.ReadToEnd();
                            var resObj = JsonConvert.DeserializeObject<Object>(resp);
                            throw new Exception(resObj?.ToString());

                        }
                    }
                }
                else
                {
                    throw new Exception(ex.Message); // If it's not a WebException, rethrow the original exception
                }
            }
            catch (Exception ex)
            {
                resTime = DateTime.Now;
                throw new Exception(ex.Message);
            }
        }
        public async Task<object> HttpGetRequestAsync(string apiUrl)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var uri = new Uri(baseUrl + apiUrl);

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));

                using var response = await httpClient.GetAsync(uri);

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
                    throw new Exception(resp);
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException webException && webException.Response != null)
                {
                    using (var responseStream = webException.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var resp = reader.ReadToEnd();
                            var resObj = JsonConvert.DeserializeObject<Object>(resp);
                            throw new Exception(resObj?.ToString());

                        }
                    }
                }
                else
                {
                    throw new Exception(ex.Message); // If it's not a WebException, rethrow the original exception
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
