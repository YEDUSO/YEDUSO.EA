using System;

namespace YellowDuckSoftware.EA
{
    public interface IGlobalEventMonitor
    {
        void HandleAnyMessage(Guid transactionGuid, object message);
        void MessageHandledBy(Guid transactionGuid, object handler);
    }
}
