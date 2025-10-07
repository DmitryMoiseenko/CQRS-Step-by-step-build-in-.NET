using FluentValidation;

public class CreateOrderCommandValidator : AbstractValidator<CreateOderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Status).NotEmpty();
        RuleFor(x => x.TotalCost).GreaterThan(0);
    }
}