using Newtonsoft.Json;
using System.Net;
using System.Text;
using Vin.Web.Models;
using Vin.Web.Service.IService;
using static Vin.Web.Utility.StaticDetail;

namespace Vin.Web.Service
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BaseService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResponseDTO?> SendAsync(RequestDTO requsetDTO)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient("VinAPI");
                HttpRequestMessage message = new();
                message.Headers.Add("Accept", "application/json");
                //token

                message.RequestUri = new Uri(requsetDTO.Url);
                if (requsetDTO != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(requsetDTO.Data), Encoding.UTF8, "application/json");
                }
                HttpResponseMessage? apiRes = null;

                switch (requsetDTO.ApiType)
                {
                    case ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;

                    case ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;

                    case ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;

                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                apiRes = await client.SendAsync(message);

                switch (apiRes.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new()
                        {
                            IsSuccess = false,
                            Message = "Not Found"
                        };

                    case HttpStatusCode.Forbidden:
                        return new()
                        {
                            IsSuccess = false,
                            Message = "Access Denied"
                        };

                    case HttpStatusCode.Unauthorized:
                        return new()
                        {
                            IsSuccess = false,
                            Message = "Unauthorized"
                        };

                    case HttpStatusCode.InternalServerError:
                        return new()
                        {
                            IsSuccess = false,
                            Message = "InternalServerError"
                        };

                    default:
                        var apiContent = await apiRes.Content.ReadAsStringAsync();
                        var apiResDTO = JsonConvert.DeserializeObject<ResponseDTO>(apiContent);
                        return apiResDTO;
                }
            }
            catch (Exception ex)
            {
                var dto = new ResponseDTO
                {
                    Message = ex.Message.ToString(),
                    IsSuccess = false,
                };
                return dto;
            }
        }
    }
}