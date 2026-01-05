namespace FakeXrmEasy.Core.Query.Aggregations
{
    internal class IntAggregationValue: AggregationValue
    {
        private int? _intValue;
        
        internal IntAggregationValue(int? value)
        {
            _intValue = value;
            _type = AggregationValueType.Int;
        }

        internal override object GetValue()
        {
            return _intValue;
        }

        public static IntAggregationValue operator +(IntAggregationValue a, IntAggregationValue b)
        {
            return new IntAggregationValue(a._intValue + b._intValue);
        }
        
        public static IntAggregationValue operator /(IntAggregationValue a, int b)
        {
            int value1 = a?._intValue ?? 0;
            
            return new IntAggregationValue(value1 / b);
        }
    }
}