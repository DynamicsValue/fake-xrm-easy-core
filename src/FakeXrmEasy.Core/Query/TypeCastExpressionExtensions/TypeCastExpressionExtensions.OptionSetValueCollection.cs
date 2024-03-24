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
#if FAKE_XRM_EASY_9
        internal static Expression GetAppropriateCastExpressionBasedOnOptionSetValueCollection(Expression input)
        {
            return Expression.Call(typeof(OptionSetValueCollectionExtensions).GetMethod("ConvertToHashSetOfInt"), input, Expression.Constant(true));
        }
#endif
    }
}
