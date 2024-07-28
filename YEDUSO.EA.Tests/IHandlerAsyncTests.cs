using System.Threading.Tasks;
using Xunit;
using YEDUSO.EA;

namespace YelllowDuckSoftware.EA.Tests
{
    public class IHandlerAsyncTests
    {
        public class TestMessage
        {
            public string Value { get; set; }

            public TestMessage(string value)
            {
                Value = value;
            }
        }

        public class CallingClass :
            IHandleAsync<TestMessage>
        {
            private readonly IEventAggregator _eventAggregator;

            public CallingClass(ValueContainer valueContainer, IEventAggregator eventAggregator)
            {
                ValueContainer = valueContainer;
                _eventAggregator = eventAggregator;
                _eventAggregator.Subscribe(this);
            }

            public ValueContainer ValueContainer { get; }

            public async Task HandleAsync(TestMessage message)
            {
                await Task.Delay(250);
                ValueContainer.Value = message.Value;
            }
        }

        public class ValueContainer
        {
            public string Value { get; set; }
        }

        private ValueContainer _valueContainer;
        private IEventAggregator _eventAggregator;

        public IHandlerAsyncTests()
        {
            _eventAggregator = new EventAggregator();
            _valueContainer = new ValueContainer();
        }

        [Fact]
        public async Task WillSetValue()
        {
            var cc = new CallingClass(_valueContainer, _eventAggregator);
            _valueContainer.Value = "";
            await _eventAggregator.PublishAsync(new TestMessage("VALUE"));
            Assert.Equal("VALUE", _valueContainer.Value);
        }
    }
}
