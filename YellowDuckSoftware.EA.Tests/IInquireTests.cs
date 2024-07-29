using System.Linq;
using Tests;
using Xunit;
using YellowDuckSoftware.EA;

namespace YelllowDuckSoftware.EA.Tests
{
    public class IInquireTests :
        IInquire<TestInquiry>
    {
        private IEventAggregator _eventAggregator;

        public IInquireTests()
        {
            _eventAggregator = new EventAggregator();
            _eventAggregator.Subscribe(this);
        }

        [Fact]
        public void ShouldGetSingleOne()
        {
            var result = _eventAggregator.Inquire(new TestInquiry());
            Assert.NotNull(result);
            Assert.Equal(1, result.Count());
            Assert.Equal("ONE", result.First());
        }

        public object Inquire(TestInquiry inquiry)
        {
            return "ONE";
        }
    }
}
