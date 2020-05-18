using FakeXrmEasy.Abstractions;
using FakeXrmEasy.FakeMessageExecutors;
using FakeXrmEasy.Middleware;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Linq;
using System.ServiceModel;
using Xunit;

namespace FakeXrmEasy.Tests.FakeContextTests.CloseIncidentRequestTests
{
    public class CloseIncidentRequestTests
    {
        private const int StatusProblemSolved = 5;

        private readonly IXrmFakedContext _context;
        private readonly IOrganizationService _service;
        
        public CloseIncidentRequestTests()
        {
            _context = XrmFakedContextFactory.New();
            _service = _context.GetOrganizationService();
        }

        [Fact]
        public void When_a_request_is_called_Incident_Is_Resolved()
        {
            var incident = new Entity
            {
                LogicalName = Crm.Incident.EntityLogicalName,
                Id = Guid.NewGuid()
            };

            _context.Initialize(new[]
            {
                incident
            });

            var executor = new CloseIncidentRequestExecutor();

            Entity incidentResolution = new Entity
            {
                LogicalName = Crm.IncidentResolution.EntityLogicalName,
                ["subject"] = "subject",
                ["incidentid"] = new EntityReference(Crm.Incident.EntityLogicalName, incident.Id)
            };

            CloseIncidentRequest closeIncidentRequest = new CloseIncidentRequest
            {
                IncidentResolution = incidentResolution,
                Status = new OptionSetValue(StatusProblemSolved)
            };

            executor.Execute(closeIncidentRequest, _context);

            var retrievedIncident = _context.CreateQuery(Crm.Incident.EntityLogicalName).Single();

            Assert.Equal(StatusProblemSolved, retrievedIncident.GetAttributeValue<OptionSetValue>("statuscode").Value);
            Assert.Equal((int)Crm.IncidentState.Resolved, retrievedIncident.GetAttributeValue<OptionSetValue>("statecode").Value);
        }

        [Fact]
        public void When_a_request_with_invalid_incidentid_is_called_exception_is_raised()
        {
            
            _context.Initialize(new Entity(Crm.Incident.EntityLogicalName) { Id = Guid.NewGuid() });
            var executor = new CloseIncidentRequestExecutor();

            Entity incidentResolution = new Entity
            {
                LogicalName = Crm.IncidentResolution.EntityLogicalName,
                ["subject"] = "subject",
                ["incidentid"] = new EntityReference(Crm.Incident.EntityLogicalName, Guid.NewGuid())
            };

            CloseIncidentRequest closeIncidentRequest = new CloseIncidentRequest
            {
                IncidentResolution = incidentResolution,
                Status = new OptionSetValue(StatusProblemSolved)
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => executor.Execute(closeIncidentRequest, _context));
        }

        [Fact]
        public void When_a_request_without_incident_resolution_is_called_exception_is_raised()
        {
            

            var incident = new Entity
            {
                LogicalName = Crm.Incident.EntityLogicalName,
                Id = Guid.NewGuid()
            };

            _context.Initialize(new[]
            {
                incident
            });

            var executor = new CloseIncidentRequestExecutor();

            CloseIncidentRequest closeIncidentRequest = new CloseIncidentRequest
            {
                IncidentResolution = null,
                Status = new OptionSetValue(StatusProblemSolved)
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => executor.Execute(closeIncidentRequest, _context));
        }

        [Fact]
        public void When_a_request_without_status_is_called_exception_is_raised()
        {
            

            var incident = new Entity
            {
                LogicalName = Crm.Incident.EntityLogicalName,
                Id = Guid.NewGuid()
            };

            _context.Initialize(new[]
            {
                incident
            });

            Entity incidentResolution = new Entity
            {
                LogicalName = Crm.IncidentResolution.EntityLogicalName,
                ["subject"] = "subject",
                ["incidentid"] = new EntityReference(Crm.Incident.EntityLogicalName, incident.Id)
            };

            var executor = new CloseIncidentRequestExecutor();

            CloseIncidentRequest closeIncidentRequest = new CloseIncidentRequest
            {
                IncidentResolution = incidentResolution,
                Status = null
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => executor.Execute(closeIncidentRequest, _context));
        }

        [Fact]
        public void When_can_execute_is_called_with_an_invalid_request_result_is_false()
        {
            var executor = new CloseIncidentRequestExecutor();
            var anotherRequest = new RetrieveMultipleRequest();
            Assert.False(executor.CanExecute(anotherRequest));
        }
    }
}