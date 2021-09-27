using System.Collections.Generic;

namespace EPiServer.Automation.commercehapi.Models
{
    /// <summary>
    /// ErrorResponse Object
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// The error object
        /// </summary>
        public ErrorMessage Error { get; set; }

        /// <summary>
        /// Check if error message is equal to expected message
        /// </summary>
        /// <param name="expectedMessage">expected message</param>
        /// <returns>return true if error message is equal to the expected message</returns>
        public bool ErrorMessageIs(string expectedMessage)
        {
            return expectedMessage == Error.Message;
        }

        /// <summary>
        /// Check if error code is equal to expected code
        /// </summary>
        /// <param name="expectedCode">expected code</param>
        /// <returns>return true if error code is equal to the expected code</returns>
        public bool ErrorCodeIs(string expectedCode)
        {
            return expectedCode == Error.Code;
        }
    }

    /// <summary>
    /// Error message class
    /// </summary>
    public class ErrorMessage
    {
        /// <summary>
        /// Error code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// The target of the error
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// Details error
        /// </summary>
        public List<DetailError> Details { get; set; }
    }

    /// <summary>
    /// Detail Error Class
    /// </summary>
    public class DetailError
    {
        /// <summary>
        /// Error code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The target of the error
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// List of details about specific errors
        /// </summary>
        public List<object> Details { get; set; }
    }

    public class ErrorResponse1
    {
        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// Detail
        /// </summary>
        public string Detail { get; set; }
        /// <summary>
        /// TraceId
        /// </summary>
        public string TraceId { get; set; }
        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Details error
        /// </summary>
        public List<DetailError> Details { get; set; }
    }
}
