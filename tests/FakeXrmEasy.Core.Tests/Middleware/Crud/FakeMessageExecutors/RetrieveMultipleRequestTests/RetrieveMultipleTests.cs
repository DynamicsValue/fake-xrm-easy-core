using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Crm;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.RetrieveMultipleRequestTests
{
    public class RetrieveMultipleTests : FakeXrmEasyTestsBase
    {
        #region Paging

        /// <summary>
        /// Tests that paging works correctly
        /// </summary>
        [Fact]
        public void TestPaging()
        {
            List<Entity> initialEntities = new List<Entity>();
            int excessNumberOfRecords = 50;

            (_context as XrmFakedContext).MaxRetrieveCount = 1000;
            for (int i = 0; i < (_context as XrmFakedContext).MaxRetrieveCount + excessNumberOfRecords; i++)
            {
                Entity e = new Entity("entity");
                e.Id = Guid.NewGuid();
                initialEntities.Add(e);
            }

            _context.Initialize(initialEntities);

            List<Entity> allRecords = new List<Entity>();
            QueryExpression query = new QueryExpression("entity");
            EntityCollection result = _service.RetrieveMultiple(query);
            allRecords.AddRange(result.Entities);
            Assert.Equal((_context as XrmFakedContext).MaxRetrieveCount, result.Entities.Count);
            Assert.True(result.MoreRecords);
            Assert.NotNull(result.PagingCookie);

            query.PageInfo = new PagingInfo()
            {
                PagingCookie = result.PagingCookie,
                PageNumber = 2,
            };
            result = _service.RetrieveMultiple(query);
            allRecords.AddRange(result.Entities);
            Assert.Equal(excessNumberOfRecords, result.Entities.Count);
            Assert.False(result.MoreRecords);

            foreach (Entity e in initialEntities)
            {
                Assert.Contains(allRecords, r => r.Id == e.Id);
            }
        }

        [Fact]
        public void Should_return_correct_slices_and_empty_when_page_number_exceeds_limit_and_no_cookie()
        {
            //Arrange
            // Initialize 10 entities
            var entities = new List<Entity>();
            for (int i = 0; i < 10; i++)
            {
                var e = new Entity("entity")
                {
                    Id = Guid.NewGuid(),
                    ["index"] = i
                };
                entities.Add(e);
            }

            _context.Initialize(entities);

            List<List<Entity>> actual = [];
            var query = new QueryExpression("entity")
            {
                PageInfo = new PagingInfo
                {
                    Count = 3
                }
            };

            //Act
            for (var page = 1; page <= 6; page++)
            {
                query.PageInfo.PageNumber = page;
                var result = _service.RetrieveMultiple(query);
                actual.Add(result.Entities.ToList());
            }

            //Assert
            List<List<Entity>> expected =
            [
                [entities[0], entities[1], entities[2]],
                [entities[3], entities[4], entities[5]],
                [entities[6], entities[7], entities[8]],
                [entities[9]], [], []
            ];

            AssertPagesEqual(expected, actual);
        }

        [Fact]
        public void Should_return_correct_slices_and_empty_when_page_number_exceeds_limit_and_cookie_supplied()
        {
            //Arrange
            // Initialize 10 entities
            var entities = new List<Entity>();
            for (int i = 0; i < 10; i++)
            {
                var e = new Entity("entity")
                {
                    Id = Guid.NewGuid(),
                    ["index"] = i
                };
                entities.Add(e);
            }

            _context.Initialize(entities);

            List<List<Entity>> actual = [];
            var query = new QueryExpression("entity")
            {
                PageInfo = new PagingInfo
                {
                    Count = 3
                }
            };

            //Act
            string cookie = null;
            //continue fetching pages
            for (var page = 1; page <= 6; page++)
            {
                query.PageInfo.PageNumber = 1;
                query.PageInfo.PagingCookie = cookie;
                var results = _service.RetrieveMultiple(query);
                actual.Add(results.Entities.ToList());
                cookie = results.PagingCookie;
            }

            //Assert
            List<List<Entity>> expected =
            [
                [entities[0], entities[1], entities[2]],
                [entities[3], entities[4], entities[5]],
                [entities[6], entities[7], entities[8]],
                [entities[9]], [], []
            ];

            AssertPagesEqual(expected, actual);
        }

        [Fact]
        public void Should_format_paging_cookie_correctly_when_basic_iteration()
        {
            //Arrange
            // Initialize 10 entities
            var entities = new List<Entity>();
            for (int i = 0; i < 10; i++)
            {
                var e = new Entity("entity")
                {
                    Id = Guid.NewGuid(),
                    ["index"] = i
                };
                entities.Add(e);
            }

            _context.Initialize(entities);

            List<string> cookies = [];
            var query = new QueryExpression("entity")
            {
                PageInfo = new PagingInfo
                {
                    Count = 3
                }
            };

            //Act
            //continue fetching pages
            string cookie = null;

            for (var page = 1; page <= 6; page++)
            {
                query.PageInfo.PageNumber = 1;
                query.PageInfo.PagingCookie = cookie;
                var results = _service.RetrieveMultiple(query);
                cookie = results.PagingCookie;
                cookies.Add(cookie);
            }

            //Assert
            var expected = new List<string>
            {
                GenerateCookie(1, entities[2].Id, entities[0].Id),
                GenerateCookie(2, entities[5].Id, entities[3].Id),
                GenerateCookie(3, entities[8].Id, entities[6].Id),
                GenerateCookie(4, entities[9].Id, entities[9].Id), // only one record left
                null, // no cookie when there are no more records
                null
            };

            Assert.Equal(expected, cookies);
        }

        [Fact]
        public void Should_fetch_n_returns_from_first_when_page_number_matches_paging_cookie()
        {
            // Arrange
            var entities = new List<Entity>();
            for (int i = 0; i < 20; i++)
            {
                var e = new Entity("entity")
                {
                    Id = Guid.NewGuid(),
                    ["index"] = i,
                    ["active"] = true
                };
                entities.Add(e);
            }

            _context.Initialize(entities);

            var query = new QueryExpression("entity")
            {
                PageInfo = new PagingInfo
                {
                    Count = 3
                },
                Criteria = new FilterExpression(LogicalOperator.And)
            };
            query.Criteria.AddCondition("active", ConditionOperator.Equal, true);

            // Form a cookie for page 4
            // Last = 9th entity (page 4’s last), First = 11th (anchor beyond)
            string cookie =
                GenerateCookie(4, entities[11].Id, entities[9].Id);

            // Act 1: Query page 4 with cookie
            query.PageInfo.PageNumber = 4;
            query.PageInfo.PagingCookie = cookie;
            var resultPage4 = _service.RetrieveMultiple(query);

            // Act 2: Invalidate one entity in page 4 (set active = false)
            entities[9]["active"] = false;
            _context.UpdateEntity(entities[9]);
            var resultPage4AfterInvalid = _service.RetrieveMultiple(query);

            // Act 3: Change page size, still page 4 + cookie
            query.PageInfo.Count = 2;
            var resultPage4WithDifferentCount = _service.RetrieveMultiple(query);

            // Assert 1: Original slice → expect 9, 10, 11
            var expectedPage4 = new List<Guid> { entities[9].Id, entities[10].Id, entities[11].Id };
            Assert.Equal(expectedPage4, resultPage4.Entities.Select(e => e.Id).ToList());

            // Assert 2: After invalidating entity[9] -> expect 10, 11, 12
            // This is because we are fetching entities after the first, because entities[9] was invalidated, we are now getting 10,11,12 instead 
            var expectedPage4AfterInvalid = new List<Guid> { entities[10].Id, entities[11].Id, entities[12].Id };
            Assert.Equal(expectedPage4AfterInvalid, resultPage4AfterInvalid.Entities.Select(e => e.Id).ToList());

            // Assert 3: With smaller page size (2) -> expect just first 2 (10, 11)
            var expectedPage4WithDifferentCount = new List<Guid> { entities[10].Id, entities[11].Id };
            Assert.Equal(expectedPage4WithDifferentCount,
                resultPage4WithDifferentCount.Entities.Select(e => e.Id).ToList());
        }

        [Fact]
        public void Should_anchor_forward_paging_correctly_when_entities_deleted()
        {
            // Arrange
            var entities = new List<Entity>();
            for (int i = 0; i < 20; i++)
            {
                var e = new Entity("entity")
                {
                    Id = Guid.NewGuid(),
                    ["index"] = i,
                    ["active"] = true
                };
                entities.Add(e);
            }

            _context.Initialize(entities);

            var query = new QueryExpression("entity")
            {
                PageInfo = new PagingInfo
                {
                    Count = 3
                },
                Criteria = new FilterExpression(LogicalOperator.And)
            };
            query.Criteria.AddCondition("active", ConditionOperator.Equal, true);

            // Form a cookie for page 4 (anchor at [9,10,11])
            string cookie =
                GenerateCookie(4, entities[11].Id, entities[9].Id);

            query.PageInfo.PageNumber = 5;
            query.PageInfo.PagingCookie = cookie;

            //invalidate a bunch of records, because we are using a paging cookie, none of these previous ones matter
            entities[0]["active"] = false;
            _context.UpdateEntity(entities[0]);
            entities[5]["active"] = false;
            _context.UpdateEntity(entities[5]);
            entities[11]["active"] = false;
            _context.UpdateEntity(entities[11]);
            entities[10]["active"] = false;
            _context.UpdateEntity(entities[10]);

            // Act 1: Fetch page 5
            var page5 = _service.RetrieveMultiple(query);

            // Act 2: Fetch page 6
            query.PageInfo.PageNumber = 6;
            var page6 = _service.RetrieveMultiple(query);

            // Invalidate [13]
            entities[13]["active"] = false;
            _context.UpdateEntity(entities[13]);

            // Act 3: Fetch page 5 again
            query.PageInfo.PageNumber = 5;
            var page5AfterDelete = _service.RetrieveMultiple(query);

            // Invalidate [16]
            entities[16]["active"] = false;
            _context.UpdateEntity(entities[16]);

            // Act 4: Fetch page 6 again
            query.PageInfo.PageNumber = 6;
            var page6AfterDelete = _service.RetrieveMultiple(query);

            // Assert baseline
            var expectedPage5 = new List<Guid> { entities[12].Id, entities[13].Id, entities[14].Id };
            Assert.Equal(expectedPage5, page5.Entities.Select(e => e.Id).ToList());

            var expectedPage6 = new List<Guid> { entities[15].Id, entities[16].Id, entities[17].Id };
            Assert.Equal(expectedPage6, page6.Entities.Select(e => e.Id).ToList());

            // After deleting [13]
            var expectedPage5AfterDelete = new List<Guid> { entities[12].Id, entities[14].Id, entities[15].Id };
            Assert.Equal(expectedPage5AfterDelete, page5AfterDelete.Entities.Select(e => e.Id).ToList());

            // After deleting [16]
            var expectedPage6AfterDelete = new List<Guid> { entities[17].Id, entities[18].Id, entities[19].Id };
            Assert.Equal(expectedPage6AfterDelete, page6AfterDelete.Entities.Select(e => e.Id).ToList());
        }

        [Fact]
        public void Should_page_backwards_relative_to_cookie_with_invalidation()
        {
            // Arrange
            var entities = new List<Entity>();
            for (int i = 0; i < 20; i++)
            {
                var e = new Entity("entity")
                {
                    Id = Guid.NewGuid(),
                    ["goal"] = (i + 1).ToString(),
                    ["active"] = true
                };
                entities.Add(e);
            }

            _context.Initialize(entities);


            //invalidating these later entries shouldn't do anything
            entities[11]["active"] = false;
            entities[12]["active"] = false;
            entities[19]["active"] = false;

            _context.UpdateEntity(entities[11]);
            _context.UpdateEntity(entities[12]);
            _context.UpdateEntity(entities[19]);

            var query = new QueryExpression("entity")
            {
                PageInfo = new PagingInfo
                {
                    Count = 3
                },
                Criteria = new FilterExpression(LogicalOperator.And)
            };
            query.Criteria.AddCondition("active", ConditionOperator.Equal, true);

            // Form a cookie for page 4 (anchor at [9,10,11])
            string cookie =
                GenerateCookie(4, entities[11].Id, entities[9].Id);

            // --- Page 3 baseline
            query.PageInfo.PageNumber = 3;
            query.PageInfo.PagingCookie = cookie;
            var page3 = _service.RetrieveMultiple(query);

            // --- Page 2 baseline
            query.PageInfo.PageNumber = 2;
            var page2 = _service.RetrieveMultiple(query);

            // --- Delete entity[0]
            entities[0]["active"] = false;
            _context.UpdateEntity(entities[0]);

            // Page 3 after delete [0]
            query.PageInfo.PageNumber = 3;
            var page3AfterDelete0 = _service.RetrieveMultiple(query);

            // --- Delete entity[8]
            entities[8]["active"] = false;
            _context.UpdateEntity(entities[8]);

            // Page 3 after delete [8]
            query.PageInfo.PageNumber = 3;
            var page3AfterDelete8 = _service.RetrieveMultiple(query);

            // --- Page 2 again
            query.PageInfo.PageNumber = 2;
            var page2AfterDeletes = _service.RetrieveMultiple(query);

            // Assert
            // page 3 is fine, we get 6,7,8, deleting the after records should have done nothing
            Assert.Equal(
                new[] { entities[6].Id, entities[7].Id, entities[8].Id },
                page3.Entities.Select(e => e.Id).ToArray());

            //once again page 2 should be 3,4,5 because deleting the after records doesn't do anything
            Assert.Equal(
                new[] { entities[3].Id, entities[4].Id, entities[5].Id },
                page2.Entities.Select(e => e.Id).ToArray());

            //after invalidating entities[0] we expect it to skip 2 * 3 = 6 entities, so 7 - 9
            Assert.Equal(
                new[] { entities[7].Id, entities[8].Id, entities[9].Id },
                page3AfterDelete0.Entities.Select(e => e.Id).ToArray());

            // After deleting entity[8], it still skips 6 entities, but it can't get any entities greater than paging cookie.first,
            // so it cuts out entities[10]
            Assert.Equal(
                new[] { entities[7].Id, entities[9].Id },
                page3AfterDelete8.Entities.Select(e => e.Id).ToArray());

            // Page 2 should remain unaffected: [4,5,6], deleting 8 shouldn't have done anything, only deleting[0] does. 
            Assert.Equal(
                new[] { entities[4].Id, entities[5].Id, entities[6].Id },
                page2AfterDeletes.Entities.Select(e => e.Id).ToArray());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(-100)]
        public void Should_throw_when_page_number_less_than_1(int pageNumber)
        {
            var query = new QueryExpression("entity")
                { PageInfo = new PagingInfo { PageNumber = pageNumber, Count = 3 } };
            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(query));
            Assert.Equal("0x80040203", ex.Detail.ErrorCode.ToString("X"));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(-100)]
        public void Should_throw_when_cookie_page_number_is_negative(int pageNumber)
        {
            var query = new QueryExpression("entity")
            {
                PageInfo = new PagingInfo
                {
                    PageNumber = 1,
                    PagingCookie = GenerateCookie(pageNumber, Guid.NewGuid(), Guid.NewGuid())
                }
            };
            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(query));
            Assert.Equal("0x80040216", ex.Detail.ErrorCode.ToString("X"));
        }

        [Fact]
        public void Should_throw_when_topcount_and_pageinfo_used_together()
        {
            var query = new QueryExpression("entity")
            {
                PageInfo = new PagingInfo { PageNumber = 1, Count = 3 },
                TopCount = 5
            };
            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(query));
            Assert.Equal("0x80040203", ex.Detail.ErrorCode.ToString("X"));
        }

        [Fact]
        public void Should_throw_when_cookie_is_malformed()
        {
            var query = new QueryExpression("entity")
            {
                PageInfo = new PagingInfo
                {
                    PageNumber = 1,
                    PagingCookie = "<cookie bad"
                }
            };
            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(query));
            Assert.Equal("0x80040201", ex.Detail.ErrorCode.ToString("X"));
        }

        [Fact]
        public void Should_throw_when_cookie_missing_first_or_last()
        {
            var query = new QueryExpression("entity")
            {
                PageInfo = new PagingInfo
                {
                    PageNumber = 4,
                    PagingCookie = "<cookie page=\"4\"><entityid /></cookie>"
                }
            };

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(query));
            Assert.Equal("0x80040201", ex.Detail.ErrorCode.ToString("X"));
            Assert.Contains("Malformed XML Passed to in the Paging Cookie", ex.Detail.Message);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(-2)]
        [InlineData(-100)]
        public void Should_allow_negative_or_zero_count_values(int count)
        {
            // Arrange
            var entities = new List<Entity>();
            for (int i = 0; i < 10; i++)
            {
                entities.Add(new Entity("entity")
                {
                    Id = Guid.NewGuid(),
                    ["index"] = i
                });
            }

            _context.Initialize(entities);

            var query = new QueryExpression("entity")
            {
                PageInfo = new PagingInfo
                {
                    PageNumber = 1,
                    Count = count
                }
            };

            // Act
            var result = _service.RetrieveMultiple(query);

            // Assert
            // In real Dataverse, this returns "everything" (up to max page size).
            // So here we assert all records are returned, ignoring the invalid Count.
            Assert.Empty(result.Entities);
            Assert.False(result.MoreRecords);
        }


        /// <summary>
        /// Tests that if we ask for a non-existant page we don't get anything back and an error doesn't occur
        /// </summary>
        [Fact]
        public void TestAskingForEmptyPage()
        {
            List<Entity> initialEntities = new List<Entity>();

            Entity first = new Entity("entity");
            first.Id = Guid.NewGuid();
            initialEntities.Add(first);

            _context.Initialize(initialEntities);

            QueryExpression query = new QueryExpression("entity");
            query.PageInfo = new PagingInfo() { PageNumber = 2, Count = 20 };
            Assert.Empty(_service.RetrieveMultiple(query).Entities);
        }


        /// <summary>
        /// Helper method for checking two sets of entity pages are equal. Both expected and actual are lists of lists,
        /// where each outer list represents a page retrieved, and the inner list is the entites in the list
        /// </summary>
        /// <param name="expected">The expected page structure</param>
        /// <param name="actual">The actual page structure</param>
        private void AssertPagesEqual(List<List<Entity>> expected, List<List<Entity>> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            for (int pageIndex = 0; pageIndex < expected.Count; pageIndex++)
            {
                var expectedPage = expected[pageIndex];
                var actualPage = actual[pageIndex];

                Assert.Equal(expectedPage.Count, actualPage.Count);

                for (int i = 0; i < expectedPage.Count; i++)
                {
                    Assert.Equal(expectedPage[i].Id, actualPage[i].Id);
                }
            }
        }

        private string GenerateCookie(int pageNumber, Guid last, Guid first)
        {
            return $"<cookie page=\"{pageNumber}\"><entityid last=\"{{{last}}}\" first=\"{{{first}}}\" /></cookie>";
        }

        #endregion

        /// <summary>
        /// Tests that paging works correctly
        /// </summary>
        [Fact]
        public void TestDistinct()
        {
            Entity e1 = new Entity("entity");
            e1.Id = Guid.NewGuid();
            e1["name"] = "FakeXrmEasy";

            Entity e2 = new Entity("entity");
            e2.Id = Guid.NewGuid();
            e2["name"] = "FakeXrmEasy";

            _context.Initialize(new Entity[] { e1, e2 });

            var fetchXml =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' returntotalrecordcount='true'>
                              <entity name='entity'>
                                    <attribute name='name' />                                    
                              </entity>
                            </fetch>";
            var query = new FetchExpression(fetchXml);
            EntityCollection result = _service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
            Assert.False(result.MoreRecords);
        }


        /// <summary>
        /// Tests that an empty result set doesn't cause an error and that more records is correctly set to false
        /// </summary>
        [Fact]
        public void TestEmptyResultSet()
        {
            List<Entity> initialEntities = new List<Entity>();

            Entity e = new Entity("entity");
            e.Id = Guid.NewGuid();
            e["retrieve"] = false;
            initialEntities.Add(e);

            _context.Initialize(initialEntities);

            QueryExpression query = new QueryExpression("entity");
            query.Criteria.AddCondition("retrieve", ConditionOperator.Equal, true);
            EntityCollection result = _service.RetrieveMultiple(query);
            Assert.Empty(result.Entities);
            Assert.False(result.MoreRecords);
        }

        /// <summary>
        /// Tests that a query with a filter on a link entity works the second time it is used (this was due to a shallow copy issue) 
        /// </summary>
        [Fact]
        public void TestMultiplePagesWithLinkedEntity()
        {
            List<Entity> initialEntities = new List<Entity>();
            int excessNumberOfRecords = 50;

            (_context as XrmFakedContext).MaxRetrieveCount = 1000;
            for (int i = 0; i < (_context as XrmFakedContext).MaxRetrieveCount + excessNumberOfRecords; i++)
            {
                Entity second = new Entity("second");
                second.Id = Guid.NewGuid();
                second["filter"] = true;
                initialEntities.Add(second);
                Entity first = new Entity("entity");
                first.Id = Guid.NewGuid();
                first["secondid"] = second.ToEntityReference();
                initialEntities.Add(first);
            }

            _context.Initialize(initialEntities);

            QueryExpression query = new QueryExpression("entity");
            LinkEntity link = new LinkEntity("entity", "second", "secondid", "secondid", JoinOperator.Inner);
            link.EntityAlias = "second";
            link.LinkCriteria.AddCondition("filter", ConditionOperator.Equal, true);
            query.LinkEntities.Add(link);
            EntityCollection result = _service.RetrieveMultiple(query);
            Assert.Equal((_context as XrmFakedContext).MaxRetrieveCount, result.Entities.Count);
            Assert.True(result.MoreRecords);
            Assert.NotNull(result.PagingCookie);

            query.PageInfo = new PagingInfo()
            {
                PagingCookie = result.PagingCookie,
                PageNumber = 2,
            };
            result = _service.RetrieveMultiple(query);
            Assert.Equal(excessNumberOfRecords, result.Entities.Count);
            Assert.False(result.MoreRecords);
        }

        /// <summary>
        /// Tests that a link's criteria aren't changed by the query (this was a buggy behavior due to a shallow copy)
        /// </summary>
        [Fact]
        public void TestLinkCriteriaAreNotChanged()
        {
            List<Entity> initialEntities = new List<Entity>();

            Entity second = new Entity("second");
            second.Id = Guid.NewGuid();
            second["filter"] = true;
            Entity first = new Entity("entity");
            first.Id = Guid.NewGuid();
            first["secondid"] = second.ToEntityReference();
            initialEntities.Add(first);

            _context.Initialize(initialEntities);

            QueryExpression query = new QueryExpression("entity");
            LinkEntity link = new LinkEntity("entity", "second", "secondid", "secondid", JoinOperator.Inner);
            link.EntityAlias = "second";
            link.LinkCriteria.AddCondition("filter", ConditionOperator.Equal, true);
            query.LinkEntities.Add(link);
            _service.RetrieveMultiple(query);

            Assert.Equal("filter", query.LinkEntities[0].LinkCriteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.Equal, query.LinkEntities[0].LinkCriteria.Conditions[0].Operator);
            Assert.Equal(true, query.LinkEntities[0].LinkCriteria.Conditions[0].Values[0]);
        }

        /// <summary>
        /// Tests that if distinct is asked for that a distinct number of entities is returned
        /// </summary>
        [Fact]
        public void TestThatDistinctWorks()
        {
            List<Entity> initialEntities = new List<Entity>();

            Entity first = new Entity("entity");
            first.Id = Guid.NewGuid();
            first["field"] = "value";
            initialEntities.Add(first);

            Entity related = new Entity("related");
            related.Id = Guid.NewGuid();
            related["entityid"] = first.ToEntityReference();
            related["include"] = true;
            initialEntities.Add(related);

            Entity secondRelated = new Entity("related");
            secondRelated.Id = Guid.NewGuid();
            secondRelated["entityid"] = first.ToEntityReference();
            secondRelated["include"] = true;
            initialEntities.Add(secondRelated);

            _context.Initialize(initialEntities);

            QueryExpression query = new QueryExpression("entity");
            query.ColumnSet = new ColumnSet("field");
            query.Distinct = true;

            LinkEntity link = new LinkEntity("entity", "related", "entityid", "entityid", JoinOperator.Inner);
            link.LinkCriteria.AddCondition("include", ConditionOperator.Equal, true);

            query.LinkEntities.Add(link);

            Assert.Single(_service.RetrieveMultiple(query).Entities);
        }

        /// <summary>
        /// Tests that if distinct is asked for and fields are pulled in from the link entities that the correct 
        /// records are returned
        /// </summary>
        [Fact]
        public void TestThatDistinctWorksWithLinkEntityFields()
        {
            List<Entity> initialEntities = new List<Entity>();

            Entity first = new Entity("entity");
            first.Id = Guid.NewGuid();
            first["field"] = "value";
            initialEntities.Add(first);

            Entity related = new Entity("related");
            related.Id = Guid.NewGuid();
            related["entityid"] = first.ToEntityReference();
            related["include"] = true;
            related["linkfield"] = "value";
            initialEntities.Add(related);

            Entity secondRelated = new Entity("related");
            secondRelated.Id = Guid.NewGuid();
            secondRelated["entityid"] = first.ToEntityReference();
            secondRelated["include"] = true;
            secondRelated["linkfield"] = "other value";
            initialEntities.Add(secondRelated);

            _context.Initialize(initialEntities);

            QueryExpression query = new QueryExpression("entity");
            query.ColumnSet = new ColumnSet("field");
            query.Distinct = true;

            LinkEntity link = new LinkEntity("entity", "related", "entityid", "entityid", JoinOperator.Inner);
            link.LinkCriteria.AddCondition("include", ConditionOperator.Equal, true);
            link.Columns = new ColumnSet("linkfield");

            query.LinkEntities.Add(link);

            Assert.Equal(2, _service.RetrieveMultiple(query).Entities.Count);
        }

        /// <summary>
        /// Tests that if PageInfo's ReturnTotalRecordCount sets total record count.
        /// </summary>
        [Fact]
        public void TestThatPageInfoTotalRecordCountWorks()
        {
            List<Entity> initialEntities = new List<Entity>();

            Entity e = new Entity("entity");
            e.Id = Guid.NewGuid();
            e["retrieve"] = true;
            initialEntities.Add(e);

            Entity e2 = new Entity("entity");
            e2.Id = Guid.NewGuid();
            e2["retrieve"] = true;
            initialEntities.Add(e2);

            Entity e3 = new Entity("entity");
            e3.Id = Guid.NewGuid();
            e3["retrieve"] = false;
            initialEntities.Add(e3);

            _context.Initialize(initialEntities);

            QueryExpression query = new QueryExpression("entity");
            query.PageInfo.ReturnTotalRecordCount = true;
            query.Criteria.AddCondition("retrieve", ConditionOperator.Equal, true);

            EntityCollection result = _service.RetrieveMultiple(query);
            Assert.Equal(2, result.Entities.Count);
            Assert.Equal(2, result.TotalRecordCount);
            Assert.False(result.MoreRecords);
        }

        /// <summary>
        /// Tests that if PageInfo's ReturnTotalRecordCount works correctly with paging 
        /// </summary>
        [Fact]
        public void TestThatPageInfoTotalRecordCountWorksWithPaging()
        {
            List<Entity> initialEntities = new List<Entity>();

            for (int i = 0; i < 100; i++)
            {
                Entity e = new Entity("entity");
                e.Id = Guid.NewGuid();
                initialEntities.Add(e);
            }

            _context.Initialize(initialEntities);

            QueryExpression query = new QueryExpression("entity");
            query.PageInfo.ReturnTotalRecordCount = true;
            query.PageInfo.PageNumber = 1;
            query.PageInfo.Count = 10;

            EntityCollection result = _service.RetrieveMultiple(query);
            Assert.Equal(10, result.Entities.Count);
            Assert.Equal(100, result.TotalRecordCount);
            Assert.True(result.MoreRecords);

            query.PageInfo.PageNumber++;
            query.PageInfo.Count = 20;
            query.PageInfo.PagingCookie = result.PagingCookie;

            result = _service.RetrieveMultiple(query);
            Assert.Equal(20, result.Entities.Count);
            Assert.Equal(100, result.TotalRecordCount);
            Assert.True(result.MoreRecords);
        }

        [Fact]
        public void TestNestedFiltersWithLateBoundEntities()
        {
            Entity account = new Entity("account") { Id = Guid.NewGuid() };
            account["name"] = "test";

            Entity contact = new Entity("contact") { Id = Guid.NewGuid() };
            contact["accountid"] = account.ToEntityReference();
            contact["birthdate"] = null;
            contact["territorycode"] = null;

            _context.Initialize(new List<Entity>
            {
                account,
                contact
            });

            var query = new QueryExpression("account");
            query.ColumnSet = new ColumnSet("name");
            query.Criteria.AddCondition(new ConditionExpression("name", ConditionOperator.Like, "test"));

            var linkEntity = query.AddLink("contact", "accountid", "accountid", JoinOperator.Inner);
            linkEntity.LinkCriteria.AddFilter(new FilterExpression(LogicalOperator.Or)
            {
                Filters =
                {
                    new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("birthdate", ConditionOperator.Null),
                            new ConditionExpression("territorycode", ConditionOperator.Null)
                        }
                    },
                    new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("birthdate", ConditionOperator.NotNull),
                            new ConditionExpression("territorycode", ConditionOperator.NotNull)
                        }
                    }
                }
            });

            var results = _service.RetrieveMultiple(query).Entities;
            Assert.Single(results);
        }

        [Fact]
        public void TestNestedFiltersWithEarlyBoundEntities()
        {
            Account account = new Account() { Id = Guid.NewGuid() };
            account.Name = "test";

            Contact contact = new Contact() { Id = Guid.NewGuid() };
            contact["accountid"] = account.ToEntityReference();
            contact.BirthDate = null;
            contact.TerritoryCode = null;

            _context.Initialize(new List<Entity>
            {
                account,
                contact
            });

            var query = new QueryExpression("account");
            query.ColumnSet = new ColumnSet("name");
            query.Criteria.AddCondition(new ConditionExpression("name", ConditionOperator.Like, "test"));

            var linkEntity = query.AddLink("contact", "accountid", "accountid", JoinOperator.Inner);
            linkEntity.LinkCriteria.AddFilter(new FilterExpression(LogicalOperator.Or)
            {
                Filters =
                {
                    new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("birthdate", ConditionOperator.Null),
                            new ConditionExpression("territorycode", ConditionOperator.Null)
                        }
                    },
                    new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("birthdate", ConditionOperator.NotNull),
                            new ConditionExpression("territorycode", ConditionOperator.NotNull)
                        }
                    }
                }
            });

            var results = _service.RetrieveMultiple(query).Entities.Cast<Account>().ToList();
            Assert.Single(results);
        }

        [Fact]
        public void Should_Populate_EntityReference_Name_When_Metadata_Is_Provided()
        {
            var userMetadata = new EntityMetadata() { LogicalName = "systemuser" };
            userMetadata.SetSealedPropertyValue("PrimaryNameAttribute", "fullname");

            var user = new Entity() { LogicalName = "systemuser", Id = Guid.NewGuid() };
            user["fullname"] = "Fake XrmEasy";

            _context.InitializeMetadata(userMetadata);
            _context.Initialize(user);
            (_context as XrmFakedContext).CallerProperties.CallerId = user.ToEntityReference();

            var account = new Entity() { LogicalName = "account" };
            var accountId = _service.Create(account);

            QueryExpression query = new QueryExpression("account");
            query.ColumnSet = new ColumnSet(true);

            var accounts = _service.RetrieveMultiple(query);

            Assert.Equal("Fake XrmEasy", accounts.Entities[0].GetAttributeValue<EntityReference>("ownerid").Name);
        }


