using FakeXrmEasy.Query;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Tests.Query
{
    public class QueryExpressionPagingInfoExtensionsTests
    {
        private readonly QueryExpression _queryExpression;

        public QueryExpressionPagingInfoExtensionsTests()
        {
            _queryExpression = new QueryExpression();
        }

        [Fact]
        public void Should_return_empty_page_info_if_null()
        {
            _queryExpression.PageInfo = null;
            Assert.True(_queryExpression.IsPageInfoEmpty());
        }

        [Fact]
        public void Should_return_empty_page_info_with_default_constructor()
        {
            _queryExpression.PageInfo = new PagingInfo();
            Assert.True(_queryExpression.IsPageInfoEmpty());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(10)]
        public void Should_return_non_empty_page_info_if_page_number_is_set_to_a_number_different_than_zero(int pageNumber)
        {
            _queryExpression.PageInfo = new PagingInfo() { PageNumber = pageNumber };
            Assert.False(_queryExpression.IsPageInfoEmpty());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(10)]
        public void Should_return_non_empty_page_info_if_count_is_set_to_a_number_different_than_zero(int count)
        {
            _queryExpression.PageInfo = new PagingInfo() { Count = count };
            Assert.False(_queryExpression.IsPageInfoEmpty());
        }

        [Fact]
        public void Should_return_non_empty_page_info_if_paging_cookie_is_non_empty()
        {
            _queryExpression.PageInfo = new PagingInfo() { PagingCookie = "some dummy value" };
            Assert.False(_queryExpression.IsPageInfoEmpty());
        }

        [Fact]
        public void Should_return_non_empty_page_info_if_return_total_count_was_set()
        {
            _queryExpression.PageInfo = new PagingInfo() { ReturnTotalRecordCount = true };
            Assert.False(_queryExpression.IsPageInfoEmpty());
        }
    }
}
