using System;
using System.Globalization;
using System.Linq.Expressions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Query
{
    /// <summary>
    /// 
    /// </summary>
    public static class TypeCastExpressionExtensions
    {
        internal static Expression GetAppropriateCastExpressionBasedOnType(this Type t, Expression input, object value)
        {
            var typedExpression = t.GetAppropriateCastExpressionBasedOnAttributeTypeOrValue(input, value);

            //Now, any value (entity reference, string, int, etc,... could be wrapped in an AliasedValue object
            //So let's add this
            var getValueFromAliasedValueExp = Expression.Call(Expression.Convert(input, typeof(AliasedValue)),
                                            typeof(AliasedValue).GetMethod("get_Value"));

            var exp = Expression.Condition(Expression.TypeIs(input, typeof(AliasedValue)),
                    t.GetAppropriateCastExpressionBasedOnAttributeTypeOrValue(getValueFromAliasedValueExp, value),
                    typedExpression //Not an aliased value
                );

            return exp;
        }

        internal static Expression GetAppropriateCastExpressionBasedOnAttributeTypeOrValue(this Type attributeType, Expression input, object value)
        {
            if (attributeType != null)
            {
                if (Nullable.GetUnderlyingType(attributeType) != null)
                {
                    attributeType = Nullable.GetUnderlyingType(attributeType);
                }
                if (attributeType == typeof(Guid))
                    return GetAppropriateCastExpressionBasedGuid(input);
                if (attributeType == typeof(EntityReference))
                    return GetAppropriateCastExpressionBasedOnEntityReference(input, value);
                if (attributeType == typeof(int) || attributeType == typeof(Nullable<int>) || attributeType.IsOptionSet())
                    return GetAppropriateCastExpressionBasedOnInt(input);
                if (attributeType == typeof(decimal) || attributeType == typeof(Money))
                    return GetAppropriateCastExpressionBasedOnDecimal(input);
                if (attributeType == typeof(bool) || attributeType == typeof(BooleanManagedProperty))
                    return GetAppropriateCastExpressionBasedOnBoolean(input);
                if (attributeType == typeof(string))
                    return GetAppropriateCastExpressionBasedOnStringAndType(input, value, attributeType);
                if (attributeType.IsDateTime())
                    return GetAppropriateCastExpressionBasedOnDateTime(input, value);
#if FAKE_XRM_EASY_9
                if (attributeType.IsOptionSetValueCollection())
                    return GetAppropriateCastExpressionBasedOnOptionSetValueCollection(input);
#endif

                return GetAppropriateCastExpressionDefault(input, value); //any other type
            }

            return GetAppropriateCastExpressionBasedOnValueInherentType(input, value); //Dynamic / late bound entities
        }

        internal static Expression GetAppropriateCastExpressionBasedGuid(Expression input)
        {
            var getIdFromEntityReferenceExpr = Expression.Call(Expression.TypeAs(input, typeof(EntityReference)),
                typeof(EntityReference).GetMethod("get_Id"));

            return Expression.Condition(
                Expression.TypeIs(input, typeof(EntityReference)),  //If input is an entity reference, compare the Guid against the Id property
                Expression.Convert(
                    getIdFromEntityReferenceExpr,
                    typeof(Guid)),
                Expression.Condition(Expression.TypeIs(input, typeof(Guid)),  //If any other case, then just compare it as a Guid directly
                    Expression.Convert(input, typeof(Guid)),
                    Expression.Constant(Guid.Empty, typeof(Guid))));
        }


        internal static Expression GetAppropriateCastExpressionBasedOnEntityReference(Expression input, object value)
        {
            Guid guid;
            if (value is string && !Guid.TryParse((string)value, out guid))
            {
                var getNameFromEntityReferenceExpr = Expression.Call(Expression.TypeAs(input, typeof(EntityReference)),
                    typeof(EntityReference).GetMethod("get_Name"));

                return Expression.Condition(Expression.TypeIs(input, typeof(EntityReference)),
                    Expression.Convert(getNameFromEntityReferenceExpr, typeof(string)),
                    Expression.Constant(string.Empty, typeof(string))).ToCaseInsensitiveExpression();
            }

            var getIdFromEntityReferenceExpr = Expression.Call(Expression.TypeAs(input, typeof(EntityReference)),
                typeof(EntityReference).GetMethod("get_Id"));

            return Expression.Condition(
                Expression.TypeIs(input, typeof(EntityReference)),  //If input is an entity reference, compare the Guid against the Id property
                Expression.Convert(
                    getIdFromEntityReferenceExpr,
                    typeof(Guid)),
                Expression.Condition(Expression.TypeIs(input, typeof(Guid)),  //If any other case, then just compare it as a Guid directly
                    Expression.Convert(input, typeof(Guid)),
                    Expression.Constant(Guid.Empty, typeof(Guid))));

        }

        internal static Expression GetAppropriateCastExpressionBasedOnInt(Expression input)
        {
            return Expression.Condition(
                        Expression.TypeIs(input, typeof(OptionSetValue)),
                                            Expression.Convert(
                                                Expression.Call(Expression.TypeAs(input, typeof(OptionSetValue)),
                                                        typeof(OptionSetValue).GetMethod("get_Value")),
                                                        typeof(int)),
                                                    Expression.Convert(input, typeof(int)));
        }

        internal static Expression GetAppropriateCastExpressionBasedOnDecimal(Expression input)
        {
            return Expression.Condition(
                        Expression.TypeIs(input, typeof(Money)),
                                Expression.Convert(
                                    Expression.Call(Expression.TypeAs(input, typeof(Money)),
                                            typeof(Money).GetMethod("get_Value")),
                                            typeof(decimal)),
                           Expression.Condition(Expression.TypeIs(input, typeof(decimal)),
                                        Expression.Convert(input, typeof(decimal)),
                                        Expression.Constant(0.0M)));

        }

        internal static Expression GetAppropriateCastExpressionBasedOnBoolean(Expression input)
        {
            return Expression.Condition(
                        Expression.TypeIs(input, typeof(BooleanManagedProperty)),
                                Expression.Convert(
                                    Expression.Call(Expression.TypeAs(input, typeof(BooleanManagedProperty)),
                                            typeof(BooleanManagedProperty).GetMethod("get_Value")),
                                            typeof(bool)),
                           Expression.Condition(Expression.TypeIs(input, typeof(bool)),
                                        Expression.Convert(input, typeof(bool)),
                                        Expression.Constant(false)));

        }

        internal static Expression GetAppropriateCastExpressionBasedOnStringAndType(Expression input, object value, Type attributeType)
        {
            var defaultStringExpression = GetAppropriateCastExpressionDefault(input, value).ToCaseInsensitiveExpression();

            int iValue;
            if (attributeType.IsOptionSet() && int.TryParse(value.ToString(), out iValue))
            {
                return Expression.Condition(Expression.TypeIs(input, typeof(OptionSetValue)),
                    GetAppropriateCastExpressionBasedOnInt(input).ToStringExpression<Int32>(),
                    defaultStringExpression
                );
            }

            return defaultStringExpression;
        }

        internal static Expression GetAppropriateCastExpressionBasedOnDateTime(Expression input, object value)
        {
            // Convert to DateTime if string
            DateTime _;
            if (value is DateTime || value is string && DateTime.TryParse(value.ToString(), out _))
            {
                return Expression.Convert(input, typeof(DateTime));
            }

            return input; // return directly
        }

#if FAKE_XRM_EASY_9
        internal static Expression GetAppropriateCastExpressionBasedOnOptionSetValueCollection(Expression input)
        {
            return Expression.Call(typeof(OptionSetValueCollectionExtensions).GetMethod("ConvertToHashSetOfInt"), input, Expression.Constant(true));
        }
#endif

        internal static Expression GetAppropriateCastExpressionDefault(Expression input, object value)
        {
            return Expression.Convert(input, value.GetType());  //Default type conversion
        }

        internal static Expression GetAppropriateCastExpressionBasedOnValueInherentType(Expression input, object value)
        {
            if (value is Guid || value is EntityReference)
                return GetAppropriateCastExpressionBasedGuid(input); //Could be compared against an EntityReference
            if (value is int || value is OptionSetValue)
                return GetAppropriateCastExpressionBasedOnInt(input); //Could be compared against an OptionSet
            if (value is decimal || value is Money)
                return GetAppropriateCastExpressionBasedOnDecimal(input); //Could be compared against a Money
            if (value is bool)
                return GetAppropriateCastExpressionBasedOnBoolean(input); //Could be a BooleanManagedProperty
            if (value is string)
            {
                return GetAppropriateCastExpressionBasedOnString(input, value);
            }
            return GetAppropriateCastExpressionDefault(input, value); //any other type
        }

        internal static Expression GetAppropriateCastExpressionBasedOnString(Expression input, object value)
        {
            var defaultStringExpression = GetAppropriateCastExpressionDefault(input, value).ToCaseInsensitiveExpression();

            DateTime dtDateTimeConversion;
            if (DateTime.TryParse(value.ToString(), out dtDateTimeConversion))
            {
                return Expression.Convert(input, typeof(DateTime));
            }

            int iValue;
            if (int.TryParse(value.ToString(), out iValue))
            {
                return Expression.Condition(Expression.TypeIs(input, typeof(OptionSetValue)),
                    GetAppropriateCastExpressionBasedOnInt(input).ToStringExpression<Int32>(),
                    defaultStringExpression
                );
            }

            return defaultStringExpression;
        }


        internal static Expression GetAppropriateTypedValue(object value)
        {
            //Basic types conversions
            //Special case => datetime is sent as a string
            if (value is string)
            {
                DateTime dtDateTimeConversion;
                if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dtDateTimeConversion))
                {
                    return Expression.Constant(dtDateTimeConversion, typeof(DateTime));
                }
                else
                {
                    return Expression.Constant(value, typeof(string)).ToCaseInsensitiveExpression();
                }
            }
            else if (value is EntityReference)
            {
                var cast = (value as EntityReference).Id;
                return Expression.Constant(cast);
            }
            else if (value is OptionSetValue)
            {
                var cast = (value as OptionSetValue).Value;
                return Expression.Constant(cast);
            }
            else if (value is Money)
            {
                var cast = (value as Money).Value;
                return Expression.Constant(cast);
            }
            return Expression.Constant(value);
        }

        internal static Expression GetAppropriateTypedValueAndType(object value, Type attributeType)
        {
            if (attributeType == null)
                return GetAppropriateTypedValue(value);

            if (Nullable.GetUnderlyingType(attributeType) != null)
            {
                attributeType = Nullable.GetUnderlyingType(attributeType);
            }

            //Basic types conversions
            //Special case => datetime is sent as a string
            if (value is string)
            {
                int iValue;

                DateTime dtDateTimeConversion;
                Guid id;
                if (attributeType.IsDateTime()  //Only convert to DateTime if the attribute's type was DateTime
                    && DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dtDateTimeConversion))
                {
                    return Expression.Constant(dtDateTimeConversion, typeof(DateTime));
                }
                else if (attributeType.IsOptionSet() && int.TryParse(value.ToString(), out iValue))
                {
                    return Expression.Constant(iValue, typeof(int));
                }
                else if ((attributeType == typeof(EntityReference) || attributeType == typeof(Guid)) && Guid.TryParse((string)value, out id))
                {
                    return Expression.Constant(id);
                }
                else
                {
                    return Expression.Constant(value, typeof(string)).ToCaseInsensitiveExpression();
                }
            }
            else if (value is int)
            {
                if (attributeType.IsMoney())
                {
                    return Expression.Constant(((int)value)*1m, typeof(decimal));
                }
                return Expression.Constant(value);
            }
            else if (value is EntityReference)
            {
                var cast = (value as EntityReference).Id;
                return Expression.Constant(cast);
            }
            else if (value is OptionSetValue)
            {
                var cast = (value as OptionSetValue).Value;
                return Expression.Constant(cast);
            }
            else if (value is Money)
            {
                var cast = (value as Money).Value;
                return Expression.Constant(cast);
            }
            return Expression.Constant(value);
        }

        internal static Type GetAppropriateTypeForValue(object value)
        {
            //Basic types conversions
            //Special case => datetime is sent as a string
            if (value is string)
            {
                DateTime dtDateTimeConversion;
                if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dtDateTimeConversion))
                {
                    return typeof(DateTime);
                }
                else
                {
                    return typeof(string);
                }
            }
            else
                return value.GetType();
        }
    }
}