#if !FAKE_XRM_EASY
        [Fact]
        public void Can_Filter_Using_Entity_Name_Without_Alias()
        {
            Entity e = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["retrieve"] = true
            };

            Entity e2 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["contactid"] = e.ToEntityReference()
            };

            _context.Initialize(new Entity[] { e, e2 });

            QueryExpression query = new QueryExpression("account");
            query.Criteria.AddCondition("contact", "retrieve", ConditionOperator.Equal, true);
            query.AddLink("contact", "contactid", "contactid");
            EntityCollection result = _service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Can_Filter_Using_Entity_Name_With_Alias()
        {
            Entity e = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["retrieve"] = true
            };

            Entity e2 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["contactid"] = e.ToEntityReference()
            };

            _context.Initialize(new Entity[] { e, e2 });

            QueryExpression query = new QueryExpression("account");
            query.Criteria.AddCondition("mycontact", "retrieve", ConditionOperator.Equal, true);
            query.AddLink("contact", "contactid", "contactid").EntityAlias = "mycontact";
            EntityCollection result = _service.RetrieveMultiple(query);
            Assert.Single(result.Entities);
        }
#endif

        [Fact]
        public void Should_Allow_Using_Aliases_with_Dot()
        {
            var contact = new Entity("contact") { Id = Guid.NewGuid() };
            contact["firstname"] = "Jordi";

            var account = new Entity("account") { Id = Guid.NewGuid() };
            account["primarycontactid"] = contact.ToEntityReference();
            account["name"] = "Dynamics Value";

            _context.Initialize(new Entity[] { contact, account });

            QueryExpression query = new QueryExpression("account");
            query.ColumnSet = new ColumnSet("name");
            var link = query.AddLink("contact", "contactid", "primarycontactid");
            link.EntityAlias = "primary.contact";
            link.Columns = new ColumnSet("firstname");

            var accounts = _service.RetrieveMultiple(query);

            Assert.True(accounts.Entities.First().Contains("primary.contact.firstname"));
            Assert.Equal("Jordi",
                accounts.Entities.First().GetAttributeValue<AliasedValue>("primary.contact.firstname").Value);
        }

        [Fact]
        public void TheCorrectResultIsReturnedWhenUsingConditionOperatorInWithGuid()
        {
            var contact = new Contact()
            {
                Id = Guid.NewGuid()
            };
            _context.Initialize(contact);

            var Ids = new string[] { Guid.NewGuid().ToString(), contact.Id.ToString() };

            var query = new QueryExpression("contact");
            query.Criteria.AddCondition("contactid", ConditionOperator.In, Ids);

            var result = _service.RetrieveMultiple(query).Entities;
            Assert.True(result.Any());
            Assert.Equal(contact.Id, result[0].Id);
        }
    }
}