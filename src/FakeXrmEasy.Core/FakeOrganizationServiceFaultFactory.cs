using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeOrganizationServiceFaultFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Exception New(ErrorCodes errorCode, string message)
        {
            return new FaultException<OrganizationServiceFault>(new OrganizationServiceFault() { ErrorCode = (int)errorCode, Message = message }, new FaultReason(message));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Exception New(string message)
        {
            return new FaultException<OrganizationServiceFault>(new OrganizationServiceFault() { Message = message }, new FaultReason(message));
        }
    }
}