using Crm;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Tests.Issues
{
    public class Issue300: FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_Create_Account_With_Local_DateTime_And_Retrieve_Utc()
        {
            DateTime dateTimeNow = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            Account account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Goggle ltd",
                LastUsedInCampaign = dateTimeNow
            };

            _service.Create(account);

            var retrievedAccount = _context.CreateQuery<Account>().SingleOrDefault(p => p.Id == account.Id);

            Assert.NotNull(retrievedAccount);
            Assert.True(retrievedAccount.LastUsedInCampaign.HasValue);
            Assert.Equal(DateTime.SpecifyKind(dateTimeNow, DateTimeKind.Utc).Kind, retrievedAccount.LastUsedInCampaign.Value.Kind);
        }

        [Fact]
        public void Should_Initialize_Account_With_Local_DateTime_And_Retrieve_Utc()
        {
            DateTime dateTimeNow = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            Account account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Goggle ltd",
                LastUsedInCampaign = dateTimeNow
            };

            _context.Initialize(new List<Entity> { account });

            var retrievedAccount = _context.CreateQuery<Account>().SingleOrDefault(p => p.Id == account.Id);

            Assert.NotNull(retrievedAccount);
            Assert.True(retrievedAccount.LastUsedInCampaign.HasValue);
            Assert.Equal(DateTime.SpecifyKind(dateTimeNow, DateTimeKind.Utc).Kind, retrievedAccount.LastUsedInCampaign.Value.Kind);
        }

        [Fact]
        public void Should_Update_Account_With_Local_DateTime_And_Retrieve_Utc()
        {
            DateTime dateTimeNow = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            Account account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Goggle ltd",
                LastUsedInCampaign = dateTimeNow
            };

            _service.Create(account);

            account.LastUsedInCampaign = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            _service.Update(account);

            var retrievedAccount = _context.CreateQuery<Account>().SingleOrDefault(p => p.Id == account.Id);

            Assert.NotNull(retrievedAccount);
            Assert.True(retrievedAccount.LastUsedInCampaign.HasValue);
            Assert.Equal(DateTime.SpecifyKind(dateTimeNow, DateTimeKind.Utc).Kind, retrievedAccount.LastUsedInCampaign.Value.Kind);
        }
    }
}