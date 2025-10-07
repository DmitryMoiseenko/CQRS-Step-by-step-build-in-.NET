using System.ComponentModel.DataAnnotations;
using FluentValidation;

public class CreateOrderCommandHandler : ICommandHandler<CreateOderCommand, OrderDto>
{
    private AppDbContext _context;
    private IValidator<CreateOderCommand> _validator;

    public CreateOrderCommandHandler(AppDbContext context, IValidator<CreateOderCommand> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<OrderDto> HandleAsync(CreateOderCommand command)
    {
        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        var order = new Order
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Status = command.Status,
            CreatedAt = DateTime.Now,
            TotalCost = command.TotalCost
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

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