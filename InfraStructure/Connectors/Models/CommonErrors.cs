using System;
using System.Collections.Generic;
using System.Text;

namespace InfraStructure.Connectors.Models
{
    public static class CommonErrors
    {

        /// <summary>
        /// Error header code
        /// </summary>
        public enum ErrorHeaderCode
        {
            BadArgument,
            NotFound,
            TooManyRequests,
            UnexpectedParameters,
            InternalError,
            ServiceUnavailable
        }

        /// <summary>
        /// Error detail code
        /// </summary>
        public enum ErrorDetailCode
        {
            NullValue,
            MalformedValue,
            TooManyRequests,
            UnexpectedParameter,
            BadArgument,
            InternalError,
            ExternalServiceFailure,
            AuthenticationError,
            AuthorisationError,
            ExternalResourceNotFound,
            UnexpectedParameterUsedWithDependentService,
            BadArgumentUsedWithDependentService,
            BadData
        }

        /// <summary>
        /// Common error header messages
        /// </summary>
        public struct CommonErrorHeaderMessage
        {
            public const string InvalidParameterValueProvidedToOperation = "Invalid parameter value provided to operation";
            public const string MultipleErrorsInInputArguments = "Multiple errors in input arguments";
            public const string TooManyRequestsFromThisClient = "Too many requests from this client";
            public const string UnexpectedParametersForThisOperation = "Unexpected parameter(s) for this operation";
            public const string InternalErrorWithinApi = "Internal Error within API";
            public const string ExternalDependentServiceFailed = "External dependent service failed";
            public const string ExternalResourceNotFound = "External resource not found";
            public const string FailedToUseDependentService = "Failed to use dependent service";
            public const string BadData = "Data source contains invalid data";
            public const string InvalidParameterCombination = "Invalid parameter combination";
        }

        /// <summary>
        /// Common error detail messages
        /// </summary>
        public struct CommonErrorDetailsMessage
        {
            public const string MustNotBeNull = "Must not be null";
            public const string IsNotValid = "Is not valid";

            /// <summary>
            /// Limit of &lt;Limit&gt; calls exceeded for this client
            /// </summary>
            public const string LimitOfCallsExceededForThisClient = "Limit of {0} calls exceeded for this client";

            /// <summary>
            /// The following parameters are expected: &lt;List parameters&gt;
            /// </summary>
            public const string TheFollowingParametersAreExpected = "The following parameters are expected: {0}";

            /// <summary>
            /// The parameter is expected to be one of the following: &lt;Expected values&gt;
            /// </summary>
            public const string TheParameterIsExpectedToBeOneOfTheFollowing = "The parameter is expected to be one of the following: {0}";

            public const string FailedToPerformTranslationLogic = "Failed to perform translation logic";
            public const string FailedToConnectToDependentService = "Failed to connect to dependent service";
            public const string FailedToAuthenticateWithDependentService = "Failed to authenticate with dependent service";
            public const string NotAuthorisedToConnectToDependentService = "Not authorised to connect to dependent service";
            public const string NoResourceFound = "No resource found";

            /// <summary>
            /// Unexpected parameter used: &lt;Name of parameter&gt;
            /// </summary>
            public const string UnexpectedParameterUsed = "Unexpected parameter used: {0}";

            /// <summary>
            /// Bad argument used: &lt;Value of the argument&gt;
            /// </summary>
            public const string BadArgumentUsed = "Bad argument used: {0}";
        }

    }
}
