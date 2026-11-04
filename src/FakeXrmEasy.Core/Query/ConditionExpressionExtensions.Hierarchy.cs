#if FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace FakeXrmEasy.Query
{
    internal static partial class ConditionExpressionExtensions    
    {
        private static void RetrieveParentEntities(IXrmFakedContext context, XrmFakedRelationship hierarchicalRelationship, Entity currentRecord, DataCollection<object> values)
        {
            if (!currentRecord.Attributes.ContainsKey(hierarchicalRelationship.Entity2Attribute) || currentRecord.Attributes[hierarchicalRelationship.Entity2Attribute] == null)
            {
                return;
            }

            var parentRecord = context.CreateQuery(hierarchicalRelationship.Entity1LogicalName).FirstOrDefault(e => ((Guid)e.Attributes[hierarchicalRelationship.Entity1Attribute]) == ((EntityReference)currentRecord.Attributes[hierarchicalRelationship.Entity2Attribute]).Id);
            if (parentRecord != null)
            {
                values.Add(parentRecord.Id);
                RetrieveParentEntities(context, hierarchicalRelationship, parentRecord, values);
            }
        }
        
        private static void RetrieveChildEntities(IXrmFakedContext context, XrmFakedRelationship hierarchicalRelationship, Entity currentRecord, DataCollection<object> values)
        {
            var childAccounts = context.CreateQuery(hierarchicalRelationship.Entity1LogicalName)
                .Where(childAccount => childAccount.Attributes.Contains(hierarchicalRelationship.Entity2Attribute) && ((EntityReference)childAccount.Attributes[hierarchicalRelationship.Entity2Attribute]).Id == (Guid) currentRecord.Attributes[hierarchicalRelationship.Entity1Attribute])
                .ToList();

            foreach (var childAccount in childAccounts)
            {
                values.Add(childAccount.Id);
                RetrieveChildEntities(context, hierarchicalRelationship, childAccount, values);
            }
        }

        private static XrmFakedRelationship GetHierarchicalRelationship(IXrmFakedContext context,
            TypedConditionExpression tc)
        {
            var c = tc.CondExpression;
            var entityLogicalName = !string.IsNullOrEmpty(c.EntityName) ? c.EntityName : tc.QueryExpression.EntityName;
            var relationship = context.Relationships.FirstOrDefault(r => r.IsHierarchical && r.Entity1LogicalName.Equals(entityLogicalName) && r.Entity1Attribute.Equals(c.AttributeName));
            if(relationship == null)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.MissingHierarchicalRelationshipForOperator,
                    $"This query uses a hierarchical operator, but the {entityLogicalName} entity doesn't have a hierarchical relationship.");
            }
            return relationship;
        }

        private static Entity GetCurrentRecord(IXrmFakedContext context,
            TypedConditionExpression tc)
        {
            var c = tc.CondExpression;
            var entityLogicalName = !string.IsNullOrEmpty(c.EntityName) ? c.EntityName : tc.QueryExpression.EntityName;
            return context.CreateQuery(entityLogicalName).FirstOrDefault(e => ((Guid)e.Attributes[c.AttributeName]) == ((Guid)c.Values[0]));
        }
    }
}
#endif