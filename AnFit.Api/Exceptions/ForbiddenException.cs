namespace AnFit.Api.Exceptions
{
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message = "Bạn không có quyền thực hiện hành động này.")
            : base(message, 403)
        {
        }
    }
}