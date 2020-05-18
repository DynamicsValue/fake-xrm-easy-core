#if FAKE_XRM_EASY_2013 || FAKE_XRM_EASY_2015 || FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using FakeXrmEasy.FakeMessageExecutors;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using Crm;
using Xunit;
using FakeXrmEasy.Abstractions.Permissions;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware;

namespace FakeXrmEasy.Tests.FakeContextTests.AddUserToRecordTeamRequestTests
{
    public class AddUserToRecordTeamRequestTests
    {
        private readonly IXrmFakedContext _context;
        private readonly IOrganizationService _service;

        public AddUserToRecordTeamRequestTests()
        {
            _context = XrmFakedContextFactory.New();
            _service = _context.GetOrganizationService();
        }

        [Fact]
        public void When_can_execute_is_called_with_an_invalid_request_result_is_false()
        {
            var executor = new AddUserToRecordTeamRequestExecutor();
            var anotherRequest = new AddToQueueRequest();
            Assert.False(executor.CanExecute(anotherRequest));
        }

        [Fact]
        public void When_a_request_is_called_User_Is_Added_To_Record_Team()
        {

            var teamTemplate = new TeamTemplate
            {
                Id = Guid.NewGuid(),
                DefaultAccessRightsMask = (int)AccessRights.ReadAccess
            };

            var user = new SystemUser
            {
                Id = Guid.NewGuid()
            };

            var account = new Account
            {
                Id = Guid.NewGuid()
            };

            _context.Initialize(new Entity[]
            {
                teamTemplate, user, account
            });

            var executor = new AddUserToRecordTeamRequestExecutor();

            var req = new AddUserToRecordTeamRequest
            {
                Record = account.ToEntityReference(),
                SystemUserId = user.Id,
                TeamTemplateId = teamTemplate.Id
            };

            executor.Execute(req, _context);

            var team = _context.CreateQuery<Team>().FirstOrDefault(p => p.TeamTemplateId.Id == teamTemplate.Id);
            Assert.NotNull(team);

            var teamMembership = _context.CreateQuery<TeamMembership>().FirstOrDefault(p => p.SystemUserId == user.Id && p.TeamId == team.Id);
            Assert.NotNull(teamMembership);

            var poa = _context.CreateQuery("principalobjectaccess").FirstOrDefault(p => (Guid)p["objectid"] == account.Id && 
                                                                                       (Guid)p["principalid"] == team.Id);
            Assert.NotNull(poa);

            var response = _context.GetProperty<IAccessRightsRepository>().RetrievePrincipalAccess(account.ToEntityReference(),
                user.ToEntityReference());
            Assert.Equal((AccessRights)teamTemplate.DefaultAccessRightsMask, response.AccessRights);

        }
    }
}
#endif