using System.Collections.Generic;

namespace AnFit.Api.Exceptions
{
    public class ValidationException : ApiException
    {
        public ValidationException(IEnumerable<string> errors)
            : base("Dữ liệu đầu vào không hợp lệ.", 422, errors)
        {
        }
    }
}