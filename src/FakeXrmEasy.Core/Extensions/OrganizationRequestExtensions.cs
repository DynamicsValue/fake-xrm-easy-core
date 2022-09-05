using FakeXrmEasy.Exceptions.OrganizationRequestExtensionsExceptions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Extensions.OrganizationRequests
{
    /// <summary>
    /// Extension methods for OrganizationRequest 
    /// </summary>
    public static class OrganizationRequestExtensions
    {
        /// <summary>
        /// Returns true if the request is a strongly-typed CreateRequest or its equivalent generic OrganizationRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsCreateRequest(this OrganizationRequest request)
        {
            return request is CreateRequest || request.RequestName?.ToLower() == "create";
        }

        /// <summary>
        /// Returns true if the request is a strongly-typed UpdateRequest or its equivalent generic OrganizationRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsUpdateRequest(this OrganizationRequest request)
        {
            return request is UpdateRequest || request.RequestName?.ToLower() == "update";
        }

        /// <summary>
        /// Returns true if the request is a strongly-typed DeleteRequest or its equivalent generic OrganizationRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsDeleteRequest(this OrganizationRequest request)
        {
            return request is DeleteRequest || request.RequestName?.ToLower() == "delete";
        }

        /// <summary>
        /// Returns true if the request is a strongly-typed RetrieveRequest or its equivalent generic OrganizationRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsRetrieveRequest(this OrganizationRequest request)
        {
            return request is RetrieveRequest || request.RequestName?.ToLower() == "retrieve";
        }

        /// <summary>
        /// Returns true if the request is a strongly-typed RetrieveMultipleRequest or its equivalent generic OrganizationRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsRetrieveMultipleRequest(this OrganizationRequest request)
        {
            return request is RetrieveMultipleRequest || request.RequestName?.ToLower() == "retrievemultiple";
        }

        /// <summary>
        /// Returns a consistent stronly-typed CreateRequest regardless if the original was strongly-typed or not.
        /// Will throw an exception if it's not a valid request to convert to
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static CreateRequest ToCreateRequest(this OrganizationRequest request)
        {
            if (!request.IsCreateRequest())
                throw new ToInvalidOrganizationRequestException();

            if (request is CreateRequest)
                return (CreateRequest) request;

            return new CreateRequest()
            {
                Target = (Entity) request.Parameters["Target"]
            };
        }

        /// <summary>
        /// Returns a consistent stronly-typed UpdateRequest regardless if the original was strongly-typed or not.
        /// Will throw an exception if it's not a valid request to convert to
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static UpdateRequest ToUpdateRequest(this OrganizationRequest request)
        {
            if (!request.IsUpdateRequest())
                throw new ToInvalidOrganizationRequestException();

            if (request is UpdateRequest)
                return (UpdateRequest)request;

            var updateRequest = new UpdateRequest()
            {
                Target = (Entity)request.Parameters["Target"]
            };

#if FAKE_XRM_EASY_2015 || FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
            updateRequest.ConcurrencyBehavior = request.Parameters.ContainsKey("ConcurrencyBehavior") ?
                                        (ConcurrencyBehavior)request.Parameters["ConcurrencyBehavior"]
                                        : ConcurrencyBehavior.Default;
#endif

            return updateRequest;
        }

        /// <summary>
        /// Returns a consistent stronly-typed DeleteRequest regardless if the original was strongly-typed or not.
        /// Will throw an exception if it's not a valid request to convert to
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static DeleteRequest ToDeleteRequest(this OrganizationRequest request)
        {
            if (!request.IsDeleteRequest())
                throw new ToInvalidOrganizationRequestException();

            if (request is DeleteRequest)
                return (DeleteRequest)request;

            var deleteRequest = new DeleteRequest()
            {
                Target = (EntityReference) request.Parameters["Target"]
            };

#if FAKE_XRM_EASY_2015 || FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
            deleteRequest.ConcurrencyBehavior = request.Parameters.ContainsKey("ConcurrencyBehavior") ?
                                        (ConcurrencyBehavior)request.Parameters["ConcurrencyBehavior"]
                                        : ConcurrencyBehavior.Default;
#endif
            return deleteRequest;
        }

        /// <summary>
        /// Returns a consistent stronly-typed RetrieveRequest regardless if the original was strongly-typed or not.
        /// Will throw an exception if it's not a valid request to convert to
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static RetrieveRequest ToRetrieveRequest(this OrganizationRequest request)
        {
            if (!request.IsRetrieveRequest())
                throw new ToInvalidOrganizationRequestException();

            if (request is RetrieveRequest)
                return (RetrieveRequest)request;

            return new RetrieveRequest()
            {
                Target = (EntityReference)request.Parameters["Target"]
            };
        }

        /// <summary>
        /// Returns a consistent stronly-typed RetrieveMultipleRequest regardless if the original was strongly-typed or not.
        /// Will throw an exception if it's not a valid request to convert to
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static RetrieveMultipleRequest ToRetrieveMultipleRequest(this OrganizationRequest request)
        {
            if (!request.IsRetrieveMultipleRequest())
                throw new ToInvalidOrganizationRequestException();

            if (request is RetrieveMultipleRequest)
                return (RetrieveMultipleRequest)request;

            return new RetrieveMultipleRequest()
            {
                Query = (QueryBase)request.Parameters["Query"]
            };
        }

        /// <summary>
        /// Returns true if the request is a Crud organization request
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <returns></returns>
        public static bool IsCrudRequest(this OrganizationRequest request)
        {
            return request.IsCreateRequest()
                || request.IsUpdateRequest()
                || request.IsDeleteRequest()
                || request.IsRetrieveRequest()
                || request.IsRetrieveMultipleRequest();
        }

        /// <summary>
        /// Converts a CRUD request to its strongly typed version
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static OrganizationRequest ToStronglyTypedCrudRequest(this OrganizationRequest request)
        {
            if (request.IsCreateRequest()) return request.ToCreateRequest();
            if (request.IsUpdateRequest()) return request.ToUpdateRequest();
            if (request.IsDeleteRequest()) return request.ToDeleteRequest();
            if (request.IsRetrieveRequest()) return request.ToRetrieveRequest();
            if (request.IsRetrieveMultipleRequest()) return request.ToRetrieveMultipleRequest();

            return request;
        }
    }
}
