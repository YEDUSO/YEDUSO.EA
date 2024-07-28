using Xunit;
using YEDUSO.EA;

namespace YelllowDuckSoftware.EA.Tests
{
    public interface IBaseCommandType
    {
        int ComputeValue(int input);
    }

    public class DerivedCommand : IBaseCommandType
    {
        public int Value;

        public DerivedCommand(int value)
        {
            Value = value;
        }

        public int ComputeValue(int input)
        {
            return input + 10;
        }
    }

    public class TestMessage
    {
        public string Value { get; set; }

        public TestMessage(string value)
        {
            Value = value;
        }
    }

    public class CallingClass :
        IHandle<TestMessage>
    {
        private readonly IEventAggregator _eventAggregator;

        public CallingClass(ValueContainer valueContainer, IEventAggregator eventAggregator)
        {
            ValueContainer = valueContainer;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }

        public ValueContainer ValueContainer { get; }

        public void Handle(TestMessage message)
        {
            ValueContainer.Value = message.Value;
        }
    }

    public class ValueContainer
    {
        public string Value { get; set; }
    }

    public class IHandlerTests :
        IHandle<DerivedCommand>

    {
        private int _value;
        private ValueContainer _valueContainer;
        private IEventAggregator _eventAggregator;

        public IHandlerTests()
        {
            _eventAggregator = new EventAggregator();
            _valueContainer = new ValueContainer();
            _eventAggregator.Subscribe(this);
        }

        [Fact]
        public void WillSetValue()
        {
            var cc = new CallingClass(_valueContainer, _eventAggregator);
            _valueContainer.Value = "";
            _eventAggregator.Publish(new TestMessage("VALUE"));
            Assert.Equal("VALUE", _valueContainer.Value);
        }

        [Fact]
        public void WillRestrictToBaseType()
        {
            _eventAggregator.RestrictToBaseType(typeof(IBaseCommandType));
            _value = 10;
            _eventAggregator.Publish(new DerivedCommand(_value));
            Assert.Equal(20, _value);

            Assert.Throws<EventAggregatorIncorrectedRestrictedTypeException>(() => _eventAggregator.Publish(new ValueContainer()));

            _eventAggregator.RemoveRestrictedBaseType();

            //Assert.exc .DoesNotThrow(() => _eventAggregator.Publish(new ValueContainer()));
        }

        public void Handle(DerivedCommand message)
        {
            _value = message.Value;
            _value = message.ComputeValue(_value);
        }
    }
}
