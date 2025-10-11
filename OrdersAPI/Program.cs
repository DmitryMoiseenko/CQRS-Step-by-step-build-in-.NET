using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ReadDbContext>(opt => opt.UseSqlite(
    builder.Configuration.GetConnectionString("ReadDbConnection")));
builder.Services.AddDbContext<WriteDbContext>(opt => opt.UseSqlite(
    builder.Configuration.GetConnectionString("WriteDbConnection")));


builder.Services.AddScoped<ICommandHandler<CreateOderCommand, OrderDto>, CreateOrderCommandHandler>();
builder.Services.AddScoped<IValidator<CreateOderCommand>, CreateOrderCommandValidator>();

builder.Services.AddScoped<IQueryHandler<GetOrderByIdQuery, OrderDto>, GetOrderByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetOrderSummariesQuery, List<OrderSummaryDto>>, GetOrderSummariesQueryHandler>();

builder.Services.AddSingleton<IEventPublisher, InProcessEventPublisher>();
builder.Services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedProjectionHandler>();

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

app.MapGet("/api/orders", async (IQueryHandler<GetOrderSummariesQuery, List<OrderSummaryDto>> handler) =>
{
    var summaries = await handler.HandleAsync(new GetOrderSummariesQuery());

    return Results.Ok(summaries);
});

app.Run();