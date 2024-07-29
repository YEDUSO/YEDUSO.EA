using System.Threading.Tasks;

namespace YellowDuckSoftware.EA
{
    public interface IInquireAsync : IEvent
    {
    }

    public interface IInquireAsync<in T> : IInquire
    {
        Task<object> InquireAsync(T message);
    }
}
