namespace Vin.MessageBus.Settings
{
    public sealed class ServiceBusSettings
    {
        public string? MsB_ConnectionString { get; init; }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(MsB_ConnectionString);
    }
}
