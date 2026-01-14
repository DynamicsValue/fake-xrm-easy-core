#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Core.Query.Aggregations;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    internal static class XrmAttributeExpressionExtensions
    {
        /// <summary>
        /// Translates an attribute expression that represents an aggregate function from the specified sequence into the aggregate record specified
        /// </summary>
        /// <param name="expr">The attribute expression that will be translated</param>
        /// <param name="qe">The QueryExpression to which this attribute expression belongs</param>
        /// <param name="sequence">The precomputed sequence where the aggregate function will be calculated</param>
        /// <param name="aggregateRecord">The resulting record where the aggregate function result will be added</param>
        internal static void ToAggregatedAttributeValue(this XrmAttributeExpression expr, QueryExpression qe, List<Entity> sequence, Entity aggregateRecord, IXrmFakedContext context)
        {
            switch (expr.AggregateType)
            {
                case XrmAggregateType.Count:
                    aggregateRecord[expr.Alias] = new AliasedValue(qe.EntityName, expr.AttributeName, sequence.Count());
                    break;
                
                case XrmAggregateType.CountColumn:
                    aggregateRecord[expr.Alias] = new AliasedValue(qe.EntityName, expr.AttributeName, sequence.Count(e => e.ContainsData(expr.AttributeName)));
                    break;
                case XrmAggregateType.Sum:
                    var sumSeed = GetAggregationDefaultSumValue(qe.EntityName, expr.AttributeName, context);

                    var sumAggregationValue = sequence.Aggregate<Entity, AggregationValue>(sumSeed,
                        (accumulation, entity) =>
                            accumulation + entity.GetAggregationValue(expr.AttributeName, context));
                    
                    aggregateRecord[expr.Alias] = new AliasedValue(qe.EntityName, expr.AttributeName, sumAggregationValue.GetValue());
                    break;
                case XrmAggregateType.Avg:
                    var avgSeed = GetAggregationDefaultSumValue(qe.EntityName, expr.AttributeName, context);

                    var sumValue = sequence.Aggregate<Entity, AggregationValue>(avgSeed,
                        (accumulation, entity) =>
                            accumulation + entity.GetAggregationValue(expr.AttributeName, context));

                    var avgAggregationValue = sumValue / sequence.Count(e => e.ContainsData(expr.AttributeName));
                    
                    aggregateRecord[expr.Alias] = new AliasedValue(qe.EntityName, expr.AttributeName, avgAggregationValue.GetValue());
                    break;
                case XrmAggregateType.Min:
                    var minSeed = GetAggregationDefaultMinValue(qe.EntityName, expr.AttributeName, context);

                    var minValue = sequence.Aggregate<Entity, AggregationValue>(minSeed,
                        (accumulation, entity) =>
                        {
                            var currentValue = entity.GetAggregationValue(expr.AttributeName, context);
                            return currentValue < accumulation ? currentValue : accumulation;
                        });
                    
                    aggregateRecord[expr.Alias] = new AliasedValue(qe.EntityName, expr.AttributeName, minValue.GetValue());
                    break;
                
                case XrmAggregateType.Max:
                    var maxSeed = GetAggregationDefaultMaxValue(qe.EntityName, expr.AttributeName, context);

                    var maxValue = sequence.Aggregate<Entity, AggregationValue>(maxSeed,
                        (accumulation, entity) =>
                        {
                            var currentValue =  entity.GetAggregationValue(expr.AttributeName, context);
                            return currentValue > accumulation ? currentValue : accumulation;
                        });
                    
                    aggregateRecord[expr.Alias] = new AliasedValue(qe.EntityName, expr.AttributeName, maxValue.GetValue());
                    break;
            }
        }

        private static AggregationValue GetAggregationDefaultSumValue(string entityLogicalName, string attributeName,
            IXrmFakedContext context)
        {
            var attributeType = context.FindReflectedAttributeType(context.FindReflectedType(entityLogicalName),
                entityLogicalName, attributeName);

            if (attributeType == typeof(int))
            {
                return new IntAggregationValue(0);
            }
            if (attributeType == typeof(double))
            {
                return new DoubleAggregationValue(0.0);
            }
            if (attributeType == typeof(decimal))
            {
                return new DecimalAggregationValue(0m);
            }
            if (attributeType == typeof(Money))
            {
                return new MoneyAggregationValue(new Money(0m));
            }
            

            return null;
        }
        
        private static AggregationValue GetAggregationDefaultMinValue(string entityLogicalName, string attributeName,
            IXrmFakedContext context)
        {
            var attributeType = context.FindReflectedAttributeType(context.FindReflectedType(entityLogicalName),
                entityLogicalName, attributeName);

            if (attributeType == typeof(int))
            {
                return new IntAggregationValue(int.MaxValue);
            }
            if (attributeType == typeof(double))
            {
                return new DoubleAggregationValue(double.MaxValue);
            }
            if (attributeType == typeof(decimal))
            {
                return new DecimalAggregationValue(decimal.MaxValue);
            }
            if (attributeType == typeof(Money))
            {
                return new MoneyAggregationValue(new Money(decimal.MaxValue));
            }
            if (attributeType == typeof(DateTime))
            {
                return new DateTimeAggregationValue(DateTime.MaxValue);
            }
            
            return null;
        }
        
        private static AggregationValue GetAggregationDefaultMaxValue(string entityLogicalName, string attributeName,
            IXrmFakedContext context)
        {
            var attributeType = context.FindReflectedAttributeType(context.FindReflectedType(entityLogicalName),
                entityLogicalName, attributeName);

            if (attributeType == typeof(int))
            {
                return new IntAggregationValue(int.MinValue);
            }
            if (attributeType == typeof(double))
            {
                return new DoubleAggregationValue(double.MinValue);
            }
            if (attributeType == typeof(decimal))
            {
                return new DecimalAggregationValue(decimal.MinValue);
            }
            if (attributeType == typeof(Money))
            {
                return new MoneyAggregationValue(new Money(decimal.MinValue));
            }
            if (attributeType == typeof(DateTime))
            {
                return new DateTimeAggregationValue(DateTime.MinValue);
            }
            
            return null;
        }
    }
}
#endif