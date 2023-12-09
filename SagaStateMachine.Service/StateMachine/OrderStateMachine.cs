using MassTransit;
using SagaStateMachine.Service.StateInstance;
using Shared.Messages;
using Shared.OrderEvents;
using Shared.PaymentEvents;
using Shared.Settings;
using Shared.StockEvents;

namespace SagaStateMachine.Service.StateMachine
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        //Consume edilecek eventler.
        public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
        public Event<StockReservedEvent> StockReservedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }

        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }
        public State StockNotReserved { get; set; }


        public OrderStateMachine()
        {
            InstanceState(ins => ins.CurrentState);
            //StateMachine de gelen order ıd yok ise yeni corelation ıd oluşturulur.
            Event(() => OrderStartedEvent,
                orderStateIns => orderStateIns.CorrelateBy<int>(
                    db => db.OrderId, @event => @event.Message.OrderId)
                .SelectId(e => Guid.NewGuid()));
            Event(() => StockReservedEvent,
                orderStateIns => orderStateIns.CorrelateById(@event =>
                @event.Message.CorrelationId));
            Event(() => StockNotReservedEvent,
                orderStateIns => orderStateIns.CorrelateById(@event =>
                @event.Message.CorrelationId));
            Event(() => PaymentCompletedEvent,
                orderStateIns => orderStateIns.CorrelateById(@event =>
                @event.Message.CorrelationId));
            Event(() => PaymentFailedEvent,
                orderStateIns => orderStateIns.CorrelateById(@event =>
                @event.Message.CorrelationId));

            Initially(When(OrderStartedEvent).
                Then(context =>
                {
                    //Instance Veri tabanındaki siparişin ınstance ---- Context Data eventden gelen data.
                    context.Instance.BuyerId = context.Data.BuyerId;
                    context.Instance.OrderId = context.Data.OrderId;
                    context.Instance.TotalPrice = context.Data.TotalPrice;
                    context.Instance.CreatedDate = DateTime.UtcNow;
                })
                .TransitionTo(OrderCreated)
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"),
                context => new OrderCreatedEvent(context.Instance.CorrelationId)
                {
                    OrderItemMessages = context.Data.OrderItemMessages,
                }));

            During(OrderCreated, When(StockReservedEvent)
                .TransitionTo(StockReserved)
                 .Send(new Uri($"queue:{RabbitMQSettings.Payment_StartedEventQueue}"),
                 context => new PaymentStatedEvent(context.Instance.CorrelationId)
                 {
                     TotalPrice = context.Instance.TotalPrice,
                     OrderItemMessages = context.Data.OrderItemMessages
                 }),
                 When(StockNotReservedEvent)
                 .TransitionTo(StockNotReserved)
                  .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                  context => new OrderFailedEvent
                  {
                      OrderId = context.Instance.OrderId,
                      Message = context.Data.Message
                  }));
            During(StockReserved,
                When(PaymentFailedEvent)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderCompletedEventQueue}"),
                context => new OrderCompletedEvent
                {
                    OrderId = context.Instance.OrderId
                })
                .Finalize(),
                When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                context => new OrderFailedEvent
                {
                    OrderId = context.Instance.OrderId,
                    Message = context.Data.Message
                })
               .Send(new Uri($"queue:{RabbitMQSettings.Stock_RollbackMessageQueue}"),
                context => new StockRollBeckMessage
                {
                    OrderItemMessages = context.Data.OrderItemMessages
                }));
            SetCompletedWhenFinalized();
        }
    }
}
