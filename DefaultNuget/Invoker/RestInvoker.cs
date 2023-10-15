using System.Net;
using System.Text;
using Newtonsoft.Json;
using DefaultNuget.Dto;
using DefaultNuget.Utils;
using DefaultNuget.Context;
using DefaultNuget.Validator;
using Microsoft.AspNetCore.Http;
using DefaultNuget.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.WebUtilities;

namespace DefaultNuget.Invoker
{
    public class RestInvoker : IRestInvoker
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IOptions<AppConfig> config;

        public RestInvoker(IHttpContextAccessor _httpContextAccessor, IHttpClientFactory _httpClientFactory, IOptions<AppConfig> _config)
        {
            httpContextAccessor = _httpContextAccessor;
            httpClientFactory = _httpClientFactory;
            config = _config;
        }

        public async Task<ApiResponse> SendAsync<T>(ApiRequest apiRequest) where T : class, new()
        {
            Guard.NotNull(apiRequest, nameof(apiRequest));

            var httpClient = httpClientFactory.CreateClient();

            var url = RewriteUrl(config.Value.UrlMapping[apiRequest.Endpoint]);

            httpClient.BaseAddress = GenerateUri(url, apiRequest.ForceHttps);
            httpClient.Timeout = TimeSpan.FromSeconds(Constants.DEFAULT_TIMEOUT_IN_SECONDS);

            string apiUrl = GenerateEndpoint(apiRequest.Prefix, apiRequest.Controller, apiRequest.MethodName);

            if (apiRequest.Parameters != null && apiRequest.Parameters.Count > 0)
            {
                apiUrl = QueryHelpers.AddQueryString(apiUrl, apiRequest.Parameters);
            }

            if (apiRequest.Headers != null && apiRequest.Headers.Count > 0)
            {
                foreach (var item in apiRequest.Headers)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }

            if (apiRequest.UseContext)
            {
                var context = System.Text.Json.JsonSerializer.Serialize(DefaultContext.Current);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("DefaultContext", context);
            }

            var content = new StringContent(JsonConvert.SerializeObject(apiRequest.Request), Encoding.UTF8, "application/json");

            HttpResponseMessage httpResponseMessage = null;
            var apiResponse = new ApiResponse();

            try
            {
                if (apiRequest.MethodType == HttpMethod.Get)
                {
                    httpResponseMessage = await httpClient.GetAsync(apiUrl);
                }
                else if (apiRequest.MethodType == HttpMethod.Post)
                {
                    httpResponseMessage = await httpClient.PostAsync(apiUrl, content);
                }
                else
                {
                    throw new NotSupportedException();
                }

                var contentRead = await httpResponseMessage.Content.ReadAsStringAsync();

                if (httpResponseMessage != null && httpResponseMessage.IsSuccessStatusCode)
                {
                    apiResponse.IsSuccessful = true;
                    apiResponse.Response = JsonConvert.DeserializeObject<T>(contentRead);
                }
                else
                {
                    apiResponse.Message = httpResponseMessage == null ? "SystemError" : contentRead;
                }
            }
            catch (Exception ex) when (ex is WebException || ex is SystemException)
            {
                apiResponse.IsSuccessful = false;
                apiResponse.Message = ex.Message;
            }
            finally
            {
                content.Dispose();
            }

            return apiResponse;
        }

        private string RewriteUrl(string url)
        {
            if (!url.EndsWith("/"))
            {
                return $"{url}/";
            }

            return url;
        }

        private Uri GenerateUri(string url, bool forceHttps)
        {
            var uri = new Uri(url);
            if (forceHttps && uri.Scheme.ToUpper() != "HTTPS")
            {
                url = url.Replace("http", "https");
                uri = new Uri(url);
            }

            return uri;
        }

        private string GenerateEndpoint(string prefix, string controller, string methodName)
        {
            StringBuilder stringBuilder = new();
            if (!string.IsNullOrEmpty(prefix))
            {
                stringBuilder.Append(prefix);
                stringBuilder.Append("/");
            }

            if (!string.IsNullOrEmpty(controller))
            {
                stringBuilder.Append(controller);
                stringBuilder.Append("/");
            }

            if (!string.IsNullOrEmpty(methodName))
            {
                stringBuilder.Append(methodName);
            }

            return stringBuilder.ToString();
        }
    }
}