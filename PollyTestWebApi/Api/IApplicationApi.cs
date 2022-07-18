using System.Collections.Generic;
using System.Threading.Tasks;

namespace PollyTestWebApi.Api
{
    public interface IApplicationApi
    {
        Task<List<string>> GetSomeSleepAsync(int sleepTimeSeconds, int responseHttpStatus);
    }
}
