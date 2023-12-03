using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service.StateInstance;
using SagaStateMachine.Service.StateMachine;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMassTransit(conf =>
{
    conf.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
    .EntityFrameworkRepository(opt =>
    {
        opt.AddDbContext<DbContext, OrderStateDbContext>((provider, _builder) =>
        {
            _builder.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
        });
    });


    conf.UsingRabbitMq((context, _conf) =>
    {
        _conf.Host("localhost");

    });
});

var host = builder.Build();
host.Run();
