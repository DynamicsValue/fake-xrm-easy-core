
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Integrity;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;

namespace FakeXrmEasy.Middleware
{
    /// <summary>
    /// XrmFakedContextFactory contains helper methods to setup the middleware in a number of different scenarios
    /// </summary>
    public class XrmFakedContextFactory
    {
        /// <summary>
        /// Used to create a IXrmFakedContext with default middleware settings
        /// </summary>
        /// <param name="license">The license to use</param>
        /// <returns></returns>
        public static IXrmFakedContext New(FakeXrmEasyLicense license)
        {
            return MiddlewareBuilder
                        .New()
       
                        // Add* -> Middleware configuration
                        .AddCrud()
                        .AddFakeMessageExecutors()
                        .AddGenericFakeMessageExecutors()

                        // Use* -> Defines pipeline sequence
                        .UseCrud()
                        .UseMessages()

                        .SetLicense(license)
                        .Build();
        }

        /// <summary>
        /// Used to create a IXrmFakedContext with specific integrity options
        /// </summary>
        /// <param name="license">The license to use</param>
        /// <param name="integrityOptions">The integrity options</param>
        /// <returns></returns>
        public static IXrmFakedContext New(FakeXrmEasyLicense license, IIntegrityOptions integrityOptions)
        {
            return MiddlewareBuilder
                        .New()
       
                        // Add* -> Middleware configuration
                        .AddCrud(integrityOptions)
                        .AddFakeMessageExecutors()
                        .AddGenericFakeMessageExecutors()

                        // Use* -> Defines pipeline sequence
                        .UseCrud()
                        .UseMessages()

                        .SetLicense(license)
                        .Build();
        }
    }
}