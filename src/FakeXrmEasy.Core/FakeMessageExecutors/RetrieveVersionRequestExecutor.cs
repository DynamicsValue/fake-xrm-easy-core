using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.FakeMessageExecutors
{
    public class RetrieveVersionRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request is RetrieveVersionRequest;
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            string version =  "";

#if FAKE_XRM_EASY
           version = "5.0.0.0";
#elif FAKE_XRM_EASY_2013
           version = "6.0.0.0";
#elif FAKE_XRM_EASY_2015
           version = "7.0.0.0"; 
#elif FAKE_XRM_EASY_2016
           version = "8.0.0.0"; 
#elif FAKE_XRM_EASY_365
           version = "8.2.0.0"; 
#elif FAKE_XRM_EASY_9
           version = "9.0.0.0"; 
#endif

            return new RetrieveVersionResponse
            {
                Results = new ParameterCollection
                {
                    { "Version", version }
                }
            };
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(RetrieveVersionRequest);
        }
    }
}
