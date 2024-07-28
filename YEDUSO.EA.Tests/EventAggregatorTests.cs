using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YEDUSO.EA;

namespace Tests
{
    public interface ITestEventCommand
    {
    }

    public class TestEventCommandBase : ITestEventCommand
    {
    }

    public class TestEvent : TestEventCommandBase
    {
    }

    public class TestMessage
    {
    }

    public class TestInquiry
    {
        public int Value { get; set; } = 1;
    }

    public class AdditionalHandler :
        IHandle<TestEvent>,
        IInquire<TestInquiry>
    {
        private IEventAggregator _eventAggregator;
        private string _inquireResponse;

        public bool IHandleWasRun;
        public bool IInquireWasRun;

        public AdditionalHandler(IEventAggregator eventAggregator, string inquireResponse)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            _inquireResponse = inquireResponse;
        }

        public void Handle(TestEvent message)
        {
            IHandleWasRun = true;
        }

        public object Inquire(TestInquiry message)
        {
            IInquireWasRun = true;
            return _inquireResponse;
        }
    }

    public class AdditionalAsyncHandler :
        IHandleAsync<TestEvent>,
        IInquireAsync<TestInquiry>
    {
        private IEventAggregator _eventAggregator;
        private string _inquireResponse;

        public bool IHandleAsyncWasRun;
        public bool IInquireAsyncWasRun;

        public AdditionalAsyncHandler(IEventAggregator eventAggregator, string inquireResponse)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            _inquireResponse = inquireResponse;
        }

        public Task HandleAsync(TestEvent message)
        {
            IHandleAsyncWasRun = true;
            return Task.CompletedTask;
        }

        public async Task<object> InquireAsync(TestInquiry message)
        {
            IInquireAsyncWasRun = true;
            return await Task.FromResult(_inquireResponse);
        }
    }

    public class Tests :
        IGlobalEventMonitor,
        IInquire<TestInquiry>,
        IInquireAsync<TestInquiry>,
        IHandle<TestMessage>,
        IHandle<TestEvent>,
        IHandleAsync<TestEvent>
    {
        private IEventAggregator _eventAggregator;
        private int _value;

        public Tests()
        {
            _eventAggregator = new EventAggregator();
            _eventAggregator.Subscribe(this);
        }

        [Fact]
        public void InquireWillReturnValue()
        {
            var ah1 = new AdditionalHandler(_eventAggregator, "TWO");
            var ah2 = new AdditionalHandler(_eventAggregator, "THREE");
            var ah3 = new AdditionalAsyncHandler(_eventAggregator, "FOUR");
            var ah4 = new AdditionalAsyncHandler(_eventAggregator, "FIVE");

            var results = _eventAggregator.Inquire(new TestInquiry());
            var x = results.Count();
            Assert.Equal(3, x);
            Assert.Equal("ONE", results.First() as string);
            Assert.Equal(5, _eventAggregator.GetNumberOfHandlers());

            Assert.Equal(true, results.Any(r => (r as string) == "ONE"));
        }

        [Fact]
        public async Task InquireAsyncWillReturnValue()
        {
            _eventAggregator.SubscribeToGlobalMonitoring(this);

            var ah1 = new AdditionalHandler(_eventAggregator, "TWO");
            var ah2 = new AdditionalHandler(_eventAggregator, "THREE");
            var ah3 = new AdditionalAsyncHandler(_eventAggregator, "FOUR");
            var ah4 = new AdditionalAsyncHandler(_eventAggregator, "FIVE");

            var results = await _eventAggregator.InquireAsync(new TestInquiry());
            var x = results.Count();
            Assert.Equal(3, x);
            Assert.Equal("SIX", results.First() as string);
            Assert.Equal(5, _eventAggregator.GetNumberOfHandlers());

            Assert.Equal(true, results.Any(r => (r as string) == "FIVE"));
        }

        [Fact]
        public void InquiryWillReturnResultBasedOnPublish()
        {
            _eventAggregator.Publish(new TestMessage());
            var results = _eventAggregator.Inquire(new TestInquiry()); ;
            //Assert.Equal(11, value);
        }

        [Fact]
        public void PublishWillSetValue()
        {
            _eventAggregator.Publish(new TestEvent());
            Assert.Equal(15, _value);
        }

        public object Inquire(TestInquiry message)
        {
            return "ONE";
        }

        public void Handle(TestMessage message)
        {
            _value = 10;
        }

        public void Handle(TestEvent message)
        {
            _value = 15;
        }

        public async Task HandleAsync(TestEvent message)
        {
            await Task.FromResult(1);
        }

        public async Task<object> InquireAsync(TestInquiry message)
        {
            return await Task.FromResult("SIX");
        }

        #region IGlobalEventMonitor

        public void HandleAnyMessage(Guid transactionGuid, object message)
        {
        }

        public void MessageHandledBy(Guid transactionGuid, object handler)
        {
        }

        #endregion
    }
}