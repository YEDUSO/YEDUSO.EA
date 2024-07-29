namespace YellowDuckSoftware.EA
{
    public interface IInquire : IEvent
    {
    }

    public interface IInquire<in T> : IInquire
    {
        object Inquire(T message);
    }
}
