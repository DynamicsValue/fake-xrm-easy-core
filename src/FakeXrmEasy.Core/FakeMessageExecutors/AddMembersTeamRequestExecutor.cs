using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;

namespace FakeXrmEasy.FakeMessageExecutors
{
	public class AddMembersTeamRequestExecutor : IFakeMessageExecutor
	{
		public bool CanExecute(OrganizationRequest request)
		{
			return request is AddMembersTeamRequest;
		}

		public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
		{
			var req = (AddMembersTeamRequest)request;

			if (req.MemberIds == null)
			{
				throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument, "MemberIds parameter is required");
			}

			if (req.TeamId == Guid.Empty)
			{
				throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument, "TeamId parameter is required");
			}

			var service = ctx.GetOrganizationService();

			// Find the list
			var team = ctx.CreateQuery("team").FirstOrDefault(e => e.Id == req.TeamId);

			if (team == null)
			{
				throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.ObjectDoesNotExist, string.Format("Team with Id {0} wasn't found", req.TeamId.ToString()));
			}

			//ToDo:	throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.CannotAddMembersToDefaultTeam, "Can't add members to the default business unit team.");

			foreach (var memberId in req.MemberIds)
			{
				var user = ctx.CreateQuery("systemuser").FirstOrDefault(e => e.Id == memberId);
				if (user == null)
				{
					throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.ObjectDoesNotExist, string.Format("SystemUser with Id {0} wasn't found", memberId.ToString()));
				}

				// Create teammembership
				var teammembership = new Entity("teammembership");
				teammembership["teamid"] = team.Id;
				teammembership["systemuserid"] = memberId;
				service.Create(teammembership);
			}

			return new AddMembersTeamResponse();
		}

		public Type GetResponsibleRequestType()
		{
			return typeof(AddMembersTeamRequest);
		}
	}
}