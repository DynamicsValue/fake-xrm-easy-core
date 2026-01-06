namespace FakeXrmEasy.Core.Query.Aggregations
{
    internal class DoubleAggregationValue: AggregationValue
    {
        private double? _doubleValue;
        
        internal DoubleAggregationValue(double? value)
        {
            _type = AggregationValueType.Double;
            _doubleValue = value;
        }

        internal override object GetValue()
        {
            return _doubleValue;
        }

        public static DoubleAggregationValue operator +(DoubleAggregationValue a, DoubleAggregationValue b)
        {
            double value1 = a?._doubleValue ?? 0.0;
            double value2 = b?._doubleValue ?? 0.0;
            
            return new DoubleAggregationValue(value1 + value2);
        }
        
        public static DoubleAggregationValue operator /(DoubleAggregationValue a, int b)
        {
            double value1 = a?._doubleValue ?? 0.0;
            
            return new DoubleAggregationValue(value1 / b);
        }
        
        public static bool operator <(DoubleAggregationValue a, DoubleAggregationValue b)
        {
            double? value1 = a?._doubleValue;
            double? value2 = b?._doubleValue;
            
            return value1 < value2;
        }
        
        public static bool operator >(DoubleAggregationValue a, DoubleAggregationValue b)
        {
            double? value1 = a?._doubleValue;
            double? value2 = b?._doubleValue;
            
            return value1 > value2;
        }
    }
}