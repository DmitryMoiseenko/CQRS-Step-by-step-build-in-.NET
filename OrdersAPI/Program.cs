using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ReadDbContext>(opt => opt.UseSqlite(
    builder.Configuration.GetConnectionString("ReadDbConnection")));
builder.Services.AddDbContext<WriteDbContext>(opt => opt.UseSqlite(
    builder.Configuration.GetConnectionString("WriteDbConnection")));
builder.Services.AddScoped<IValidator<CreateOderCommand>, CreateOrderCommandValidator>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

app.MapPost("/api/orders", async (IMediator mediator, CreateOderCommand command) =>
{
    var createdOrder = await mediator.Send(command);
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

app.MapGet("/api/orders/{id}", async (IMediator mediator, int id) =>
{
    var order = await mediator.Send(new GetOrderByIdQuery(id));

    if (order == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(order);
});

app.MapGet("/api/orders", async (IMediator mediator) =>
{
    var summaries = await mediator.Send(new GetOrderSummariesQuery());

    return Results.Ok(summaries);
});

app.Run();