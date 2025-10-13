using MediatR;

public record OrderCreatedEvent
(
    int OderId,
    string FirstName,
    string LastName,
    decimal TotalCost
) : INotification;