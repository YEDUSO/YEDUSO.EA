using System;
using System.Threading.Tasks;

namespace YEDUSO.EA
{
    public class Handler
    {
        public WeakReference Reference { get; private set; }

        public Handler(object handler)
        {
            Reference = new WeakReference(handler);
        }

        public bool Matches(object ob)
        {
            return Reference.Target == ob;
        }

        public bool Handle<T>(T message, out bool messageDelivered) where T : class
        {
            messageDelivered = false;
            if (Reference.Target == null) return false;
            var castOb = Reference.Target as IHandle<T>;
            if (castOb != null)
            {
                castOb.Handle(message);
                messageDelivered = true;
            }

            var castObAsync = Reference.Target as IHandleAsync<T>;
            if (castObAsync != null)
            {
                Task.Run(async () => await castObAsync.HandleAsync(message));
                messageDelivered = true;
            }

            return true;
        }

        public async Task<(bool NoReference, bool MessageDelivered)> HandleAsync<T>(T message) where T : class
        {
            var messageDelivered = false;
            if (Reference.Target == null) return (false, messageDelivered);
            var castOb = Reference.Target as IHandle<T>;
            if (castOb != null)
            {
                castOb.Handle(message);
                messageDelivered = true;
            }

            var castObAsync = Reference.Target as IHandleAsync<T>;
            if (castObAsync != null)
            {
                await castObAsync.HandleAsync(message);
                messageDelivered = true;
            }

            return (true, messageDelivered);
        }

        public bool Inquire<T>(T message, ref object output, ref bool valueAssigned) where T : class
        {
            valueAssigned = false;
            if (Reference.Target == null) return false;
            var castOb = Reference.Target as IInquire<T>;
            if (castOb != null)
            {
                valueAssigned = true;
                output = castOb.Inquire(message);
            }
            return true;
        }

        public async Task<(bool NoReference, bool ValueAssigned, object Output)> InquireAsync<T>(T message) where T : class
        {
            if (Reference.Target == null) return (false, false, null);
            var castOb = Reference.Target as IInquireAsync<T>;
            if (castOb != null)
            {
                var result = await castOb.InquireAsync(message);
                return (true, true, result);
            }
            return (true, false, null);
        }

        public override string ToString()
        {
            return Reference == null
                ? "NULL REFERENCE"
                : $"REF: {Reference.Target}";
        }
    }
}