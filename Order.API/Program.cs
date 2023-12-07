using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Context;
using Order.API.ViewModel;
using Shared.OrderEvents;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderDbContext>(conf => conf.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

builder.Services.AddMassTransit(conf =>
{
    conf.UsingRabbitMq((context, _conf) =>
    {
        _conf.Host("localhost");

    });
});


var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrderVM model, OrderDbContext context, ISendEndpointProvider sendEndpointProvider ) =>
{
    Order.API.Models.Order order = new()
    {
        BuyerId = model.BuyerId,
        CreatedDate = DateTime.UtcNow,
        OrderStatus = Order.API.Enum.OrderStatus.Suspend,
        TotalPrice = model.OrderItems.Sum(x => x.Count * x.Price),
        OrderItems = model.OrderItems.Select(
            x => new Order.API.Models.OrderItem
            {
                Price = x.Price,
                Count = x.Count,
                ProductId = x.ProductId
            }).ToList()
    };

    await context.AddAsync(order);
    await context.SaveChangesAsync();
    OrderStartedEvent orderStartedEvent = new()
    {
        BuyerId = model.BuyerId,
        OrderId = order.Id,
        TotalPrice = model.OrderItems.Sum(x => x.Price * x.Count),
        OrderItemMessages = model.OrderItems.Select(x => new Shared.Messages.OrderItemMessage
        {
            Price = x.Price,
            Count = x.Count,
            ProductId = x.ProductId
        }).ToList()
    };
    var send =await sendEndpointProvider.GetSendEndpoint(new Uri($"queue: {Shared.Settings.RabbitMQSettings.StateMachineQueue}"));
    await send.Send<OrderStartedEvent>(orderStartedEvent);
});

app.Run();
