using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using Xunit;

namespace FakeXrmEasy.Tests
{
    public class TestDefaultEntityInitializer
    {
        private readonly IXrmFakedContext _context;
        private readonly IOrganizationService _service;
        public TestDefaultEntityInitializer()
        {
            _context = XrmFakedContextFactory.New();
            _service = _context.GetOrganizationService();
        }

        [Fact]
        public void TestWithUnpopulatedValues()
        {
            List<Entity> initialEntities = new List<Entity>();

            Entity user = new Entity("systemuser");
            user.Id = Guid.NewGuid();
            initialEntities.Add(user);

            (_context as XrmFakedContext).CallerId = user.ToEntityReference();

            Entity testEntity = new Entity("test");
            testEntity.Id = Guid.NewGuid();
            initialEntities.Add(testEntity);

            _context.Initialize(initialEntities);
            Entity testPostCreate = _service.Retrieve("test", testEntity.Id, new ColumnSet(true));
            Assert.Equal(user.ToEntityReference(), testPostCreate["ownerid"]);
            Assert.Equal(user.ToEntityReference(), testPostCreate["createdby"]);
            Assert.Equal(user.ToEntityReference(), testPostCreate["modifiedby"]);
        }
    }
}