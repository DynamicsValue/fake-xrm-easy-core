using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Integration.Tests
{
    public class FakeXrmEasyTestsBase
    {
        protected readonly IXrmFakedContext _context;
        protected readonly IOrganizationService _service;
        
        protected FakeXrmEasyTestsBase()
        {
            _context = XrmFakedContextFactory.New(FakeXrmEasyLicense.RPL_1_5);
            _service = _context.GetOrganizationService();
        }
    }
}