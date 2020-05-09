using System;
using System.Globalization;
using System.Linq.Expressions;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Query
{
    public static class TypeCastExpressions
    {
        internal static Expression GetAppropiateCastExpressionBasedOnType(this Type t, Expression input, object value)
        {
            var typedExpression = t.GetAppropiateCastExpressionBasedOnAttributeTypeOrValue(input, value);

            //Now, any value (entity reference, string, int, etc,... could be wrapped in an AliasedValue object
            //So let's add this
            var getValueFromAliasedValueExp = Expression.Call(Expression.Convert(input, typeof(AliasedValue)),
                                            typeof(AliasedValue).GetMethod("get_Value"));

            var exp = Expression.Condition(Expression.TypeIs(input, typeof(AliasedValue)),
                    t.GetAppropiateCastExpressionBasedOnAttributeTypeOrValue(getValueFromAliasedValueExp, value),
                    typedExpression //Not an aliased value
                );

            return exp;
        }

        internal static Expression GetAppropiateCastExpressionBasedOnAttributeTypeOrValue(this Type attributeType, Expression input, object value)
        {
            if (attributeType != null)
            {
                if (Nullable.GetUnderlyingType(attributeType) != null)
                {
                    attributeType = Nullable.GetUnderlyingType(attributeType);
                }
                if (attributeType == typeof(Guid))
                    return GetAppropiateCastExpressionBasedGuid(input);
                if (attributeType == typeof(EntityReference))
                    return GetAppropiateCastExpressionBasedOnEntityReference(input, value);
                if (attributeType == typeof(int) || attributeType == typeof(Nullable<int>) || attributeType.IsOptionSet())
                    return GetAppropiateCastExpressionBasedOnInt(input);
                if (attributeType == typeof(decimal) || attributeType == typeof(Money))
                    return GetAppropiateCastExpressionBasedOnDecimal(input);
                if (attributeType == typeof(bool) || attributeType == typeof(BooleanManagedProperty))
                    return GetAppropiateCastExpressionBasedOnBoolean(input);
                if (attributeType == typeof(string))
                    return GetAppropiateCastExpressionBasedOnStringAndType(input, value, attributeType);
                if (attributeType.IsDateTime())
                    return GetAppropiateCastExpressionBasedOnDateTime(input, value);
#if FAKE_XRM_EASY_9
                if (attributeType.IsOptionSetValueCollection())
                    return GetAppropiateCastExpressionBasedOnOptionSetValueCollection(input);
#endif

                return GetAppropiateCastExpressionDefault(input, value); //any other type
            }

            return GetAppropiateCastExpressionBasedOnValueInherentType(input, value); //Dynamic entities
        }

        internal static Expression GetAppropiateCastExpressionBasedGuid(Expression input)
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


        internal static Expression GetAppropiateCastExpressionBasedOnEntityReference(Expression input, object value)
        {
            Guid guid;
            if (value is string && !Guid.TryParse((string)value, out guid))
            {
                var getNameFromEntityReferenceExpr = Expression.Call(Expression.TypeAs(input, typeof(EntityReference)),
                    typeof(EntityReference).GetMethod("get_Name"));

                return GetCaseInsensitiveExpression(Expression.Condition(Expression.TypeIs(input, typeof(EntityReference)),
                    Expression.Convert(getNameFromEntityReferenceExpr, typeof(string)),
                    Expression.Constant(string.Empty, typeof(string))));
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

        internal static Expression GetAppropiateCastExpressionBasedOnInt(Expression input)
        {
            return Expression.Condition(
                        Expression.TypeIs(input, typeof(OptionSetValue)),
                                            Expression.Convert(
                                                Expression.Call(Expression.TypeAs(input, typeof(OptionSetValue)),
                                                        typeof(OptionSetValue).GetMethod("get_Value")),
                                                        typeof(int)),
                                                    Expression.Convert(input, typeof(int)));
        }

        internal static Expression GetAppropiateCastExpressionBasedOnDecimal(Expression input)
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

        internal static Expression GetAppropiateCastExpressionBasedOnBoolean(Expression input)
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

        internal static Expression GetAppropiateCastExpressionBasedOnStringAndType(Expression input, object value, Type attributeType)
        {
            var defaultStringExpression = GetCaseInsensitiveExpression(GetAppropiateCastExpressionDefault(input, value));

            int iValue;
            if (attributeType.IsOptionSet() && int.TryParse(value.ToString(), out iValue))
            {
                return Expression.Condition(Expression.TypeIs(input, typeof(OptionSetValue)),
                    GetToStringExpression<Int32>(GetAppropiateCastExpressionBasedOnInt(input)),
                    defaultStringExpression
                );
            }

            return defaultStringExpression;
        }

        internal static Expression GetAppropiateCastExpressionBasedOnDateTime(Expression input, object value)
        {
            // Convert to DateTime if string
            DateTime _;
            if (value is DateTime || value is string && DateTime.TryParse(value.ToString(), out _))
            {
                return Expression.Convert(input, typeof(DateTime));
            }

            return input; // return directly
        }

        internal static Expression GetAppropiateCastExpressionBasedOnOptionSetValueCollection(Expression input)
        {
            return Expression.Call(typeof(XrmFakedContext).GetMethod("ConvertToHashSetOfInt"), input, Expression.Constant(true));
        }

        internal static Expression GetAppropiateCastExpressionDefault(Expression input, object value)
        {
            return Expression.Convert(input, value.GetType());  //Default type conversion
        }

        internal static Expression GetAppropiateCastExpressionBasedOnValueInherentType(Expression input, object value)
        {
            if (value is Guid || value is EntityReference)
                return GetAppropiateCastExpressionBasedGuid(input); //Could be compared against an EntityReference
            if (value is int || value is OptionSetValue)
                return GetAppropiateCastExpressionBasedOnInt(input); //Could be compared against an OptionSet
            if (value is decimal || value is Money)
                return GetAppropiateCastExpressionBasedOnDecimal(input); //Could be compared against a Money
            if (value is bool)
                return GetAppropiateCastExpressionBasedOnBoolean(input); //Could be a BooleanManagedProperty
            if (value is string)
            {
                return GetAppropiateCastExpressionBasedOnString(input, value);
            }
            return GetAppropiateCastExpressionDefault(input, value); //any other type
        }

        internal static Expression GetAppropiateCastExpressionBasedOnString(Expression input, object value)
        {
            var defaultStringExpression = GetCaseInsensitiveExpression(GetAppropiateCastExpressionDefault(input, value));

            DateTime dtDateTimeConversion;
            if (DateTime.TryParse(value.ToString(), out dtDateTimeConversion))
            {
                return Expression.Convert(input, typeof(DateTime));
            }

            int iValue;
            if (int.TryParse(value.ToString(), out iValue))
            {
                return Expression.Condition(Expression.TypeIs(input, typeof(OptionSetValue)),
                    GetToStringExpression<Int32>(GetAppropiateCastExpressionBasedOnInt(input)),
                    defaultStringExpression
                );
            }

            return defaultStringExpression;
        }

        internal static Expression GetToStringExpression<T>(Expression e)
        {
            return Expression.Call(e, typeof(T).GetMethod("ToString", new Type[] { }));
        }
        internal static Expression GetCaseInsensitiveExpression(Expression e)
        {
            return Expression.Call(e,
                                typeof(string).GetMethod("ToLowerInvariant", new Type[] { }));
        }

        internal static Expression GetAppropiateTypedValue(object value)
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
                    return GetCaseInsensitiveExpression(Expression.Constant(value, typeof(string)));
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

        internal static Expression GetAppropiateTypedValueAndType(object value, Type attributeType)
        {
            if (attributeType == null)
                return GetAppropiateTypedValue(value);

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
                    return GetCaseInsensitiveExpression(Expression.Constant(value, typeof(string)));
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

        internal static Type GetAppropiateTypeForValue(object value)
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
