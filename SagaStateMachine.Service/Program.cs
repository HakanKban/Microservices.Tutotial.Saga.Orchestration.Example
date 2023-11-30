using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
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

var host = builder.Build();
host.Run();
