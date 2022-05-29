using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;

namespace FakeXrmEasy.Core.Exceptions.Query
{
    /// <summary>
    /// Exception thrown when both PageInfo and TopCount properties are set in a query.  More info: https://docs.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.query.querybyattribute.topcount?view=dynamics-general-ce-9
    /// </summary>
    public class CantSetBothPageInfoAndTopCountException
    {
        /// <summary>
        /// Throws a new FaulException using a platform error
        /// </summary>
        /// <param name="topCountValue"></param>
        /// <returns></returns>
        public static FaultException<OrganizationServiceFault> New(int topCountValue)
        {
            return new FaultException<OrganizationServiceFault>(new OrganizationServiceFault(), new FaultReason($"The Top.Count = {topCountValue} can't be specified with pagingInfo: You can't set both a PageInfo and a TopCount property at the same time. More info: https://docs.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.query.querybyattribute.topcount?view=dynamics-general-ce-9"));
        }

    }
}
