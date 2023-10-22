using Crm;
using FakeXrmEasy.Exceptions.OrganizationRequestExtensionsExceptions;
using FakeXrmEasy.Extensions.OrganizationRequests;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Extensions
{
    public class OrganizationRequestExtensionsTests
    {
        private readonly Contact _target;
        private readonly EntityReference _entityReferenceTarget;

        public OrganizationRequestExtensionsTests()
        {
            _target = new Contact() { Id = Guid.NewGuid() };
            _entityReferenceTarget = _target.ToEntityReference();
        }

        [Fact]
        public void Should_return_true_for_valid_create_requests()
        {
            Assert.True(new CreateRequest().IsCreateRequest());
            Assert.True(new OrganizationRequest() { RequestName = "Create" }.IsCreateRequest());

            Assert.False(new UpdateRequest().IsCreateRequest());
            Assert.False(new OrganizationRequest() { RequestName = "Other" }.IsCreateRequest());
        }

        [Fact]
        public void Should_return_true_for_valid_update_requests()
        {
            Assert.True(new UpdateRequest().IsUpdateRequest());
            Assert.True(new OrganizationRequest() { RequestName = "Update" }.IsUpdateRequest());

            Assert.False(new CreateRequest().IsUpdateRequest());
            Assert.False(new OrganizationRequest() { RequestName = "Other" }.IsUpdateRequest());
        }

        [Fact]
        public void Should_return_true_for_valid_delete_requests()
        {
            Assert.True(new DeleteRequest().IsDeleteRequest());
            Assert.True(new OrganizationRequest() { RequestName = "Delete" }.IsDeleteRequest());

            Assert.False(new CreateRequest().IsDeleteRequest());
            Assert.False(new OrganizationRequest() { RequestName = "Other" }.IsDeleteRequest());
        }

        [Fact]
        public void Should_return_true_for_valid_retrieve_requests()
        {
            Assert.True(new RetrieveRequest().IsRetrieveRequest());
            Assert.True(new OrganizationRequest() { RequestName = "Retrieve" }.IsRetrieveRequest());

            Assert.False(new CreateRequest().IsRetrieveRequest());
            Assert.False(new OrganizationRequest() { RequestName = "Other" }.IsRetrieveRequest());
        }

        [Fact]
        public void Should_return_true_for_valid_retrieve_multiple_requests()
        {
            Assert.True(new RetrieveMultipleRequest().IsRetrieveMultipleRequest());
            Assert.True(new OrganizationRequest() { RequestName = "RetrieveMultiple" }.IsRetrieveMultipleRequest());

            Assert.False(new CreateRequest().IsRetrieveMultipleRequest());
            Assert.False(new OrganizationRequest() { RequestName = "Other" }.IsRetrieveMultipleRequest());
        }

        [Fact]
        public void Should_convert_to_create_request()
        {
            var createRequest = new CreateRequest() { Target = _target }.ToCreateRequest();
            Assert.Equal(_target, createRequest.Target);

            createRequest = new OrganizationRequest() 
            { 
                RequestName = "Create",
                Parameters = new ParameterCollection()
                {
                    { "Target", _target }
                }
            }.ToCreateRequest();

            Assert.Equal(_target, createRequest.Target);
        }

        [Fact]
        public void Should_throw_exception_when_converting_to_create_request_if_not_valid()
        {
            Assert.Throws<ToInvalidOrganizationRequestException>(() => new UpdateRequest() { Target = _target }.ToCreateRequest());
        }

        [Fact]
        public void Should_convert_to_update_request()
        {
            var request = new UpdateRequest() { Target = _target }.ToUpdateRequest();
            Assert.Equal(_target, request.Target);

            request = new OrganizationRequest()
            {
                RequestName = "Update",
                Parameters = new ParameterCollection()
                {
                    { "Target", _target }
                }
            }.ToUpdateRequest();

            Assert.Equal(_target, request.Target);
        }

        [Fact]
        public void Should_throw_exception_when_converting_to_update_request_if_not_valid()
        {
            Assert.Throws<ToInvalidOrganizationRequestException>(() => new CreateRequest() { Target = _target }.ToUpdateRequest());
        }

        [Fact]
        public void Should_convert_to_delete_request()
        {
            var request = new DeleteRequest() { Target = _entityReferenceTarget }.ToDeleteRequest();
            Assert.Equal(_entityReferenceTarget, request.Target);

            request = new OrganizationRequest()
            {
                RequestName = "Delete",
                Parameters = new ParameterCollection()
                {
                    { "Target", _entityReferenceTarget }
                }
            }.ToDeleteRequest();

            Assert.Equal(_entityReferenceTarget, request.Target);
        }

        [Fact]
        public void Should_throw_exception_when_converting_to_delete_request_if_not_valid()
        {
            Assert.Throws<ToInvalidOrganizationRequestException>(() => new CreateRequest() { Target = _target }.ToDeleteRequest());
        }

        [Fact]
        public void Should_convert_to_retrieve_request()
        {
            var request = new RetrieveRequest() { Target = _entityReferenceTarget }.ToRetrieveRequest();
            Assert.Equal(_entityReferenceTarget, request.Target);

            request = new OrganizationRequest()
            {
                RequestName = "Retrieve",
                Parameters = new ParameterCollection()
                {
                    { "Target", _entityReferenceTarget }
                }
            }.ToRetrieveRequest();

            Assert.Equal(_entityReferenceTarget, request.Target);
        }

        [Fact]
        public void Should_throw_exception_when_converting_to_retrieve_request_if_not_valid()
        {
            Assert.Throws<ToInvalidOrganizationRequestException>(() => new CreateRequest() { Target = _target }.ToRetrieveRequest());
        }

        [Fact]
        public void Should_convert_to_retrieve_multiple_request()
        {
            var query = new QueryExpression();
            var request = new RetrieveMultipleRequest() { Query = query }.ToRetrieveMultipleRequest();
            Assert.Equal(query, request.Query);

            request = new OrganizationRequest()
            {
                RequestName = "RetrieveMultiple",
                Parameters = new ParameterCollection()
                {
                    { "Query", query }
                }
            }.ToRetrieveMultipleRequest();

            Assert.Equal(query, request.Query);
        }

        [Fact]
        public void Should_throw_exception_when_converting_to_retrieve_multiple_request_if_not_valid()
        {
            Assert.Throws<ToInvalidOrganizationRequestException>(() => new CreateRequest() { Target = _target }.ToRetrieveMultipleRequest());
        }
    }
}
