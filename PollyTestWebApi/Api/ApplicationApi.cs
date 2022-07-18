﻿using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyTestWebApi.Api
{
    public class ApplicationApi : BaseApi, IApplicationApi
    {
        public ApplicationApi(ILoggerFactory loggerFactory, HttpClient httpClient) : base(loggerFactory, httpClient)
        {
        }
        public async Task<List<string>> GetSomeSleepAsync(int sleepTimeInSeconds, int responseHttpStatus)
        {
            var resp = await ExecuteRestfulApi(sleepTimeInSeconds, responseHttpStatus);
            return new List<string>{
                ""
            };
        }
    }
}
