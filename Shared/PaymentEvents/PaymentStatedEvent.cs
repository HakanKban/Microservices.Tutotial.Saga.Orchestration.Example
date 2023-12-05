using MassTransit;
using Shared.Messages;

namespace Shared.PaymentEvents;
public class PaymentStatedEvent : CorrelatedBy<Guid>
{
    public PaymentStatedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
    public Guid CorrelationId { get; }
    public decimal TotalPrice { get; set; }
    public List<OrderItemMessage> OrderItemMessages { get; set; }
}
