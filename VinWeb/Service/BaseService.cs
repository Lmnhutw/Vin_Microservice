using System.Net;
using System.Text;
using Newtonsoft.Json;
using Vin.Web.Models;
using Vin.Web.Models.BaseMessage;
using Vin.Web.Service.IService;
using static Vin.Web.Utility.StaticDetail;

public class BaseService : IBaseService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenProvider _tokenProvider;

    public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
    {
        _httpClientFactory = httpClientFactory;
        _tokenProvider = tokenProvider;
    }

    /*public async Task<ResponseDTO?> SendAsync(RequestDTO requestDTO, bool withBearer = true)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient("VinAPI");
            HttpRequestMessage message = new();
            message.Headers.Add("Accept", "application/json");

            //pass token
            if (withBearer)
            {
                var token = _tokenProvider.GetToken();
                message.Headers.Add("Authorization", $"Bearer {token}");
            }

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
            // Check if we got an error about ambiguous routes
            if (apiContent.Contains("AmbiguousMatchException") ||
                apiContent.Contains("matched multiple endpoints"))
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "API routing configuration error",
                    ErrorMessages = new List<string>
                {
                    "The API has multiple endpoints matching this route. Please contact support."
                }
                };
            }
            // Check content type
            var contentType = apiResponse.Content.Headers.ContentType?.MediaType;
            if (contentType != "application/json")
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Unexpected response format: {contentType}",
                    ErrorMessages = new List<string>
                {
                    apiContent.Substring(0, Math.Min(200, apiContent.Length))
                }
                };
            }
            try
            {
                var apiResDTO = JsonConvert.DeserializeObject<ResponseDTO>(apiContent);
                if (apiResDTO == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Failed to parse server response",
                        ErrorMessages = new List<string> { "Response was empty or invalid" }
                    };
                }
                return apiResDTO;
            }
            catch (JsonReaderException jex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Invalid JSON response",
                    ErrorMessages = new List<string>
                {
                    $"Error: {jex.Message}",
                    $"Content: {apiContent.Substring(0, Math.Min(100, apiContent.Length))}"
                }
                };
            }

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
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(apiContent);
                        if (errorResponse?.Errors?.Contains("MessageBusFailure") == true)
                        {
                            return new()
                            {
                                IsSuccess = true,
                                Message = "Added to cart with some issues",
                                ErrorMessages = errorResponse.Errors.ToList()
                            };
                        }
                        return new() { IsSuccess = false, Message = "Internal Server Error" };
                    }
                    catch (JsonReaderException)
                    {
                        return new()
                        {
                            IsSuccess = false,
                            Message = "Internal Server Error",
                            ErrorMessages = new List<string> { apiContent }
                        };
                    }
                default:
                    try
                    {
                        var apiResDTO = JsonConvert.DeserializeObject<ResponseDTO>(apiContent);
                        return apiResDTO;
                    }
                    catch (JsonReaderException)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Failed to parse server response",
                            ErrorMessages = new List<string> { apiContent }
                        };
                    }
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
    }*/

    public async Task<ResponseDTO?> SendAsync(RequestDTO requestDTO, bool withBearer = true)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient("VinAPI");
            HttpRequestMessage message = new();
            message.Headers.Add("Accept", "application/json");

            //pass token
            if (withBearer)
            {
                var token = _tokenProvider.GetToken();
                message.Headers.Add("Authorization", $"Bearer {token}");
            }

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

            // First, check content type
            var contentType = apiResponse.Content.Headers.ContentType?.MediaType;
            if (contentType != "application/json")
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Unexpected response format: {contentType}",
                    ErrorMessages = new List<string>
                {
                    apiContent.Substring(0, Math.Min(200, apiContent.Length))
                }
                };
            }

            // Handle specific response scenarios
            return apiResponse.StatusCode switch
            {
                HttpStatusCode.OK => TryParseSuccessResponse(apiContent),
                HttpStatusCode.BadRequest => HandleBadRequest(apiContent),
                HttpStatusCode.NotFound => new ResponseDTO { IsSuccess = false, Message = "Not Found" },
                HttpStatusCode.Forbidden => new ResponseDTO { IsSuccess = false, Message = "Access Denied" },
                HttpStatusCode.Unauthorized => new ResponseDTO { IsSuccess = false, Message = "Unauthorized" },
                HttpStatusCode.InternalServerError => HandleInternalServerError(apiContent),
                _ => new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Unexpected response",
                    ErrorMessages = new List<string> { apiContent }
                }
            };
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

    // Helper method to parse successful responses
    private ResponseDTO? TryParseSuccessResponse(string apiContent)
    {
        try
        {
            var apiResDTO = JsonConvert.DeserializeObject<ResponseDTO>(apiContent);
            if (apiResDTO == null)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Failed to parse server response",
                    ErrorMessages = new List<string> { "Response was empty or invalid" }
                };
            }
            return apiResDTO;
        }
        catch (JsonReaderException jex)
        {
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = "Invalid JSON response",
                ErrorMessages = new List<string>
            {
                $"Error: {jex.Message}",
                $"Content: {apiContent.Substring(0, Math.Min(100, apiContent.Length))}"
            }
            };
        }
    }

    // Handle BadRequest (400) scenarios
    private ResponseDTO HandleBadRequest(string apiContent)
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

            // If not validation errors, try general error response
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

    // Handle InternalServerError (500) scenarios
    private ResponseDTO HandleInternalServerError(string apiContent)
    {
        try
        {
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(apiContent);
            if (errorResponse?.Errors?.Contains("MessageBusFailure") == true)
            {
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Added to cart with some issues",
                    ErrorMessages = errorResponse.Errors.ToList()
                };
            }
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = "Internal Server Error",
                ErrorMessages = new List<string> { apiContent }
            };
        }
        catch (JsonReaderException)
        {
            return new ResponseDTO
            {
                IsSuccess = false,
                Message = "Internal Server Error",
                ErrorMessages = new List<string> { apiContent }
            };
        }
    }
}