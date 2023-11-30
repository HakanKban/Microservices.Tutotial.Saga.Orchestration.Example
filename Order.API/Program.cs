using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Context;
using Order.API.ViewModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderDbContext>(conf => conf.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

builder.Services.AddMassTransit(conf =>
{
    //conf.AddConsumer<OrderCreatedEventConsumer>();
    //conf.AddConsumer<PaymentFailedEventConsumer>();
    conf.UsingRabbitMq((context, _conf) =>
    {
        _conf.Host("localhost");
        //_conf.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue,
        //    e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));

        //_conf.ReceiveEndpoint(RabbitMQSettings.Stock_PaymentFailedEventQueue,
        //    e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));

    });
});


var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrderVM model, OrderDbContext context) =>
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
});

app.Run();
