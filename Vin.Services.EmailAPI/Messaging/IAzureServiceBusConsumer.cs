namespace Vin.Services.EmailAPI.Messaging
{
    public interface IAzureServiceBusConsumer
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
