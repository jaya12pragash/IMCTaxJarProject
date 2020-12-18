using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using IMCTaxJarProject.Infra;
using IMCTaxJarProject.Model;
using RestRequest = RestSharp.RestRequest;

namespace IMCTaxJarProject
{

    public static class TaxjarConstants
    {
        public const string DefaultApiUrl = "https://api.taxjar.com";
        public const string ApiVersion = "v2";
    }
    public class TaxjarApi
    {
        internal RestClient apiClient;
        public string apiToken { get; set; }
        public string apiUrl { get; set; }
        public IDictionary<string, string> headers { get; set; }
        public int timeout { get; set; }

        public TaxjarApi(string token, object parameters = null)
        {
            apiToken = token;
            apiUrl = TaxjarConstants.DefaultApiUrl + "/" + TaxjarConstants.ApiVersion + "/";
            headers = new Dictionary<string, string>();
            timeout = 0; // Milliseconds

            if (parameters != null)
            {
                if (parameters.GetType().GetProperty("apiUrl") != null)
                {
                    apiUrl = parameters.GetType().GetProperty("apiUrl").GetValue(parameters).ToString();
                    apiUrl += "/" + TaxjarConstants.ApiVersion + "/";
                }

                if (parameters.GetType().GetProperty("headers") != null)
                {
                    headers = (IDictionary<string, string>)parameters.GetType().GetProperty("headers").GetValue(parameters);
                }

                if (parameters.GetType().GetProperty("timeout") != null)
                {
                    timeout = (int)parameters.GetType().GetProperty("timeout").GetValue(parameters);
                }
            }

            if (string.IsNullOrWhiteSpace(apiToken))
            {
                throw new ArgumentException("Please provide a TaxJar API key.", nameof(apiToken));
            }

            apiClient = new RestClient(apiUrl);
            apiClient.UserAgent = GetUserAgent();
        }

        public virtual void SetApiConfig(string key, object value)
        {
            if (key == "apiUrl")
            {
                value += "/" + TaxjarConstants.ApiVersion + "/";
                apiClient = new RestClient(value.ToString());
            }

            GetType().GetProperty(key).SetValue(this, value, null);
        }

        public virtual object GetApiConfig(string key)
        {
            return GetType().GetProperty(key).GetValue(this);
        }

        protected virtual RestRequest CreateRequest(string action, Method method = Method.POST, object body = null)
        {
            var request = new RestRequest(action, method)
            {
                RequestFormat = DataFormat.Json
            };
            var includeBody = new[] { Method.POST, Method.PUT, Method.PATCH }.Contains(method);

            foreach (var header in headers)
            {
                request.AddHeader(header.Key, header.Value);
            }

            request.AddHeader("Authorization", "Bearer " + apiToken);
            request.AddHeader("User-Agent", GetUserAgent());

            request.Timeout = timeout;

            if (body != null)
            {
                if (IsAnonymousType(body.GetType()))
                {
                    if (includeBody)
                    {
                        request.AddJsonBody(body);
                    }
                    else
                    {
                        foreach (var prop in body.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                        {
                            request.AddQueryParameter(prop.Name, prop.GetValue(body).ToString());
                        }
                    }
                }
                else
                {
                    if (includeBody)
                    {
                        request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);
                    }
                    else
                    {
                        body = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(body));

                        foreach (var prop in JObject.FromObject(body).Properties())
                        {
                            request.AddQueryParameter(prop.Name, prop.Value.ToString());
                        }
                    }
                }
            }

            return request;
        }

        protected virtual T SendRequest<T>(string endpoint, object body = null, Method httpMethod = Method.POST) where T : new()
        {
            var request = CreateRequest(endpoint, httpMethod, body);
            var response = apiClient.Execute<T>(request);

            if ((int)response.StatusCode >= 400)
            {
                var taxjarError = JsonConvert.DeserializeObject<TaxjarError>(response.Content);
                var errorMessage = taxjarError.Error + " - " + taxjarError.Detail;
                throw new TaxjarException(response.StatusCode, taxjarError, errorMessage);
            }

            if (response.ErrorException != null)
            {
                throw new Exception(response.ErrorMessage, response.ErrorException);
            }

            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        protected virtual async Task<T> SendRequestAsync<T>(string endpoint, object body = null, Method httpMethod = Method.POST) where T : new()
        {
            var request = CreateRequest(endpoint, httpMethod, body);
            var response = await apiClient.ExecuteAsync<T>(request).ConfigureAwait(false);

            if ((int)response.StatusCode >= 400)
            {
                var taxjarError = JsonConvert.DeserializeObject<TaxjarError>(response.Content);
                var errorMessage = taxjarError.Error + " - " + taxjarError.Detail;
                throw new TaxjarException(response.StatusCode, taxjarError, errorMessage);
            }

            if (response.ErrorException != null)
            {
                throw new Exception(response.ErrorMessage, response.ErrorException);
            }

            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        protected virtual bool IsAnonymousType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

       

        public virtual RateResponseAttributes RatesForLocation(string zip, object parameters = null)
        {
            var response = SendRequest<RateResponse>("rates/" + zip, parameters, Method.GET);
            return response.Rate;
        }

        public virtual TaxResponseAttributes TaxForOrder(object parameters)
        {
            var response = SendRequest<TaxResponse>("taxes", parameters, Method.POST);
            return response.Tax;
        }

       

        public virtual async Task<RateResponseAttributes> RatesForLocationAsync(string zip, object parameters = null)
        {
            var response = await SendRequestAsync<RateResponse>("rates/" + zip, parameters, Method.GET).ConfigureAwait(false);
            return response.Rate;
        }

        public virtual async Task<TaxResponseAttributes> TaxForOrderAsync(object parameters)
        {
            var response = await SendRequestAsync<TaxResponse>("taxes", parameters, Method.POST).ConfigureAwait(false);
            return response.Tax;
        }

        private string GetUserAgent()
        {

            string platform = "Na";
            string arch = "Na";
            string framework = "Na";
            string version = GetType().Assembly.GetName().Version.ToString(3);
            return $"TaxJar/.NET ({platform}; {arch}; {framework}) taxjar.net/{version}";
        }
    }
}
