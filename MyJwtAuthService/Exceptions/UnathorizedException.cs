using System.Net;

namespace MyJwtAuthService.Exceptions
{
    public sealed class UnathorizedException : AppException
    {
        public UnathorizedException() : this("Unauthorized") { }
        public UnathorizedException(string message)
            : base(message, HttpStatusCode.Unauthorized)
        {
        }
    }
}
