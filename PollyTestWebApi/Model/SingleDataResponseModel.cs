using System.Net;

namespace PollyTestWebApi.Model
{
    public class SingleDataResponseModel<T>
    {
        public HttpStatusCode Status { get; set; }
        public T Data { get; set; }
        public string Message = "";
        public long QueryTime { get; set; }

        public static SingleDataResponseModel<T> Success(T data)
        {
            return new SingleDataResponseModel<T>
            {
                Status = HttpStatusCode.OK,
                Data = data
            };
        }
    }
}
