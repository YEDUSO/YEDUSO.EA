using Xunit;
using YEDUSO.EA;

namespace YelllowDuckSoftware.EA.Tests
{
    public class GenericCommand
    {
        public int NewValue;
    }

    public class BaseClassNoHandler
    {
        protected IEventAggregator _eventAggregator;
        public int Value;

        public BaseClassNoHandler(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Value = 0;
        }

        public void DoSubscribe()
        {
            _eventAggregator.Subscribe(this);
        }

        public void DoUnsubscribe()
        {
            _eventAggregator.Unsubscribe(this);
        }
    }

    public class SubClassWithHandler :
        BaseClassNoHandler,
        IHandle<GenericCommand>
    {
        public SubClassWithHandler(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public void Handle(GenericCommand command)
        {
            Value = command.NewValue;
        }
    }

    public class SubscribeUnsubscribeTests
    {
        [Fact]
        public void ShouldSubscribeAndUnsubscribeFromBase()
        {
            // Create an instance of a BaseClassNoHandler and tell it to subscribe.  This won't
            // actually do anything with the events since it doesn't have any handlers.  The
            // point here is that it can subscribe and unsunscribe without causing an error.
            var eventAggregator = new EventAggregator();
            var baseClass = new BaseClassNoHandler(eventAggregator);
            baseClass.DoSubscribe();
            baseClass.Value = 0;
            eventAggregator.Publish(new GenericCommand() { NewValue = 1 });
            Assert.Equal(0, baseClass.Value);
            baseClass.DoUnsubscribe();
        }

        [Fact]
        public void ShouldSubscribeAndUnsubscribeFromSub()
        {
            // Create an instance of the SubClassWithHandler and work it out.  This will
            // handle the events since it has handlers but the point here is that the
            // subscribe and unsubscribe are being called by the base class which does
            // not have any handlers, but the EA can still work properly.
            var eventAggregator = new EventAggregator();
            var subClass = new SubClassWithHandler(eventAggregator);
            subClass.DoSubscribe();
            subClass.Value = 0;
            eventAggregator.Publish(new GenericCommand() { NewValue = 1 });
            Assert.Equal(1, subClass.Value);
            subClass.DoUnsubscribe();
            eventAggregator.Publish(new GenericCommand() { NewValue = 2 });
            Assert.Equal(1, subClass.Value);
        }
    }
}
