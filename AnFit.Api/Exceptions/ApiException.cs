using System;
using System.Collections.Generic;

namespace AnFit.Api.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public ApiException(string message, int statusCode = 500, IEnumerable<string>? errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}