using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using Xunit;
using FakeXrmEasy.Services;

namespace FakeXrmEasy.Tests.Services.EntityInitializer
{
    public class InvoiceInitializerServiceTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void TestPopulateFields()
        {
            (_context as XrmFakedContext).InitializationLevel = EntityInitializationLevel.PerEntity;
            List<Entity> initialEntities = new List<Entity>();

            Entity invoice = new Entity("invoice");
            invoice.Id = Guid.NewGuid();
            initialEntities.Add(invoice);

            _context.Initialize(initialEntities);
            Entity testPostCreate = _service.Retrieve("invoice", invoice.Id, new ColumnSet(true));
            Assert.NotNull(testPostCreate["invoicenumber"]);
        }

        [Fact]
        public void When_InvoiceNumberSet_DoesNot_Overridde_It()
        {
            List<Entity> initialEntities = new List<Entity>();

            Entity invoice = new Entity("invoice");
            invoice.Id = Guid.NewGuid();
            invoice["invoicenumber"] = "TEST";
            initialEntities.Add(invoice);

            _context.Initialize(initialEntities);
            Entity testPostCreate = _service.Retrieve("invoice", invoice.Id, new ColumnSet(true));
            Assert.NotNull(testPostCreate["invoicenumber"]);
            Assert.Equal("TEST", testPostCreate["invoicenumber"]);
        }
    }
}
