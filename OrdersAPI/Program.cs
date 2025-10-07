using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(
    builder.Configuration.GetConnectionString("BaseConnection")));

builder.Services.AddScoped<ICommandHandler<CreateOderCommand, OrderDto>, CreateOrderCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetOrderByIdQuery, OrderDto>, GetOrderByIdQueryHandler>();
builder.Services.AddScoped<IValidator<CreateOderCommand>, CreateOrderCommandValidator>();

var app = builder.Build();

app.MapPost("/api/orders", async (ICommandHandler<CreateOderCommand, OrderDto> handler, CreateOderCommand command) =>
{
    var createdOrder = await handler.HandleAsync(command);
    try
    {
        if (createdOrder == null)
        {
            return Results.BadRequest("Failed to create order");
        }

        return Results.Created($"/api/orders/{createdOrder.Id}", createdOrder);
    }
    catch (ValidationException exception)
    {
        var errors = exception.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
        return Results.BadRequest(errors);
    }
});

app.MapGet("/api/orders/{id}", async (IQueryHandler<GetOrderByIdQuery, OrderDto> handler, int id) =>
{
    var order = await handler.HandleAsync(new GetOrderByIdQuery(id));

    if (order == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(order);
});

app.Run();
