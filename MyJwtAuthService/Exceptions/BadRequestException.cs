using System.Net;

namespace MyJwtAuthService.Exceptions
{
    public sealed class BadRequestException : AppException
    {
        public BadRequestException(string message)
            : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}
