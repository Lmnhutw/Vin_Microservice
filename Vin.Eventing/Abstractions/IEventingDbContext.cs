using Microsoft.EntityFrameworkCore;
using Vin.Eventing.Inbox;
using Vin.Eventing.Outbox;

namespace Vin.Eventing.Abstractions;

public interface IEventingDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<InboxMessage> InboxMessages { get; }
}
