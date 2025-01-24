namespace Vin.MessageBus
{
    public interface IMessageBus
    {
        Task PublishMessageAsync(object message, string topic_queue_Name);
    }
}
