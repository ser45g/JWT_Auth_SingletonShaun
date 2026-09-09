using System.Net;

namespace MyJwtAuthService.Exceptions
{
    public class InternalServerErrorException : AppException
    {
        public InternalServerErrorException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
