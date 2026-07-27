using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vin.Web.Models;
using Vin.Web.Models.OrderModels;
using Vin.Web.Service.IService;
using Vin.Web.Utility;

namespace Vin.Web.Service;

public sealed class OrderService : IOrderService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenProvider _tokenProvider;

    public OrderService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
    {
        _httpClientFactory = httpClientFactory;
        _tokenProvider = tokenProvider;
    }

    public Task<ResponseDTO> CreateOrderAsync(CreateOrderRequestDTO request, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Post, StaticDetail.OrderAPIBase + "/api/orders", request, cancellationToken);

    public Task<ResponseDTO> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Get, $"{StaticDetail.OrderAPIBase}/api/orders/{orderId}", null, cancellationToken);

    private async Task<ResponseDTO> SendAsync(HttpMethod method, string url, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var token = _tokenProvider.GetToken();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        if (body is not null)
        {
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        }

        try
        {
            using var response = await _httpClientFactory.CreateClient("VinAPI").SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return new ResponseDTO { Result = JsonConvert.DeserializeObject<object>(content) };
            }

            var message = ParseProblemMessage(content) ?? response.StatusCode switch
            {
                HttpStatusCode.NotFound => "Order not found.",
                HttpStatusCode.Unauthorized => "Your session has expired.",
                _ => "Order service could not process the request."
            };
            return new ResponseDTO { IsSuccess = false, Message = message };
        }
        catch (HttpRequestException)
        {
            return new ResponseDTO { IsSuccess = false, Message = "Order service is unavailable." };
        }
        catch (TaskCanceledException)
        {
            return new ResponseDTO { IsSuccess = false, Message = "Order service request timed out." };
        }
    }

    private static string? ParseProblemMessage(string content)
    {
        try
        {
            return JObject.Parse(content).Value<string>("detail") ?? JObject.Parse(content).Value<string>("title");
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
