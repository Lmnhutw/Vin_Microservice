namespace Vin.MessageBus.Abtractions
{
    public interface IMessage
    {
        Guid Id { get; }
        DateTime CreatedAt { get; }

    }
}
