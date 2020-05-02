using Crm;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using Xunit;

namespace FakeXrmEasy.Tests
{
    public class Issue180
    {
        private readonly IXrmFakedContext _context;
        private readonly IOrganizationService _service;
        public Issue180() 
        {
            _context = XrmFakedContextFactory.New();
            _service = _context.GetOrganizationService();
        }

        [Fact]
        public void When_a_query_on_lookup_with_condition_in_contains_a_match_it_should_return()
        {
            _context.EnableProxyTypes(typeof(Account).Assembly);

            var account = new Account
            {
                Id = Guid.NewGuid(),
                OriginatingLeadId = new EntityReference("lead", Guid.NewGuid())
            };

            _context.Initialize(new List<Entity> { account });
            var ids = new[] { account.OriginatingLeadId.Id, Guid.NewGuid(), Guid.NewGuid() };

            var qe = new QueryExpression(Account.EntityLogicalName);
            qe.Criteria.AddCondition("originatingleadid", ConditionOperator.In, ids);

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(entities.Count, 1);
        }

        [Fact]
        public void When_a_query_on_lookup_with_condition_in_contains_no_match_it_should_not_return()
        {
            _context.EnableProxyTypes(typeof(Account).Assembly);

            var account = new Account
            {
                Id = Guid.NewGuid(),
                OriginatingLeadId = new EntityReference("lead", Guid.NewGuid())
            };

            _context.Initialize(new List<Entity> { account });

            var ids = new[] { Guid.Empty, Guid.Empty, Guid.Empty };

            var qe = new QueryExpression(Account.EntityLogicalName);
            qe.Criteria.AddCondition("originatingleadid", ConditionOperator.In, ids);

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(entities.Count, 0);
        }
    }
}