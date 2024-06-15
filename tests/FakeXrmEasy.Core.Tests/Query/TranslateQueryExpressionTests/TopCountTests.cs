using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests
{
    public class TopCountTests : FakeXrmEasyTestsBase
    {
        private readonly List<Entity> _entities;

        public TopCountTests()
        {
            _entities = new List<Entity>();

            for (int i = 0; i < 10; i++)
            {
                _entities.Add(new Contact() { Id = Guid.NewGuid() });
            }
        }

        [Fact]
        public void Should_return_top_5()
        {
            _context.Initialize(_entities);

            QueryExpression query = new QueryExpression(Contact.EntityLogicalName);
            query.TopCount = 5;
            EntityCollection result = _service.RetrieveMultiple(query);
            Assert.Equal(query.TopCount, result.Entities.Count);
            Assert.False(result.MoreRecords);
        }

        [Fact]
        public void Should_return_top_5_even_if_page_info_is_set_but_empty()
        {
            _context.Initialize(_entities);

            QueryByAttribute queryByAttribute = new QueryByAttribute("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                PageInfo = new PagingInfo(),
                TopCount = 5
            };
            var result = _service.RetrieveMultiple(queryByAttribute);
            Assert.Equal(queryByAttribute.TopCount, result.Entities.Count);

            QueryExpression query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                PageInfo = new PagingInfo(),
                TopCount = 5
            };

            result = _service.RetrieveMultiple(query);
            Assert.Equal(query.TopCount, result.Entities.Count);
        }

        [Theory]
        [InlineData(1, 0, "", false)]
        [InlineData(0, 1, "", false)]
        [InlineData(0, 0, "asdasd", false)]
        [InlineData(0, 0, "", true)]
        public void Should_throw_exception_if_both_top_count_is_set_and_page_info_is_not_empty(int pageNumber, int count, string pagingCookie, bool returnTotalCount)
        {
            _context.Initialize(_entities);

            QueryByAttribute queryByAttribute = new QueryByAttribute("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                PageInfo = new PagingInfo() 
                { 
                    Count = count,
                    PageNumber = pageNumber,
                    PagingCookie = pagingCookie,
                    ReturnTotalRecordCount = returnTotalCount
                },
                TopCount = 5
            };
            Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(queryByAttribute));

            QueryExpression query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname"),
                PageInfo = new PagingInfo()
                {
                    Count = count,
                    PageNumber = pageNumber,
                    PagingCookie = pagingCookie,
                    ReturnTotalRecordCount = returnTotalCount
                },
                TopCount = 5
            };
            Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(query));
        }
    }
}
