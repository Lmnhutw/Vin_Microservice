using System.Net;
using System.Text;
using Newtonsoft.Json;
using Vin.Web.Models;
using Vin.Web.Service.IService;
using static Vin.Web.Utility.StaticDetail;

public class BaseService : IBaseService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BaseService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ResponseDTO?> SendAsync(RequestDTO requestDTO)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient("VinAPI");
            HttpRequestMessage message = new();
            message.Headers.Add("Accept", "application/json");
            message.RequestUri = new Uri(requestDTO.Url);

            if (requestDTO.Data != null)
            {
                message.Content = new StringContent(JsonConvert.SerializeObject(requestDTO.Data),
                    Encoding.UTF8, "application/json");
            }

            switch (requestDTO.ApiType)
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

            var apiResponse = await client.SendAsync(message);
            var apiContent = await apiResponse.Content.ReadAsStringAsync();

            // Handle validation errors (400 Bad Request)
            if (apiResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                try
                {
                    // Try to deserialize as validation error response
                    var validationResult = JsonConvert.DeserializeObject<ValidationErrorResponse>(apiContent);
                    if (validationResult?.Errors != null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            ErrorMessages = validationResult.Errors
                                .SelectMany(e => e.Value)
                                .ToList()
                        };
                    }
                }
                catch
                {
                    // If can't deserialize as validation errors, try general error response
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(apiContent);
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = errorResponse?.Message,
                            ErrorMessages = errorResponse?.Errors?.ToList()
                        };
                    }
                    catch
                    {
                        // If all else fails, return the raw content
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = apiContent
                        };
                    }
                }
            }

            switch (apiResponse.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new() { IsSuccess = false, Message = "Not Found" };
                case HttpStatusCode.Forbidden:
                    return new() { IsSuccess = false, Message = "Access Denied" };
                case HttpStatusCode.Unauthorized:
                    return new() { IsSuccess = false, Message = "Unauthorized" };
                case HttpStatusCode.InternalServerError:
                    return new() { IsSuccess = false, Message = "Internal Server Error" };
                default:
                    var apiResDTO = JsonConvert.DeserializeObject<ResponseDTO>(apiContent);
                    return apiResDTO;
            }
        }
        catch (Exception ex)
        {
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }
}

// Add these classes to handle different error response formats
public class ValidationErrorResponse
{
    [JsonProperty("errors")]
    public Dictionary<string, string[]> Errors { get; set; }
}

public class ErrorResponse
{
    public string Message { get; set; }
    public string[] Errors { get; set; }
}