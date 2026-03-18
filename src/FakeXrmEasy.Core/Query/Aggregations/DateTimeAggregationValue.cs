using System;

namespace FakeXrmEasy.Core.Query.Aggregations
{
    internal class DateTimeAggregationValue: AggregationValue
    {
        private readonly DateTime? _dateValue;
        
        internal DateTimeAggregationValue(DateTime? value)
        {
            _type = AggregationValueType.DateTime;
            _dateValue = value;
        }

        internal override object GetValue()
        {
            return _dateValue;
        }
        
        public static bool operator <(DateTimeAggregationValue a, DateTimeAggregationValue b)
        {
            DateTime? value1 = a?._dateValue;
            DateTime? value2 = b?._dateValue;
            
            return value1 < value2;
        }
        
        public static bool operator >(DateTimeAggregationValue a, DateTimeAggregationValue b)
        {
            DateTime? value1 = a?._dateValue;
            DateTime? value2 = b?._dateValue;
            
            return value1 > value2;
        }
    }
}