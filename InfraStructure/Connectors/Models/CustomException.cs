using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace InfraStructure.Connectors.Models
{
    public class CustomException : Exception
    {
        // Main container of error details
        // ReSharper disable once ConvertToAutoProperty because this requires C# 7.3+ and results in a compiler warning when using VS2017 which uses C# 7.0 by default
        public ErrorDetailModel[] Details => _details;

        // Status code to send back to the API caller
        // ReSharper disable once ConvertToAutoProperty
        public HttpStatusCode StatusCode => _statusCode;

        // Used to specify the type of error within the API.  Sometimes it's used as a string version of the StatusCode
        // ReSharper disable once ConvertToAutoProperty
        public string ErrorCode => _errorCode;

        [NonSerialized] readonly string _errorCode = HttpStatusCode.InternalServerError.ToString();
        [NonSerialized] readonly ErrorDetailModel[] _details;
        [NonSerialized] readonly HttpStatusCode _statusCode = HttpStatusCode.InternalServerError;

        // Various constructors to capture differing combinations of the details

        public CustomException(string message, string errorCode, ErrorDetailModel[] details, HttpStatusCode statusCode = HttpStatusCode.ServiceUnavailable) : base(message)
        {
            _statusCode = statusCode;
            _errorCode = errorCode;
            _details = details;
        }

        public CustomException(string message, HttpStatusCode statusCode, ErrorDetailModel[] details) : base(message)
        {
            _statusCode = statusCode;
            _errorCode = statusCode.ToString();
            _details = details;
        }

        public CustomException(string message, Exception innerException) : base(message, innerException)
        {

        }

        public CustomException(string message, HttpStatusCode statusCode, Exception innerException) : base(message, innerException)
        {
            _statusCode = statusCode;
            _errorCode = statusCode.ToString();
        }

        public CustomException(string message) : base(message)
        {

        }

        public CustomException(string message, string errorCode) : base(message)
        {
            _errorCode = errorCode;
        }

        public CustomException(string message, HttpStatusCode statusCode) : base(message)
        {
            _statusCode = statusCode;
            _errorCode = statusCode.ToString();
        }

        public CustomException(string message, string errorCode, Exception innerException) : base(message, innerException)
        {
            _errorCode = errorCode;
        }

        protected CustomException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

    }
}
