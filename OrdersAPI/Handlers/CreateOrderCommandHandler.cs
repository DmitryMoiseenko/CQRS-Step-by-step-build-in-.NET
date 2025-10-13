using FluentValidation;
using MediatR;

public class CreateOrderCommandHandler : IRequestHandler<CreateOderCommand, OrderDto>
{
    private WriteDbContext _context;
    private IValidator<CreateOderCommand> _validator;
    private IMediator _mediator;

    public CreateOrderCommandHandler(
        WriteDbContext context,
        IValidator<CreateOderCommand> validator,
        IMediator mediator)
    {
        _context = context;
        _validator = validator;
        _mediator = mediator;
    }

    public async Task<OrderDto> Handle(CreateOderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        var order = new Order
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Status = request.Status,
            CreatedAt = DateTime.Now,
            TotalCost = request.TotalCost
        };

        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var orderCreatedEvent = new OrderCreatedEvent
        (
            order.Id,
            order.FirstName,
            order.LastName,
            order.TotalCost
        );

        await _mediator.Publish(orderCreatedEvent);

        return new OrderDto(
            order.Id,
            order.FirstName,
            order.LastName,
            order.Status,
            order.CreatedAt,
            order.TotalCost
        );
    }
}