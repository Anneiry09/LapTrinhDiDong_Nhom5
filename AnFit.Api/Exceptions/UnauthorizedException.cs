namespace AnFit.Api.Exceptions
{
    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message = "Bạn chưa đăng nhập hoặc token đã hết hạn.")
            : base(message, 401)
        {
        }
    }
}