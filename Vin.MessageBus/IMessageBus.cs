namespace Vin.MessageBus
{
    public interface IMessageBus
    {
        Task PublishMessageAsync(
            object message,
            string topicQueueName,
            CancellationToken cancellationToken = default);

        Task PublishMessageAsync(
            object message,
            string topicQueueName,
            MessageBusMetadata metadata,
            CancellationToken cancellationToken = default);
    }
}
