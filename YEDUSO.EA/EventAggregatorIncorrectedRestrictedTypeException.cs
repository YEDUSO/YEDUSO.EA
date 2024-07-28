using System;

namespace YEDUSO.EA
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
