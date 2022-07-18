using System.Collections.Generic;
using System.Net.Http;

namespace PollyTestWebApi.Model
{
    public class RequestModel
    {
        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/json";
        public HttpMethod Method { get; set; } = HttpMethod.Post;
    }
}
