using FakeXrmEasy.Abstractions;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        /// <summary>
        /// Stores a reference of an IOrganizationServiceAsync interface without cancellation token support
        /// </summary>
        protected internal readonly IOrganizationServiceAsync _serviceAsync;

        /// <summary>
        /// Reference to an async IOrganizationService instance with cancellation token support
        /// </summary>
        protected internal readonly  IOrganizationServiceAsync2 _serviceAsync2;

        /// <summary>
        /// Gets an IOrganizationService interface with async support and without cancellation tokens
        /// See https://dynamicsvalue.github.io/fake-xrm-easy-docs/quickstart/advanced/async/ for more details.
        /// </summary>
        /// <returns></returns>
        public IOrganizationServiceAsync GetAsyncOrganizationService() 
        {
            return _serviceAsync;
        }

        /// <summary>
        /// Gets an IOrganizationService interface with async support and cancellation tokens
        /// See https://dynamicsvalue.github.io/fake-xrm-easy-docs/quickstart/advanced/async/ for more details.
        /// </summary>
        /// <returns></returns>
        public IOrganizationServiceAsync2 GetAsyncOrganizationService2() 
        {
            return _serviceAsync2;
        }
    }
}