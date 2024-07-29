using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YellowDuckSoftware.EA
{
    public interface IEventAggregator
    {
        int GetNumberOfHandlers();

        void Subscribe(object subscribedObject);

        void SubscribeToGlobalMonitoring(IGlobalEventMonitor monitor);

        void Unsubscribe(object subscribedObject);

        void Publish<T>(T eventMessage) where T : class;

        Task PublishAsync<T>(T eventMessage) where T : class;

        IEnumerable<object> Inquire<T>(T eventMessage) where T : class;

        Task<IEnumerable<object>> InquireAsync<T>(T eventMessage) where T : class;

        void RestrictToBaseType(Type type);

        Type GetRestrictedBaseType();

        void RemoveRestrictedBaseType();
    }
}