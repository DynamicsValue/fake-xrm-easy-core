
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware.Crud;

namespace FakeXrmEasy.Middleware
{
    public class XrmFakedContextFactory
    {
        public static IXrmFakedContext New()
        {
            var builder = MiddlewareBuilder.New();

            //Dependency "injection"
            builder.AddCrud();

            //Pipeline sequence
            builder.UseCrud();
            
            return builder.Build();
        }
    }
}