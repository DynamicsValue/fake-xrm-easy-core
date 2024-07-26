using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FakeXrmEasy.Core.Query
{
    internal static partial class ConditionExpressionExtensions    
    {
        internal static Expression ToAboveOrEqualExpression(this TypedConditionExpression tc, Expression getAttributeValueExpr, Expression containsAttributeExpr, IXrmFakedContext context)
        {
            var c = tc.CondExpression;
            var entityLogicalName = !string.IsNullOrEmpty(c.EntityName) ? c.EntityName : tc.QueryExpression.EntityName;
            
            if(!context.Relationships.Any(r => r.IsHierarchical && r.Entity1LogicalName.Equals(entityLogicalName) && r.Entity1Attribute.Equals(c.AttributeName)))
            {
                return tc.ToEqualExpression(context, getAttributeValueExpr, containsAttributeExpr);
            }

            //TODO recursive magic to get parent records from hierarchie
            var hierarchicalRelationship = context.Relationships.FirstOrDefault(r => r.IsHierarchical && r.Entity1LogicalName.Equals(entityLogicalName) && r.Entity1Attribute.Equals(c.AttributeName));

            var currentRecord = context.CreateQuery(entityLogicalName).FirstOrDefault(e => ((Guid)e.Attributes[c.AttributeName]) == ((Guid)c.Values[0]));            
            RetrieveParentEntity(context, hierarchicalRelationship, currentRecord);            

            return tc.ToEqualExpression(context, getAttributeValueExpr, containsAttributeExpr);
        }

        private static void RetrieveParentEntity(IXrmFakedContext context, XrmFakedRelationship hierarchicalRelationship, Entity currentRecord)
        {
            if (currentRecord == null || !currentRecord.Attributes.ContainsKey(hierarchicalRelationship.Entity2Attribute) || currentRecord.Attributes[hierarchicalRelationship.Entity2Attribute] == null)
            {
                return;
            }

            var parentRecord = context.CreateQuery(hierarchicalRelationship.Entity1LogicalName).FirstOrDefault(e => ((Guid)e.Attributes[hierarchicalRelationship.Entity1Attribute]) == ((EntityReference)currentRecord.Attributes[hierarchicalRelationship.Entity2Attribute]).Id);
            RetrieveParentEntity(context, hierarchicalRelationship, parentRecord);            
        }
    }
}
