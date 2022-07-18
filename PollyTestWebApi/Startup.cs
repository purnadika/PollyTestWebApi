using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using PollyTestWebApi.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyTestWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            const int maxRetry = 5;
            const int delayInSecond = 5;
            List<TimeSpan> retries = new List<TimeSpan>();
            for (var trial = 1; trial <= maxRetry; trial += 1)
            {
                retries.Add(TimeSpan.FromMilliseconds(delayInSecond * trial));
            }
            services.AddHttpClient<IApplicationApi, ApplicationApi>()
                .AddPolicyHandler((aservices, request) => GetRetryPolicy<ApplicationApi>(retries, aservices));
            services.AddControllers();
        }
        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy<T>(List<TimeSpan> retries, IServiceProvider services)
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
                .WaitAndRetryAsync(retries,
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        var logger = services.GetService<ILogger<T>>();
                        logger?.LogError("URL: {url}", outcome.Result?.RequestMessage?.RequestUri?.ToString() ?? "-");
                        logger?.LogError("Exception: {ex}", outcome?.Exception?.ToString() ?? "-");
                        var responseStringTask = outcome?.Result?.Content?.ReadAsStringAsync();
                        var responseHeaderStringTask = outcome?.Result?.Headers;
                        logger?.LogDebug($"Response header : {JsonConvert.SerializeObject(responseHeaderStringTask)}");
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
                        logger?.LogDebug("Output from response code: {output}, response: {response}", outcome?.Result?.StatusCode.ToString() ?? "-", responseString);
                        logger?.LogDebug("Delaying for {delay} ms, then making audit trail request retry {retry}.", timespan.TotalMilliseconds, retryAttempt);
                    });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
