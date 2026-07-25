using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Vin.Web.Models;
using Vin.Web.Models.BaseMessage;
using Vin.Web.Service.IService;
using static Vin.Web.Utility.StaticDetail;

public sealed class BaseService : IBaseService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenProvider _tokenProvider;

    public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
    {
        _httpClientFactory = httpClientFactory;
        _tokenProvider = tokenProvider;
    }

    public async Task<ResponseDTO?> SendAsync(RequestDTO requestDTO, bool withBearer = true)
    {
        try
        {
            using var message = new HttpRequestMessage(ToHttpMethod(requestDTO.ApiType), requestDTO.Url);
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (withBearer && !string.IsNullOrWhiteSpace(_tokenProvider.GetToken()))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.GetToken());
            }

            if (requestDTO.Data is not null)
            {
                message.Content = new StringContent(
                    JsonConvert.SerializeObject(requestDTO.Data),
                    Encoding.UTF8,
                    "application/json");
            }

            using var response = await _httpClientFactory.CreateClient("VinAPI").SendAsync(message);
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _tokenProvider.ClearedToken();
            }

            if (response.IsSuccessStatusCode)
            {
                return ParseResponse(content, "Invalid successful API response.");
            }

            return response.StatusCode switch
            {
                HttpStatusCode.BadRequest => ParseError(content, "Invalid request."),
                HttpStatusCode.Forbidden => Failure("Access denied."),
                HttpStatusCode.NotFound => Failure("Not found."),
                HttpStatusCode.Unauthorized => Failure("Unauthorized."),
                _ => Failure("The service is unavailable.")
            };
        }
        catch (HttpRequestException)
        {
            return Failure("The service is unavailable.");
        }
        catch (TaskCanceledException)
        {
            return Failure("The service request timed out.");
        }
    }

    private static HttpMethod ToHttpMethod(ApiType apiType) => apiType switch
    {
        ApiType.POST => HttpMethod.Post,
        ApiType.PUT => HttpMethod.Put,
        ApiType.DELETE => HttpMethod.Delete,
        _ => HttpMethod.Get
    };

    private static ResponseDTO ParseResponse(string content, string fallback)
    {
        try
        {
            return JsonConvert.DeserializeObject<ResponseDTO>(content)
                ?? Failure(fallback);
        }
        catch (JsonException)
        {
            return Failure(fallback);
        }
    }

    private static ResponseDTO ParseError(string content, string fallback)
    {
        try
        {
            var validation = JsonConvert.DeserializeObject<ValidationErrorResponse>(content);
            if (validation?.Errors is not null)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = fallback,
                    ErrorMessages = validation.Errors.SelectMany(x => x.Value).ToList()
                };
            }

            var error = JsonConvert.DeserializeObject<ErrorResponse>(content);
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = string.IsNullOrWhiteSpace(error?.Message) ? fallback : error.Message,
                ErrorMessages = error?.Errors?.ToList() ?? []
            };
        }
        catch (JsonException)
        {
            return Failure(fallback);
        }
    }

    private static ResponseDTO Failure(string message) => new()
    {
        IsSuccess = false,
        Message = message
    };
}
