using System;
using MediatR;

namespace ResortManagement.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
public abstract class DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
