namespace Vin.MessageBus
{
    public interface IMessageBus
    {
        Task PublishMessageAsync(
            object message,
            string topicQueueName,
            CancellationToken cancellationToken = default);
    }
}
