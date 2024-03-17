using System;
using Crm;
using Microsoft.Xrm.Sdk.Query;
using Xunit;
using Contact = DataverseEntities.Contact;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.OperatorTests.Strings
{
    public class LikeOperatorTests: FakeXrmEasyTestsBase
    {
        private readonly Contact _contact;

        public LikeOperatorTests()
        {
            _contact = new Contact { Id = Guid.NewGuid(), FirstName = "Jimmy" };
        }
        
        [Fact]
        public void Should_return_records_where_percentage_wildcard_is_at_the_end()
        {
            _context.Initialize(_contact);
            
            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("firstname", ConditionOperator.Like, "jim%");

            Assert.Single(_service.RetrieveMultiple(qe).Entities);
        }
        
        [Fact]
        public void Should_return_records_where_percentage_wildcard_is_at_the_beginning()
        {
            _context.Initialize(_contact);
            
            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("firstname", ConditionOperator.Like, "%mmy");

            Assert.Single(_service.RetrieveMultiple(qe).Entities);
        }
        
        [Fact]
        public void Should_return_records_where_percentage_wildcard_is_in_the_middle()
        {
            _context.Initialize(_contact);
            
            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("firstname", ConditionOperator.Like, "j%my");

            Assert.Single(_service.RetrieveMultiple(qe).Entities);
        }
        
        [Fact]
        public void Should_return_records_with_underscore_wildcard()
        {
            _context.Initialize(_contact);
            
            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("firstname", ConditionOperator.Like, "j_mm_");

            Assert.Single(_service.RetrieveMultiple(qe).Entities);
        }
        
        [Theory]
        [InlineData("Jimmy", "[i-k]immy")]
        [InlineData("Alan", "[a-c]lan")]
        public void Should_return_records_with_character_range_wildcard(string firstName, string conditionValue)
        {
            _contact.FirstName = firstName;
            _context.Initialize(_contact);
            
            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("firstname", ConditionOperator.Like, conditionValue);

            Assert.Single(_service.RetrieveMultiple(qe).Entities);
        }
        
        [Theory]
        [InlineData("Jimmy", "[^a-i]immy")]
        [InlineData("Alan", "a[^m-z]an")]
        public void Should_return_records_outside_character_range_wildcard(string firstName, string conditionValue)
        {
            _contact.FirstName = firstName;
            _context.Initialize(_contact);
            
            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("firstname", ConditionOperator.Like, conditionValue);

            Assert.Single(_service.RetrieveMultiple(qe).Entities);
        }
        
        [Theory]
        [InlineData("Jim", "[jk]im")]
        [InlineData("Kim", "[jk]im")]
        public void Should_return_records_with_character_set_wildcard(string firstName, string conditionValue)
        {
            _contact.FirstName = firstName;
            _context.Initialize(_contact);
            
            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("firstname", ConditionOperator.Like, conditionValue);

            Assert.Single(_service.RetrieveMultiple(qe).Entities);
        }
        
        [Theory]
        [InlineData("Jim", "[^qwertyuiopasdfghklzxcvbnm]im")]
        [InlineData("Kim", "[^qwertyuiopasdfghjlñzxcvbnm]im")]
        public void Should_return_records_outside_character_set_wildcard(string firstName, string conditionValue)
        {
            _contact.FirstName = firstName;
            _context.Initialize(_contact);
            
            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("firstname", ConditionOperator.Like, conditionValue);

            Assert.Single(_service.RetrieveMultiple(qe).Entities);
        }
        
        [Theory]
        [InlineData("INV-761")]
        [InlineData("INV-300")]
        public void Should_return_records_with_a_combination_of_wildcards(string invoiceNumber)
        {
            var invoice = new Invoice() { Id = Guid.NewGuid(), Name = invoiceNumber };
            _context.Initialize(invoice);
            
            var qe = new QueryExpression("invoice");
            qe.Criteria.AddCondition("name", ConditionOperator.Like, "INV-[0-9][0-9][0-9]");

            Assert.Single(_service.RetrieveMultiple(qe).Entities);
        }
        
        [Theory]
        [InlineData("INV-4253")]
        [InlineData("INV-65D")]
        public void Should_not_return_records_that_do_not_match_a_combination_of_wildcards(string invoiceNumber)
        {
            var invoice = new Invoice() { Id = Guid.NewGuid(), Name = invoiceNumber };
            _context.Initialize(invoice);
            
            var qe = new QueryExpression("invoice");
            qe.Criteria.AddCondition("name", ConditionOperator.Like, "INV-[0-9][0-9][0-9]");

            Assert.Empty(_service.RetrieveMultiple(qe).Entities);
        }
        
    }
}