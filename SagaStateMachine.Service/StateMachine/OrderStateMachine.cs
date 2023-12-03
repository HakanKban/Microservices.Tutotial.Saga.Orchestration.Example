using MassTransit;
using SagaStateMachine.Service.StateInstance;

namespace SagaStateMachine.Service.StateMachine
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public OrderStateMachine()
        {
            InstanceState(ins => ins.CurrentState);
        }
    }
}
