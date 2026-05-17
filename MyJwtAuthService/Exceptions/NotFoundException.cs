using System.Net;

namespace MyJwtAuthService.Exceptions
{
    public sealed class NotFoundException : AppException
    {
        public NotFoundException(string message): base(message, HttpStatusCode.NotFound) { }
        public NotFoundException(string resourceName, object key): base($"{resourceName} with identifier '{key}' was not found.", HttpStatusCode.NotFound){}
    }
}
