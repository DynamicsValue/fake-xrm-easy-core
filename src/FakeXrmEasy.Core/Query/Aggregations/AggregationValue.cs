using System;
using FakeXrmEasy.Core.Exceptions.Query.Aggregations;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.Query.Aggregations
{
    internal enum AggregationValueType
    {
        Int = 0,
        Decimal = 1,
        Double = 2,
        Money = 3,
        DateTime = 4
    }
    
    internal class AggregationValue
    {
        protected AggregationValueType _type;

        internal AggregationValueType ValueType => _type;
        
        internal AggregationValue()
        {
            
        }

        internal virtual object GetValue()
        {
            return null;
        }

        public static AggregationValue operator +(AggregationValue a, AggregationValue b)
        {
            if (a._type != b._type)
            {
                throw new DifferentAggregationValueTypeException(a, b);
            }

            switch (a._type)
            {
                case AggregationValueType.Int:
                    return (IntAggregationValue) a + (IntAggregationValue) b;
                case AggregationValueType.Decimal:
                    return (DecimalAggregationValue) a + (DecimalAggregationValue) b;
                case AggregationValueType.Double:
                    return (DoubleAggregationValue) a + (DoubleAggregationValue) b;
                case AggregationValueType.Money:
                    return (MoneyAggregationValue) a + (MoneyAggregationValue) b;
            }

            return null;
        }
        
        public static AggregationValue operator /(AggregationValue a, int b)
        {
            switch (a._type)
            {
                case AggregationValueType.Int:
                    return (IntAggregationValue) a / b;
                case AggregationValueType.Decimal:
                    return (DecimalAggregationValue) a / b;
                case AggregationValueType.Double:
                    return (DoubleAggregationValue) a / b;
                case AggregationValueType.Money:
                    return (MoneyAggregationValue) a / b;
            }

            return null;
        }
        
        public static bool operator <(AggregationValue a, AggregationValue b)
        {
            if (a._type != b._type)
            {
                throw new DifferentAggregationValueTypeException(a, b);
            }

            switch (a._type)
            {
                case AggregationValueType.Int:
                    return (IntAggregationValue) a < (IntAggregationValue) b;
                case AggregationValueType.Decimal:
                    return (DecimalAggregationValue) a < (DecimalAggregationValue) b;
                case AggregationValueType.Double:
                    return (DoubleAggregationValue) a < (DoubleAggregationValue) b;
                case AggregationValueType.Money:
                    return (MoneyAggregationValue) a < (MoneyAggregationValue) b;
                case AggregationValueType.DateTime:
                    return (DateTimeAggregationValue) a < (DateTimeAggregationValue) b;
            }

            return false;
        }
        
        public static bool operator >(AggregationValue a, AggregationValue b)
        {
            if (a._type != b._type)
            {
                throw new DifferentAggregationValueTypeException(a, b);
            }

            switch (a._type)
            {
                case AggregationValueType.Int:
                    return (IntAggregationValue) a > (IntAggregationValue) b;
                case AggregationValueType.Decimal:
                    return (DecimalAggregationValue) a > (DecimalAggregationValue) b;
                case AggregationValueType.Double:
                    return (DoubleAggregationValue) a > (DoubleAggregationValue) b;
                case AggregationValueType.Money:
                    return (MoneyAggregationValue) a > (MoneyAggregationValue) b;
                case AggregationValueType.DateTime:
                    return (DateTimeAggregationValue) a > (DateTimeAggregationValue) b;
            }

            return false;
        }
        
    }
}