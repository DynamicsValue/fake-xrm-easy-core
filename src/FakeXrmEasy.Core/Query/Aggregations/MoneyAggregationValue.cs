using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.Query.Aggregations
{
    internal class MoneyAggregationValue: AggregationValue
    {
        private Money _moneyValue;
        
        internal MoneyAggregationValue(Money value)
        {
            _moneyValue = value;
            _type = AggregationValueType.Money;
        }

        internal override object GetValue()
        {
            return _moneyValue;
        }

        public static MoneyAggregationValue operator +(MoneyAggregationValue a, MoneyAggregationValue b)
        {
            decimal value1 = a?._moneyValue?.Value ?? 0m;
            decimal value2 = b?._moneyValue?.Value ?? 0m;
            
            return new MoneyAggregationValue(new Money(value1 + value2));
        }
        
        public static MoneyAggregationValue operator /(MoneyAggregationValue a, int b)
        {
            decimal value1 = a?._moneyValue?.Value ?? 0m;
            
            return new MoneyAggregationValue(new Money(value1 / b));
        }
    }
}