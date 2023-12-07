using MassTransit;
using SagaStateMachine.Service.StateInstance;
using Shared.OrderEvents;
using Shared.PaymentEvents;
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
        }
    }
}
