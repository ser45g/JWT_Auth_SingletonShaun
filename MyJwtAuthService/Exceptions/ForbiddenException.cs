using System.Net;

namespace MyJwtAuthService.Exceptions
{
    public sealed class ForbiddenException : AppException
    {
        public ForbiddenException() : this("Forbidden") { }
        public ForbiddenException(string message)
            : base(message, HttpStatusCode.Forbidden)
        {
        }
    }
}
