using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YEDUSO.EA
{
    public class EventAggregator : IEventAggregator
    {
        private readonly List<Handler> _handlers;
        private readonly List<IGlobalEventMonitor> _globalEventMonitors;
        private Type _restrictedBaseType;

        public EventAggregator()
        {
            _handlers = new List<Handler>();
            _globalEventMonitors = new List<IGlobalEventMonitor>();
        }

        public int GetNumberOfHandlers()
        {
            return _handlers.Count;
        }

        public void Subscribe(object subscribedObject)
        {
            if (!(subscribedObject is IEvent))
            {
                return;
            }

            lock (_handlers)
            {
                _handlers.Add(new Handler(subscribedObject as IEvent));
            }
        }

        public void SubscribeToGlobalMonitoring(IGlobalEventMonitor monitor)
        {
            lock (_globalEventMonitors)
            {
                _globalEventMonitors.Add(monitor);
            }
        }

        public void Unsubscribe(object subscribedObject)
        {
            if (!(subscribedObject is IEvent))
            {
                return;
            }

            lock (_handlers)
            {
                var match = _handlers.FirstOrDefault(_ => _.Matches(subscribedObject as IEvent));
                if (match != null)
                {
                    _handlers.Remove(match);
                }
            }
        }

        private void SubmitHandlerToMonitors(IGlobalEventMonitor[] monitors, Handler handler, Guid transactionGuid)
        {
            foreach (var monitor in monitors)
            {
                monitor.MessageHandledBy(transactionGuid, handler.Reference.Target);
            }
        }

        public void Publish<T>(T eventMessage) where T : class
        {
            VerifyRestrictedBaseType(eventMessage);

            var transactionGuid = Guid.NewGuid();

            IGlobalEventMonitor[] monitors;
            lock (_globalEventMonitors)
            {
                monitors = _globalEventMonitors.ToArray();
            }

            foreach (var monitor in monitors)
            {
                monitor.HandleAnyMessage(transactionGuid, eventMessage);
            }

            Handler[] handles;
            lock (_handlers)
            {
                handles = _handlers.ToArray();
            }

            var trashCan = handles.Where(handler =>
            {
                var noReference = handler.Handle(eventMessage, out bool messageDelivered);
                if (messageDelivered)
                {
                    SubmitHandlerToMonitors(monitors, handler, transactionGuid);
                }
                return !noReference;
            });
            if (trashCan.Any())
            {
                lock (_handlers)
                {
                    foreach (var trashItem in trashCan)
                    {
                        _handlers.Remove(trashItem);
                    }
                }
            }
        }

        public async Task PublishAsync<T>(T eventMessage) where T : class
        {
            VerifyRestrictedBaseType(eventMessage);

            var transactionGuid = Guid.NewGuid();

            IGlobalEventMonitor[] monitors;
            lock (_globalEventMonitors)
            {
                monitors = _globalEventMonitors.ToArray();
            }

            foreach (var monitor in monitors)
            {
                monitor.HandleAnyMessage(transactionGuid, eventMessage);
            }

            Handler[] handles;
            lock (_handlers)
            {
                handles = _handlers.ToArray();
            }

            var trashCan = new List<Handler>();
            foreach (var handle in handles)
            {
                var result = await handle.HandleAsync(eventMessage);
                if (result.MessageDelivered)
                {
                    SubmitHandlerToMonitors(monitors, handle, transactionGuid);
                }

                if (!result.NoReference)
                {
                    trashCan.Add(handle);
                }
            }

            if (trashCan.Any())
            {
                lock (_handlers)
                {
                    foreach (var trashItem in trashCan)
                    {
                        _handlers.Remove(trashItem);
                    }
                }
            }
        }

        public IEnumerable<object> Inquire<T>(T eventMessage) where T : class
        {
            VerifyRestrictedBaseType(eventMessage);

            var transactionGuid = Guid.NewGuid();

            IGlobalEventMonitor[] monitors;
            lock (_globalEventMonitors)
            {
                monitors = _globalEventMonitors.ToArray();
            }

            foreach (var monitor in monitors)
            {
                monitor.HandleAnyMessage(transactionGuid, eventMessage);
            }

            Handler[] handles;

            lock (_handlers)
            {
                handles = _handlers.ToArray();
            }

            var results = new List<object>();
            var trashCan = new List<Handler>();
            foreach (var handle in handles)
            {
                object output = null;
                bool valueAssigned = false;
                if (!handle.Inquire(eventMessage, ref output, ref valueAssigned))
                {
                    trashCan.Add(handle);
                }
                else
                {
                    if (valueAssigned)
                    {
                        SubmitHandlerToMonitors(monitors, handle, transactionGuid);
                        results.Add(output);
                    }
                }
            }

            if (trashCan.Any())
            {
                lock (_handlers)
                {
                    foreach (var trashItem in trashCan)
                    {
                        _handlers.Remove(trashItem);
                    }
                }
            }
            return results;
        }

        public async Task<IEnumerable<object>> InquireAsync<T>(T eventMessage) where T : class
        {
            VerifyRestrictedBaseType(eventMessage);

            var transactionGuid = Guid.NewGuid();

            IGlobalEventMonitor[] monitors;
            lock (_globalEventMonitors)
            {
                monitors = _globalEventMonitors.ToArray();
            }

            foreach (var monitor in monitors)
            {
                monitor.HandleAnyMessage(transactionGuid, eventMessage);
            }

            Handler[] handles;

            lock (_handlers)
            {
                handles = _handlers.ToArray();
            }

            var results = new List<object>();
            var trashCan = new List<Handler>();
            foreach (var handle in handles)
            {
                var result = await handle.InquireAsync(eventMessage);
                if (!result.NoReference)
                {
                    trashCan.Add(handle);
                }
                else
                {
                    if (result.ValueAssigned)
                    {
                        SubmitHandlerToMonitors(monitors, handle, transactionGuid);
                        results.Add(result.Output);
                    }
                }
            }

            if (trashCan.Any())
            {
                lock (_handlers)
                {
                    foreach (var trashItem in trashCan)
                    {
                        _handlers.Remove(trashItem);
                    }
                }
            }
            return results;
        }

        private void VerifyRestrictedBaseType<T>(T eventMessage) where T : class
        {
            if (_restrictedBaseType != null && !_restrictedBaseType.IsInstanceOfType(eventMessage))
            {
                throw new EventAggregatorIncorrectedRestrictedTypeException(_restrictedBaseType);
            }
        }

        public void RestrictToBaseType(Type type)
        {
            _restrictedBaseType = type;
        }

        public Type GetRestrictedBaseType()
        {
            return _restrictedBaseType;
        }

        public void RemoveRestrictedBaseType()
        {
            _restrictedBaseType = null;
        }
    }
}
