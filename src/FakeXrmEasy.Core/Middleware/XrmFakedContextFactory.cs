
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;

namespace FakeXrmEasy.Middleware
{
    public class XrmFakedContextFactory
    {
        public static IXrmFakedContext New()
        {
            return MiddlewareBuilder
                        .New()
       
                        // Add* -> Middleware configuration
                        .AddCrud()   
                        .AddFakeMessageExecutors()

                        // Use* -> Defines pipeline sequence
                        .UseCrud() 
                        .UseMessages()


                        .Build();
        }
    }
}