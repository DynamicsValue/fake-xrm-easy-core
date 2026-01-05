namespace FakeXrmEasy.Core.Query.Aggregations
{
    internal class DecimalAggregationValue: AggregationValue
    {
        private readonly decimal? _decValue;
        
        internal DecimalAggregationValue(decimal? value)
        {
            _type = AggregationValueType.Decimal;
            _decValue = value;
        }

        internal override object GetValue()
        {
            return _decValue;
        }

        public static DecimalAggregationValue operator +(DecimalAggregationValue a, DecimalAggregationValue b)
        {
            decimal value1 = a?._decValue ?? 0m;
            decimal value2 = b?._decValue ?? 0m;
            
            return new DecimalAggregationValue(value1 + value2);
        }
        
        public static DecimalAggregationValue operator /(DecimalAggregationValue a, int b)
        {
            decimal value1 = a?._decValue ?? 0m;
            
            return new DecimalAggregationValue(value1 / b);
        }
    }
}