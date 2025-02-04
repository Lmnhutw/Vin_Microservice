namespace Vin.MessageBus.Settings
{
    public class ServiceBusSettings
    {
        public string MsB_ConnectionString { get; set; }
        public string QueueName { get; set; }

        /*public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(MsB_ConnectionString) &&
                   !string.IsNullOrWhiteSpace(QueueName);
        }*/
    }
}
