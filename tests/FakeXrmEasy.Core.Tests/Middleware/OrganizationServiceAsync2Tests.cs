

using System.Threading;
using FakeItEasy;
using Microsoft.PowerPlatform.Dataverse.Client;
using Xunit;

namespace FakeXrmEasy.Tests.Middleware
{
    public class OrganizationServiceAsync2Tests : OrganizationServiceAsyncTests
    {
        protected IOrganizationServiceAsync2 _serviceAsync2;

        public OrganizationServiceAsync2Tests() : base() 
        {
            _serviceAsync2 = _context.GetAsyncOrganizationService2();
        }

        protected override void InitServiceAsync()
        {
            _serviceAsync = _context.GetAsyncOrganizationService2();
        }

        [Fact]
        public async void Should_call_create_when_calling_async_create_with_cancellation() 
        {
            var asyncResult = await _serviceAsync2.CreateAsync(_contact, new CancellationToken());

            A.CallTo(() => _service.Create(_contact)).MustHaveHappened(); 
        }
    }
}