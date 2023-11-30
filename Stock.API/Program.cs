using MassTransit;
using Stock.API.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSingleton<MongoDbService>();

var app = builder.Build();
using IServiceScope scope = app.Services.CreateScope();
var mongoDBService = scope.ServiceProvider.GetService<MongoDbService>();
var collection =  mongoDBService.GetCollection<Stock.API.Models.Stock>();
if (! collection.FindSync(session => true).Any())
{
    await collection.InsertOneAsync(new Stock.API.Models.Stock() { ProductId = 1, Count = 100 });
    await collection.InsertOneAsync(new Stock.API.Models.Stock() { ProductId = 2, Count = 200 });
    await collection.InsertOneAsync(new Stock.API.Models.Stock() { ProductId = 3, Count = 300 });
    await collection.InsertOneAsync(new Stock.API.Models.Stock() { ProductId = 4, Count = 400 });

}

app.Run();
