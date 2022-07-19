using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PollyTestWebApi.Model;
using PollyTestWebApi.Utilities;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PollyTestWebApi.Api
{
    public abstract class BaseApi
    {
        protected static HttpMethod GET_METHOD = HttpMethod.Get;
        protected static HttpMethod POST_METHOD = HttpMethod.Post;
        protected static HttpMethod PATCH_METHOD = HttpMethod.Patch;
        protected static HttpMethod DELETE_METHOD = HttpMethod.Delete;

        private readonly ILogger<BaseApi> _logger;
        private readonly HttpClient _httpClient;
        private readonly PollyPolicy _policies;

        protected BaseApi(ILoggerFactory loggerFactory,
            HttpClient httpClient, PollyPolicy policies)
        {
            _policies = policies;
            _logger = loggerFactory.CreateLogger<BaseApi>();
            _httpClient = httpClient;
        }

        protected async Task<SingleDataResponseModel<string>> ExecuteRestfulApi(int sleep, int responsestatus)
        {
            return await _MainRequest(sleep, responsestatus);
        }

        private async Task<SingleDataResponseModel<string>> _MainRequest(int sleep, int responsestatus)
        {
            var url = $"https://httpstat.us/{responsestatus}?sleep={sleep * 1000}";
            try
            {
                _logger.LogDebug("Start Execute API");
                _logger.LogDebug($"Current HttpClient.Timeout : {_httpClient.Timeout.TotalSeconds} s");
                var response = await _policies.GetRetryPolicy().ExecuteAsync(async () => await _httpClient.GetAsync(url));
                var respMessage = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception();
                }

                return new SingleDataResponseModel<string>
                {
                    Status = response.StatusCode,
                    Data = respMessage
                };
            }
            catch (AggregateException ex)
            {
                _logger.LogError(ex.ToString());
                var innerException = ex.InnerException;
                throw innerException;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

    }
}
