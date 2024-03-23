using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace FakeXrmEasy.Query
{
    /// <summary>
    /// A condition expression with a decorated type of the attribute in the condition expression
    /// </summary>
    public class TypedConditionExpression
    {
        /// <summary>
        /// The QueryExpression to which this condition belongs
        /// </summary>
        internal QueryExpression QueryExpression { get; set; }
        
        /// <summary>
        /// The original condition expression
        /// </summary>
        internal ConditionExpression CondExpression { get; set; }
 
        /// <summary>
        /// The attribute type of the condition expression, if known (i.e. was generated via a strongly-typed generation tool)
        /// </summary>
        internal Type AttributeType { get; set; }
        
        /// <summary>
        /// True if the condition came from a left outer join, in which case should be applied only if not null
        /// </summary>
        public bool IsOuter { get; set; }

        /// <summary>
        /// Creates a TypedConditionExpression from an existing ConditionExpression with no attribute type information
        /// </summary>
        /// <param name="c"></param>
        public TypedConditionExpression(ConditionExpression c, QueryExpression qe)
        {
            IsOuter = false;
            CondExpression = c;
            QueryExpression = qe;
        }

        internal void ValidateSupportedTypedExpression()
        {
            Expression validateOperatorTypeExpression = Expression.Empty();
            ConditionOperator[] supportedOperators = (ConditionOperator[])Enum.GetValues(typeof(ConditionOperator));

#if FAKE_XRM_EASY_9
            if (AttributeType == typeof(OptionSetValueCollection))
            {
                supportedOperators = new[]
                {
                    ConditionOperator.ContainValues,
                    ConditionOperator.DoesNotContainValues,
                    ConditionOperator.Equal,
                    ConditionOperator.NotEqual,
                    ConditionOperator.NotNull,
                    ConditionOperator.Null,
                    ConditionOperator.In,
                    ConditionOperator.NotIn,
                };
            }
#endif

            if (!supportedOperators.Contains(CondExpression.Operator))
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidOperatorCode, "The operator is not valid or it is not supported.");
            }
        }

        internal object GetSingleConditionValue()
        {
            if (CondExpression.Values.Count != 1)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument, $"The {CondExpression.Operator} requires 1 value/s, not {CondExpression.Values.Count}.Parameter name: {CondExpression.AttributeName}");
            }

            var conditionValue = CondExpression.Values.Single();

            if (!(conditionValue is string) && conditionValue is IEnumerable)
            {
                var conditionValueEnumerable = conditionValue as IEnumerable;
                var count = 0;

                foreach (var obj in conditionValueEnumerable)
                {
                    count++;
                    conditionValue = obj;
                }

                if (count != 1)
                {
                    throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument, $"The {CondExpression.Operator} requires 1 value/s, not {count}.Parameter name: {CondExpression.AttributeName}");
                }
            }

            return conditionValue;
        }

        /// <summary>
        /// Returns the attribute name that participates in this condition expression
        /// </summary>
        /// <returns></returns>
        internal string GetAttributeName()
        {
            string attributeName = "";
            //If the attribute comes from a joined entity, then we need to access the attribute from the join
            //But the entity name attribute only exists >= 2013
#if FAKE_XRM_EASY_2013 || FAKE_XRM_EASY_2015 || FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
            //Do not prepend the entity name if the EntityLogicalName is the same as the QueryExpression main logical name

            if (!string.IsNullOrWhiteSpace(CondExpression.EntityName) && !CondExpression.EntityName.Equals(QueryExpression.EntityName))
            {
                attributeName = CondExpression.EntityName + "." + CondExpression.AttributeName;
            }
            else
                attributeName = CondExpression.AttributeName;
#else
          attributeName = CondExpression.AttributeName;          
#endif
            return attributeName;
        }
    }
}