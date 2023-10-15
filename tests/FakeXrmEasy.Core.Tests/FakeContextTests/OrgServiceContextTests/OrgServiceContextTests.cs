#if !FXE_V2 //Rpcrt4 related, not working on Linux
using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Linq;
using Xunit;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.Tests.FakeContextTests.OrgServiceContextTests
{
    public class OrgServiceContextTests : FakeXrmEasyTestsBase
    {
        //   MS_ISSUE  Rpcrt4 related
        [Fact]
        public void When_calling_context_add_and_save_changes_entity_is_added_to_the_faked_context()
        {
            using (var ctx = new XrmServiceContext(_service))
            {
                ctx.AddObject(new Account() { Name = "Test account" });
                ctx.SaveChanges();

                var account = ctx.CreateQuery<Account>()
                            .ToList()
                            .FirstOrDefault();

                Assert.Equal("Test account", account.Name);
            }
        }



        //    MS_ISSUE  Rpcrt4 related

        [Fact]
        public void When_calling_context_add_and_save_changes_returns_correct_result()
        {
            using (var ctx = new XrmServiceContext(_service))
            {
                ctx.AddObject(new Account() { Name = "Test account" });
                var result = ctx.SaveChanges();

                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.Equal("Create", result.First().Response.ResponseName);
                Assert.IsType<CreateResponse>(result.First().Response);
                Assert.NotEqual(Guid.Empty, ((CreateResponse)result.First().Response).id);
            }
        }



        //   MS_ISSUE  Rpcrt4 related

        [Fact]
        public void When_calling_context_add_addrelated_and_save_changes_entities_are_added_to_the_faked_context()
        {
            var relationship = new XrmFakedRelationship()
            {
                IntersectEntity = "accountleads",
                Entity1Attribute = "accountid",
                Entity2Attribute = "leadid",
                Entity1LogicalName = "account",
                Entity2LogicalName = "lead"
            };
            _context.AddRelationship("accountleads", relationship);

            using (var ctx = new XrmServiceContext(_service))
            {
                var account = new Account() { Name = "Test account" };
                ctx.AddObject(account);

                var contact = new Lead() { FirstName = "Jane", LastName = "Doe" };
                ctx.AddRelatedObject(account, new Relationship("accountleads"), contact);
                var result = ctx.SaveChanges();

                var resultaccount = ctx.CreateQuery<Account>()
                                       .ToList()
                                       .FirstOrDefault();

                Assert.NotNull(resultaccount);
                Assert.Equal("Test account", resultaccount.Name);

                var reaultlead = ctx.CreateQuery<Lead>()
                                    .ToList()
                                    .FirstOrDefault();

                Assert.NotNull(reaultlead);
                Assert.Equal("Jane", reaultlead.FirstName);
                Assert.Equal("Doe", reaultlead.LastName);

                var relationshipRecords = ctx.CreateQuery("accountleads")
                                             .ToList();
                Assert.NotEmpty(relationshipRecords);
            }
        }

    }
}
#endif