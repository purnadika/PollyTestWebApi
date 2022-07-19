using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyTestWebApi.Utilities
{
    public class PollyPolicy
    {
        const int maxRetry = 5;
        const int delayInSecond = 5;
        private readonly List<TimeSpan> _retries;
        private readonly ILogger<PollyPolicy> _logger;
        public PollyPolicy(ILoggerFactory log)
        {
            _logger = log.CreateLogger<PollyPolicy>();
            _retries = new List<TimeSpan>();
            for (var trial = 1; trial <= maxRetry; trial += 1)
            {
                _retries.Add(TimeSpan.FromMilliseconds(delayInSecond * trial));
            }
        }
        public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .OrResult(msg =>
                {
                    var requestUri = msg.RequestMessage?.RequestUri?.ToString();
                    return !string.IsNullOrEmpty(requestUri)
                    && requestUri.Contains("conditionalAccess", StringComparison.OrdinalIgnoreCase)
                    && msg.StatusCode == System.Net.HttpStatusCode.BadRequest;
                })
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(_retries,
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogError("URL: {url}", outcome.Result?.RequestMessage?.RequestUri?.ToString() ?? "-");
                        _logger.LogError("Exception: {ex}", outcome?.Exception?.ToString() ?? "-");
                        var responseStringTask = outcome?.Result?.Content?.ReadAsStringAsync();
                        var responseHeaderStringTask = outcome?.Result?.Headers;
                        _logger.LogDebug($"Response header : {JsonConvert.SerializeObject(responseHeaderStringTask)}");
                        var retryAfterString = responseHeaderStringTask?.RetryAfter?.ToString();
                        var responseString = "-";
                        if (responseStringTask != null)
                        {
                            responseStringTask.Wait();
                            responseString = responseStringTask.Result;
                            // get response header. ambil value Retry-After di header. Assign ke retryAfter
                            var retryAfterInt = int.Parse(retryAfterString);
                            Task.Delay(retryAfterInt * 1000);
                        }
                        _logger.LogDebug("Output from response code: {output}, response: {response}", outcome?.Result?.StatusCode.ToString() ?? "-", responseString);
                        _logger.LogDebug("Delaying for {delay} ms, then making audit trail request retry {retry}.", timespan.TotalMilliseconds, retryAttempt);
                    });
        }
    }
}
