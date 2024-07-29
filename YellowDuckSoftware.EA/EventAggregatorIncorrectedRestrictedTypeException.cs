using System;

namespace YellowDuckSoftware.EA
{
    public class EventAggregatorIncorrectedRestrictedTypeException : Exception
    {
        public Type OffendingType;

        public EventAggregatorIncorrectedRestrictedTypeException(Type offendingType)
        {
            OffendingType = offendingType;
        }
    }
}
