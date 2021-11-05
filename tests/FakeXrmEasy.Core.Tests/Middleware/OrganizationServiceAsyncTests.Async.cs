using Crm;
using FakeItEasy;
using Xunit;

namespace FakeXrmEasy.Tests.Middleware
{
    public partial class OrganizationServiceAsyncTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public async void Should_call_sync_create_when_calling_async_create() 
        {
            var asyncService = _context.GetAsyncOrganizationService();

            var entity = new Contact();
            var asyncResult = await asyncService.CreateAsync(entity);

            A.CallTo(() => _service.Create(entity)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_sync_create_when_sync_create() 
        {
            var asyncService = _context.GetAsyncOrganizationService();

            var entity = new Contact();
            var asyncResult = await asyncService.CreateAsync(entity);

            A.CallTo(() => _service.Create(entity)).MustHaveHappened(); 
        }

        
        

    }


    
    
}
