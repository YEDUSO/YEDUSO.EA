namespace YEDUSO.EA
{
    public interface IHandle : IEvent
    {
    }

    public interface IHandle<in T> : IHandle
    {
        void Handle(T message);
    }
}