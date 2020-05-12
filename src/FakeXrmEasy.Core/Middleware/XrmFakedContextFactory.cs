
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
                        
                        .AddCrud()   //Crud  setup 
                        .AddMessages()

                        .UseCrud()   //Pipeline sequence
                        .UseMessages()

                        .Build();
        }
    }
}