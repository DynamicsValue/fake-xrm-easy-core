using Microsoft.Xrm.Sdk;
using System.ServiceModel;

namespace FakeXrmEasy
{
    public class FakeOrganizationServiceFaultFactory
    {
        public static void Throw(ErrorCodes errorCode, string message)
        {
            throw new FaultException<OrganizationServiceFault>(new OrganizationServiceFault() { ErrorCode = (int)errorCode, Message = message }, new FaultReason(message));
        }

        public static void Throw(string message)
        {
            throw new FaultException<OrganizationServiceFault>(new OrganizationServiceFault() { Message = message }, new FaultReason(message));
        }
    }
}