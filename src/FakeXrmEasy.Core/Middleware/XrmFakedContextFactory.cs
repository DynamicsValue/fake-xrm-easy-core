
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware.Crud;

namespace FakeXrmEasy.Middleware
{
    public class XrmFakedContextFactory
    {
        public static IXrmFakedContext New()
        {
            return MiddlewareBuilder
                            .New()
                            .AddCrud()   //Crud  setup 
                            .UseCrud()   //Pipeline sequence
                            .Build();
        }
    }
}