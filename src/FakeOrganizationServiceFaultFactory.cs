using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy
{
    public class FakeOrganizationServiceFaultFactory
    {
        public static Exception New(ErrorCodes errorCode, string message)
        {
            return new FaultException<OrganizationServiceFault>(new OrganizationServiceFault() { ErrorCode = (int)errorCode, Message = message }, new FaultReason(message));
        }

        public static Exception New(string message)
        {
            return new FaultException<OrganizationServiceFault>(new OrganizationServiceFault() { Message = message }, new FaultReason(message));
        }
    }
}