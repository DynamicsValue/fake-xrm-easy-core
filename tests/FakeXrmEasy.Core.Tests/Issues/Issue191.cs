using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using Xunit;

namespace FakeXrmEasy.Tests.Issues
{
    public class Issue191: FakeXrmEasyTestsBase
    {
        [Fact]
        public void Testing_191()
        {
            // create a contact
            var contact = new Entity
            {
                LogicalName = "contact",
                Id = Guid.NewGuid(),
            };

            // link a child to the contact
            var child = new Entity
            {
                LogicalName = "child",
                Id = Guid.NewGuid(),
                Attributes = new AttributeCollection { { "contactid", new EntityReference("contact", contact.Id) } }
            };

            // link a pet to the child
            var pet = new Entity
            {
                LogicalName = "pet",
                Id = Guid.NewGuid(),
                Attributes = new AttributeCollection { { "childid", new EntityReference("child", child.Id) } }
            };

            // initialise
            _context.Initialize(new[] { contact, child, pet });
            
            // join contact and child and pet
            var query2 = new QueryExpression("contact");

            LinkEntity link2 = new LinkEntity()
            {
                LinkFromEntityName = "contact",
                LinkFromAttributeName = "contactid",
                LinkToEntityName = "child",
                LinkToAttributeName = "contactid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("contactid")
            };
            query2.LinkEntities.Add(link2);

            LinkEntity link22 = new LinkEntity()
            {
                LinkFromEntityName = "child",
                LinkFromAttributeName = "childid",
                LinkToEntityName = "pet",
                LinkToAttributeName = "childid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("childid")
            };
            link2.LinkEntities.Add(link22);

            var count2 = _service.RetrieveMultiple(query2).Entities.Count;
            Console.WriteLine(count2); // returns 1 record

            var results = _service.RetrieveMultiple(query2);
            Assert.True(results.Entities[0].Attributes.ContainsKey("child1.contactid"));
            Assert.True(results.Entities[0].Attributes.ContainsKey("pet1.childid")); //test fails unless link22 is Inner join
        }
    }
}