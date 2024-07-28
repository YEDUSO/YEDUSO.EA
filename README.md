## Overview
This event aggregator was specifically written for my exploration of games written in Unity and other utilities I have made for my day-to-day operations.
By default, this will create a nuget package in the D:\nuget folder every time it is built, eliminating the need to push to a server.
## Usage
After the package is built, set up a package source that refers to the D:\nuget folder and use this just as any other nuget package.  To make use of
the aggregator, a receiver needs to implement an IHandle<T> or IInquire<T> on its class and define the function of for the event class that is being passed.
Once the object is instantiated, it should subscribe itself to the aggregator.  This EA does not use reflection for performance sake.
For instance:
```
using YUEDUSO.EA;

public class CustomMessage
{
    public string Value { get; set; }

    public CustomMessage(string value)
    {
        Value = value;
    }
}

public class Recipient : IHandle<CustomMessage>
{
    public Recipient(IEventAggregator eventAggregator)
    {
        eventAggregator.Subscribe(this);
    }

    public void Handle(CustomMessage message)
    {
        var customMessage = message.Value;
        // Work with the message here.
    }
}
```

To send the event, the sender would need to have the EA from the constructor and just call the necessary publish function.
```
using YEDUSO.EA;

public class Sender
{
    private IEventAggregator _eventAggregator;

    public Sender(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }

    public void SendTheMessage()
    {
        _eventAggregator.Publish(new CustomMessage("TEST"));
    }
}
```
## Inquiries
This EA has an Inquire setup which allows a recipient to respond to a query.  It works just like the Publish but it will wait for a response.
```
using YUEDUSO.EA;

public class CustomQuery
{
    public int Value { get; set; }

    public CustomQuery(int value)
    {
        Value = value;
    }
}

public class Recipient : IInquire<CustomMessage>
{
    public Recipient(IEventAggregator eventAggregator)
    {
        eventAggregator.Subscribe(this);
    }

    public object Inquire(CustomMessage message)
    {
        return message.Value + 1;
    }
}
```
And the sender can respond to the result of the message accordingly.
```
using YEDUSO.EA;

public class Sender
{
    private IEventAggregator _eventAggregator;

    public Sender(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }

    public void SendTheMessage()
    {
        var xObj = _eventAggregator.Inquire(new CustomMessage(10));
        var x = (int)xObj;
    }
}
```
