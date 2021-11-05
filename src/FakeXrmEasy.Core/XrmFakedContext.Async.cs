using FakeXrmEasy.Abstractions;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        protected internal readonly IOrganizationServiceAsync _serviceAsync;
        protected internal readonly  IOrganizationServiceAsync2 _serviceAsync2;
        
        public IOrganizationServiceAsync GetAsyncOrganizationService() 
        {
            return _serviceAsync;
        }

        public IOrganizationServiceAsync2 GetAsyncOrganizationService2() 
        {
            return _serviceAsync2;
        }
    }
}