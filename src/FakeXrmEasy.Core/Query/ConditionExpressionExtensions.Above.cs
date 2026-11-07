#if FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FakeXrmEasy.Query
{
    internal static partial class ConditionExpressionExtensions    
    {
        internal static Expression ToAboveExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr, IXrmFakedContext context)
        {
            var c = tc.CondExpression;
            var entityLogicalName = !string.IsNullOrEmpty(c.EntityName) ? c.EntityName : tc.QueryExpression.EntityName;
            
            var hierarchicalRelationship = GetHierarchicalRelationship(context, tc);
            
            //Retrieve initial record from the hierarchical tree
            var currentRecord = GetCurrentRecord(context, tc);
            if(currentRecord == null)
            {
                return Expression.Constant(false);
            }

            //Iterate through parents via the relationship attributes to walk through the entire hierarchy and build a list of identifiers of the nodes in the hierarchy.
            RetrieveParentEntities(context, hierarchicalRelationship, currentRecord, c.Values, new HashSet<Guid>());

            c.Values.RemoveAt(0); //removes the first value from the list to exclude the "equal current record" condition
            
            return tc.ToInExpression(getAttributeValueExpr, containsAttributeExpr);
        }
    }
}
#endif