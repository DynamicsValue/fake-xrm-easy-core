using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Tests
{
    public class FakeXrmEasyTests
    {
        protected readonly IXrmFakedContext _context;
        protected readonly IOrganizationService _service;
        
        protected FakeXrmEasyTests()
        {
            _context = XrmFakedContextFactory.New();
            _service = _context.GetOrganizationService();
        }
    }
}