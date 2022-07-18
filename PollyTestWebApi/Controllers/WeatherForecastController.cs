using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PollyTestWebApi.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PollyTestWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IApplicationApi _appApi;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IApplicationApi appApi)
        {
            _logger = logger;
            _appApi = appApi;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get([FromQuery]int sleep, int responsecode)
        {
            _appApi.GetSomeSleepAsync(sleep, responsecode);
            return null;
        }
    }
}
