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
    internal static partial class TypeCastExpressionExtensions
    {
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
    }
}
